using System.IO;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("assign expression", "<target_expression> = <value_expression>")]
    public class ScriptAssignExpression : ScriptExpression
    {
        public ScriptExpression Target { get; set; }

        public ScriptExpression Value { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var valueObject = context.Evaluate(Value);
            context.SetValue(Target, valueObject);
            return valueObject;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            context.Write("=");
            context.Write(Value);
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", Target, Value);
        }
    }
}