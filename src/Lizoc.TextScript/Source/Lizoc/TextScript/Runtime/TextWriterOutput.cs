// -----------------------------------------------------------------------
// <copyright file="TextWriterOutput.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Output to a <see cref="TextWriter"/>
    /// </summary>
    public class TextWriterOutput : IScriptOutput
    {
        /// <summary>
        /// Initialize a new instance of <see cref="TextWriterOutput"/> with a writer default to <see cref="StringWriter"/>
        /// </summary>
        public TextWriterOutput()
            : this(new StringWriter())
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="TextWriterOutput"/> with the specified <see cref="TextWriter"/>
        /// </summary>
        /// <param name="writer">An existing <see cref="TextWriter"/></param>
        public TextWriterOutput(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// The underlying <see cref="TextWriter"/>
        /// </summary>
        public TextWriter Writer { get; }

        public IScriptOutput Write(char c)
        {
            Writer.Write(c);
            return this;
        }

        public IScriptOutput Write(string text)
        {
            Writer.Write(text);
            return this;
        }

        public IScriptOutput Write(int number)
        {
            Writer.Write(number);
            return this;
        }

        public IScriptOutput Write(string text, int offset, int count)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            Writer.Write(text.Substring(offset, count));
            return this;
        }

        public IScriptOutput WriteLine(string text)
        {
            Writer.WriteLine(text);
            return this;
        }

        public IScriptOutput WriteLine()
        {
            Writer.WriteLine();
            return this;
        }

        public override string ToString()
        {
            return Writer.ToString();
        }
    }
}