﻿// -----------------------------------------------------------------------
// <copyright file="LexerOptions.cs" repo="TextScript">
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
    /// Defines the options for the lexer.
    /// </summary>
    public struct LexerOptions
    {
        public const string DefaultFrontMatterMarker = "+++";

        /// <summary>
        /// Default <see cref="LexerOptions"/>
        /// </summary>
        public static readonly LexerOptions Default = new LexerOptions()
        {
            FrontMatterMarker = DefaultFrontMatterMarker
        };

        /// <summary>
        /// Gets or sets the template mode (text and script, script only, script with frontmatter...etc.). Default is <see cref="ScriptMode.Default"/> text and script mixed.
        /// </summary>
        public ScriptMode Mode { get; set; }

        /// <summary>
        /// If selected mode is <see cref="ScriptMode.FrontMatterOnly"/> or <see cref="ScriptMode.FrontMatterAndContent"/>, this marker will be used
        /// </summary>
        public string FrontMatterMarker { get; set; }

        /// <summary>
        /// Set to `true` to parse the include target as an implicit string (to support Jekyll passing raw path /a/b/c.txt as an include target).
        /// Only valid if Mode == <see cref="ScriptMode.Liquid"/> as well.
        /// </summary>
        public bool EnableIncludeImplicitString { get; set; }

        /// <summary>
        /// Defines the position to start the lexer parsing relative to the input text passed to <see cref="Lexer"/> constructor
        /// </summary>
        public TextPosition StartPosition { get; set; }

        /// <summary>
        /// The lexer will return whitespaces tokens
        /// </summary>
        public bool KeepTrivia { get; set; }
    }
}