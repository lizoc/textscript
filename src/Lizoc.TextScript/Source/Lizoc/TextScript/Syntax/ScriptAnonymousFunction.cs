// -----------------------------------------------------------------------
// <copyright file="ScriptAnonymousFunction.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    public class ScriptAnonymousFunction : ScriptExpression
    {
        public ScriptFunction Function { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return Function;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("do").ExpectSpace();
            context.Write(Function);
        }
    }
}