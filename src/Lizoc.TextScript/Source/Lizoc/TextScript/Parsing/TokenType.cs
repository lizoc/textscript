// -----------------------------------------------------------------------
// <copyright file="TokenType.cs" repo="TextScript">
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
    /// An enumeration to categorize tokens.
    /// </summary>
    public enum TokenType
    {
        Invalid,

        FrontMatterMarker,

        [TokenText("{{")]
        CodeEnter,

        [TokenText("{%")]
        LiquidTagEnter,

        [TokenText("}}")]
        CodeExit,

        [TokenText("%}")]
        LiquidTagExit,

        Raw,

        Escape,

        EscapeCount1,
        EscapeCount2,
        EscapeCount3,
        EscapeCount4,
        EscapeCount5,
        EscapeCount6,
        EscapeCount7,
        EscapeCount8,
        EscapeCount9,

        NewLine,

        Whitespace,

        WhitespaceFull,

        Comment,

        CommentMulti,

        /// <summary>
        /// An identifier starting by a $
        /// </summary>
        IdentifierSpecial,

        /// <summary>
        /// An identifier
        /// </summary>
        Identifier,

        /// <summary>
        /// An integer (int, long...)
        /// </summary>
        Integer,

        /// <summary>
        /// A floating point number
        /// </summary>
        Float,

        /// <summary>
        /// A string
        /// </summary>
        String,

        /// <summary>
        /// An implicit string with quotes
        /// </summary>
        ImplicitString,

        /// <summary>
        /// A verbatim string
        /// </summary>
        VerbatimString,

        [TokenText(";")]
        SemiColon,

        [TokenText("@")]
        Arroba,

        [TokenText("^")]
        Caret,

        [TokenText(":")]
        Colon,

        [TokenText("=")]
        Equal,

        [TokenText("|")]
        Pipe,  // |

        [TokenText("!")]
        Not, // !

        [TokenText("&&")]
        And, // &&

        [TokenText("||")]
        Or,  // ||

        [TokenText("?")]
        Question,

        [TokenText("??")]
        EmptyCoalescing,

        [TokenText("==")]
        CompareEqual,

        [TokenText("!=")]
        CompareNotEqual,

        [TokenText("<")]
        CompareLess,

        [TokenText(">")]
        CompareGreater,

        [TokenText("<=")]
        CompareLessOrEqual,

        [TokenText(">=")]
        CompareGreaterOrEqual,

        [TokenText("/")]
        Divide,

        [TokenText("//")]
        DoubleDivide,

        [TokenText("*")]
        Multiply,

        [TokenText("+")]
        Plus,

        [TokenText("-")]
        Minus,

        [TokenText("%")]
        Modulus,

        [TokenText("<<")]
        ShiftLeft,

        [TokenText(">>")]
        ShiftRight,

        [TokenText(",")]
        Comma,

        [TokenText(".")]
        Dot,

        [TokenText("..")]
        DoubleDot,

        [TokenText("..<")]
        DoubleDotLess,

        [TokenText("(")]
        OpenParent,
        [TokenText(")")]
        CloseParent,

        [TokenText("{")]
        OpenBrace,
        [TokenText("}")]
        CloseBrace,

        [TokenText("[")]
        OpenBracket,
        [TokenText("]")]
        CloseBracket,

        Eof,
    }
}