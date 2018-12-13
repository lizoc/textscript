// -----------------------------------------------------------------------
// <copyright file="ScriptIfStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("if statement", "if <expression> ... end|else|else if")]
    public class ScriptIfStatement : ScriptConditionStatement
    {
        /// <summary>
        /// Get or sets the condition of this if statement.
        /// </summary>
        public ScriptExpression Condition { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating that the result of the condition is inverted
        /// </summary>
        public bool InvertCondition { get; set; }

        public ScriptBlockStatement Then { get; set; }

        public ScriptConditionStatement Else { get; set; }


        public bool IsElseIf { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            bool conditionValue = context.ToBool(Condition.Span, context.Evaluate(Condition));
            if (InvertCondition)
                conditionValue = !conditionValue;

            return conditionValue ? context.Evaluate(Then) : context.Evaluate(Else);
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (IsElseIf)
                context.Write("else ");

            context.Write("if").ExpectSpace();
            if (InvertCondition)
                context.Write("!(");

            context.Write(Condition);
            if (InvertCondition)
                context.Write(")");

            context.ExpectEos();

            context.Write(Then);

            context.Write(Else);

            if (!IsElseIf)
                context.ExpectEnd();
        }

        public override string ToString()
        {
            return string.Format("if {0}", Condition);
        }
    }
}