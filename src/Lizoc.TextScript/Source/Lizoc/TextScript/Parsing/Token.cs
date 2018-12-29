﻿// -----------------------------------------------------------------------
// <copyright file="Token.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Parsing
{
    /// <summary>
    /// A lightweight token struct to avoid GC allocations.
    /// </summary>
    public struct Token : IEquatable<Token>
    {
        public static readonly Token Eof = new Token(TokenType.Eof, TextPosition.Eof, TextPosition.Eof);

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public Token(TokenType type, TextPosition start, TextPosition end)
        {
            Type = type;
            Start = start;
            End = end;
        }

        /// <summary>
        /// The type of token.
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The start position of this token.
        /// </summary>
        public readonly TextPosition Start;

        /// <summary>
        /// The end position of this token.
        /// </summary>
        public TextPosition End;

        public override string ToString()
        {
            return string.Format("{0}({1}:{2})", Type, Start, End);
        }

        public string GetText(string text)
        {
            if (Type == TokenType.Eof)
                return "<eof>";

            if (Start.Offset < text.Length && End.Offset < text.Length)
                return text.Substring(Start.Offset, End.Offset - Start.Offset + 1);

            return "<error>";
        }

        public bool Match(string text, string lexerText)
        {
            int length = End.Offset - Start.Offset + 1;
            if (text.Length != length)
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                if (lexerText[Start.Offset + i] != text[i])
                    return false;
            }
            return true;
        }

        public bool Equals(Token other)
        {
            return Type == other.Type && Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Token && Equals((Token) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)Type;
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }
    }
}