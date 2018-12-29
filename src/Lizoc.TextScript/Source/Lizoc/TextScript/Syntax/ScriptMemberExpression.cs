﻿// -----------------------------------------------------------------------
// <copyright file="ScriptMemberExpression.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;
using System.Reflection;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("member expression", "<expression>.<variable_name>")]
    public class ScriptMemberExpression : ScriptExpression, IScriptVariablePath
    {
        public ScriptExpression Target { get; set; }

        public ScriptVariable Member { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            context.Write(".");
            context.Write(Member);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public object GetValue(TemplateContext context)
        {
            var targetObject = GetTargetObject(context, false);
            // In case TemplateContext.EnableRelaxedMemberAccess
            if (targetObject == null)
                return null;

            IObjectAccessor accessor = context.GetMemberAccessor(targetObject);

            string memberName = this.Member.Name;

            object value;
            if (!accessor.TryGetValue(context, Span, targetObject, memberName, out value))
                context.TryGetMember?.Invoke(context, Span, targetObject, memberName, out value);

            return value;
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            var targetObject = GetTargetObject(context, true);
            IObjectAccessor accessor = context.GetMemberAccessor(targetObject);

            string memberName = this.Member.Name;

            if (!accessor.TrySetValue(context, this.Span, targetObject, memberName, valueToSet))
                throw new ScriptRuntimeException(this.Member.Span, string.Format(RS.CannotSetReadOnlyMember, this)); // unit test: 132-member-accessor-error3.txt
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
                    throw new ScriptRuntimeException(this.Span, string.Format(RS.NoPropertyForNull, this.Target, this)); // unit test: 131-member-accessor-error1.txt
            }
            else if (targetObject is string || targetObject.GetType().IsPrimitiveOrDecimal())
            {
                if (isSet || !context.EnableRelaxedMemberAccess)
                    throw new ScriptRuntimeException(this.Span, string.Format(RS.AccessMemberOfPrimitiveFailed, targetObject, targetObject.GetType(), this)); // unit test: 132-member-accessor-error2.txt

                // If this is relaxed, set the target object to null
                if (context.EnableRelaxedMemberAccess)
                    targetObject = null;
            }

            return targetObject;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Target, Member);
        }
    }
}