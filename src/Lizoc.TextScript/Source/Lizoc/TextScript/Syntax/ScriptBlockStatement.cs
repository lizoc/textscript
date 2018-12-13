using System.Collections.Generic;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("block statement", "<statement>...end")]
    public sealed class ScriptBlockStatement : ScriptStatement
    {
        public ScriptBlockStatement()
        {
            Statements = new List<ScriptStatement>();
        }

        public List<ScriptStatement> Statements { get; private set; }

        public override object Evaluate(TemplateContext context)
        {
            object result = null;
            for (int i = 0; i < Statements.Count; i++)
            {
                ScriptStatement statement = Statements[i];

                var expressionStatement = statement as ScriptExpressionStatement;
                bool isAssign = expressionStatement?.Expression is ScriptAssignExpression;

                result = context.Evaluate(statement);

                // Top-level assignment expression don't output anything
                if (isAssign)
                {
                    result = null;
                }
                else if (result != null && context.FlowState != ScriptFlowState.Return && context.EnableOutput)
                {
                    context.Write(Span, result);
                    result = null;
                }

                // If flow state is different, we need to exit this loop
                if (context.FlowState != ScriptFlowState.None)
                    break;
            }
            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            foreach (ScriptStatement scriptStatement in Statements)
            {
                context.Write(scriptStatement);
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override string ToString()
        {
            return string.Format("<statements[{0}]>", Statements.Count);
        }
    }
}