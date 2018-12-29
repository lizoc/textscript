// -----------------------------------------------------------------------
// <copyright file="ScriptAssignExpression.cs" repo="TextScript">
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
    [ScriptSyntax("assign expression", "<target_expression> = <value_expression>")]
    public class ScriptAssignExpression : ScriptExpression
    {
        public ScriptExpression Target { get; set; }

        public ScriptExpression Value { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var valueObject = context.Evaluate(Value);
            context.SetValue(Target, valueObject);
            return valueObject;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            context.Write("=");
            context.Write(Value);
        }

        public override string ToString()
        {
            return string.Format("{0} = {1}", Target, Value);
        }
    }
}