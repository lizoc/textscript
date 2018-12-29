// -----------------------------------------------------------------------
// <copyright file="ScriptReturnStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("return statement", "ret <expression>?")]
    public class ScriptReturnStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.FlowState = ScriptFlowState.Return;
            return context.Evaluate(Expression);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("ret").ExpectSpace();
            context.Write(Expression);
            context.ExpectEos();
        }
    }
}