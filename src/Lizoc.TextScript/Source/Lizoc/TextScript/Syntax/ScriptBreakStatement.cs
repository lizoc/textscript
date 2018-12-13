namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("break statement", "break")]
    public class ScriptBreakStatement : ScriptStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            // Only valid when we are in a loop (this should not happen as this is detected by the parser)
            if (context.IsInLoop)
            {
                context.FlowState = ScriptFlowState.Break;
            }
            else
            {
                if (context.EnableBreakAndContinueAsReturnOutsideLoop)
                {
                    context.FlowState = ScriptFlowState.Return;
                }
                else
                {
                    // unit test: 216-break-continue-error1.txt
                    throw new ScriptRuntimeException(Span, string.Format(RS.InvalidStatementOutsideLoop, "break"));
                }
            }
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("break").ExpectEos();
        }
    }
}