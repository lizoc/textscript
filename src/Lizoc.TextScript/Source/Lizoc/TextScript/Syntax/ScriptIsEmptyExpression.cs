// -----------------------------------------------------------------------
// <copyright file="ScriptIsEmptyExpression.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("empty expression", "<expression>.empty?")]
    public class ScriptIsEmptyExpression: ScriptExpression, IScriptVariablePath
    {
        public ScriptExpression Target { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            context.Write(".empty?");
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public object GetValue(TemplateContext context)
        {
            var targetObject = GetTargetObject(context, false);
            return context.IsEmpty(Span, targetObject);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            throw new ScriptRuntimeException(Span, RS.CannotSetEmptyQProperty);
        }

        public string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath();
        }

        private object GetTargetObject(TemplateContext context, bool isSet)
        {
            var targetObject = context.GetValue(Target);

            if (targetObject == null)
            {
                if (isSet || !context.EnableRelaxedMemberAccess)
                    throw new ScriptRuntimeException(this.Span, string.Format(RS.NoEmptyQPropertyForNull, this.Target));
            }
            return targetObject;
        }

        public override string ToString()
        {
            return string.Format("{0}.empty?", Target);
        }
    }
}