// -----------------------------------------------------------------------
// <copyright file="ScriptUnaryOperator.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Syntax
{
    public enum ScriptUnaryOperator
    {
        Not,
        Negate,
        Plus,
        FunctionAlias,
        FunctionParametersExpand
    }

    public static class ScriptUnaryOperatorExtensions
    {
        public static string ToText(this ScriptUnaryOperator op)
        {
            switch (op)
            {
                case ScriptUnaryOperator.Not:
                    return "!";
                case ScriptUnaryOperator.Negate:
                    return "-";
                case ScriptUnaryOperator.Plus:
                    return "+";
                case ScriptUnaryOperator.FunctionAlias:
                    return "@";
                case ScriptUnaryOperator.FunctionParametersExpand:
                    return "^";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }
        }
    }
}