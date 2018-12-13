// -----------------------------------------------------------------------
// <copyright file="ScriptFunction.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("function statement", "func <variable> ... end")]
    public class ScriptFunction : ScriptStatement
    {
        public ScriptVariable Name { get; set; }

        public ScriptStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Name != null)
                context.SetValue(Name, this);

            return null;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return Name != null;
        }

        public override string ToString()
        {
            return string.Format("func {0} ... end", Name);
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Name != null)
            {
                context.Write("func").ExpectSpace();
                context.Write(Name);
            }
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
    }
}