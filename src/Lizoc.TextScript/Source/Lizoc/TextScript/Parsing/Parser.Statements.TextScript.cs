// -----------------------------------------------------------------------
// <copyright file="Parser.Statements.TextScript.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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
        private void ParseTextScriptStatement(string identifier, ScriptStatement parent, ref ScriptStatement statement, ref bool hasEnd, ref bool nextStatement)
        {
            Token startToken = Current;
            switch (identifier)
            {
                case "end":
                    hasEnd = true;
                    nextStatement = false;

                    if (_isKeepTrivia)
                        _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.End, _lexer.Text));

                    NextToken();

                    ScriptStatement matchingStatement = FindFirstStatementExpectingEnd();
                    ExpectEndOfStatement(matchingStatement);
                    if (_isKeepTrivia)
                        FlushTrivias(matchingStatement, false);

                    break;
                case "wrap":
                    CheckNotInCase(parent, startToken);
                    statement = ParseWrapStatement();
                    break;
                case "if":
                    CheckNotInCase(parent, startToken);
                    statement = ParseIfStatement(false, false);
                    break;
                case "case":
                    CheckNotInCase(parent, startToken);
                    statement = ParseCaseStatement();
                    break;
                case "when":
                    ScriptWhenStatement whenStatement = ParseWhenStatement();
                    var whenParent = parent as ScriptConditionStatement;
                    if (parent is ScriptWhenStatement)
                    {
                        ((ScriptWhenStatement)whenParent).Next = whenStatement;
                    }
                    else if (parent is ScriptCaseStatement)
                    {
                        statement = whenStatement;
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: TODO
                        LogError(startToken, RS.WhenConditionSyntaxError);
                    }
                    hasEnd = true;
                    break;
                case "else":
                    ScriptConditionStatement nextCondition = ParseElseStatement(false);
                    var parentCondition = parent as ScriptConditionStatement;
                    if (parent is ScriptIfStatement || parent is ScriptWhenStatement)
                    {
                        if (parent is ScriptIfStatement)
                            ((ScriptIfStatement)parentCondition).Else = nextCondition;
                        else
                            ((ScriptWhenStatement)parentCondition).Next = nextCondition;
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: 201-if-else-error3.txt
                        LogError(startToken, RS.ElseConditionSyntaxError);
                    }
                    hasEnd = true;
                    break;
                case "for":
                    CheckNotInCase(parent, startToken);
                    if (PeekToken().Type == TokenType.Dot)
                        statement = ParseExpressionStatement();
                    else
                        statement = ParseForStatement<ScriptForStatement>();
                    break;
                case "tablerow":
                    CheckNotInCase(parent, startToken);
                    if (PeekToken().Type == TokenType.Dot)
                        statement = ParseExpressionStatement();
                    else
                        statement = ParseForStatement<ScriptTableRowStatement>();
                    break;
                case "with":
                    CheckNotInCase(parent, startToken);
                    statement = ParseWithStatement();
                    break;
                case "import":
                    CheckNotInCase(parent, startToken);
                    statement = ParseImportStatement();
                    break;
                case "readonly":
                    CheckNotInCase(parent, startToken);
                    statement = ParseReadOnlyStatement();
                    break;
                case "while":
                    CheckNotInCase(parent, startToken);
                    if (PeekToken().Type == TokenType.Dot)
                        statement = ParseExpressionStatement();
                    else
                        statement = ParseWhileStatement();
                    break;
                case "break":
                    CheckNotInCase(parent, startToken);
                    statement = Open<ScriptBreakStatement>();
                    NextToken();
                    ExpectEndOfStatement(statement);
                    Close(statement);

                    // This has to be done at execution time, because of the wrap statement
                    //if (!IsInLoop())
                    //{
                    //    LogError(statement, RS.StatementOutsideLoopError);
                    //}
                    break;
                case "continue":
                    CheckNotInCase(parent, startToken);
                    statement = Open<ScriptContinueStatement>();
                    NextToken();
                    ExpectEndOfStatement(statement);
                    Close(statement);

                    // This has to be done at execution time, because of the wrap statement
                    //if (!IsInLoop())
                    //{
                    //    LogError(statement, RS.StatementOutsideLoopError);
                    //}
                    break;
                case "func":
                    CheckNotInCase(parent, startToken);
                    statement = ParseFunctionStatement(false);
                    break;
                case "ret":
                    CheckNotInCase(parent, startToken);
                    statement = ParseReturnStatement();
                    break;
                case "capture":
                    CheckNotInCase(parent, startToken);
                    statement = ParseCaptureStatement();
                    break;
                default:
                    CheckNotInCase(parent, startToken);
                    // Otherwise it is an expression statement
                    statement = ParseExpressionStatement();
                    break;
            }
        }

        private ScriptFunction ParseFunctionStatement(bool isAnonymous)
        {
            ScriptFunction scriptFunction = Open<ScriptFunction>();
            NextToken(); // skip func or do

            if (!isAnonymous)
                scriptFunction.Name = ExpectAndParseVariable(scriptFunction);

            ExpectEndOfStatement(scriptFunction);

            scriptFunction.Body = ParseBlockStatement(scriptFunction);
            return Close(scriptFunction);
        }

        private ScriptImportStatement ParseImportStatement()
        {
            ScriptImportStatement importStatement = Open<ScriptImportStatement>();
            NextToken(); // skip import

            importStatement.Expression = ExpectAndParseExpression(importStatement);
            ExpectEndOfStatement(importStatement);

            return Close(importStatement);
        }

        private ScriptReadOnlyStatement ParseReadOnlyStatement()
        {
            ScriptReadOnlyStatement readOnlyStatement = Open<ScriptReadOnlyStatement>();
            NextToken(); // Skip readonly keyword

            readOnlyStatement.Variable = ExpectAndParseVariable(readOnlyStatement);
            ExpectEndOfStatement(readOnlyStatement);

            return Close(readOnlyStatement);
        }

        private ScriptReturnStatement ParseReturnStatement()
        {
            ScriptReturnStatement ret = Open<ScriptReturnStatement>();
            NextToken(); // skip ret

            if (StartAsExpression())
                ret.Expression = ParseExpression(ret);

            ExpectEndOfStatement(ret);

            return Close(ret);
        }

        private ScriptWhileStatement ParseWhileStatement()
        {
            ScriptWhileStatement whileStatement = Open<ScriptWhileStatement>();
            NextToken(); // Skip while

            // Parse the condition
            // unit test: 220-while-error1.txt
            whileStatement.Condition = ExpectAndParseExpression(whileStatement);

            if (ExpectEndOfStatement(whileStatement))
            {
                FlushTrivias(whileStatement.Condition, false);
                whileStatement.Body = ParseBlockStatement(whileStatement);
            }

            return Close(whileStatement);
        }

        private ScriptWithStatement ParseWithStatement()
        {
            ScriptWithStatement withStatement = Open<ScriptWithStatement>();
            NextToken();
            withStatement.Name = ExpectAndParseExpression(withStatement);

            if (ExpectEndOfStatement(withStatement))
                withStatement.Body = ParseBlockStatement(withStatement);

            return Close(withStatement);
        }

        private ScriptWrapStatement ParseWrapStatement()
        {
            ScriptWrapStatement wrapStatement = Open<ScriptWrapStatement>();
            NextToken(); // skip wrap

            wrapStatement.Target = ExpectAndParseExpression(wrapStatement);

            if (ExpectEndOfStatement(wrapStatement))
            {
                FlushTrivias(wrapStatement.Target, false);
                wrapStatement.Body = ParseBlockStatement(wrapStatement);
            }

            return Close(wrapStatement);
        }

        private void FixRawStatementAfterFrontMatter(ScriptPage page)
        {
            // In case of parsing a front matter, we don't want to include any \r\n after the end of the front-matter
            // So we manipulate back the syntax tree for the expected raw statement (if any), otherwise we can early 
            // exit.
            var rawStatement = page.Body.Statements.FirstOrDefault() as ScriptRawStatement;
            if (rawStatement == null)
                return;

            int startOffset = rawStatement.Span.Start.Offset;
            int endOffset = rawStatement.Span.End.Offset;
            for (int i = startOffset; i <= endOffset; i++)
            {
                char c = rawStatement.Text[i];
                if (c == ' ' || c == '\t')
                    continue;

                if (c == '\r')
                {
                    if (i + 1 <= endOffset && rawStatement.Text[i + 1] == '\n')
                        rawStatement.Span.Start = new TextPosition(i + 2, rawStatement.Span.Start.Line + 1, 0);

                    break;
                }

                if (c == '\n')
                    rawStatement.Span.Start = new TextPosition(i + 1, rawStatement.Span.Start.Line + 1, 0);

                break;
            }
        }

        private static bool IsTextScriptKeyword(string text)
        {
            switch (text)
            {
                case "if":
                case "else":
                case "end":
                case "for":
                case "case":
                case "when":
                case "while":
                case "break":
                case "continue":
                case "func":
                case "import":
                case "readonly":
                case "with":
                case "capture":
                case "ret":
                case "wrap":
                case "do":
                    return true;
            }
            return false;
        }
    }
}