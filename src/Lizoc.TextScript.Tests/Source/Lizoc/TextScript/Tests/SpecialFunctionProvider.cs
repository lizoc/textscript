// -----------------------------------------------------------------------
// <copyright file="SpecialFunctionProvider.cs" repo="TextScript">
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

using System.Text;

namespace Lizoc.TextScript.Tests
{
    public static class SpecialFunctionProvider
    {
        public static string SpecialConcatStrings(string a = "default_a", string b = "default_b", string c = "default_c")
        {
            return "a: " + (a ?? string.Empty) + ", b: " + (b ?? string.Empty) + ", c: " + (c ?? string.Empty);
        }

        public static string SpecialConcatStringAndParams(string a, params object[] args)
        {
            var builder = new StringBuilder();
            builder.Append("a: " + (a ?? string.Empty));
            for (int i = 0; i < args.Length; i++)
            {
                builder.Append($", v{i}: " + (args[i] ?? string.Empty));
            }
            return builder.ToString();
        }

        public static string SpecialConcatParams(params object[] args)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append($"v{i}: " + (args[i] ?? string.Empty));
            }
            return builder.ToString();
        }
    }
}
