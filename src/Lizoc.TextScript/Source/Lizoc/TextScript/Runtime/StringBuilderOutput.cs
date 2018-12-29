// -----------------------------------------------------------------------
// <copyright file="StringBuilderOutput.cs" repo="TextScript">
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

using System;
using System.Text;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Output to a <see cref="StringBuilder"/>
    /// </summary>
    public class StringBuilderOutput : IScriptOutput
    {
        /// <summary>
        /// Initialize a new instance of <see cref="StringBuilderOutput"/>
        /// </summary>
        public StringBuilderOutput()
            : this(new StringBuilder())
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="StringBuilderOutput"/>
        /// </summary>
        /// <param name="builder">An existing <see cref="StringBuilder"/></param>
        public StringBuilderOutput(StringBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// The underlying <see cref="StringBuilder"/>
        /// </summary>
        public StringBuilder Builder { get; }

        public IScriptOutput Write(char c)
        {
            Builder.Append(c);
            return this;
        }

        public IScriptOutput Write(string text)
        {
            Builder.Append(text);
            return this;
        }

        public IScriptOutput Write(int number)
        {
            Builder.Append(number);
            return this;
        }

        public IScriptOutput Write(string text, int offset, int count)
        {
            Builder.Append(text, offset, count);
            return this;
        }

        public IScriptOutput WriteLine(string text)
        {
            Builder.AppendLine(text);
            return this;
        }

        public IScriptOutput WriteLine()
        {
            Builder.AppendLine();
            return this;
        }

        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}