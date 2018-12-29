﻿// -----------------------------------------------------------------------
// <copyright file="ScriptMode.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Parsing
{
    /// <summary>
    /// Defines how the parser should parse a TextScript text.
    /// </summary>
    public enum ScriptMode
    {
        /// <summary>
        /// The template contains a regular content (text and script mixed).
        /// </summary>
        Default,

        /// <summary>
        /// The template contains a liquid content (text and script mixed).
        /// </summary>
        Liquid,

        /// <summary>
        /// The template contains a frontmatter (script only) and the parser will parse only this part.
        /// </summary>
        FrontMatterOnly,

        /// <summary>
        /// The template contains a frontmatter (script only) and a content (text and script mixed) and will parse both.
        /// </summary>
        FrontMatterAndContent,

        /// <summary>
        /// The template is directly code (script only) so it is not necessary {{ }} for entering a code block
        /// </summary>
        ScriptOnly,
    }
}