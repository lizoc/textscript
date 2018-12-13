// -----------------------------------------------------------------------
// <copyright file="Parser.Expressions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Lizoc.TextScript.Functions;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Parsing
{
    public partial class Parser
    {
        private static readonly Dictionary<TokenType, ScriptBinaryOperator> BinaryOperators = new Dictionary<TokenType, ScriptBinaryOperator>();

        private int _allowNewLineLevel = 0;
        private int _expressionLevel = 0;

        static Parser()
        {
            BinaryOperators.Add(TokenType.Multiply, ScriptBinaryOperator.Multiply);
            BinaryOperators.Add(TokenType.Divide, ScriptBinaryOperator.Divide);
            BinaryOperators.Add(TokenType.DoubleDivide, ScriptBinaryOperator.DivideRound);
            BinaryOperators.Add(TokenType.Plus, ScriptBinaryOperator.Add);
            BinaryOperators.Add(TokenType.Minus, ScriptBinaryOperator.Substract);
            BinaryOperators.Add(TokenType.Modulus, ScriptBinaryOperator.Modulus);
            BinaryOperators.Add(TokenType.ShiftLeft, ScriptBinaryOperator.ShiftLeft);
            BinaryOperators.Add(TokenType.ShiftRight, ScriptBinaryOperator.ShiftRight);
            BinaryOperators.Add(TokenType.EmptyCoalescing, ScriptBinaryOperator.EmptyCoalescing);
            BinaryOperators.Add(TokenType.And, ScriptBinaryOperator.And);
            BinaryOperators.Add(TokenType.Or, ScriptBinaryOperator.Or);
            BinaryOperators.Add(TokenType.CompareEqual, ScriptBinaryOperator.CompareEqual);
            BinaryOperators.Add(TokenType.CompareNotEqual, ScriptBinaryOperator.CompareNotEqual);
            BinaryOperators.Add(TokenType.CompareGreater, ScriptBinaryOperator.CompareGreater);
            BinaryOperators.Add(TokenType.CompareGreaterOrEqual, ScriptBinaryOperator.CompareGreaterOrEqual);
            BinaryOperators.Add(TokenType.CompareLess, ScriptBinaryOperator.CompareLess);
            BinaryOperators.Add(TokenType.CompareLessOrEqual, ScriptBinaryOperator.CompareLessOrEqual);
            BinaryOperators.Add(TokenType.DoubleDot, ScriptBinaryOperator.RangeInclude);
            BinaryOperators.Add(TokenType.DoubleDotLess, ScriptBinaryOperator.RangeExclude);
        }

        private ScriptExpression ParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int precedence = 0, ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            bool hasAnonymousFunction = false;
            return ParseExpression(parentNode, ref hasAnonymousFunction, parentExpression, precedence, mode);
        }

        private ScriptExpression ParseExpression(ScriptNode parentNode, ref bool hasAnonymousFunction, ScriptExpression parentExpression = null, int precedence = 0,
            ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            int expressionCount = 0;
            _expressionLevel++;
            int expressionDepthBeforeEntering = _expressionDepth;
            EnterExpression();

            try
            {
                ScriptFunctionCall functionCall = null;
                parseExpression:
                expressionCount++;
                ScriptExpression leftOperand = null;
                switch (Current.Type)
                {
                    case TokenType.Identifier:
                    case TokenType.IdentifierSpecial:
                        leftOperand = ParseVariable();

                        // In case of liquid template, we accept the syntax colon after a tag
                        if (_isLiquid && parentNode is ScriptPipeCall && Current.Type == TokenType.Colon)
                            NextToken();

                        // Special handle of the $$ block delegate variable
                        if (ScriptVariable.BlockDelegate.Equals(leftOperand))
                        {
                            if (expressionCount != 1 || _expressionLevel > 1)
                                LogError(RS.DelegateBlockInNestedExpr);

                            if (!(parentNode is ScriptExpressionStatement))
                                LogError(parentNode, RS.DelegateBlockOutsideExpr);

                            return leftOperand;
                        }
                        break;
                    case TokenType.Integer:
                        leftOperand = ParseInteger();
                        break;
                    case TokenType.Float:
                        leftOperand = ParseFloat();
                        break;
                    case TokenType.String:
                        leftOperand = ParseString();
                        break;
                    case TokenType.ImplicitString:
                        leftOperand = ParseImplicitString();
                        break;
                    case TokenType.VerbatimString:
                        leftOperand = ParseVerbatimString();
                        break;
                    case TokenType.OpenParent:
                        leftOperand = ParseParenthesis(ref hasAnonymousFunction);
                        break;
                    case TokenType.OpenBrace:
                        leftOperand = ParseObjectInitializer();
                        break;
                    case TokenType.OpenBracket:
                        leftOperand = ParseArrayInitializer();
                        break;
                    case TokenType.Not:
                    case TokenType.Minus:
                    case TokenType.Arroba:
                    case TokenType.Plus:
                    case TokenType.Caret:
                        leftOperand = ParseUnaryExpression(ref hasAnonymousFunction);
                        break;
                }

                // Should not happen but in case
                if (leftOperand == null)
                {
                    if (functionCall != null)
                        LogError(string.Format(RS.UnexpectedTokenInFunc, GetAsText(Current), functionCall));
                    else
                        LogError(string.Format(RS.UnexpectedTokenInExpr, GetAsText(Current)));

                    return null;
                }

                if (leftOperand is ScriptAnonymousFunction)
                    hasAnonymousFunction = true;

                while (!hasAnonymousFunction)
                {
                    if (_isLiquid && Current.Type == TokenType.Comma && functionCall != null)
                        NextToken(); // Skip the comma for arguments in a function call

                    // Parse Member expression are expected to be followed only by an identifier
                    if (Current.Type == TokenType.Dot)
                    {
                        Token nextToken = PeekToken();
                        if (nextToken.Type == TokenType.Identifier)
                        {
                            NextToken();

                            if (GetAsText(Current) == "empty" && PeekToken().Type == TokenType.Question)
                            {
                                ScriptIsEmptyExpression memberExpression = Open<ScriptIsEmptyExpression>();
                                NextToken(); // skip empty
                                NextToken(); // skip ?
                                memberExpression.Target = leftOperand;
                                leftOperand = Close(memberExpression);
                            }
                            else
                            {
                                ScriptMemberExpression memberExpression = Open<ScriptMemberExpression>();
                                memberExpression.Target = leftOperand;
                                ScriptExpression member = ParseVariable();
                                if (!(member is ScriptVariable))
                                {
                                    LogError(string.Format(RS.UnexpectedLiteralMember, member));
                                    return null;
                                }
                                memberExpression.Member = (ScriptVariable)member;
                                leftOperand = Close(memberExpression);
                            }
                        }
                        else
                        {
                            LogError(nextToken, string.Format(RS.InvalidTokenAfterDot, nextToken.Type));
                            return null;
                        }
                        continue;
                    }

                    // If we have a bracket but left operand is a (variable || member || indexer), then we consider next as an indexer
                    // unit test: 130-indexer-accessor-accept1.txt
                    if (Current.Type == TokenType.OpenBracket && leftOperand is IScriptVariablePath && !IsPreviousCharWhitespace())
                    {
                        NextToken();
                        ScriptIndexerExpression indexerExpression = Open<ScriptIndexerExpression>();
                        indexerExpression.Target = leftOperand;
                        // unit test: 130-indexer-accessor-error5.txt
                        indexerExpression.Index = ExpectAndParseExpression(indexerExpression, ref hasAnonymousFunction, functionCall, 0, string.Format(RS.ExpectToken, "index_expression", Current.Type));

                        if (Current.Type != TokenType.CloseBracket)
                            LogError(string.Format(RS.ExpectToken, "]", Current.Type));
                        else
                            NextToken();

                        leftOperand = Close(indexerExpression);
                        continue;
                    }

                    if (mode == ParseExpressionMode.BasicExpression)
                    {
                        break;
                    }

                    if (Current.Type == TokenType.Equal)
                    {
                        ScriptAssignExpression assignExpression = Open<ScriptAssignExpression>();

                        if (_expressionLevel > 1)
                        {
                            // unit test: 101-assign-complex-error1.txt
                            LogError(assignExpression, RS.ExprForTopLevelAssignmentOnly);
                        }

                        NextToken();

                        assignExpression.Target = TransformKeyword(leftOperand);

                        // unit test: 105-assign-error3.txt
                        assignExpression.Value = ExpectAndParseExpression(assignExpression, ref hasAnonymousFunction, parentExpression);

                        leftOperand = Close(assignExpression);
                        continue;
                    }

                    // Handle binary operators here
                    ScriptBinaryOperator binaryOperatorType;
                    if (BinaryOperators.TryGetValue(Current.Type, out binaryOperatorType) || (_isLiquid && TryLiquidBinaryOperator(out binaryOperatorType)))
                    {
                        int newPrecedence = GetOperatorPrecedence(binaryOperatorType);

                        // Check precedence to see if we should "take" this operator here (Thanks TimJones for the tip code! ;)
                        if (newPrecedence < precedence)
                            break;

                        // We fake entering an expression here to limit the number of expression
                        EnterExpression();
                        ScriptBinaryExpression binaryExpression = Open<ScriptBinaryExpression>();
                        binaryExpression.Left = leftOperand;
                        binaryExpression.Operator = binaryOperatorType;

                        NextToken(); // skip the operator

                        // unit test: 110-binary-simple-error1.txt
                        binaryExpression.Right = ExpectAndParseExpression(binaryExpression, ref hasAnonymousFunction,
                            functionCall ?? parentExpression, newPrecedence,
                            string.Format(RS.ExpectTokenInRightOperator, "expression", Current.Type));
                        leftOperand = Close(binaryExpression);

                        continue;
                    }

                    if (precedence > 0)
                        break;

                    if (StartAsExpression())
                    {
                        // If we can parse a statement, we have a method call
                        if (parentExpression != null)
                            break;

                        // Parse named parameters
                        var paramContainer = parentNode as IScriptNamedArgumentContainer;
                        if (Current.Type == TokenType.Identifier && (parentNode is IScriptNamedArgumentContainer || !_isLiquid && PeekToken().Type == TokenType.Colon))
                        {
                            if (paramContainer == null)
                            {
                                if (functionCall == null)
                                {
                                    functionCall = Open<ScriptFunctionCall>();
                                    functionCall.Target = leftOperand;
                                    functionCall.Span.Start = leftOperand.Span.Start;
                                }
                                else
                                {
                                    functionCall.Arguments.Add(leftOperand);
                                }
                                Close(leftOperand);
                            }

                            while (true)
                            {
                                if (Current.Type != TokenType.Identifier)
                                    break;

                                ScriptNamedArgument parameter = Open<ScriptNamedArgument>();
                                string parameterName = GetAsText(Current);
                                parameter.Name = parameterName;

                                // Skip argument name
                                NextToken();

                                if (paramContainer != null)
                                    paramContainer.AddParameter(Close(parameter));
                                else
                                    functionCall.Arguments.Add(parameter);

                                // If we have a colon, we have a value
                                // otherwise it is a boolean argument name
                                if (Current.Type == TokenType.Colon)
                                {
                                    NextToken();
                                    parameter.Value = ExpectAndParseExpression(parentNode, mode: ParseExpressionMode.BasicExpression);
                                    parameter.Span.End = parameter.Value.Span.End;
                                }

                                if (functionCall != null)
                                    functionCall.Span.End = parameter.Span.End;
                            }

                            // As we have handled leftOperand here, we don't let the function out of this while to pick up the leftOperand
                            if (functionCall != null)
                            {
                                leftOperand = functionCall;
                                functionCall = null;
                            }

                            // We don't allow anything after named parameters
                            break;
                        }

                        if (functionCall == null)
                        {
                            functionCall = Open<ScriptFunctionCall>();
                            functionCall.Target = leftOperand;

                            // If we need to convert liquid to textscript functions:
                            if (_isLiquid && Options.ConvertLiquidFunctions)
                                TransformFromLiquidFunctionCall(functionCall);

                            functionCall.Span.Start = leftOperand.Span.Start;
                        }
                        else
                        {
                            functionCall.Arguments.Add(leftOperand);
                        }

                        goto parseExpression;
                    }

                    if (Current.Type == TokenType.Pipe)
                    {
                        if (functionCall != null)
                        {
                            functionCall.Arguments.Add(leftOperand);
                            leftOperand = functionCall;
                        }

                        ScriptPipeCall pipeCall = Open<ScriptPipeCall>();
                        pipeCall.From = leftOperand;
                        NextToken(); // skip |

                        // unit test: 310-func-pipe-error1.txt
                        pipeCall.To = ExpectAndParseExpression(pipeCall, ref hasAnonymousFunction);
                        return Close(pipeCall);
                    }

                    break;
                }

                if (functionCall != null)
                {
                    functionCall.Arguments.Add(leftOperand);
                    functionCall.Span.End = leftOperand.Span.End;
                    return functionCall;
                }
                return Close(leftOperand);
            }
            finally
            {
                LeaveExpression();
                // Force to restore back to a level
                _expressionDepth = expressionDepthBeforeEntering;
                _expressionLevel--;
            }
        }

        private ScriptExpression ParseArrayInitializer()
        {
            ScriptArrayInitializerExpression scriptArray = Open<ScriptArrayInitializerExpression>();

            // Should happen before the NextToken to consume any EOL after
            _allowNewLineLevel++;
            NextToken(); // Skip [

            bool expectingEndOfInitializer = false;

            // unit test: 120-array-initializer-accessor.txt
            while (true)
            {
                if (Current.Type == TokenType.CloseBracket)
                    break;

                if (!expectingEndOfInitializer)
                {
                    // unit test: 120-array-initializer-error2.txt
                    ScriptExpression expression = ExpectAndParseExpression(scriptArray);
                    if (expression == null)
                        break;

                    scriptArray.Values.Add(expression);

                    if (Current.Type == TokenType.Comma)
                    {
                        // Record trailing Commas
                        if (_isKeepTrivia)
                            expression.AddTrivia(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Comma, _lexer.Text), false);

                        NextToken();

                        if (_isKeepTrivia && _trivias.Count > 0)
                        {
                            expression.AddTrivias(_trivias, false);
                            _trivias.Clear();
                        }
                    }
                    else
                    {
                        expectingEndOfInitializer = true;
                    }
                }
                else
                {
                    // unit test: 120-array-initializer-error1.txt
                    LogError(string.Format(RS.ArrayCloseBracketMismatch, Current.Type));
                    break;
                }
            }

            // Should happen before NextToken() to stop on the next EOF
            _allowNewLineLevel--;
            NextToken(); // Skip ]

            return Close(scriptArray);
        }

        private ScriptExpression ParseObjectInitializer()
        {
            ScriptObjectInitializerExpression scriptObject = Open<ScriptObjectInitializerExpression>();

            // Should happen before the NextToken to consume any EOL after
            _allowNewLineLevel++;
            NextToken(); // Skip {

            // unit test: 140-object-initializer-accessor.txt
            bool expectingEndOfInitializer = false;
            while (true)
            {
                if (Current.Type == TokenType.CloseBrace)
                    break;

                if (!expectingEndOfInitializer && (Current.Type == TokenType.Identifier || Current.Type == TokenType.String))
                {
                    Token positionBefore = Current;
                    ScriptExpression variableOrLiteral = ParseExpression(scriptObject);
                    var variable = variableOrLiteral as ScriptVariable;
                    var literal = variableOrLiteral as ScriptLiteral;

                    if (variable == null && literal == null)
                    {
                        LogError(positionBefore, string.Format(RS.InvalidObjectInitMemberNameType, variableOrLiteral, ScriptSyntaxAttribute.Get(variableOrLiteral).Name));
                        break;
                    }

                    if (literal != null && !(literal.Value is string))
                    {
                        LogError(positionBefore, string.Format(RS.InvalidObjectInitMemberName, literal.Value, literal.Value?.GetType()));
                        break;
                    }

                    if (variable != null)
                    {
                        if (variable.Scope != ScriptVariableScope.Global)
                        {
                            // unit test: 140-object-initializer-error3.txt
                            LogError(RS.InvalidMemberName);
                            break;
                        }
                    }

                    if (Current.Type != TokenType.Colon)
                    {
                        // unit test: 140-object-initializer-error4.txt
                        LogError(string.Format(RS.UnexpectedTokenInObjectMemberName, Current.Type, variable.Name));
                        break;
                    }

                    // TODO: record as trivia
                    NextToken(); // Skip :

                    if (!StartAsExpression())
                    {
                        // unit test: 140-object-initializer-error5.txt
                        LogError(string.Format(RS.InvalidMemberValue, Current.Type, GetAsText(Current)));
                        break;
                    }

                    ScriptExpression expression = ParseExpression(scriptObject);

                    // Erase any previous declaration of this member
                    scriptObject.Members[variableOrLiteral] = expression;

                    if (Current.Type == TokenType.Comma)
                    {
                        // Record trailing Commas
                        if (_isKeepTrivia)
                            expression.AddTrivia(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Comma, _lexer.Text), false);

                        NextToken();

                        if (_isKeepTrivia && _trivias.Count > 0)
                        {
                            expression.AddTrivias(_trivias, false);
                            _trivias.Clear();
                        }
                    }
                    else
                    {
                        expectingEndOfInitializer = true;
                    }
                }
                else
                {
                    // unit test: 140-object-initializer-error1.txt
                    LogError(string.Format(RS.UnexpectedTokenInObjectInit, Current.Type, GetAsText(Current)));
                    break;
                }
            }

            // Should happen before NextToken() to stop on the next EOF
            _allowNewLineLevel--;
            NextToken(); // Skip }

            return Close(scriptObject);
        }

        private ScriptExpression ParseParenthesis(ref bool hasAnonymousFunction)
        {
            // unit test: 106-parenthesis.txt
            ScriptNestedExpression expression = Open<ScriptNestedExpression>();
            NextToken(); // Skip (
            expression.Expression = ExpectAndParseExpression(expression, ref hasAnonymousFunction);

            if (Current.Type == TokenType.CloseParent)
            {
                NextToken();
            }
            else
            {
                // unit test: 106-parenthesis-error1.txt
                LogError(Current, string.Format(RS.SpanCloseBracketMismatch, expression.Span.Start, Current.Type));
            }
            return Close(expression);
        }

        private ScriptExpression ParseUnaryExpression(ref bool hasAnonymousFunction)
        {
            // unit test: 113-unary.txt
            ScriptUnaryExpression unaryExpression = Open<ScriptUnaryExpression>();
            switch (Current.Type)
            {
                case TokenType.Not:
                    unaryExpression.Operator = ScriptUnaryOperator.Not;
                    break;
                case TokenType.Minus:
                    unaryExpression.Operator = ScriptUnaryOperator.Negate;
                    break;
                case TokenType.Plus:
                    unaryExpression.Operator = ScriptUnaryOperator.Plus;
                    break;
                case TokenType.Arroba:
                    unaryExpression.Operator = ScriptUnaryOperator.FunctionAlias;
                    break;
                case TokenType.Caret:
                    unaryExpression.Operator = ScriptUnaryOperator.FunctionParametersExpand;
                    break;
                default:
                    LogError(string.Format(RS.UnexpectedTokenInUnaryExpr, Current.Type));
                    break;
            }
            int newPrecedence = GetOperatorPrecedence(unaryExpression.Operator);
            NextToken();
            // unit test: 115-unary-error1.txt
            unaryExpression.Right = ExpectAndParseExpression(unaryExpression, ref hasAnonymousFunction, null, newPrecedence);
            return Close(unaryExpression);
        }

        private ScriptExpression TransformKeyword(ScriptExpression leftOperand)
        {
            // In case we are in liquid and we are assigning to a textscript keyword, we escape the variable with a nested expression
            if (_isLiquid && leftOperand is IScriptVariablePath &&
                IsTextScriptKeyword(((IScriptVariablePath) leftOperand).GetFirstPath()) &&
                !(leftOperand is ScriptNestedExpression))
            {
                ScriptNestedExpression nestedExpression = new ScriptNestedExpression
                {
                    Expression = leftOperand,
                    Span = leftOperand.Span
                };

                // If the variable has any trivia, we copy them to the NestedExpression instead
                if (_isKeepTrivia && leftOperand.Trivias != null)
                {
                    nestedExpression.Trivias = leftOperand.Trivias;
                    leftOperand.Trivias = null;
                }

                return nestedExpression;
            }

            return leftOperand;
        }

        private void TransformFromLiquidFunctionCall(ScriptFunctionCall functionCall)
        {
            var liquidTarget = functionCall.Target as ScriptVariable;
            string targetName;
            string memberName;
            // In case of cycle we transform it to array.cycle at runtime
            if (liquidTarget != null && LiquidBuiltinsFunctions.TryImportLiquid(liquidTarget.Name, out targetName, out memberName))
            {
                ScriptMemberExpression arrayCycle = new ScriptMemberExpression
                {
                    Span = liquidTarget.Span,
                    Target = new ScriptVariableGlobal(targetName) {Span = liquidTarget.Span},
                    Member = new ScriptVariableGlobal(memberName) {Span = liquidTarget.Span},
                };

                // Transfer trivias accordingly to target (trivias before) and member (trivias after)
                if (_isKeepTrivia && liquidTarget.Trivias != null)
                {
                    arrayCycle.Target.AddTrivias(liquidTarget.Trivias.Before, true);
                    arrayCycle.Member.AddTrivias(liquidTarget.Trivias.After, false);
                }
                functionCall.Target = arrayCycle;
            }
        }

        private void EnterExpression()
        {
            _expressionDepth++;
            if (Options.ExpressionDepthLimit.HasValue && !_isExpressionDepthLimitReached && _expressionDepth > Options.ExpressionDepthLimit.Value)
            {
                LogError(GetSpanForToken(Previous), string.Format(RS.StatementDepthLimitReached, Options.ExpressionDepthLimit.Value));
                _isExpressionDepthLimitReached = true;
            }
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null,
            ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            if (StartAsExpression())
                return ParseExpression(parentNode, parentExpression, newPrecedence, mode);

            LogError(parentNode, CurrentSpan, message ?? string.Format(RS.ExpectToken, "expression", Current.Type));
            return null;
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ref bool hasAnonymousExpression, ScriptExpression parentExpression = null, int newPrecedence = 0,
            string message = null, ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            if (StartAsExpression())
                return ParseExpression(parentNode, ref hasAnonymousExpression, parentExpression, newPrecedence, mode);

            LogError(parentNode, CurrentSpan, message ?? string.Format(RS.ExpectToken, "expression", Current.Type));
            return null;
        }

        private ScriptExpression ExpectAndParseExpressionAndAnonymous(ScriptNode parentNode, out bool hasAnonymousFunction, ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            hasAnonymousFunction = false;
            if (StartAsExpression())
                return ParseExpression(parentNode, ref hasAnonymousFunction, null, 0, mode);

            LogError(parentNode, CurrentSpan, string.Format(RS.ExpectToken, "expression", Current.Type));
            return null;
        }

        private bool StartAsExpression()
        {
            switch (Current.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                case TokenType.Integer:
                case TokenType.Float:
                case TokenType.String:
                case TokenType.ImplicitString:
                case TokenType.VerbatimString:
                case TokenType.OpenParent:
                case TokenType.OpenBrace:
                case TokenType.OpenBracket:
                case TokenType.Not:
                case TokenType.Minus:
                case TokenType.Plus:
                case TokenType.Arroba:
                case TokenType.Caret:
                    return true;
            }

            return false;
        }

        private bool TryLiquidBinaryOperator(out ScriptBinaryOperator binOp)
        {
            binOp = 0;
            if (Current.Type != TokenType.Identifier)
                return false;

            string text = GetAsText(Current);
            switch (text)
            {
                case "or":
                    binOp = ScriptBinaryOperator.Or;
                    return true;
                case "and":
                    binOp = ScriptBinaryOperator.And;
                    return true;
                case "contains":
                    binOp = ScriptBinaryOperator.LiquidContains;
                    return true;
                case "startsWith":
                    binOp = ScriptBinaryOperator.LiquidStartsWith;
                    return true;
                case "endsWith":
                    binOp = ScriptBinaryOperator.LiquidEndsWith;
                    return true;
                case "hasKey":
                    binOp = ScriptBinaryOperator.LiquidHasKey;
                    return true;
                case "hasValue":
                    binOp = ScriptBinaryOperator.LiquidHasValue;
                    return true;
            }

            return false;
        }

        private static int GetOperatorPrecedence(ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.EmptyCoalescing:
                    return 20;
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                    return 25;
                case ScriptBinaryOperator.Or:
                    return 30;
                case ScriptBinaryOperator.And:
                    return 40;
                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                    return 50;
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return 60;

                case ScriptBinaryOperator.LiquidContains:
                case ScriptBinaryOperator.LiquidStartsWith:
                case ScriptBinaryOperator.LiquidEndsWith:
                case ScriptBinaryOperator.LiquidHasKey:
                case ScriptBinaryOperator.LiquidHasValue:
                    return 65;

                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Substract:
                    return 70;
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                    return 80;
                case ScriptBinaryOperator.RangeInclude:
                case ScriptBinaryOperator.RangeExclude:
                    return 90;
                default:
                    return 0;
            }
        }

        private static int GetOperatorPrecedence(ScriptUnaryOperator op)
        {
            switch (op)
            {
                case ScriptUnaryOperator.Not:
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                case ScriptUnaryOperator.FunctionAlias:
                case ScriptUnaryOperator.FunctionParametersExpand:
                    return 10;
                default:
                    return 0;
            }
        }

        bool IsPreviousCharWhitespace()
        {
            int position = Current.Start.Offset - 1;
            if (position >= 0)
                return char.IsWhiteSpace(_lexer.Text[position]);

            return false;
        }

        private void LeaveExpression()
        {
            _expressionDepth--;
        }

        private enum ParseExpressionMode
        {
            /// <summary>
            /// All expressions (e.g literals, function calls, function pipes...etc.)
            /// </summary>
            Default,

            /// <summary>
            /// Only literal, unary, nested, array/object initializer, dot access, array access
            /// </summary>
            BasicExpression,
        }
    }
}