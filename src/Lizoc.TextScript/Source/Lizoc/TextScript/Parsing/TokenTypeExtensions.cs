// -----------------------------------------------------------------------
// <copyright file="TokenTypeExtensions.cs" repo="TextScript">
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lizoc.TextScript.Parsing
{
    public static class TokenTypeExtensions
    {
        private static readonly Dictionary<TokenType, string> TokenTexts = new Dictionary<TokenType, string>();

        static TokenTypeExtensions()
        {
            foreach (var field in typeof(TokenType).GetTypeInfo().GetDeclaredFields().Where(field => field.IsPublic && field.IsStatic))
            {
                TokenTextAttribute tokenText = field.GetCustomAttribute<TokenTextAttribute>();
                if (tokenText != null)
                {
                    TokenType type = (TokenType)field.GetValue(null);
                    TokenTexts.Add(type, tokenText.Text);
                }
            }
        }

        public static bool HasText(this TokenType type)
        {
            return TokenTexts.ContainsKey(type);
        }

        public static string ToText(this TokenType type)
        {
            string value;
            TokenTexts.TryGetValue(type, out value);
            return value;
        }
    }
}