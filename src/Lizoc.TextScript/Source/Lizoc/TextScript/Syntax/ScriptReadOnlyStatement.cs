// -----------------------------------------------------------------------
// <copyright file="ScriptReadOnlyStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("readonly statement", "readonly <variable>")]
    public class ScriptReadOnlyStatement : ScriptStatement
    {
        public ScriptVariable Variable { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.SetReadOnly(Variable);
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("readonly").ExpectSpace();
            context.Write(Variable);
            context.ExpectEos();
        }
    }
}