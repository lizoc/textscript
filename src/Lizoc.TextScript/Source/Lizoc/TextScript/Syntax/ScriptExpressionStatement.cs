﻿// -----------------------------------------------------------------------
// <copyright file="ScriptExpressionStatement.cs" repo="TextScript">
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
    [ScriptSyntax("expression statement", "<expression>")]
    public class ScriptExpressionStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var result = context.Evaluate(Expression);
            // This code is necessary for wrap to work
            var codeDelegate = result as ScriptNode;
            if (codeDelegate != null)
                return context.Evaluate(codeDelegate);

            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Expression);
            context.ExpectEos();
        }

        public override string ToString()
        {
            return Expression?.ToString();
        }
    }
}