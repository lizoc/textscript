// -----------------------------------------------------------------------
// <copyright file="ScriptNopStatement.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// Empty instruction for an empty code block
    /// </summary>
    public class ScriptNopStatement : ScriptStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
        }
    }
}