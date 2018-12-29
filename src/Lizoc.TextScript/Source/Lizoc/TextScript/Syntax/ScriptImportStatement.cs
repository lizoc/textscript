// -----------------------------------------------------------------------
// <copyright file="ScriptImportStatement.cs" repo="TextScript">
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
    [ScriptSyntax("import statement", "import <expression>")]
    public class ScriptImportStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var value = context.Evaluate(Expression);
            if (value == null)
                return null;

            var scriptObject = value as ScriptObject;
            if (scriptObject == null)
                throw new ScriptRuntimeException(Expression.Span, string.Format(RS.InvalidImportType, value.GetType()));

            context.CurrentGlobal.Import(scriptObject);
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("import").ExpectSpace();
            context.Write(Expression);
            context.ExpectEos();
        }
    }
}