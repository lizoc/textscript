﻿// -----------------------------------------------------------------------
// <copyright file="MathFunctions.cs" repo="TextScript">
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
using System.Globalization;
using System.Reflection;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// Math functions available through the object `math`.
    /// </summary>
    public class MathFunctions : ScriptObject
    {
        /// <summary>
        /// Returns the absolute value of a specified number.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value.</param>
        /// <returns>The absolute value of the input value.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ -15.5 | math.abs }}
        /// {{ -5 | math.abs }}
        /// ```
        /// ```html
        /// 15.5
        /// 5
        /// ```
        /// </remarks>
        public static object Abs(TemplateContext context, SourceSpan span, object value)
        {
            if (value == null)
                return null;

            if (value is int)
                return Math.Abs((int)value);

            if (value is float)
                return Math.Abs((float)value);

            if (value is double)
                return Math.Abs((double)value);

            if (value is sbyte)
                return Math.Abs((sbyte)value);

            if (value is short)
                return Math.Abs((short)value);

            if (value is long)
                return Math.Abs((long)value);

            if (value is decimal)
                return Math.Abs((decimal)value);

            // If it is a primitive it is already unsigned
            if (value.GetType().GetTypeInfo().IsPrimitive)
                return value;

            // otherwise we don't have a number, throw an exception just in case
            throw new ScriptRuntimeException(span, string.Format(RS.NotNumberError, value));
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to the specified number.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The smallest integer greater than or equal to the specified number.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 4.6 | math.ceil }}
        /// {{ 4.3 | math.ceil }}
        /// ```
        /// ```html
        /// 5
        /// 5
        /// ```
        /// </remarks>
        public static double Ceil(double value)
        {
            return Math.Ceiling(value);
        }

        /// <summary>
        /// Divides the specified value by another value. If the divisor is an integer, the result will
        /// be floor to and converted back to an integer.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value.</param>
        /// <param name="divisor">The divisor value.</param>
        /// <returns>The division of `value` by `divisor`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 8.4 | math.divided_by 2.0 | math.round 1 }}
        /// {{ 8.4 | math.divided_by 2 }}
        /// ```
        /// ```html
        /// 4.2
        /// 4
        /// ```
        /// </remarks>
        public static object DividedBy(TemplateContext context, SourceSpan span, double value, object divisor)
        {
            var result = ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Divide, value, divisor);

            // If the divisor is an integer, return a an integer
            if (divisor is int)
            {
                if (result is double)
                    return (int)Math.Floor((double) result);

                if (result is float)
                    return (int)Math.Floor((float) result);
            }
            return result;
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The largest integer less than or equal to the specified number.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 4.6 | math.floor }}
        /// {{ 4.3 | math.floor }}
        /// ```
        /// ```html
        /// 4
        /// 4
        /// ```
        /// </remarks>
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        /// <summary>
        /// Formats a number value with specified [.NET standard numeric format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value.</param>
        /// <param name="format">The format string.</param>
        /// <param name="culture">The culture as a string (e.g `en-US`). By default the culture from <see cref="TemplateContext.CurrentCulture"/> is used.</param>
        /// <returns>The number value formatted to a string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 255 | math.format "X4" }}
        /// ```
        /// ```html
        /// 00FF
        /// ```
        /// </remarks>
        public static string Format(TemplateContext context, SourceSpan span, object value, string format, string culture = null)
        {
            if (value == null)
                return string.Empty;

            format = format ?? string.Empty;
            var formattable = value as IFormattable;
            if (!IsNumber(value) || formattable == null)
                throw new ScriptRuntimeException(span, string.Format(RS.NotFormatNumberError, value));

            return formattable.ToString(format, culture != null ? new CultureInfo(culture) : context.CurrentCulture);
        }

        /// <summary>
        /// Returns a boolean indicating if the input value is a number.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>`true` if the input value is a number. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 255 | math.is_number }}
        /// {{ "yo" | math.is_number }}
        /// ```
        /// ```html
        /// true
        /// false
        /// ```
        /// </remarks>
        public static bool IsNumber(object value)
        {
            return value is sbyte ||
                value is byte ||
                value is short ||
                value is ushort ||
                value is int ||
                value is uint ||
                value is long ||
                value is ulong ||
                value is float ||
                value is double ||
                value is decimal;
        }

        /// <summary>
        /// Substracts the specified number from the input value.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value.</param>
        /// <param name="with">The number to subtract.</param>
        /// <returns>The results of the substraction: `value` - `with`</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 255 | math.minus 5}}
        /// ```
        /// ```html
        /// 250
        /// ```
        /// </remarks>
        public static object Minus(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Substract, value, with);
        }

        /// <summary>
        /// Performs the modulo of the input value with the specified number.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value that is the dividend.</param>
        /// <param name="with">The divisor.</param>
        /// <returns>The results of the modulo: `value` % `with`</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 11 | math.modulo 10}}
        /// ```
        /// ```html
        /// 1
        /// ```
        /// </remarks>
        public static object Modulo(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Modulus, value, with);
        }

        /// <summary>
        /// Adds the specified number to the input value.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value.</param>
        /// <param name="with">The number to add.</param>
        /// <returns>The results of the addition: `value` + `with`</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 1 | math.plus 2}}
        /// ```
        /// ```html
        /// 3
        /// ```
        /// </remarks>
        public static object Plus(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Add, value, with);
        }

        /// <summary>
        /// Rounds a value to the nearest integer or to the specified number of fractional digits.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <param name="precision">The number of fractional digits in the return value. Default is 0.</param>
        /// <returns>A value rounded to the nearest integer or to the specified number of fractional digits.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 4.6 | math.round }}
        /// {{ 4.3 | math.round }}
        /// {{ 4.5612 | math.round 2 }}
        /// ```
        /// ```html
        /// 5
        /// 4
        /// 4.56
        /// ```
        /// </remarks>
        public static double Round(double value, int precision = 0)
        {
            return Math.Round(value, precision);
        }

        /// <summary>
        /// Multiply the input value with the specified number.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value.</param>
        /// <param name="with">The multiplier.</param>
        /// <returns>The results of the multiplication: `value` * `with`</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 2 | math.times 3}}
        /// ```
        /// ```html
        /// 6
        /// ```
        /// </remarks>
        public static object Times(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Multiply, value, with);
        }
    }
}