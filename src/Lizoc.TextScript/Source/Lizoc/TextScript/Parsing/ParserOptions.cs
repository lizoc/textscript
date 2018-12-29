// -----------------------------------------------------------------------
// <copyright file="ParserOptions.cs" repo="TextScript">
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
    /// Defines the options used when parsing a template.
    /// </summary>
    public struct ParserOptions
    {
        /// <summary>
        /// Sets the depth limit of nested statements (e.g nested if/else) to disallow deep/potential stack-overflow exploits. Default is null, so there is no limit.
        /// </summary>
        public int? ExpressionDepthLimit { get; set; }


        /// <summary>
        /// <c>true</c> to convert liquid builtin function calls to TextScript function calls (e.g abs = math.abs, downcase = string.downcase)
        /// </summary>
        public bool ConvertLiquidFunctions { get; set; }
    }
}