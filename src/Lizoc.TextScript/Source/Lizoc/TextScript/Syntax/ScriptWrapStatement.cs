// -----------------------------------------------------------------------
// <copyright file="ScriptWrapStatement.cs" repo="TextScript">
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

using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("wrap statement", "wrap <function_call> ... end")]
    public class ScriptWrapStatement : ScriptStatement
    {
        public ScriptExpression Target { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            // Check that the Target is actually a function
            var functionCall = Target as ScriptFunctionCall;
            if (functionCall == null)
            {
                var parameterLessFunction = context.Evaluate(Target, true);
                if (!(parameterLessFunction is IScriptCustomFunction))
                {
                    ScriptSyntaxAttribute targetPrettyname = ScriptSyntaxAttribute.Get(Target);
                    throw new ScriptRuntimeException(Target.Span, string.Format(RS.WrapEvalFailed, Target, targetPrettyname.Name));
                }

                context.BlockDelegates.Push(Body);
                return ScriptFunctionCall.Call(context, this, parameterLessFunction, false);
            }
            else
            {
                context.BlockDelegates.Push(Body);
                return context.Evaluate(functionCall);
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("wrap").ExpectSpace();
            context.Write(Target);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
    }
}