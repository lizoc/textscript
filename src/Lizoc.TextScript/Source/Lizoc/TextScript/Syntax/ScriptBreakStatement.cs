﻿// -----------------------------------------------------------------------
// <copyright file="ScriptBreakStatement.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("break statement", "break")]
    public class ScriptBreakStatement : ScriptStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            // Only valid when we are in a loop (this should not happen as this is detected by the parser)
            if (context.IsInLoop)
            {
                context.FlowState = ScriptFlowState.Break;
            }
            else
            {
                if (context.EnableBreakAndContinueAsReturnOutsideLoop)
                {
                    context.FlowState = ScriptFlowState.Return;
                }
                else
                {
                    // unit test: 216-break-continue-error1.txt
                    throw new ScriptRuntimeException(Span, string.Format(RS.InvalidStatementOutsideLoop, "break"));
                }
            }
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("break").ExpectEos();
        }
    }
}