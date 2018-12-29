﻿// -----------------------------------------------------------------------
// <copyright file="Parser.Statements.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Parsing
{
    public partial class Parser
    {
        private ScriptBlockStatement ParseBlockStatement(ScriptStatement parentStatement)
        {
#if DEBUG
            Debug.Assert(!(parentStatement is ScriptBlockStatement));
#endif
            Blocks.Push(parentStatement);

            _blockLevel++;
            EnterExpression();

            ScriptBlockStatement blockStatement = Open<ScriptBlockStatement>();

            ScriptStatement statement;
            bool hasEnd;
            while (TryParseStatement(parentStatement, out statement, out hasEnd))
            {
                // statement may be null if we have parsed an else continuation of a previous block
                if (statement != null)
                    blockStatement.Statements.Add(statement);

                if (hasEnd)
                    break;
            }

            if (!hasEnd)
            {
                // If there are any end block not matching, we have an error
                if (_blockLevel > 1)
                {
                    if (_isLiquid)
                    {
                        ScriptSyntaxAttribute syntax = ScriptSyntaxAttribute.Get(parentStatement);
                        LogError(parentStatement, parentStatement?.Span ?? CurrentSpan, string.Format(RS.EndLiquidStatementNotFound, syntax.Name));
                    }
                    else
                    {
                        // unit test: 201-if-else-error2.txt
                        LogError(parentStatement, GetSpanForToken(Previous), RS.EndStatementNotFound);
                    }
                }
            }

            LeaveExpression();
            _blockLevel--;

            Blocks.Pop();
            return Close(blockStatement);
        }

        private bool TryParseStatement(ScriptStatement parent, out ScriptStatement statement, out bool hasEnd)
        {
            hasEnd = false;
            bool nextStatement = true;
            statement = null;

            continueParsing:

            if (_hasFatalError)
                return false;

            if (_pendingStatements.Count > 0)
            {
                statement = _pendingStatements.Dequeue();
                return true;
            }

            switch (Current.Type)
            {
                case TokenType.Eof:
                    // Early exit
                    nextStatement = false;
                    break;

                case TokenType.Raw:
                case TokenType.Escape:
                    statement = ParseRawStatement();
                    if (parent is ScriptCaseStatement)
                    {
                        // In case we have a raw statement within directly a case
                        // we don't keep it
                        statement = null;
                        goto continueParsing;
                    }
                    break;

                case TokenType.CodeEnter:
                case TokenType.LiquidTagEnter:
                    if (_inCodeSection)
                        LogError(RS.UnexpectedTokenInCodeBlock);

                    _isLiquidTagSection = Current.Type == TokenType.LiquidTagEnter;
                    _inCodeSection = true;

                    // If we have any pending trivias before processing this code enter and we want to keep trivia
                    // we need to generate a RawStatement to store these trivias
                    if (_isKeepTrivia && (_trivias.Count > 0 || Previous.Type == TokenType.CodeEnter))
                    {
                        ScriptRawStatement rawStatement = Open<ScriptRawStatement>();
                        Close(rawStatement);
                        if (_trivias.Count > 0)
                        {
                            rawStatement.Trivias.After.AddRange(rawStatement.Trivias.Before);
                            rawStatement.Trivias.Before.Clear();
                            SourceSpan firstTriviaSpan = rawStatement.Trivias.After[0].Span;
                            SourceSpan lastTriviaSpan = rawStatement.Trivias.After[rawStatement.Trivias.After.Count - 1].Span;
                            rawStatement.Span = new SourceSpan(firstTriviaSpan.FileName, firstTriviaSpan.Start, lastTriviaSpan.End);
                        }
                        else
                        {
                            // Else Add an empty trivia
                            rawStatement.AddTrivia(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Empty, null), false);
                        }
                        statement = rawStatement;
                    }

                    NextToken();

                    if (Current.Type == TokenType.CodeExit)
                    {
                        ScriptNopStatement nopStatement = Open<ScriptNopStatement>();
                        Close(nopStatement);
                        if (statement == null)
                            statement = nopStatement;
                        else
                            _pendingStatements.Enqueue(nopStatement);
                    }

                    // If we have a ScriptRawStatement previously defined, we need to break out of the loop to record it
                    if (statement == null)
                        goto continueParsing;

                    break;
                case TokenType.FrontMatterMarker:
                    if (_inFrontMatter)
                    {
                        _inFrontMatter = false;
                        _inCodeSection = false;
                        // When we expect to parse only the front matter, don't try to tokenize the following text
                        // Keep the current token as the code exit of the front matter
                        if (CurrentParsingMode != ScriptMode.FrontMatterOnly)
                            NextToken();

                        if (CurrentParsingMode == ScriptMode.FrontMatterAndContent || CurrentParsingMode == ScriptMode.FrontMatterOnly)
                        {
                            // Once the FrontMatter has been parsed, we can switch to default parsing mode.

                            CurrentParsingMode = ScriptMode.Default;
                            nextStatement = false;
                        }
                    }
                    else
                    {
                        LogError(string.Format(RS.UnexpectedFrontMatterMarker, _lexer.Options.FrontMatterMarker));
                        NextToken();
                    }
                    break;

                case TokenType.CodeExit:
                case TokenType.LiquidTagExit:
                    if (!_inCodeSection)
                        LogError(RS.OrphanCodeBlockExitError);
                    else if (CurrentParsingMode == ScriptMode.ScriptOnly)
                        LogError(RS.CodeBlockNotAllowedInScriptMode);

                    _isLiquidTagSection = false;
                    _inCodeSection = false;

                    // We clear any trivia that might not have been takend by a node
                    // so that they don't end up after this token
                    if (_isKeepTrivia)
                        _trivias.Clear();

                    NextToken();

                    // If next token is directly a code enter or an eof but we want to keep trivia
                    // and with have trivias
                    // we need to generate a RawStatement to store these trivias
                    if (_isKeepTrivia && (Current.Type == TokenType.CodeEnter || Current.Type == TokenType.Eof))
                    {
                        ScriptRawStatement rawStatement = Open<ScriptRawStatement>();
                        Close(rawStatement);
                        if (_trivias.Count > 0)
                        {
                            SourceSpan firstTriviaSpan = rawStatement.Trivias.Before[0].Span;
                            SourceSpan lastTriviaSpan = rawStatement.Trivias.Before[rawStatement.Trivias.Before.Count - 1].Span;
                            rawStatement.Span = new SourceSpan(firstTriviaSpan.FileName, firstTriviaSpan.Start, lastTriviaSpan.End);
                        }
                        else
                        {
                            // Else Add an empty trivia
                            rawStatement.AddTrivia(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Empty, null), false);
                        }
                        statement = rawStatement;
                    }
                    else
                    {
                        goto continueParsing;
                    }
                    break;

                default:
                    if (_inCodeSection)
                    {
                        switch (Current.Type)
                        {
                            case TokenType.NewLine:
                            case TokenType.SemiColon:
                                PushTokenToTrivia();
                                NextToken();
                                goto continueParsing;
                            case TokenType.Identifier:
                            case TokenType.IdentifierSpecial:
                                string identifier = GetAsText(Current);
                                if (_isLiquid)
                                    ParseLiquidStatement(identifier, parent, ref statement, ref hasEnd, ref nextStatement);
                                else
                                    ParseTextScriptStatement(identifier, parent, ref statement, ref hasEnd, ref nextStatement);
                                break;
                            default:
                                if (StartAsExpression())
                                {
                                    statement = ParseExpressionStatement();
                                }
                                else
                                {
                                    nextStatement = false;
                                    LogError(string.Format(RS.UnexpectedToken, Current.Type));
                                }
                                break;
                        }
                    }
                    else
                    {
                        nextStatement = false;
                        LogError(string.Format(RS.UnexpectedTokenOutsideCodeBlock, Current.Type));
                        // LOG an ERROR. Don't expect any other tokens outside a code section
                    }
                    break;
            }

            return nextStatement;
        }

        private ScriptCaptureStatement ParseCaptureStatement()
        {
            ScriptCaptureStatement captureStatement = Open<ScriptCaptureStatement>();
            NextToken(); // Skip capture keyword

            // unit test: 231-capture-error1.txt
            captureStatement.Target = ExpectAndParseExpression(captureStatement);
            ExpectEndOfStatement(captureStatement);
            captureStatement.Body = ParseBlockStatement(captureStatement);

            return Close(captureStatement);
        }

        private ScriptCaseStatement ParseCaseStatement()
        {
            ScriptCaseStatement caseStatement = Open<ScriptCaseStatement>();
            NextToken(); // skip case

            caseStatement.Value = ExpectAndParseExpression(caseStatement);

            if (ExpectEndOfStatement(caseStatement))
            {
                FlushTrivias(caseStatement.Value, false);
                caseStatement.Body = ParseBlockStatement(caseStatement);
            }

            return Close(caseStatement);
        }

        private ScriptConditionStatement ParseElseStatement(bool isElseIf)
        {
            // Case of elsif
            if (isElseIf)
                return ParseIfStatement(false, true);

            // unit test: 200-if-else-statement.txt
            /*
            Token nextToken = PeekToken();
            if (!_isLiquid && 
                nextToken.Type == TokenType.Identifier && 
                GetAsText(nextToken) == "if")
            {
                NextToken();

                if (_isKeepTrivia)
                {
                    // We don't store the trivias here
                    _trivias.Clear();
                }
                return ParseIfStatement(false, true);
            }
            */

            ScriptElseStatement elseStatement = Open<ScriptElseStatement>();
            NextToken(); // skip else

            // unit test: 201-if-else-error4.txt
            if (ExpectEndOfStatement(elseStatement))
                elseStatement.Body = ParseBlockStatement(elseStatement);

            return Close(elseStatement);
        }

        private ScriptExpressionStatement ParseExpressionStatement()
        {
            ScriptExpressionStatement expressionStatement = Open<ScriptExpressionStatement>();
            bool hasAnonymous;
            expressionStatement.Expression = TransformKeyword(ExpectAndParseExpressionAndAnonymous(expressionStatement, out hasAnonymous));

            // In case of an anonymous, there was already an ExpectEndOfStatement issued for the function
            // so we don't have to verify this here again
            if (!hasAnonymous)
                ExpectEndOfStatement(expressionStatement);

            return Close(expressionStatement);
        }

        private T ParseForStatement<T>() where T : ScriptForStatement, new()
        {
            T forStatement = Open<T>();
            NextToken(); // skip for

            // unit test: 211-for-error1.txt
            forStatement.Variable = ExpectAndParseExpression(forStatement, mode: ParseExpressionMode.BasicExpression);

            if (forStatement.Variable != null)
            {
                if (!(forStatement.Variable is IScriptVariablePath))
                    LogError(forStatement, string.Format(RS.ExpectVariable, forStatement.Variable));

                // in 
                if (Current.Type != TokenType.Identifier || GetAsText(Current) != "in")
                {
                    // unit test: 211-for-error2.txt
                    LogError(forStatement, string.Format(RS.ForLoopExpectInKeyword, Current.Type, GetAsText(Current)));
                }
                else
                {
                    NextToken(); // skip in
                }

                // unit test: 211-for-error3.txt
                forStatement.Iterator = ExpectAndParseExpression(forStatement);

                if (ExpectEndOfStatement(forStatement))
                {
                    FlushTrivias(forStatement.IteratorOrLastParameter, false);
                    forStatement.Body = ParseBlockStatement(forStatement);
                }
            }

            return Close(forStatement);
        }

        private ScriptIfStatement ParseIfStatement(bool invert, bool isElseIf)
        {
            // unit test: 200-if-else-statement.txt
            ScriptIfStatement condition = Open<ScriptIfStatement>();
            condition.IsElseIf = isElseIf;
            condition.InvertCondition = invert;
            NextToken(); // skip if

            condition.Condition = ExpectAndParseExpression(condition);

            if (ExpectEndOfStatement(condition))
            {
                FlushTrivias(condition.Condition, false);
                condition.Then = ParseBlockStatement(condition);
            }

            return Close(condition);
        }

        private ScriptRawStatement ParseRawStatement()
        {
            ScriptRawStatement scriptStatement = Open<ScriptRawStatement>();

            // We keep span End here to update it with the raw span
            TextPosition spanEnd = Current.End;

            // If we have an escape, we can fetch the escape count
            if (Current.Type == TokenType.Escape)
            {
                NextToken(); // Skip escape
                if (Current.Type < TokenType.EscapeCount1 && Current.Type > TokenType.EscapeCount9)
                    LogError(Current, string.Format(RS.ExpectEscapeCount, GetAsText(Current)));
                else
                    scriptStatement.EscapeCount = (Current.Type - TokenType.EscapeCount1) + 1;
            }

            scriptStatement.Text = _lexer.Text;
            NextToken(); // Skip raw or escape count
            Close(scriptStatement);

            // Because the previous will update the ScriptStatement with the wrong Span End for escape (escapecount1+)
            // We make sure that we use the span end of the Raw token
            scriptStatement.Span.End = spanEnd;
            return scriptStatement;
        }

        private ScriptWhenStatement ParseWhenStatement()
        {
            ScriptWhenStatement whenStatement = Open<ScriptWhenStatement>();
            NextToken(); // skip when

            // Parse the when values
            // - a, b, c
            // - a || b || c (textscript)
            // - a or b or c (liquid)
            while (true)
            {
                if (!IsVariableOrLiteral(Current))
                    break;

                ScriptExpression variableOrLiteral = ParseVariableOrLiteral();
                whenStatement.Values.Add(variableOrLiteral);

                if (Current.Type == TokenType.Comma || (!_isLiquid && Current.Type == TokenType.Or) || (_isLiquid && GetAsText(Current) == "or"))
                    NextToken();
            }

            if (whenStatement.Values.Count == 0)
                LogError(Current, RS.WhenExpectAtLeastOneValue);

            if (ExpectEndOfStatement(whenStatement))
            {
                if (_isKeepTrivia && whenStatement.Values.Count > 0)
                    FlushTrivias(whenStatement.Values[whenStatement.Values.Count - 1], false);

                whenStatement.Body = ParseBlockStatement(whenStatement);
            }

            return Close(whenStatement);
        }

        private void CheckNotInCase(ScriptStatement parent, Token token)
        {
            if (parent is ScriptCaseStatement)
            {
                // 205-case-when-statement-error1.txt
                LogError(token, string.Format(RS.UnexpectedTokenInCase, GetAsText(token)));
            }
        }

        private ScriptVariable ExpectAndParseVariable(ScriptNode parentNode)
        {
            if (parentNode == null)
                throw new ArgumentNullException(nameof(parentNode));

            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.IdentifierSpecial)
            {
                ScriptExpression variableOrLiteral = ParseVariable();
                var variable = variableOrLiteral as ScriptVariable;
                if (variable != null && variable.Scope != ScriptVariableScope.Loop)
                    return (ScriptVariable)variableOrLiteral;

                LogError(parentNode, string.Format(RS.UnexpectedVariable, variableOrLiteral));
            }
            else
            {
                LogError(parentNode, string.Format(RS.ExpectVariable, Current.Type));
            }
            return null;
        }

        private bool ExpectEndOfStatement(ScriptStatement statement)
        {
            if (_isLiquid)
            {
                if (Current.Type == TokenType.CodeExit || (_isLiquidTagSection && Current.Type == TokenType.LiquidTagExit))
                    return true;
            }
            else if (Current.Type == TokenType.NewLine || Current.Type == TokenType.CodeExit || Current.Type == TokenType.SemiColon || Current.Type == TokenType.Eof)
            {
                if (Current.Type == TokenType.NewLine || Current.Type == TokenType.SemiColon)
                {
                    PushTokenToTrivia();
                    NextToken();
                }
                return true;
            }
            // If we are not finding an end of statement, log a fatal error
            LogError(statement, string.Format(RS.UnexpectedTokenAfterEnd, GetAsText(Current)), true);
            return false;
        }

        private static bool ExpectStatementEnd(ScriptNode scriptNode)
        {
            return (scriptNode is ScriptIfStatement && !((ScriptIfStatement)scriptNode).IsElseIf) || 
                scriptNode is ScriptForStatement ||
                scriptNode is ScriptCaptureStatement ||
                scriptNode is ScriptWithStatement ||
                scriptNode is ScriptWhileStatement ||
                scriptNode is ScriptWrapStatement ||
                scriptNode is ScriptCaseStatement ||
                scriptNode is ScriptFunction ||
                scriptNode is ScriptAnonymousFunction;
        }

        private ScriptStatement FindFirstStatementExpectingEnd()
        {
            foreach (ScriptNode scriptNode in Blocks)
            {
                if (ExpectStatementEnd(scriptNode))
                    return (ScriptStatement)scriptNode;
            }
            return null;
        }
    }
}