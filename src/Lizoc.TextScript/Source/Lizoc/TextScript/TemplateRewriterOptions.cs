﻿// -----------------------------------------------------------------------
// <copyright file="TemplateRewriterOptions.cs" repo="TextScript">
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
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript
{
    /// <summary>
    /// Defines the options used for rendering back an AST/<see cref="ScriptNode"/> to a text.
    /// </summary>
    public struct TemplateRewriterOptions
    {
        /// <summary>
        /// The mode used to render back an AST
        /// </summary>
        public ScriptMode Mode;
    }
}