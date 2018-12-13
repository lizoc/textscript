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