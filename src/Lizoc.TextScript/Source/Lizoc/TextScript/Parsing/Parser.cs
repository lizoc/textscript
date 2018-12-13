using System;
using System.Collections.Generic;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Parsing
{
    /// <summary>
    /// The parser.
    /// </summary>
    public partial class Parser
    {
        private readonly Lexer _lexer;
        private readonly bool _isLiquid;
        private Lexer.TokenEnumerator _tokenIt;
        private readonly List<Token> _tokensPreview;
        private int _tokensPreviewStart;
        private Token _previousToken;
        private Token _token;
        private bool _inCodeSection;
        private bool _isLiquidTagSection;
        private int _blockLevel;
        private bool _inFrontMatter;
        private bool _isExpressionDepthLimitReached;
        private int _expressionDepth;
        private bool _hasFatalError;
        private readonly bool _isKeepTrivia;
        private readonly List<ScriptTrivia> _trivias;
        private readonly Queue<ScriptStatement> _pendingStatements;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="lexer"/> parameter is null.</exception>
        public Parser(Lexer lexer, ParserOptions? options = null)
        {
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            _isLiquid = _lexer.Options.Mode == ScriptMode.Liquid;
            _tokensPreview = new List<Token>(4);
            Messages = new List<LogMessage>();
            _trivias = new List<ScriptTrivia>();

            Options = options ?? new ParserOptions();
            CurrentParsingMode = lexer.Options.Mode;

            _isKeepTrivia = lexer.Options.KeepTrivia;

            _pendingStatements = new Queue<ScriptStatement>(2);
            Blocks = new Stack<ScriptNode>();

            // Initialize the iterator
            _tokenIt = lexer.GetEnumerator();
            NextToken();
        }

        public readonly ParserOptions Options;

        public List<LogMessage> Messages { get; private set; }

        public bool HasErrors { get; private set; }

        private Stack<ScriptNode> Blocks { get; }

        private Token Current => _token;

        private Token Previous => _previousToken;

        public SourceSpan CurrentSpan => GetSpanForToken(Current);

        private ScriptMode CurrentParsingMode { get; set; }

        public ScriptPage Run()
        {
            Messages = new List<LogMessage>();
            HasErrors = false;
            _blockLevel = 0;
            _isExpressionDepthLimitReached = false;
            Blocks.Clear();

            ScriptPage page = Open<ScriptPage>();
            ScriptMode parsingMode = CurrentParsingMode;
            switch (parsingMode)
            {
                case ScriptMode.FrontMatterAndContent:
                case ScriptMode.FrontMatterOnly:
                    if (Current.Type != TokenType.FrontMatterMarker)
                    {
                        LogError(string.Format(RS.FrontMatterMarkerMissing, CurrentParsingMode, _lexer.Options.FrontMatterMarker, Current.GetText(_lexer.Text)));
                        return null;
                    }

                    _inFrontMatter = true;
                    _inCodeSection = true;

                    // Skip the frontmatter marker
                    NextToken();

                    // Parse the front matter
                    page.FrontMatter = ParseBlockStatement(null);

                    // We should not be in a frontmatter after parsing the statements
                    if (_inFrontMatter)
                        LogError(string.Format(RS.EndOfFrontMatterNotFound, _lexer.Options.FrontMatterMarker));

                    if (parsingMode == ScriptMode.FrontMatterOnly)
                        return page;

                    break;
                case ScriptMode.ScriptOnly:
                    _inCodeSection = true;
                    break;
            }

            page.Body = ParseBlockStatement(null);

            if (page.FrontMatter != null)
                FixRawStatementAfterFrontMatter(page);

            if (_lexer.HasErrors)
            {
                foreach (LogMessage lexerError in _lexer.Errors)
                {
                    Log(lexerError);
                }
            }

            return !HasErrors ? page : null;
        }

        private void PushTokenToTrivia()
        {
            if (_isKeepTrivia)
            {
                if (Current.Type == TokenType.NewLine)
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.NewLine, _lexer.Text));
                else if (Current.Type == TokenType.SemiColon)
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.SemiColon, _lexer.Text));
            }
        }

        private T Open<T>() where T : ScriptNode, new()
        {
            T element = new T()
            {
                Span =
                {
                    FileName = _lexer.SourcePath,
                    Start = Current.Start
                }
            };
            FlushTrivias(element, true);
            return element;
        }

        private void FlushTrivias(ScriptNode element, bool isBefore)
        {
            if (_isKeepTrivia && _trivias.Count > 0 && !(element is ScriptBlockStatement))
            {
                element.AddTrivias(_trivias, isBefore);
                _trivias.Clear();
            }
        }

        private T Close<T>(T statement) where T : ScriptNode
        {
            statement.Span.End = Previous.End;
            FlushTrivias(statement, false);
            return statement;
        }

        private string GetAsText(Token localToken)
        {
            return localToken.GetText(_lexer.Text);
        }

        private void NextToken()
        {
            _previousToken = _token;
            bool result;

            while (_tokensPreviewStart < _tokensPreview.Count)
            {
                _token = _tokensPreview[_tokensPreviewStart];
               _tokensPreviewStart++;

                // We can reset the tokens if we hit the upper limit of the preview
                if (_tokensPreviewStart == _tokensPreview.Count)
                {
                    _tokensPreviewStart = 0;
                    _tokensPreview.Clear();
                }

                if (IsHidden(_token.Type))
                {
                    if (_isKeepTrivia)
                        PushTrivia(_token);
                }
                else
                {
                    return;
                }
                
            }

            // Skip Comments
            while ((result = _tokenIt.MoveNext()))
            {
                if (IsHidden(_tokenIt.Current.Type))
                {
                    if (_isKeepTrivia)
                        PushTrivia(_tokenIt.Current);
                }
                else
                {
                    break;
                }
            }

            _token = result ? _tokenIt.Current : Token.Eof;
        }

        private void PushTrivia(Token token)
        {
            ScriptTriviaType type;
            switch (token.Type)
            {
                case TokenType.Comment:
                    type = ScriptTriviaType.Comment;
                    break;

                case TokenType.CommentMulti:
                    type = ScriptTriviaType.CommentMulti;
                    break;

                case TokenType.Whitespace:
                    type = ScriptTriviaType.Whitespace;
                    break;

                case TokenType.WhitespaceFull:
                    type = ScriptTriviaType.WhitespaceFull;
                    break;

                case TokenType.NewLine:
                    type = ScriptTriviaType.NewLine;
                    break;

                default:
                    throw new InvalidOperationException(string.Format(RS.InvalidTokenInTrivia, token.Type));
            }

            ScriptTrivia trivia = new ScriptTrivia(GetSpanForToken(token), type,  _lexer.Text);
            _trivias.Add(trivia);
        }

        private Token PeekToken()
        {
            // Do we have preview token available?
            for (int i = _tokensPreviewStart; i < _tokensPreview.Count; i++)
            {
                Token nextToken = _tokensPreview[i];
                if (!IsHidden(nextToken.Type))
                    return nextToken;
            }

            // Else try to find the first token not hidden
            while (_tokenIt.MoveNext())
            {
                Token nextToken = _tokenIt.Current;
                _tokensPreview.Add(nextToken);
                if (!IsHidden(nextToken.Type))
                    return nextToken;
            }

            return Token.Eof;
        }

        private bool IsHidden(TokenType tokenType)
        {
            return tokenType == TokenType.Comment ||
                tokenType == TokenType.CommentMulti ||
                tokenType == TokenType.Whitespace ||
                tokenType == TokenType.WhitespaceFull ||
                (tokenType == TokenType.NewLine && _allowNewLineLevel > 0);
        }

        private void LogError(string text, bool isFatal = false)
        {
            LogError(Current, text, isFatal);
        }

        private void LogError(Token tokenArg, string text, bool isFatal = false)
        {
            LogError(GetSpanForToken(tokenArg), text, isFatal);
        }

        private SourceSpan GetSpanForToken(Token tokenArg)
        {
            return new SourceSpan(_lexer.SourcePath, tokenArg.Start, tokenArg.End);
        }

        private void LogError(SourceSpan span, string text, bool isFatal = false)
        {
            Log(new LogMessage(ParserMessageType.Error, span, text), isFatal);
        }

        private void LogError(ScriptNode node, string message, bool isFatal = false)
        {
            LogError(node, node.Span, message, isFatal);
        }

        private void LogError(ScriptNode node, SourceSpan span, string message, bool isFatal = false)
        {
            ScriptSyntaxAttribute syntax = ScriptSyntaxAttribute.Get(node);
            // #todo
            string inSeparator = " >>> ";
            if (message.EndsWith("after"))
                inSeparator = string.Empty;

            LogError(span, string.Format(RS.ParseError, syntax.Name, message, inSeparator, syntax.Example), isFatal);
        }

        private void Log(LogMessage logMessage, bool isFatal = false)
        {
            if (logMessage == null)
                throw new ArgumentNullException(nameof(logMessage));

            Messages.Add(logMessage);
            if (logMessage.Type == ParserMessageType.Error)
            {
                HasErrors = true;
                if (isFatal)
                    _hasFatalError = true;
            }
        }
    }
}