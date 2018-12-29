// -----------------------------------------------------------------------
// <copyright file="ScriptPage.cs" repo="TextScript">
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

using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Syntax
{
    public class ScriptPage : ScriptNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptPage"/> class.
        /// </summary>
        public ScriptPage()
        {
        }

        /// <summary>
        /// Gets or sets the front matter. May be <c>null</c> if script is not parsed using  <see cref="ScriptMode.FrontMatterOnly"/> or <see cref="ScriptMode.FrontMatterAndContent"/>. See remarks.
        /// </summary>
        /// <remarks>
        /// Note that this code block is not executed when evaluating this page. It has to be evaluated separately (usually before evaluating the page).
        /// </remarks>
        public ScriptBlockStatement FrontMatter { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.Evaluate(Body);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Body);
        }
    }
}