// -----------------------------------------------------------------------
// <copyright file="ScriptElseStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("else statement", "else | else if <expression> ... end|else|else if")]
    public class ScriptElseStatement : ScriptConditionStatement
    {
        public ScriptBlockStatement Body { get; set; }

        public ScriptConditionStatement Else { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.Evaluate(Body);
            return context.Evaluate(Else);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("else").ExpectEos();
            context.Write(Body);
            context.Write(Else);
        }
    }
}