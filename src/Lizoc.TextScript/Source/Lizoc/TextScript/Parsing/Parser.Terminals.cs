using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Parsing
{
    public partial class Parser
    {
        private ScriptExpression ParseVariableOrLiteral()
        {
            ScriptExpression literal = null;
            switch (Current.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                    literal = ParseVariable();
                    break;
                case TokenType.Integer:
                    literal = ParseInteger();
                    break;
                case TokenType.Float:
                    literal = ParseFloat();
                    break;
                case TokenType.String:
                    literal = ParseString();
                    break;
                case TokenType.ImplicitString:
                    literal = ParseImplicitString();
                    break;
                case TokenType.VerbatimString:
                    literal = ParseVerbatimString();
                    break;
                default:
                    LogError(Current, string.Format(RS.UnexpectedTokenInLiteral, GetAsText(Current)));
                    break;
            }
            return literal;
        }

        private ScriptLiteral ParseFloat()
        {
            ScriptLiteral literal = Open<ScriptLiteral>();

            string text = GetAsText(Current);
            double floatResult;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out floatResult))
                literal.Value = floatResult;
            else
                LogError(string.Format(RS.ParseLiteralFailed, text, "double"));

            NextToken(); // Skip the float
            return Close(literal);
        }

        private ScriptLiteral ParseImplicitString()
        {
            ScriptLiteral literal = Open<ScriptLiteral>();
            literal.Value = GetAsText(Current);
            Close(literal);
            NextToken();
            return literal;
        }

        private ScriptLiteral ParseInteger()
        {
            ScriptLiteral literal = Open<ScriptLiteral>();

            string text = GetAsText(Current);
            long result;
            if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                LogError(string.Format(RS.ParseLiteralFailed, text, "integer"));

            if (result >= int.MinValue && result <= int.MaxValue)
                literal.Value = (int) result;
            else
                literal.Value = result;

            NextToken(); // Skip the literal
            return Close(literal);
        }

        private ScriptLiteral ParseString()
        {
            ScriptLiteral literal = Open<ScriptLiteral>();
            string text = _lexer.Text;
            StringBuilder builder = new StringBuilder(Current.End.Offset - Current.Start.Offset - 1);

            literal.StringQuoteType = _lexer.Text[Current.Start.Offset] == '\''
                ? ScriptLiteralStringQuoteType.SimpleQuote
                : ScriptLiteralStringQuoteType.DoubleQuote;

            int end = Current.End.Offset;
            for (int i = Current.Start.Offset + 1; i < end; i++)
            {
                char c = text[i];
                // Handle escape characters
                if (text[i] == SpecialChar.EscapeChar)
                {
                    i++;
                    switch (text[i])
                    {
                        case '0':
                            builder.Append(SpecialChar.Null);
                            break;
                        case SpecialChar.NewLine:
                            // line break right after an escape char - just ignore
                            break;
                        case SpecialChar.CarriageReturn:
                            // line break right after an escape char - just ignore
                            i++; // skip next \n that was validated by the lexer
                            break;
                        case SpecialChar.SingleQuote:
                            builder.Append(SpecialChar.SingleQuote);
                            break;
                        case SpecialChar.DoubleQuote:
                            builder.Append(SpecialChar.DoubleQuote);
                            break;
                        case SpecialChar.EscapeChar:
                            builder.Append(SpecialChar.EscapeChar);
                            break;
                        case 'b':
                            builder.Append(SpecialChar.Backspace);
                            break;
                        case 'f':
                            builder.Append(SpecialChar.FormFeed);
                            break;
                        case 'n':
                            builder.Append(SpecialChar.NewLine);
                            break;
                        case 'r':
                            builder.Append(SpecialChar.CarriageReturn);
                            break;
                        case 't':
                            builder.Append(SpecialChar.Tab);
                            break;
                        case 'v':
                            builder.Append(SpecialChar.VerticalTab);
                            break;
                        case 'u':
                        {
                            // unicode value - u???? - where ???? in 0000 to ffff
                            i++;
                            int value = (text[i++].HexToInt() << 12) +
                                (text[i++].HexToInt() << 8) +
                                (text[i++].HexToInt() << 4) +
                                text[i].HexToInt();

                            // Is it correct?
                            builder.Append(ConvertFromUtf32(value));
                            break;
                        }
                        case 'x':
                        {
                            // hex value - x?? - where ?? in 00 to ff

                            i++;
                            int value = (text[i++].HexToInt() << 4) +
                                text[i++].HexToInt();

                            builder.Append((char)value);
                            break;
                        }
                        default:
                        {
                            // This should not happen as the lexer is supposed to prevent this
                            LogError(string.Format(RS.InvalidEscapeChar, text[i]));
                            break;
                        }
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }
            literal.Value = builder.ToString();

            NextToken();
            return Close(literal);
        }

        private ScriptExpression ParseVariable()
        {
            Token currentToken = Current;
            SourceSpan currentSpan = CurrentSpan;
            SourceSpan endSpan = currentSpan;
            string text = GetAsText(currentToken);

            // Return ScriptLiteral for null, true, false
            // Return ScriptAnonymousFunction 
            switch (text)
            {
                case "null":
                    ScriptLiteral nullValue = Open<ScriptLiteral>();
                    NextToken();
                    return Close(nullValue);
                case "true":
                    ScriptLiteral trueValue = Open<ScriptLiteral>();
                    trueValue.Value = true;
                    NextToken();
                    return Close(trueValue);
                case "false":
                    ScriptLiteral falseValue = Open<ScriptLiteral>();
                    falseValue.Value = false;
                    NextToken();
                    return Close(falseValue);
                case "do":
                    ScriptAnonymousFunction functionExp = Open<ScriptAnonymousFunction>();
                    functionExp.Function = ParseFunctionStatement(true);
                    ScriptAnonymousFunction func = Close(functionExp);
                    return func;
                case "this":
                    if (!_isLiquid)
                    {
                        ScriptThisExpression thisExp = Open<ScriptThisExpression>();
                        NextToken();
                        return Close(thisExp);
                    }
                    break;
            }

            // Keeps trivia before this token
            List<ScriptTrivia> triviasBefore = null;
            if (_isKeepTrivia && _trivias.Count > 0)
            {
                triviasBefore = new List<ScriptTrivia>();
                triviasBefore.AddRange(_trivias);
                _trivias.Clear();
            }

            NextToken();
            ScriptVariableScope scope = ScriptVariableScope.Global;
            if (text.StartsWith("$"))
            {
                scope = ScriptVariableScope.Local;
                text = text.Substring(1);

                // Convert $0, $1... $n variable into $[0] $[1]...$[n] variables
                int index;
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    ScriptIndexerExpression indexerExpression = new ScriptIndexerExpression
                    {
                        Span = currentSpan,

                        Target = new ScriptVariableLocal(ScriptVariable.Arguments.Name)
                        {
                            Span = currentSpan
                        },

                        Index = new ScriptLiteral() {Span = currentSpan, Value = index}
                    };

                    if (_isKeepTrivia)
                    {
                        if (triviasBefore != null)
                            indexerExpression.Target.AddTrivias(triviasBefore, true);

                        FlushTrivias(indexerExpression.Index, false);
                    }

                    return indexerExpression;
                }
            }
            else if (text == "for" || text == "while" || text == "tablerow" || (_isLiquid && (text == "forloop" || text == "tablerowloop")))
            {
                if (Current.Type == TokenType.Dot)
                {
                    NextToken();
                    if (Current.Type == TokenType.Identifier)
                    {
                        endSpan = CurrentSpan;
                        string loopVariableText = GetAsText(Current);
                        NextToken();

                        scope = ScriptVariableScope.Loop;
                        if (_isLiquid)
                        {
                            switch (loopVariableText)
                            {
                                case "first":
                                    text = ScriptVariable.LoopFirst.Name;
                                    break;
                                case "last":
                                    text = ScriptVariable.LoopLast.Name;
                                    break;
                                case "index0":
                                    text = ScriptVariable.LoopIndex.Name;
                                    break;
                                case "rindex0":
                                    text = ScriptVariable.LoopRIndex.Name;
                                    break;
                                case "rindex":
                                case "index":
                                    // Because forloop.index is 1 based index, we need to create a binary expression
                                    // to support it here
                                    bool isrindex = loopVariableText == "rindex";

                                    ScriptNestedExpression nested = new ScriptNestedExpression()
                                    {
                                        Expression = new ScriptBinaryExpression()
                                        {
                                            Operator = ScriptBinaryOperator.Add,
                                            Left = new ScriptVariableLoop(isrindex ? ScriptVariable.LoopRIndex.Name : ScriptVariable.LoopIndex.Name)
                                            {
                                                Span = currentSpan
                                            },
                                            Right = new ScriptLiteral(1)
                                            {
                                                Span = currentSpan
                                            },
                                            Span = currentSpan
                                        },
                                        Span = currentSpan
                                    };

                                    if (_isKeepTrivia)
                                    {
                                        if (triviasBefore != null)
                                            nested.AddTrivias(triviasBefore, true);

                                        FlushTrivias(nested, false);
                                    }
                                    return nested;
                                case "length":
                                    text = ScriptVariable.LoopLength.Name;
                                    break;
                                case "col":
                                    if (text != "tablerowloop")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, string.Format(RS.InvalidLoopVariable, text + ".col"));
                                    }
                                    text = ScriptVariable.TableRowCol.Name;
                                    break;

                                default:
                                    text = text + "." + loopVariableText;
                                    LogError(currentToken, string.Format(RS.InvalidLiquidLoopVariable, text));
                                    break;
                            }
                        }
                        else
                        {
                            switch (loopVariableText)
                            {
                                case "first":
                                    text = ScriptVariable.LoopFirst.Name;
                                    break;
                                case "last":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, string.Format(RS.InvalidLoopVariable, "while.last"));
                                    }
                                    text = ScriptVariable.LoopLast.Name;
                                    break;
                                case "changed":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, string.Format(RS.InvalidLoopVariable, "while.changed"));
                                    }
                                    text = ScriptVariable.LoopChanged.Name;
                                    break;
                                case "length":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, string.Format(RS.InvalidLoopVariable, "while.length"));
                                    }
                                    text = ScriptVariable.LoopLength.Name;
                                    break;
                                case "even":
                                    text = ScriptVariable.LoopEven.Name;
                                    break;
                                case "odd":
                                    text = ScriptVariable.LoopOdd.Name;
                                    break;
                                case "index":
                                    text = ScriptVariable.LoopIndex.Name;
                                    break;
                                case "rindex":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, string.Format(RS.InvalidLoopVariable, "while.rindex"));
                                    }
                                    text = ScriptVariable.LoopRIndex.Name;
                                    break;
                                case "col":
                                    if (text != "tablerow")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, string.Format(RS.InvalidLoopVariable, text + ".col"));
                                    }
                                    text = ScriptVariable.TableRowCol.Name;
                                    break;
                                default:
                                    text = text + "." + loopVariableText;
                                    // unit test: 108-variable-loop-error1.txt
                                    LogError(currentToken, string.Format(RS.InvalidLoopVariable, text));
                                    break;
                            }
                        }

                        // We no longer checks at parse time usage of loop variables, as they can be used in a wrap context
                        //if (!IsInLoop())
                        //{
                        //    LogError(currentToken, string.Format(RS.InvalidLoopVariableOutsideLoop, text));
                        //}
                    }
                    else
                    {
                        LogError(currentToken, string.Format(RS.LoopVariableDotRequireIdentifier, Current.Type, text));
                    }
                }
            }
            else if (_isLiquid && text == "continue")
            {
                scope = ScriptVariableScope.Local;
            }

            ScriptVariable result = ScriptVariable.Create(text, scope);
            result.Span = new SourceSpan
            {
                FileName = currentSpan.FileName,
                Start = currentSpan.Start,
                End = endSpan.End
            };

            // A liquid variable can have `-` in its identifier
            // If this is the case, we need to translate it to `this["this"]` instead
            if (_isLiquid && text.IndexOf('-') >= 0)
            {
                ScriptIndexerExpression newExp = new ScriptIndexerExpression
                {
                    Target = new ScriptThisExpression()
                    {
                        Span = result.Span
                    },
                    Index = new ScriptLiteral(text)
                    {
                        Span = result.Span
                    },
                    Span = result.Span
                };

                // Flush any trivias after
                if (_isKeepTrivia)
                {
                    if (triviasBefore != null)
                        newExp.Target.AddTrivias(triviasBefore, true);

                    FlushTrivias(newExp, false);
                }
                // Return the expression
                return newExp;
            }

            if (_isKeepTrivia)
            {
                // Flush any trivias after
                if (triviasBefore != null)
                    result.AddTrivias(triviasBefore, true);


                FlushTrivias(result, false);
            }
            return result;
        }

        private ScriptLiteral ParseVerbatimString()
        {
            ScriptLiteral literal = Open<ScriptLiteral>();
            string text = _lexer.Text;

            literal.StringQuoteType = ScriptLiteralStringQuoteType.Verbatim;

            StringBuilder builder = null;

            // startOffset start at the first character (`a` in the string `abc`)
            int startOffset = Current.Start.Offset + 1;
            // endOffset is at the last character (`c` in the string `abc`)
            int endOffset = Current.End.Offset - 1;

            int offset = startOffset;
            while (true)
            {
                // Go to the next escape (the lexer verified that there was a following `)
                int nextOffset = text.IndexOf("`", offset, endOffset - offset + 1, StringComparison.OrdinalIgnoreCase);
                if (nextOffset < 0)
                    break;

                if (builder == null)
                    builder = new StringBuilder(endOffset - startOffset + 1);


                builder.Append(text.Substring(offset, nextOffset - offset + 1));

                // Skip the escape ``
                offset = nextOffset + 2;
            }
            if (builder != null)
            {
                int count = endOffset - offset + 1;
                if (count > 0)
                    builder.Append(text.Substring(offset, count));

                literal.Value = builder.ToString();
            }
            else
            {
                literal.Value = text.Substring(offset, endOffset - offset + 1);
            }

            NextToken();
            return Close(literal);
        }

        private static string ConvertFromUtf32(int utf32)
        {
            if (utf32 < 65536)
                return ((char)utf32).ToString();

            utf32 -= 65536;
            return new string(new char[2]
            {
                (char) (utf32 / 1024 + 55296),
                (char) (utf32 % 1024 + 56320)
            });
        }

        private static bool IsVariableOrLiteral(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                case TokenType.Integer:
                case TokenType.Float:
                case TokenType.String:
                case TokenType.ImplicitString:
                case TokenType.VerbatimString:
                    return true;
            }
            return false;
        }
    }
}