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