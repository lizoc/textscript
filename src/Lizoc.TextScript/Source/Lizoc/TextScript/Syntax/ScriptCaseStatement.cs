// -----------------------------------------------------------------------
// <copyright file="ScriptCaseStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("case statement", "case <expression> ... end|when|else")]
    public class ScriptCaseStatement : ScriptConditionStatement
    {
        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public ScriptExpression Value { get; set; }

        public ScriptBlockStatement Body { get; set; }
        
        public override object Evaluate(TemplateContext context)
        {
            var caseValue = context.Evaluate(Value);
            context.PushCase(caseValue);
            try
            {
                return context.Evaluate(Body);
            }
            finally
            {
                context.PopCase();
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("case").ExpectSpace();
            context.Write(Value).ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        public override string ToString()
        {
            return string.Format("case {0}", Value);
        }
    }
}