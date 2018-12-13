using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lizoc.TextScript.Parsing
{
    /// <summary>
    /// Lexer enumerator that generates <see cref="Token"/>, to use in a foreach.
    /// </summary>
    public class Lexer : IEnumerable<Token>
    {
        private TextPosition _position;
        private readonly int _textLength;
        private Token _token;
        private char c;
        private BlockType _blockType;
        private bool _isLiquidTagBlock;
        private List<LogMessage> _errors;
        private int _openBraceCount;
        private int _escapeRawCharCount;
        private bool _isExpectingFrontMatter;
        private readonly bool _isLiquid;

        private readonly char _stripWhiteSpaceFullSpecialChar;
        private readonly char _stripWhiteSpaceRestrictedSpecialChar;
        private const char RawEscapeSpecialChar = '%';
        private readonly Queue<Token> _pendingTokens;

        /// <summary>
        /// Lexer options.
        /// </summary>
        public readonly LexerOptions Options;

        /// <summary>
        /// Initialize a new instance of this <see cref="Lexer" />.
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <param name="sourcePath">The sourcePath</param>
        /// <param name="options">The options for the lexer</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public Lexer(string text, string sourcePath = null, LexerOptions? options = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));

            // Setup options
            LexerOptions localOptions = options ?? LexerOptions.Default;
            if (localOptions.FrontMatterMarker == null)
                localOptions.FrontMatterMarker = LexerOptions.DefaultFrontMatterMarker;

            Options = localOptions;

            _position = Options.StartPosition;

            if (_position.Offset > text.Length)
                throw new ArgumentOutOfRangeException(string.Format(RS.LexerStartPositionOutOfRange, _position.Offset, "0", text.Length - 1));

            _textLength = text.Length;

            SourcePath = sourcePath ?? "<input>";
            _blockType = Options.Mode == ScriptMode.ScriptOnly ? BlockType.Code : BlockType.Raw;
            _pendingTokens = new Queue<Token>();

            _isExpectingFrontMatter = Options.Mode == ScriptMode.FrontMatterOnly || Options.Mode == ScriptMode.FrontMatterAndContent;
            _isLiquid = Options.Mode == ScriptMode.Liquid;
            _stripWhiteSpaceFullSpecialChar = '-';
            _stripWhiteSpaceRestrictedSpecialChar = '~';
        }

        /// <summary>
        /// Gets the text being parsed by this lexer
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// Gets a boolean indicating whether this lexer has errors.
        /// </summary>
        public bool HasErrors => _errors != null && _errors.Count > 0;

        /// <summary>
        /// Gets error messages.
        /// </summary>
        public IEnumerable<LogMessage> Errors => _errors ?? Enumerable.Empty<LogMessage>();

        /// <summary>
        /// TokenEnumerator. Use simply <code>foreach</code> on this instance to automatically trigger an enumeration of the tokens.
        /// </summary>
        /// <returns></returns>
        public TokenEnumerator GetEnumerator()
        {
            return new TokenEnumerator(this);
        }

        private bool MoveNext()
        {
            TextPosition previousPosition = new TextPosition();
            bool isFirstLoop = true;

            while (true)
            {
                if (_pendingTokens.Count > 0)
                {
                    _token = _pendingTokens.Dequeue();
                    return true;
                }

                // If we have errors or we are already at the end of the file, we don't continue
                if (HasErrors || _token.Type == TokenType.Eof)
                    return false;

                if (_position.Offset == _textLength)
                {
                    _token = Token.Eof;
                    return true;
                }

                // Safe guard in any case where the lexer has an error and loop forever (in case we forget to eat a token)
                if (!isFirstLoop && previousPosition == _position)
                    throw new InvalidOperationException(RS.LexerStuckInLoop);

                isFirstLoop = false;
                previousPosition = _position;

                if (Options.Mode != ScriptMode.ScriptOnly)
                {
                    bool hasEnter = false;
                    if (_blockType == BlockType.Raw)
                    {
                        TokenType whiteSpaceMode;
                        if (IsCodeEnterOrEscape(out whiteSpaceMode))
                        {
                            ReadCodeEnterOrEscape();
                            hasEnter = true;
                            if (_blockType == BlockType.Code || _blockType == BlockType.Raw)
                                return true;
                        }
                        else if (_isExpectingFrontMatter && TryParseFrontMatterMarker())
                        {
                            _blockType = BlockType.Code;
                            return true;
                        }
#if DEBUG
                        Debug.Assert(_blockType == BlockType.Raw || _blockType == BlockType.Escape);
#endif
                        // Else we have a BlockType.EscapeRaw, so we need to parse the raw block
                    }

                    if (!hasEnter && _blockType != BlockType.Raw && IsCodeExit())
                    {
                        bool wasInBlock = _blockType == BlockType.Code;
                        ReadCodeExitOrEscape();
                        if (wasInBlock)
                            return true;

                        // We are exiting from a BlockType.EscapeRaw, so we are back to a raw block or code, so we loop again
                        continue;
                    }

                    if (_blockType == BlockType.Code && _isExpectingFrontMatter && TryParseFrontMatterMarker())
                    {
                        // Once we have parsed a front matter, we don't expect them any longer
                        _blockType = BlockType.Raw;
                        _isExpectingFrontMatter = false;
                        return true;
                    }
                }

                // We may me directly at the end of the EOF without reading anykind of block
                // So we need to exit here
                if (_position.Offset == _textLength)
                {
                    _token = Token.Eof;
                    return true;
                }

                if (_blockType == BlockType.Code)
                {
                    if (_isLiquid)
                    {
                        if (ReadCodeLiquid())
                            break;
                    }
                    else if (ReadCode())
                    {
                        break;
                    }
                }
                else
                {
                    if (ReadRaw())
                        break;
                }
            }

            return true;
        }

        private enum BlockType
        {
            Code,

            Escape,

            Raw,
        }

        private bool TryParseFrontMatterMarker()
        {
            TextPosition start = _position;
            TextPosition end = _position;

            string marker = Options.FrontMatterMarker;
            int i = 0;
            for (; i < marker.Length; i++)
            {
                if (PeekChar(i) != marker[i])
                    return false;
            }
            char pc = PeekChar(i);
            while (pc == ' ' || pc == '\t')
            {
                i++;
                pc = PeekChar(i);
            }

            bool valid = false;

            if (pc == '\n')
            {
                valid = true;
            }
            else if (pc == '\r')
            {
                valid = true;
                if (PeekChar(i + 1) == '\n')
                    i++;
            }

            if (valid)
            {
                while (i-- >= 0)
                {
                    end = _position;
                    NextChar();
                }

                _token = new Token(TokenType.FrontMatterMarker, start, end);
                return true;
            }

            return false;
        }

        private bool IsCodeEnterOrEscape(out TokenType whitespaceMode)
        {
            whitespaceMode = TokenType.Invalid;
            if (c == '{')
            {
                int i = 1;
                char nc = PeekChar(i);
                if (!_isLiquid)
                {
                    while (nc == RawEscapeSpecialChar)
                    {
                        i++;
                        nc = PeekChar(i);
                    }
                }

                if (nc == '{' || (_isLiquid && nc == '%'))
                {
                    char charSpace = PeekChar(i + 1);
                    if (charSpace == _stripWhiteSpaceFullSpecialChar)
                        whitespaceMode = TokenType.WhitespaceFull;
                    else if (!_isLiquid && charSpace == _stripWhiteSpaceRestrictedSpecialChar)
                        whitespaceMode = TokenType.Whitespace;

                    return true;
                }
            }
            return false;
        }

        private void ReadCodeEnterOrEscape()
        {
            TextPosition start = _position;
            TextPosition end = _position;

            NextChar(); // Skip {

            if (!_isLiquid)
            {
                while (c == RawEscapeSpecialChar)
                {
                    _escapeRawCharCount++;
                    end = end.NextColumn();
                    NextChar();
                }
            }

            end = end.NextColumn();
            if (_isLiquid && c == '%')
                _isLiquidTagBlock = true;

            NextChar(); // Skip {

            if (c == _stripWhiteSpaceFullSpecialChar || (!_isLiquid && c == _stripWhiteSpaceRestrictedSpecialChar))
            {
                end = end.NextColumn();
                NextChar();
            }

            if (_escapeRawCharCount > 0)
            {
                _blockType = BlockType.Escape;
            }
            else
            {
                if (_isLiquid && _isLiquidTagBlock)
                {
                    if (TryReadLiquidCommentOrRaw(start, end))
                        return;
                }
                _blockType = BlockType.Code;
                _token = new Token(_isLiquidTagBlock ? TokenType.LiquidTagEnter : TokenType.CodeEnter, start, end);
            }
        }

        private bool TryReadLiquidCommentOrRaw(TextPosition codeEnterStart, TextPosition codeEnterEnd)
        {
            TextPosition start = _position;
            int offset = 0;
            PeekSkipSpaces(ref offset);
            bool isComment;
            if ((isComment = TryMatchPeek("comment", offset, out offset)) || TryMatchPeek("raw", offset, out offset))
            {
                PeekSkipSpaces(ref offset);
                if (TryMatchPeek("%}", offset, out offset))
                {
                    start = new TextPosition(start.Offset + offset, start.Line, start.Column + offset);
                    // Reinitialize the position to the prior character
                    _position = new TextPosition(start.Offset - 1, start.Line, start.Column - 1);
                    c = '}';
                    while (true)
                    {
                        TextPosition end = _position;
                        NextChar();
                        if (c == '{')
                        {
                            NextChar();
                            if (c == '%')
                            {
                                NextChar();
                                if (c == '-')
                                    NextChar();

                                SkipSpaces();

                                if (TryMatch(isComment ? "endcomment" : "endraw"))
                                {
                                    SkipSpaces();
                                    TextPosition codeExitStart = _position;

                                    if (c == '-')
                                        NextChar();

                                    if (c == '%')
                                    {
                                        NextChar();
                                        if (c == '}')
                                        {
                                            TextPosition codeExitEnd = _position;
                                            NextChar(); // Skip }
                                            _blockType = BlockType.Raw;
                                            if (isComment)
                                            {
                                                // Convert a liquid comment into a textscript multi-line {{ ## comment ## }}
                                                _token = new Token(TokenType.CodeEnter, codeEnterStart, codeEnterEnd);
                                                _pendingTokens.Enqueue(new Token(TokenType.CommentMulti, start, end));
                                                _pendingTokens.Enqueue(new Token(TokenType.CodeExit, codeExitStart, codeExitEnd));
                                            }
                                            else
                                            {
                                                _token = new Token(TokenType.Escape, start, end);
                                                _pendingTokens.Enqueue(new Token(TokenType.EscapeCount1, end, end));
                                            }
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (c == 0)
                        {
                            break;
                        }
                    }
                }
            }
            return false;
        }

        private void SkipSpaces()
        {
            while (IsWhitespace(c))
            {
                NextChar();
            }
        }

        private void PeekSkipSpaces(ref int i)
        {
            while (true)
            {
                char nc = PeekChar(i);
                if (nc == ' ' || nc == '\t')
                    i++;
                else
                    break;
            }
        }

        private bool TryMatchPeek(string text, int offset, out int offsetOut)
        {
            offsetOut = offset;
            for (int index = 0; index < text.Length; offset++, index++)
            {
                if (PeekChar(offset) != text[index])
                    return false;
            }
            offsetOut = offset;
            return true;
        }

        private bool TryMatch(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (c != text[i])
                    return false;

                NextChar();
            }
            return true;
        }

        private bool IsCodeExit()
        {
            // Do we have any brace still opened? If yes, let ReadCode handle them
            if (_openBraceCount > 0)
                return false;

            // Do we have a ~}} or ~}%}
            int start = 0;
            if (c == _stripWhiteSpaceFullSpecialChar || (!_isLiquid && c == _stripWhiteSpaceRestrictedSpecialChar))
                start = 1;

            // Check for either }} or ( %} if liquid active)
            if (PeekChar(start) != (_isLiquidTagBlock? '%' : '}'))
                return false;

            start++;
            if (!_isLiquid)
            {
                for (int i = 0; i < _escapeRawCharCount; i++)
                {
                    if (PeekChar(i + start) != RawEscapeSpecialChar)
                        return false;
                }
            }

            return PeekChar(_escapeRawCharCount + start) == '}';
        }

        private void ReadCodeExitOrEscape()
        {
            TextPosition start = _position;

            TokenType whitespaceMode = TokenType.Invalid;
            if (c == _stripWhiteSpaceFullSpecialChar)
            {
                whitespaceMode = TokenType.WhitespaceFull;
                NextChar();
            }
            else if (!_isLiquid && c == _stripWhiteSpaceRestrictedSpecialChar)
            {
                whitespaceMode = TokenType.Whitespace;
                NextChar();
            }

            NextChar();  // skip } or %
            if (!_isLiquid)
            {
                for (int i = 0; i < _escapeRawCharCount; i++)
                    NextChar(); // skip !
            }

            TextPosition end = _position;
            NextChar(); // skip }

            if (_escapeRawCharCount > 0)
            {
                // We limit the escape count to 9 levels (only for roundtrip mode)
                _pendingTokens.Enqueue(new Token((TokenType)(TokenType.EscapeCount1 + Math.Min(_escapeRawCharCount - 1, 8)), start, end));
                _escapeRawCharCount = 0;
            }
            else
            {
                _token = new Token(_isLiquidTagBlock ? TokenType.LiquidTagExit : TokenType.CodeExit, start, end);
            }

            // Eat spaces after an exit
            if (whitespaceMode != TokenType.Invalid)
            {
                TextPosition startSpace = _position;
                TextPosition endSpace = new TextPosition();
                if (ConsumeWhitespace(whitespaceMode == TokenType.Whitespace, ref endSpace, whitespaceMode == TokenType.Whitespace))
                    _pendingTokens.Enqueue(new Token(whitespaceMode, startSpace, endSpace));
            }

            _isLiquidTagBlock = false;
            _blockType = BlockType.Raw;
        }

        private bool ReadRaw()
        {
            TextPosition start = _position;
            TextPosition end = new TextPosition(-1, 0, 0);
            bool nextCodeEnterOrEscapeExit = false;
            TokenType whitespaceMode = TokenType.Invalid;

            bool isEmptyRaw = false;

            TextPosition beforeSpaceFull = TextPosition.Eof;
            TextPosition beforeSpaceRestricted = TextPosition.Eof;
            TextPosition lastSpaceFull = TextPosition.Eof;
            TextPosition lastSpaceRestricted = TextPosition.Eof;
            while (c != '\0')
            {
                if (_blockType == BlockType.Raw && IsCodeEnterOrEscape(out whitespaceMode) || _blockType == BlockType.Escape && IsCodeExit())
                {
                    isEmptyRaw = end.Offset < 0;
                    nextCodeEnterOrEscapeExit = true;
                    break;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (lastSpaceFull.Offset < 0)
                    {
                        lastSpaceFull = _position;
                        beforeSpaceFull = end;
                    }

                    if (!(c == '\n' || (c == '\r' && PeekChar() != '\n')))
                    {
                        if (lastSpaceRestricted.Offset < 0)
                        {
                            lastSpaceRestricted = _position;
                            beforeSpaceRestricted = end;
                        }
                    }
                    else
                    {
                        lastSpaceRestricted.Offset = -1;
                        beforeSpaceRestricted.Offset = -1;
                    }
                }
                else
                {
                    // Reset white space if any
                    lastSpaceFull.Offset = -1;
                    beforeSpaceFull.Offset = -1;
                    lastSpaceRestricted.Offset = -1;
                    beforeSpaceRestricted.Offset = -1;
                }

                end = _position;
                NextChar();
            }

            if (end.Offset < 0)
            {
                end = start;
            }

            TextPosition lastSpace = lastSpaceFull;
            TextPosition beforeSpace = beforeSpaceFull;
            if (whitespaceMode == TokenType.Whitespace)
            {
                lastSpace = lastSpaceRestricted;
                beforeSpace = beforeSpaceRestricted;
            }

            if (whitespaceMode != TokenType.Invalid && lastSpace.Offset >= 0)
            {
                _pendingTokens.Enqueue(new Token(whitespaceMode, lastSpace, end));

                if (beforeSpace.Offset < 0)
                    return false;

                end = beforeSpace;
            }

#if DEBUG
            Debug.Assert(_blockType == BlockType.Raw || _blockType == BlockType.Escape);
#endif
            if (nextCodeEnterOrEscapeExit)
            {
                if (isEmptyRaw)
                    end = new TextPosition(start.Offset - 1, start.Line, start.Column - 1);
            }

            _token = new Token(_blockType == BlockType.Escape ? TokenType.Escape : TokenType.Raw, start, end);

            // Go to eof
            if (!nextCodeEnterOrEscapeExit)
                NextChar();

            return true;
        }

        private bool ReadCode()
        {
            bool hasTokens = true;
            TextPosition start = _position;
            switch (c)
            {
                case '\n':
                    _token = new Token(TokenType.NewLine, start, _position);
                    NextChar();
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false, ref _token.End);
                    break;
                case ';':
                    _token = new Token(TokenType.SemiColon, start, _position);
                    NextChar();
                    break;
                case '\r':
                    NextChar();
                    // case of: \r\n
                    if (c == '\n')
                    {
                        _token = new Token(TokenType.NewLine, start, _position);
                        NextChar();
                        // consume all remaining space including new lines
                        ConsumeWhitespace(false, ref _token.End);
                        break;
                    }
                    // case of \r
                    _token = new Token(TokenType.NewLine, start, start);
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false, ref _token.End);
                    break;
                case ':':
                    _token = new Token(TokenType.Colon, start, start);
                    NextChar();
                    break;
                case '@':
                    _token = new Token(TokenType.Arroba, start, start);
                    NextChar();
                    break;
                case '^':
                    _token = new Token(TokenType.Caret, start, start);
                    NextChar();
                    break;
                case '*':
                    _token = new Token(TokenType.Multiply, start, start);
                    NextChar();
                    break;
                case '/':
                    NextChar();
                    if (c == '/')
                    {
                        _token = new Token(TokenType.DoubleDivide, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Divide, start, start);
                    break;
                case '+':
                    _token = new Token(TokenType.Plus, start, start);
                    NextChar();
                    break;
                case '-':
                    _token = new Token(TokenType.Minus, start, start);
                    NextChar();
                    break;
                case '%':
                    _token = new Token(TokenType.Modulus, start, start);
                    NextChar();
                    break;
                case ',':
                    _token = new Token(TokenType.Comma, start, start);
                    NextChar();
                    break;
                case '&':
                    NextChar();
                    if (c == '&')
                    {
                        _token = new Token(TokenType.And, start, _position);
                        NextChar();
                        break;
                    }

                    // & is an invalid char alone
                    _token = new Token(TokenType.Invalid, start, start);
                    break;
                case '?':
                    NextChar();
                    if (c == '?')
                    {
                        _token = new Token(TokenType.EmptyCoalescing, start, _position);
                        NextChar();
                        break;
                    }

                    _token = new Token(TokenType.Question, start, start);
                    break;
                case '|':
                    NextChar();
                    if (c == '|')
                    {
                        _token = new Token(TokenType.Or, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Pipe, start, start);
                    break;
                case '.':
                    NextChar();
                    if (c == '.')
                    {
                        TextPosition index = _position;
                        NextChar();
                        if (c == '<')
                        {
                            _token = new Token(TokenType.DoubleDotLess, start, _position);
                            NextChar();
                            break;
                        }

                        _token = new Token(TokenType.DoubleDot, start, index);
                        break;
                    }
                    _token = new Token(TokenType.Dot, start, start);
                    break;

                case '!':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareNotEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Not, start, start);
                    break;

                case '=':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Equal, start, start);
                    break;
                case '<':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareLessOrEqual, start, _position);
                        NextChar();
                        break;
                    }
                    if (c == '<')
                    {
                        _token = new Token(TokenType.ShiftLeft, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.CompareLess, start, start);
                    break;
                case '>':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareGreaterOrEqual, start, _position);
                        NextChar();
                        break;
                    }
                    if (c == '>')
                    {
                        _token = new Token(TokenType.ShiftRight, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.CompareGreater, start, start);
                    break;
                case '(':
                    _token = new Token(TokenType.OpenParent, _position, _position);
                    NextChar();
                    break;
                case ')':
                    _token = new Token(TokenType.CloseParent, _position, _position);
                    NextChar();
                    break;
                case '[':
                    _token = new Token(TokenType.OpenBracket, _position, _position);
                    NextChar();
                    break;
                case ']':
                    _token = new Token(TokenType.CloseBracket, _position, _position);
                    NextChar();
                    break;
                case '{':
                    // We count brace open to match then correctly later and avoid confusing with code exit
                    _openBraceCount++;
                    _token = new Token(TokenType.OpenBrace, _position, _position);
                    NextChar();
                    break;
                case '}':
                    if (_openBraceCount > 0)
                    {
                        // We match first brace open/close
                        _openBraceCount--;
                        _token = new Token(TokenType.CloseBrace, _position, _position);
                        NextChar();
                    }
                    else
                    {
                        if (Options.Mode != ScriptMode.ScriptOnly && IsCodeExit())
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
                        else
                        {
                            // Else we have a close brace but it is invalid
                            _token = new Token(TokenType.CloseBrace, _position, _position);
                            AddError(RS.CurlyBraceMismatch, _position, _position);
                            NextChar();
                        }
                    }
                    break;
                case '#':
                    ReadComment();
                    break;
                case '"':
                case '\'':
                    ReadString();
                    break;
                case '`':
                    ReadVerbatimString();
                    break;
                case '\0':
                    _token = Token.Eof;
                    break;
                default:
                    // Eat any whitespace
                    TextPosition lastSpace = new TextPosition();
                    if (ConsumeWhitespace(true, ref lastSpace))
                    {
                        if (Options.KeepTrivia)
                        {
                            _token = new Token(TokenType.Whitespace, start, lastSpace);
                        }
                        else
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
                        break;
                    }

                    bool specialIdentifier = c == '$';
                    if (IsFirstIdentifierLetter(c) || specialIdentifier)
                    {
                        ReadIdentifier(specialIdentifier);
                        break;
                    }

                    if (char.IsDigit(c))
                    {
                        ReadNumber();
                        break;
                    }

                    // invalid char
                    _token = new Token(TokenType.Invalid, _position, _position);
                    NextChar();
                    break;
            }

            return hasTokens;
        }

        private bool ReadCodeLiquid()
        {
            bool hasTokens = true;
            TextPosition start = _position;
            switch (c)
            {
                case ':':
                    _token = new Token(TokenType.Colon, start, start);
                    NextChar();
                    break;
                case ',':
                    _token = new Token(TokenType.Comma, start, start);
                    NextChar();
                    break;
                case '|':
                    _token = new Token(TokenType.Pipe, start, start);
                    NextChar();
                    break;
                case '?':
                    NextChar();
                    _token = new Token(TokenType.Question, start, start);
                    break;
                case '.':
                    NextChar();
                    if (c == '.')
                    {
                        _token = new Token(TokenType.DoubleDot, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Dot, start, start);
                    break;

                case '!':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareNotEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Invalid, start, start);
                    break;

                case '=':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Equal, start, start);
                    break;
                case '<':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareLessOrEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.CompareLess, start, start);
                    break;
                case '>':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.CompareGreaterOrEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.CompareGreater, start, start);
                    break;
                case '(':
                    _token = new Token(TokenType.OpenParent, _position, _position);
                    NextChar();
                    break;
                case ')':
                    _token = new Token(TokenType.CloseParent, _position, _position);
                    NextChar();
                    break;
                case '[':
                    _token = new Token(TokenType.OpenBracket, _position, _position);
                    NextChar();
                    break;
                case ']':
                    _token = new Token(TokenType.CloseBracket, _position, _position);
                    NextChar();
                    break;
                case '"':
                case '\'':
                    ReadString();
                    break;
                case '\0':
                    _token = Token.Eof;
                    break;
                default:
                    // Eat any whitespace
                    TextPosition lastSpace = new TextPosition();
                    if (ConsumeWhitespace(true, ref lastSpace))
                    {
                        if (Options.KeepTrivia)
                        {
                            _token = new Token(TokenType.Whitespace, start, lastSpace);
                        }
                        else
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
                        break;
                    }

                    if (IsFirstIdentifierLetter(c))
                    {
                        ReadIdentifier(false);
                        break;
                    }

                    if (char.IsDigit(c))
                    {
                        ReadNumber();
                        break;
                    }

                    // invalid char
                    _token = new Token(TokenType.Invalid, _position, _position);
                    NextChar();
                    break;
            }

            return hasTokens;
        }

        private bool ConsumeWhitespace(bool stopAtNewLine, ref TextPosition lastSpace, bool keepNewLine = false)
        {
            TextPosition start = _position;
            while (char.IsWhiteSpace(c))
            {
                if (stopAtNewLine && IsNewLine(c))
                {
                    if (keepNewLine)
                    {
                        lastSpace = _position;
                        NextChar();
                    }
                    break;
                }
                lastSpace = _position;
                NextChar();
            }

            return start != _position;
        }

        private static bool IsNewLine(char c)
        {
            return c == '\n';
        }

        private void ReadIdentifier(bool special)
        {
            TextPosition start = _position;

            TextPosition beforePosition;
            bool first = true;
            do
            {
                beforePosition = _position;
                NextChar();

                // Special $$ variable allowed only here
                if (first && special && c == '$')
                {
                    _token = new Token(TokenType.IdentifierSpecial, start, _position);
                    NextChar();
                    return;
                }

                first = false;
            } while (IsIdentifierLetter(c));

            _token = new Token(special ? TokenType.IdentifierSpecial : TokenType.Identifier, start, beforePosition);

            // If we have an include token, we are going to parse spaces and non_white_spaces
            // in order to support the tag "include"
            if (_isLiquid && Options.EnableIncludeImplicitString && _token.Match("include", Text) && char.IsWhiteSpace(c))
            {
                TextPosition startSpace = _position;
                TextPosition endSpace = startSpace;
                ConsumeWhitespace(false, ref startSpace);
                _pendingTokens.Enqueue(new Token(TokenType.Whitespace, startSpace, endSpace));

                TextPosition startPath = _position;
                TextPosition endPath = startPath;
                while (!char.IsWhiteSpace(c) && c != 0 && c != '%' && PeekChar() != '}')
                {
                    endPath = _position;
                    NextChar();
                }
                _pendingTokens.Enqueue(new Token(TokenType.ImplicitString, startPath, endPath));
            }
        }

        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        private static bool IsFirstIdentifierLetter(char c)
        {
            return c == '_' || char.IsLetter(c);
        }

        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        private bool IsIdentifierLetter(char c)
        {
            return IsFirstIdentifierLetter(c) || char.IsDigit(c) || (_isLiquid && c ==  '-');
        }

        private void ReadNumber()
        {
            TextPosition start = _position;
            TextPosition end = _position;
            bool hasDot = false;

            // Read first part
            do
            {
                end = _position;
                NextChar();
            } while (char.IsDigit(c));


            // Read any number following
            if (c == '.')
            {
                // If the next char is a '.' it means that we have a range iterator, so we don't touch it
                if (PeekChar() != '.')
                {
                    hasDot = true;
                    end = _position;
                    NextChar();
                    while (char.IsDigit(c))
                    {
                        end = _position;
                        NextChar();
                    }
                }
            }

            if (c == 'e' || c == 'E')
            {
                end = _position;
                NextChar();
                if (c == '+' || c == '-')
                {
                    end = _position;
                    NextChar();
                }

                if (!char.IsDigit(c))
                {
                    AddError(RS.NoDigitAfterExponent, _position, _position);
                    return;
                }

                while (char.IsDigit(c))
                {
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(hasDot ? TokenType.Float : TokenType.Integer, start, end);
        }

        private void ReadString()
        {
            TextPosition start = _position;
            TextPosition end = _position;
            char startChar = c;
            NextChar(); // Skip "

            char[] badCharCode = new char[5];
            int badCharCodeLength = 1;

            while (true)
            {
                if (c == SpecialChar.EscapeChar)
                {
                    end = _position;
                    NextChar();
                    // 0 ' " b f n r t v u0000-uFFFF x00-xFF
                    // also ^ which is the escape char
                    switch (c)
                    {
                        case SpecialChar.NewLine:
                            end = _position;
                            NextChar();
                            continue;
                        case SpecialChar.CarriageReturn:
                            end = _position;
                            NextChar();
                            if (c == SpecialChar.NewLine)
                            {
                                end = _position;
                                NextChar();
                            }
                            continue;
                        case '0':
                        case SpecialChar.SingleQuote:
                        case SpecialChar.DoubleQuote:
                        case 'b':
                        case 'f':
                        case 'n':
                        case 'r':
                        case 't':
                        case 'v':
                        case SpecialChar.EscapeChar:
                            end = _position;
                            NextChar();
                            continue;
                        case 'u':
                            end = _position;
                            badCharCode[0] = 'u';
                            NextChar();
                            badCharCode[1] = c;
                            // Must be followed 4 hex numbers (0000-FFFF)
                            if (c.IsHex()) // 1
                            {
                                end = _position;
                                NextChar();
                                badCharCode[2] = c;
                                if (c.IsHex()) // 2
                                {
                                    end = _position;
                                    NextChar();
                                    badCharCode[3] = c;
                                    if (c.IsHex()) // 3
                                    {
                                        end = _position;
                                        NextChar();
                                        badCharCode[4] = c;
                                        if (c.IsHex()) // 4
                                        {
                                            end = _position;
                                            NextChar();
                                            continue;
                                        }
                                        else
                                        {
                                            badCharCodeLength = 5;
                                        }
                                    }
                                    else
                                    {
                                        badCharCodeLength = 4;
                                    }
                                }
                                else
                                {
                                    badCharCodeLength = 3;
                                }
                            }
                            else
                            {
                                badCharCodeLength = 2;
                            }
                            break;
                        case 'x':
                            end = _position;
                            badCharCode[0] = 'x';
                            NextChar();
                            badCharCode[1] = c;
                            // Must be followed 2 hex numbers (00-FF)
                            if (c.IsHex())
                            {
                                end = _position;
                                NextChar();
                                badCharCode[2] = c;
                                if (c.IsHex())
                                {
                                    end = _position;
                                    NextChar();
                                    continue;
                                }
                                else
                                {
                                    badCharCodeLength = 3;
                                }
                            }
                            else
                            {
                                badCharCodeLength = 2;
                            }
                            break;
                    }

                    AddError(string.Format(RS.InvalidEscapeChar, new string(badCharCode, 0 , badCharCodeLength)), _position, _position);
                }
                else if (c == '\0')
                {
                    AddError(string.Format(RS.UnexpectedEofInString, startChar), end, end);
                    return;
                }
                else if (c == startChar)
                {
                    end = _position;
                    NextChar();
                    break;
                }
                else
                {
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(TokenType.String, start, end);
        }

        private void ReadVerbatimString()
        {
            TextPosition start = _position;
            TextPosition end = _position;
            char startChar = c;
            NextChar(); // Skip `
            while (true)
            {
                if (c == '\0')
                {
                    AddError(string.Format(RS.UnexpectedEofInVerbatiumString, startChar), end, end);
                    return;
                }
                else if (c == startChar)
                {
                    end = _position;
                    NextChar(); // Do we have an escape?
                    if (c != startChar)
                        break;

                    end = _position;
                    NextChar();
                }
                else
                {
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(TokenType.VerbatimString, start, end);
        }

        private void ReadComment()
        {
            TextPosition start = _position;
            TextPosition end = _position;

            NextChar();

            // Is Multiline?
            bool isMulti = false;
            if (c == '#')
            {
                isMulti = true;

                end = _position;
                NextChar();

                while (!IsCodeExit())
                {
                    if (c == '\0')
                        break;

                    bool mayBeEndOfComment = c == '#';

                    end = _position;
                    NextChar();

                    if (mayBeEndOfComment && c == '#')
                    {
                        end = _position;
                        NextChar();
                        break;
                    }
                }

            }
            else
            {
                while (!IsCodeExit())
                {
                    if (c == '\0' || c == '\r' || c == '\n')
                        break;

                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(isMulti ? TokenType.CommentMulti : TokenType.Comment, start, end);
        }

        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        private char PeekChar(int count = 1)
        {
            int offset = _position.Offset + count;

            return offset >= 0 && offset < _textLength ? Text[offset] : '\0';
        }

        [MethodImpl(MethodImplOptionsHelper.AggressiveInlining)]
        private void NextChar()
        {
            _position.Offset++;
            if (_position.Offset < _textLength)
            {
                char nc = Text[_position.Offset];
                if (c == '\n' || (c == '\r' && nc != '\n'))
                {
                    _position.Column = 0;
                    _position.Line += 1;
                }
                else
                {
                    _position.Column++;
                }
                c = nc;
            }
            else
            {
                _position.Offset = _textLength;
                c = '\0';
            }
        }

        IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void AddError(string message, TextPosition start, TextPosition end)
        {
            _token = new Token(TokenType.Invalid, start, end);
            if (_errors == null)
                _errors = new List<LogMessage>();

            _errors.Add(new LogMessage(ParserMessageType.Error, new SourceSpan(SourcePath, start, end), message));
        }


        private void Reset()
        {
            c = Text.Length > 0 ? Text[Options.StartPosition.Offset] : '\0';
            _position = Options.StartPosition;
            _errors = null;
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t';
        }

        /// <summary>
        /// Custom enumerator on <see cref="Token"/>
        /// </summary>
        public struct TokenEnumerator : IEnumerator<Token>
        {
            private readonly Lexer lexer;

            public TokenEnumerator(Lexer lexer)
            {
                this.lexer = lexer;
                lexer.Reset();
            }

            public bool MoveNext()
            {
                return lexer.MoveNext();
            }

            public void Reset()
            {
                lexer.Reset();
            }

            public Token Current => lexer._token;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
