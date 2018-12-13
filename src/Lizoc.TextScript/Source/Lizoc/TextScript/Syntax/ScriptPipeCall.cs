using System.Collections;
using System.IO;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("pipe expression", "<expression> | <expression>")]
    public class ScriptPipeCall : ScriptExpression
    {
        public ScriptExpression From { get; set; }

        public ScriptExpression To { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            int beforePipeArgumentCount = context.PipeArguments.Count + 1;

            // We don't evaluate the From but we let the pipe evalute it later
            var leftResult = context.Evaluate(From);

            // Support for Parameters expansion
            var unaryExpression = From as ScriptUnaryExpression;
            if (unaryExpression != null && unaryExpression.Operator == ScriptUnaryOperator.FunctionParametersExpand)
            {
                // TODO: Pipe calls will not work correctly in case of (a | b) | ( c | d)
                var valueEnumerator = leftResult as IEnumerable;
                if (valueEnumerator != null)
                {
                    ScriptPipeArguments pipeArguments = context.PipeArguments;
                    foreach (var subValue in valueEnumerator)
                    {
                        pipeArguments.Add(subValue);
                    }
                }
                else
                {
                    context.PipeArguments.Add(leftResult);
                }
            }
            else
            {
                context.PipeArguments.Add(leftResult);
            }

            var result = context.Evaluate(To);

            int afterPipeArgumentCount = context.PipeArguments.Count;
            if (afterPipeArgumentCount >= beforePipeArgumentCount)
                throw new ScriptRuntimeException(To.Span, string.Format(RS.PipeDestNotFunc, To));

            return result;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(From);
            context.Write("|");
            context.Write(To);
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", From, To);
        }
    }
}