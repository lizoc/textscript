// -----------------------------------------------------------------------
// <copyright file="ScriptNode.cs" repo="TextScript">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// Base class for the abstract syntax tree of a TextScript program.
    /// </summary>
    public abstract class ScriptNode
    {
        /// <summary>
        /// The source span of this node.
        /// </summary>
        public SourceSpan Span;

        /// <summary>
        /// Trivias, null if <see cref="LexerOptions.KeepTrivia"/> was false.
        /// </summary>
        public ScriptTrivias Trivias { get; set; }

        /// <summary>
        /// Evaluates this instance with the specified context.
        /// </summary>
        /// <param name="context">The template context.</param>
        public abstract object Evaluate(TemplateContext context);

        public virtual bool CanHaveLeadingTrivia()
        {
            return true;
        }

        public abstract void Write(TemplateRewriterContext context);
    }

    public static class ScriptNodeExtensions
    {

        public static void AddTrivia(this ScriptNode node, ScriptTrivia trivia, bool before)
        {
            ScriptTrivias trivias = node.Trivias;
            if (trivias == null)
                node.Trivias = trivias = new ScriptTrivias();

            (before ? trivias.Before : trivias.After).Add(trivia);
        }

        public static void AddTrivias<T>(this ScriptNode node, T trivias, bool before) where T : IEnumerable<ScriptTrivia>
        {
            foreach (ScriptTrivia trivia in trivias)
            {
                node.AddTrivia(trivia, before);
            }
        }

        public static bool HasTrivia(this ScriptNode node, ScriptTriviaType triviaType, bool before)
        {
            if (node.Trivias == null)
                return false;

            foreach (ScriptTrivia trivia in (before ? node.Trivias.Before : node.Trivias.After))
            {
                if (trivia.Type == triviaType)
                    return true;
            }
            return false;
        }

        public static bool HasTriviaEndOfStatement(this ScriptNode node, bool before)
        {
            if (node.Trivias == null)
                return false;

            foreach (ScriptTrivia trivia in (before ? node.Trivias.Before : node.Trivias.After))
            {
                if (trivia.Type == ScriptTriviaType.NewLine || trivia.Type == ScriptTriviaType.SemiColon)
                    return true;
            }
            return false;
        }
    }

    public class ScriptTrivias
    {
        public ScriptTrivias()
        {
            Before = new List<ScriptTrivia>();
            After = new List<ScriptTrivia>();
        }

        public List<ScriptTrivia> Before { get; }

        public List<ScriptTrivia> After { get; }
    }

    public struct ScriptTrivia
    {
        public ScriptTrivia(SourceSpan span, ScriptTriviaType type)
        {
            Span = span;
            Type = type;
            Text = null;
        }

        public ScriptTrivia(SourceSpan span, ScriptTriviaType type, string text)
        {
            Span = span;
            Type = type;
            Text = text;
        }

        public readonly SourceSpan Span;

        public readonly ScriptTriviaType Type;

        public readonly string Text;

        public void Write(TemplateRewriterContext context)
        {
            string rawText = ToString();

            bool isRawComment = Type == ScriptTriviaType.CommentMulti && !rawText.StartsWith("##");
            if (isRawComment)
            {
                // Escape any # by \#
                rawText = rawText.Replace("#", "\\#");
                // Escape any }}
                rawText = rawText.Replace("}", "\\}");
                context.Write("## ");
            }

            context.Write(rawText);

            if (isRawComment)
                context.Write(" ##");
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ScriptTriviaType.Empty:
                    return string.Empty;
                case ScriptTriviaType.End:
                    return "end";
                case ScriptTriviaType.Comma:
                    return ",";
                case ScriptTriviaType.SemiColon:
                    return ";";
            }
            int length = Span.End.Offset - Span.Start.Offset + 1;
            return Text?.Substring(Span.Start.Offset, length);
        }
    }

    public enum ScriptTriviaType
    {
        Empty = 0,

        Whitespace,

        WhitespaceFull,

        Comment,

        Comma,

        CommentMulti,

        NewLine,

        SemiColon,

        End,
    }
}