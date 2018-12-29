﻿// -----------------------------------------------------------------------
// <copyright file="ScriptReadOnlyStatement.cs" repo="TextScript">
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
    [ScriptSyntax("readonly statement", "readonly <variable>")]
    public class ScriptReadOnlyStatement : ScriptStatement
    {
        public ScriptVariable Variable { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.SetReadOnly(Variable);
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("readonly").ExpectSpace();
            context.Write(Variable);
            context.ExpectEos();
        }
    }
}