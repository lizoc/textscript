// -----------------------------------------------------------------------
// <copyright file="ScriptWithStatement.cs" repo="TextScript">
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

using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("with statement", "with <variable> ... end")]
    public class ScriptWithStatement : ScriptStatement
    {
        public ScriptExpression Name { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var target = context.GetValue(Name);
            if (!(target is IScriptObject))
            {
                string targetName = target?.GetType().Name ?? "null";
                throw new ScriptRuntimeException(Name.Span, string.Format(RS.WithEvalFailed, Name, targetName));
            }

            context.PushGlobal((IScriptObject)target);
            try
            {
                var result = context.Evaluate(Body);
                return result;
            }
            finally
            {
                context.PopGlobal();
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("with").ExpectSpace();
            context.Write(Name);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        public override string ToString()
        {
            return string.Format("with {0} <...> end", Name);
        }
    }
}