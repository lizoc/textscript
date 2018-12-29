// -----------------------------------------------------------------------
// <copyright file="LogMessage.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Parsing
{
    public class LogMessage
    {
        public LogMessage(ParserMessageType type, SourceSpan span, string message)
        {
            Type = type;
            Span = span;
            Message = message;
        }

        public ParserMessageType Type { get; set; }

        public SourceSpan Span { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Span.ToStringSimple());
            builder.Append(" : ");
            builder.Append(Type.ToString().ToLowerInvariant());
            builder.Append(" : ");
            if (Message != null)
                builder.Append(Message);

            return builder.ToString();
        }
    }

    public enum ParserMessageType
    {
        Error,
        Warning,
    }
}