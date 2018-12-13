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