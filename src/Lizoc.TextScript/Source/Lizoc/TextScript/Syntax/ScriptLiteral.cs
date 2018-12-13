// -----------------------------------------------------------------------
// <copyright file="ScriptLiteral.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;
using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("literal", "<value>")]
    public class ScriptLiteral : ScriptExpression
    {
        public ScriptLiteral()
        {
        }

        public ScriptLiteral(object value)
        {
            Value = value;
        }

        public object Value { get; set; }

        public ScriptLiteralStringQuoteType StringQuoteType { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return Value;
        }

        public bool IsPositiveInteger()
        {
            if (Value == null)
                return false;

            Type type = Value.GetType();
            if (type == typeof(int))
                return ((int)Value) >= 0;
            else if (type == typeof(byte))
                return true;
            else if (type == typeof(sbyte))
                return ((sbyte)Value) >= 0;
            else if (type == typeof(short))
                return ((short)Value) >= 0;
            else if (type == typeof(ushort))
                return true;
            else if (type == typeof(uint))
                return true;
            else if (type == typeof(long))
                return (long)Value > 0;
            else if (type == typeof(ulong))
                return true;

            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Value == null)
            {
                context.Write("null");
                return;
            }

            Type type = Value.GetType();
            if (type == typeof(string))
                context.Write(ToLiteral(StringQuoteType, (string) Value));
            else if (type == typeof(bool))
                context.Write(((bool) Value) ? "true" : "false");
            else if (type == typeof(int))
                context.Write(((int) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(double))
                context.Write(AppendDecimalPoint(((double)Value).ToString("R", CultureInfo.InvariantCulture), true)); 
            else if (type == typeof(float))
                context.Write(AppendDecimalPoint(((float)Value).ToString("R", CultureInfo.InvariantCulture), true));
            else if (type == typeof(byte))
                context.Write(((byte) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(sbyte))
                context.Write(((sbyte) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(short))
                context.Write(((short) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(ushort))
                context.Write(((ushort) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(uint))
                context.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(long))
                context.Write(((long) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(ulong))
                context.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
            else if (type == typeof(char))
                context.Write(ToLiteral(ScriptLiteralStringQuoteType.SimpleQuote, Value.ToString()));
            else
                context.Write(Value.ToString());
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "null";
        }

        private static string ToLiteral(ScriptLiteralStringQuoteType quoteType, string input)
        {
            char quote;
            switch (quoteType)
            {
                case ScriptLiteralStringQuoteType.DoubleQuote:
                    quote = SpecialChar.DoubleQuote;
                    break;
                case ScriptLiteralStringQuoteType.SimpleQuote:
                    quote = SpecialChar.SingleQuote;
                    break;
                case ScriptLiteralStringQuoteType.Verbatim:
                    quote = '`';
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(quoteType));
            }

            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append(quote);

            if (quoteType == ScriptLiteralStringQuoteType.Verbatim)
            {
                literal.Append(input.Replace("`", "``"));
            }
            else
            { 
                foreach (char c in input)
                {
                    switch (c)
                    {
                        case SpecialChar.EscapeChar: 
                            literal.Append(SpecialChar.EscapeChar.ToString() + SpecialChar.EscapeChar.ToString()); 
                            break;
                        case SpecialChar.Null: 
                            literal.Append(SpecialChar.EscapeChar.ToString() + "0"); 
                            break;
                        case '\a': 
                            literal.Append(SpecialChar.EscapeChar.ToString() + "a"); 
                            break;
                        case SpecialChar.Backspace: 
                            literal.Append(SpecialChar.EscapeChar.ToString() + "b"); 
                            break;
                        case SpecialChar.FormFeed: 
                            literal.Append(SpecialChar.EscapeChar.ToString() + "f"); 
                            break;
                        case SpecialChar.NewLine:
                            literal.Append(SpecialChar.EscapeChar.ToString() + "n"); 
                            break;
                        case SpecialChar.CarriageReturn:
                            literal.Append(SpecialChar.EscapeChar.ToString() + "r"); 
                            break;
                        case SpecialChar.Tab: 
                            literal.Append(SpecialChar.EscapeChar.ToString() + "t"); 
                            break;
                        case SpecialChar.VerticalTab: 
                            literal.Append(SpecialChar.EscapeChar.ToString() + "v"); 
                            break;
                        default:
                            if (c == quote)
                            {
                                literal.Append(SpecialChar.EscapeChar).Append(c);
                            }
                            else if (char.IsControl(c))
                            {
                                literal.Append(SpecialChar.EscapeChar.ToString() + "u");
                                literal.Append(((ushort)c).ToString("x4"));
                            }
                            else
                            {
                                literal.Append(c);
                            }
                            break;
                    }
                }
            }
            literal.Append(quote);
            return literal.ToString();
        }

        // Code from SharpYaml
        private static string AppendDecimalPoint(string text, bool hasNaN)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                // Do not append a decimal point if floating point type value
                // - is in exponential form, or
                // - already has a decimal point
                if (c == 'e' || c == 'E' || c == '.')
                    return text;
            }
            // Special cases for floating point type supporting NaN and Infinity
            if (hasNaN && (string.Equals(text, "NaN") || text.Contains("Infinity")))
                return text;

            return text + ".0";
        }
    }

    public enum ScriptLiteralStringQuoteType
    {
        DoubleQuote,

        SimpleQuote,

        Verbatim
    }
}