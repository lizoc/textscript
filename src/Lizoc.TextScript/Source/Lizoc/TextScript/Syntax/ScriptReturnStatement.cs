namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("return statement", "return <expression>?")]
    public class ScriptReturnStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.FlowState = ScriptFlowState.Return;
            return context.Evaluate(Expression);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("ret").ExpectSpace();
            context.Write(Expression);
            context.ExpectEos();
        }
    }
}