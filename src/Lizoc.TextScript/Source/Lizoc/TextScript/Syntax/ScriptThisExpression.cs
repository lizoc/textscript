// -----------------------------------------------------------------------
// <copyright file="ScriptThisExpression.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// this expression returns the current <see cref="TemplateContext.CurrentGlobal"/> script object.
    /// </summary>
    [ScriptSyntax("this expression", "this")]
    public class ScriptThisExpression : ScriptExpression, IScriptVariablePath
    {
        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("this");
        }

        public object GetValue(TemplateContext context)
        {
            return context.CurrentGlobal;
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            throw new ScriptRuntimeException(Span, RS.CannotSetThisVariable);
        }

        public string GetFirstPath()
        {
            return "this";
        }

        public override string ToString()
        {
            return "this";
        }
    }
}