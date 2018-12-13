// -----------------------------------------------------------------------
// <copyright file="ParserOptions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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