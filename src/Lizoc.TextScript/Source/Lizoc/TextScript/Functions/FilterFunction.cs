// -----------------------------------------------------------------------
// <copyright file="FilterFunction.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// The `where` function is used to filter an array. All members that fulfils the condition specified are returned.
    /// </summary>
    public sealed class WhereFunction : IScriptCustomFunction
    {
        public WhereFunction()
        {
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            if (arguments == null || arguments.Count == 0)
                throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.BadFunctionInvokeArgEmpty, "where"));

            if (arguments.Count == 1)
                return arguments[0];

            // the second argument is a condition
            ScriptFunctionCall funcCall = (ScriptFunctionCall)callerContext;
            ScriptExpression condition = funcCall.Arguments[0];

            // hold the result in an array
            ScriptArray result = new ScriptArray();

            // the first argument doesn't have to be an array
            try
            {
                // string is enumerable but we want to treat it as a whole
                if (arguments[0] is string)
                    throw new InvalidCastException();

                IEnumerable array = (IEnumerable)arguments[0];
                foreach (object item in array)
                {
                    if ((bool)ReplaceCondition(condition, item).Evaluate(context))
                        result.Add(item);
                }
            }
            catch (InvalidCastException)
            {
                if ((bool)ReplaceCondition(condition, arguments[0]).Evaluate(context))
                    return arguments[0];
            }
            catch (Exception ex)
            {
                throw new ScriptParserRuntimeException(callerContext.Span, string.Format("RS.FilterFunctionException"), null, ex);
            }

            return result;
        }

        public ScriptExpression ReplaceCondition(ScriptExpression condition, object replaceValue)
        {
            if (condition is ScriptVariableLocal localVar)
            {
                if (localVar.Name == string.Empty)
                    return new ScriptLiteral(replaceValue);
                else
                    return localVar;
            }
            else if (condition is ScriptMemberExpression objExpr)
            {
                return new ScriptMemberExpression()
                {
                    Member = objExpr.Member,
                    Trivias = objExpr.Trivias,
                    Target = ReplaceCondition(objExpr.Target, replaceValue)
                };
            }
            else if (condition is ScriptNestedExpression nestedExpr)
            {
                return new ScriptNestedExpression()
                {
                    Expression = ReplaceCondition(nestedExpr.Expression, replaceValue),
                    Trivias = nestedExpr.Trivias
                };
            }
            else if (condition is ScriptFunctionCall funcCall)
            {
                ScriptFunctionCall funcResult = new ScriptFunctionCall()
                {
                    Target = funcCall.Target,
                    Trivias = funcCall.Trivias
                };

                foreach (ScriptExpression expr in funcCall.Arguments)
                {
                    funcResult.Arguments.Add(ReplaceCondition(expr, replaceValue));
                }

                return funcResult;
            }
            else if (condition is ScriptPipeCall pipeCall)
            {
                ScriptPipeCall pipeResult = new ScriptPipeCall()
                {
                    From = ReplaceCondition(pipeCall.From, replaceValue),
                    To = ReplaceCondition(pipeCall.To, replaceValue),
                    Trivias = pipeCall.Trivias
                };

                return pipeResult;
            }
            else if (condition is ScriptBinaryExpression binCondition)
            {
                ScriptBinaryExpression binExprResult = new ScriptBinaryExpression()
                {
                    Left = ReplaceCondition(binCondition.Left, replaceValue),
                    Right = ReplaceCondition(binCondition.Right, replaceValue),
                    Operator = binCondition.Operator,
                    Trivias = binCondition.Trivias
                };

                return binExprResult;
            }
            else
            {
                return condition;
            }
        }
    }
}