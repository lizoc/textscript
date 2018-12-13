namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// this expression returns the current <see cref="TemplateContext.CurrentGlobal"/> script object.
    /// </summary>
    [ScriptSyntax("this expression", "this")]
    public class ScriptThisExpression : ScriptExpression, IScriptVariablePath
    {
        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("this");
        }

        public object GetValue(TemplateContext context)
        {
            return context.CurrentGlobal;
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            throw new ScriptRuntimeException(Span, RS.CannotSetThisVariable);
        }

        public string GetFirstPath()
        {
            return "this";
        }

        public override string ToString()
        {
            return "this";
        }
    }
}