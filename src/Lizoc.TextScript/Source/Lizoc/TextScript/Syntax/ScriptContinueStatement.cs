namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("continue statement", "continue")]
    public class ScriptContinueStatement : ScriptStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            // Only valid when we are in a loop (this should not happen as this is detected by the parser)
            if (context.IsInLoop)
            {
                context.FlowState = ScriptFlowState.Continue;
            }
            else
            {
                if (context.EnableBreakAndContinueAsReturnOutsideLoop)
                {
                    context.FlowState = ScriptFlowState.Return;
                }
                else
                {
                    // unit test: 216-break-continue-error2.txt
                    throw new ScriptRuntimeException(Span, string.Format(RS.InvalidStatementOutsideLoop, "continue"));
                }
            }
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("continue").ExpectEos();
        }
    }
}