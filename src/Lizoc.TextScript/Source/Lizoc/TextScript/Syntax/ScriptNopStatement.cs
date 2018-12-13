namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// Empty instruction for an empty code block
    /// </summary>
    public class ScriptNopStatement : ScriptStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
        }
    }
}