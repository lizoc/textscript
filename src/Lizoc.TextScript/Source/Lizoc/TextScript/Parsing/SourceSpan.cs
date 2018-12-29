// -----------------------------------------------------------------------
// <copyright file="SourceSpan.cs" repo="TextScript">
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
    /// Defines the precise source location.
    /// </summary>
    public struct SourceSpan
    {
        public SourceSpan(string fileName, TextPosition start, TextPosition end)
        {
            FileName = fileName;
            Start = start;
            End = end;
        }

        public string FileName { get; set; }

        public TextPosition Start { get; set; }

        public TextPosition End { get; set; }

        public override string ToString()
        {
            return string.Format("{0}({1})-({2})", FileName, Start, End);
        }

        public string ToStringSimple()
        {
            return string.Format("{0}({1})", FileName, Start.ToStringSimple());
        }
    }
}