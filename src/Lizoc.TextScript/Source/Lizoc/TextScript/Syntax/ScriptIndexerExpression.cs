// -----------------------------------------------------------------------
// <copyright file="ScriptIndexerExpression.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.IO;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("indexer expression", "<expression>[<index_expression>]")]
    public class ScriptIndexerExpression : ScriptExpression, IScriptVariablePath
    {
        public ScriptExpression Target { get; set; }

        public ScriptExpression Index { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            bool isSpecialArgumentsArray = Equals(Target, ScriptVariable.Arguments) &&
                Index is ScriptLiteral &&
                ((ScriptLiteral) Index).IsPositiveInteger();

            if (!isSpecialArgumentsArray)
                context.Write("[");

            context.Write(Index);

            if (!isSpecialArgumentsArray)
                context.Write("]");
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", Target, Index);
        }

        public object GetValue(TemplateContext context)
        {
            return GetOrSetValue(context, null, false);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            GetOrSetValue(context, valueToSet, true);
        }

        public string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath();
        }

        private object GetOrSetValue(TemplateContext context, object valueToSet, bool setter)
        {
            object value = null;

            var targetObject = context.GetValue(Target);
            if (targetObject == null)
            {
                if (context.EnableRelaxedMemberAccess)
                {
                    return null;
                }
                else
                {
                    // unit test: 130-indexer-accessor-error1.txt
                    throw new ScriptRuntimeException(Target.Span, string.Format(RS.NoIndexForNull, Target, this)); 
                }
            }

            var index = context.Evaluate(Index);
            if (index == null)
            {
                if (context.EnableRelaxedMemberAccess)
                {
                    return null;
                }
                else
                {
                    // unit test: 130-indexer-accessor-error2.txt
                    throw new ScriptRuntimeException(Index.Span, string.Format(RS.NoNullIndex, Target, this));
                }
            }

            if (targetObject is IDictionary || targetObject is ScriptObject)
            {
                IObjectAccessor accessor = context.GetMemberAccessor(targetObject);
                string indexAsString = context.ToString(Index.Span, index);

                if (setter)
                {
                    if (!accessor.TrySetValue(context, Span, targetObject, indexAsString, valueToSet))
                        throw new ScriptRuntimeException(Index.Span, string.Format(RS.CannotSetReadOnlyIndexMember, indexAsString, Target)); // unit test: 130-indexer-accessor-error3.txt
                }
                else
                {
                    if (!accessor.TryGetValue(context, Span, targetObject, indexAsString, out value))
                        context.TryGetMember?.Invoke(context, Span, targetObject, indexAsString, out value);
                }
            }
            else
            {
                IListAccessor accessor = context.GetListAccessor(targetObject);
                if (accessor == null)
                    throw new ScriptRuntimeException(Target.Span, 
                        string.Format(RS.TargetObjectNotList, targetObject, targetObject.GetType().Name, Target, this)); // unit test: 130-indexer-accessor-error4.txt

                int i = context.ToInt(Index.Span, index);

                // Allow negative index from the end of the array
                if (i < 0)
                    i = accessor.GetLength(context, Span, targetObject) + i;

                if (i >= 0)
                {
                    if (setter)
                        accessor.SetValue(context, Span, targetObject, i, valueToSet);
                    else
                        value = accessor.GetValue(context, Span, targetObject, i);
                }
            }
            return value;
        }
    }
}