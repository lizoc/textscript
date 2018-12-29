// -----------------------------------------------------------------------
// <copyright file="ScriptRawStatement.cs" repo="TextScript">
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

using System.IO;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("raw statement", "<raw_text>")]
    public class ScriptRawStatement : ScriptStatement
    {
        public string Text { get; set; }

        public int EscapeCount { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Text == null)
                return null;

            int length = Span.End.Offset - Span.Start.Offset + 1;
            if (length > 0)
            {
                // If we are in the context of output, output directly to TemplateContext.Output
                if (context.EnableOutput)
                    context.Write(Text, Span.Start.Offset, length);
                else
                    return Text.Substring(Span.Start.Offset, length);
            }
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Text == null)
                return;

            if (EscapeCount > 0)
                context.WriteEnterCode(EscapeCount);

            // TODO: handle escape
            int length = Span.End.Offset - Span.Start.Offset + 1;
            if (length > 0)
                context.Write(Text.Substring(Span.Start.Offset, length));

            if (EscapeCount > 0)
                context.WriteExitCode(EscapeCount);
        }

        public override string ToString()
        {
            int length = Span.End.Offset - Span.Start.Offset + 1;
            return Text?.Substring(Span.Start.Offset, length) ?? string.Empty;
        }
    }
}