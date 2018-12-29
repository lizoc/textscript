﻿// -----------------------------------------------------------------------
// <copyright file="ScriptNestedExpression.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("nested expression", "(<expression>)")]
    public class ScriptNestedExpression : ScriptExpression, IScriptVariablePath
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            // A nested expression will reset the pipe arguments for the group
            context.PushPipeArguments();
            try
            {
                return context.GetValue(this);
            }
            finally
            {
                context.PopPipeArguments();
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("(");
            context.Write(Expression);
            context.Write(")");
        }

        public override string ToString()
        {
            return string.Format("({0})", Expression);
        }

        public object GetValue(TemplateContext context)
        {
            return context.Evaluate(Expression);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            context.SetValue(Expression, valueToSet);
        }

        public string GetFirstPath()
        {
            return (Expression as IScriptVariablePath)?.GetFirstPath();
        }
    }
}