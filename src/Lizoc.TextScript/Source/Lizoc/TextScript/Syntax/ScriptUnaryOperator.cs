// -----------------------------------------------------------------------
// <copyright file="ScriptUnaryOperator.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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