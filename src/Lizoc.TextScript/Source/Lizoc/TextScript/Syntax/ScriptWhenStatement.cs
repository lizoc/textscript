using System.Collections.Generic;
using System.Text;
using Lizoc.TextScript.Functions;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("when statement", "when <expression> ... end|when|else")]
    public class ScriptWhenStatement : ScriptConditionStatement
    {
        public ScriptWhenStatement()
        {
            Values = new List<ScriptExpression>();
        }

        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public List<ScriptExpression> Values { get; }

        public ScriptBlockStatement Body { get; set; }

        public ScriptConditionStatement Next { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var caseValue = context.PeekCase();
            foreach (ScriptExpression value in Values)
            {
                var whenValue = context.Evaluate(value);
                var result = ScriptBinaryExpression.Evaluate(context, Span, ScriptBinaryOperator.CompareEqual, caseValue, whenValue);
                if (result is bool && (bool)result)
                    return context.Evaluate(Body);
            }
            return context.Evaluate(Next);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("when").ExpectSpace();
            context.WriteListWithCommas(Values);
            context.ExpectEos();
            context.Write(Body);
            context.Write(Next);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("when ");
            for (int i = 0; i < Values.Count; i++)
            {
                ScriptExpression value = Values[i];
                if (i > 0)
                    builder.Append(", ");

                builder.Append(value);
            }

            return builder.ToString();
        }
    }
}