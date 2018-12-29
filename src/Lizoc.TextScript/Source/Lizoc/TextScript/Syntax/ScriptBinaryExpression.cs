// -----------------------------------------------------------------------
// <copyright file="ScriptBinaryExpression.cs" repo="TextScript">
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
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lizoc.TextScript.Functions;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("binary expression", "<expression> operator <expression>")]
    public class ScriptBinaryExpression : ScriptExpression
    {
        public ScriptExpression Left { get; set; }

        public ScriptBinaryOperator Operator { get; set; }

        public ScriptExpression Right { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var leftValueOriginal = context.Evaluate(Left);
            var leftValue = leftValueOriginal;
            var rightValueOriginal = context.Evaluate(Right);
            object rightValue = rightValueOriginal;

            return Evaluate(context, Span, Operator, leftValue, rightValue);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Left);
            // Because a-b is a variable name, we need to transform binary op a-b to a - b
            if (Operator == ScriptBinaryOperator.Substract && !context.PreviousHasSpace)
                context.Write(" ");

            context.Write(Operator.ToText());
            if (Operator == ScriptBinaryOperator.Substract)
                context.ExpectSpace();

            context.Write(Right);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Left, Operator.ToText(), Right);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }
       
        public static object Evaluate(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, object leftValue, object rightValue)
        {
            if (op == ScriptBinaryOperator.EmptyCoalescing)
            {
                return leftValue ?? rightValue;
            }
            else if (op == ScriptBinaryOperator.And || op == ScriptBinaryOperator.Or)
            {
                bool leftBoolValue = context.ToBool(span, leftValue);
                bool rightBoolValue = context.ToBool(span, rightValue);
                if (op == ScriptBinaryOperator.And)
                    return leftBoolValue && rightBoolValue;
                else
                    return leftBoolValue || rightBoolValue;
            }
            else
            {
                switch (op)
                {
                    case ScriptBinaryOperator.ShiftLeft:
                        var leftList = leftValue as IList;
                        if (leftList != null)
                        {
                            ScriptArray newList = new ScriptArray(leftList) { rightValue };
                            return newList;
                        }
                        break;
                    case ScriptBinaryOperator.ShiftRight:
                        var rightList = rightValue as IList;
                        if (rightList != null)
                        {
                            ScriptArray newList = new ScriptArray(rightList);
                            newList.Insert(0, leftValue);
                            return newList;
                        }
                        break;

                    case ScriptBinaryOperator.LiquidHasKey:
                        {
                            var leftDict = leftValue as IDictionary<string, object>;
                            if (leftDict != null)
                                return ObjectFunctions.HasKey(leftDict, context.ToString(span, rightValue));
                        }
                        break;

                    case ScriptBinaryOperator.LiquidHasValue:
                        {
                            var leftDict = leftValue as IDictionary<string, object>;
                            if (leftDict != null)
                                return ObjectFunctions.HasValue(leftDict, context.ToString(span, rightValue));
                        }
                        break;

                    case ScriptBinaryOperator.CompareEqual:
                    case ScriptBinaryOperator.CompareNotEqual:
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                    case ScriptBinaryOperator.Add:
                    case ScriptBinaryOperator.Substract:
                    case ScriptBinaryOperator.Multiply:
                    case ScriptBinaryOperator.Divide:
                    case ScriptBinaryOperator.DivideRound:
                    case ScriptBinaryOperator.Modulus:
                    case ScriptBinaryOperator.RangeInclude:
                    case ScriptBinaryOperator.RangeExclude:
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        {
                            if (leftValue is string || rightValue is string)
                                return CalculateToString(context, span, op, leftValue, rightValue);
                            else if (leftValue == EmptyScriptObject.Default || rightValue == EmptyScriptObject.Default)
                                return CalculateEmpty(context, span, op, leftValue, rightValue);
                            else
                                return CalculateOthers(context, span, op, leftValue, rightValue);
                        }
                }
            }

            throw new ScriptRuntimeException(span, string.Format(RS.OperatorNotImplemented, op.ToText(), leftValue, rightValue));
        }

        private static object CalculateEmpty(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, object leftValue, object rightValue)
        {

            bool leftIsEmptyObject = leftValue == EmptyScriptObject.Default;
            bool rightIsEmptyObject = rightValue== EmptyScriptObject.Default;

            // If both are empty, we return false or empty
            if (leftIsEmptyObject && rightIsEmptyObject)
            {
                switch (op)
                {
                    case ScriptBinaryOperator.CompareEqual:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                        return true;
                    case ScriptBinaryOperator.CompareNotEqual:
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        return false;
                }
                return EmptyScriptObject.Default;
            }

            var against = leftIsEmptyObject ? rightValue : leftValue;
            var againstEmpty = context.IsEmpty(span, against);

            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                    return againstEmpty;
                case ScriptBinaryOperator.CompareNotEqual:
                    return againstEmpty is bool
                        ? !(bool)againstEmpty
                        : againstEmpty; 
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.CompareLess:
                    return false;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return againstEmpty;
                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Substract:
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.RangeInclude:
                case ScriptBinaryOperator.RangeExclude:
                    return EmptyScriptObject.Default;
                case ScriptBinaryOperator.LiquidContains:
                case ScriptBinaryOperator.LiquidStartsWith:
                case ScriptBinaryOperator.LiquidEndsWith:
                    return false;
            }

            throw new ScriptRuntimeException(span, string.Format(RS.OperatorNotImplemented, op.ToText(), 
                leftIsEmptyObject ? "empty" : leftValue, 
                rightIsEmptyObject ? "empty" : rightValue));
        }

        private static object CalculateToString(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, object left, object right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return context.ToString(span, left) + context.ToString(span, right);
                case ScriptBinaryOperator.Multiply:
                    if (right is int)
                    {
                        var temp = left;
                        left = right;
                        right = temp;
                    }

                    if (left is int)
                    {
                        string rightText = context.ToString(span, right);
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < (int)left; i++)
                        {
                            builder.Append(rightText);
                        }
                        return builder.ToString();
                    }
                    throw new ScriptRuntimeException(span, $"Operator `{op.ToText()}` is not supported for the expression. Only working on string x int or int x string"); // unit test: 112-binary-string-error1.txt
                case ScriptBinaryOperator.CompareEqual:
                    return context.ToString(span, left) == context.ToString(span, right);
                case ScriptBinaryOperator.CompareNotEqual:
                    return context.ToString(span, left) != context.ToString(span, right);
                case ScriptBinaryOperator.CompareGreater:
                    return context.ToString(span, left).CompareTo(context.ToString(span, right)) > 0;
                case ScriptBinaryOperator.CompareLess:
                    return context.ToString(span, left).CompareTo(context.ToString(span, right)) < 0;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return context.ToString(span, left).CompareTo(context.ToString(span, right)) >= 0;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return context.ToString(span, left).CompareTo(context.ToString(span, right)) <= 0;
                case ScriptBinaryOperator.LiquidContains:
                    return context.ToString(span, left).Contains(context.ToString(span, right));
                case ScriptBinaryOperator.LiquidStartsWith:
                    return context.ToString(span, left).StartsWith(context.ToString(span, right));
                case ScriptBinaryOperator.LiquidEndsWith:
                    return context.ToString(span, left).EndsWith(context.ToString(span, right));
            }

            // unit test: 150-range-expression-error1.out.txt
            throw new ScriptRuntimeException(span, string.Format(RS.OperatorNotSupportedOnString, op.ToText())); // unit test: 112-binary-string-error2.txt
        }

        private static IEnumerable<int> RangeInclude(int left, int right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (int i = left; i <= right; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = left; i >= right; i--)
                {
                    yield return i;
                }
            }
        }

        private static IEnumerable<long> RangeInclude(long left, long right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (long i = left; i <= right; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (long i = left; i >= right; i--)
                {
                    yield return i;
                }
            }
        }

        private static IEnumerable<int> RangeExclude(int left, int right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (int i = left; i < right; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = left; i > right; i--)
                {
                    yield return i;
                }
            }
        }

        private static IEnumerable<long> RangeExclude(long left, long right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (long i = left; i < right; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (long i = left; i > right; i--)
                {
                    yield return i;
                }
            }
        }

        private static object CalculateOthers(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, object leftValue, object rightValue)
        {
            // Both values are null, applies the relevant binary ops
            if (leftValue == null && rightValue == null)
            {
                switch (op)
                {
                    case ScriptBinaryOperator.CompareEqual:
                        return true;
                    case ScriptBinaryOperator.CompareNotEqual:
                        return false;
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                        return false;
                    case ScriptBinaryOperator.Add:
                    case ScriptBinaryOperator.Substract:
                    case ScriptBinaryOperator.Multiply:
                    case ScriptBinaryOperator.Divide:
                    case ScriptBinaryOperator.DivideRound:
                    case ScriptBinaryOperator.Modulus:
                    case ScriptBinaryOperator.RangeInclude:
                    case ScriptBinaryOperator.RangeExclude:
                        return null;
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        return false;
                }
                return null;
            }

            // One value is null
            if (leftValue == null || rightValue == null)
            {
                switch (op)
                {
                    case ScriptBinaryOperator.CompareEqual:
                    case ScriptBinaryOperator.CompareNotEqual:
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        return false;
                    case ScriptBinaryOperator.Add:
                    case ScriptBinaryOperator.Substract:
                    case ScriptBinaryOperator.Multiply:
                    case ScriptBinaryOperator.Divide:
                    case ScriptBinaryOperator.DivideRound:
                    case ScriptBinaryOperator.Modulus:
                    case ScriptBinaryOperator.RangeInclude:
                    case ScriptBinaryOperator.RangeExclude:
                        return null;
                }
                return null;
            }

            Type leftType = leftValue.GetType();
            Type rightType = rightValue.GetType();

            // The order matters: decimal, double, float, long, int
            if (leftType == typeof(decimal))
            {
                decimal rightDecimal = (decimal)context.ToObject(span, rightValue, typeof(decimal));
                return CalculateDecimal(op, span, (decimal)leftValue, rightDecimal);
            }

            if (rightType == typeof(decimal))
            {
                decimal leftDecimal = (decimal)context.ToObject(span, leftValue, typeof(decimal));
                return CalculateDecimal(op, span, leftDecimal, (decimal)rightValue);
            }

            if (leftType == typeof(double))
            {
                double rightDouble = (double)context.ToObject(span, rightValue, typeof(double));
                return CalculateDouble(op, span, (double)leftValue, rightDouble);
            }

            if (rightType == typeof(double))
            {
                double leftDouble = (double)context.ToObject(span, leftValue, typeof(double));
                return CalculateDouble(op, span, leftDouble, (double)rightValue);
            }

            if (leftType == typeof(float))
            {
                float rightFloat = (float)context.ToObject(span, rightValue, typeof(float));
                return CalculateFloat(op, span, (float)leftValue, rightFloat);
            }

            if (rightType == typeof(float))
            {
                float leftFloat = (float)context.ToObject(span, leftValue, typeof(float));
                return CalculateFloat(op, span, leftFloat, (float)rightValue);
            }

            if (leftType == typeof(long))
            {
                long rightLong = (long)context.ToObject(span, rightValue, typeof(long));
                return CalculateLong(op, span, (long)leftValue, rightLong);
            }

            if (rightType == typeof(long))
            {
                long leftLong = (long)context.ToObject(span, leftValue, typeof(long));
                return CalculateLong(op, span, leftLong, (long)rightValue);
            }

            if (leftType == typeof(int) || (leftType != null && leftType.GetTypeInfo().IsEnum))
            {
                int rightInt = (int)context.ToObject(span, rightValue, typeof (int));
                return CalculateInt(op, span, (int) leftValue, rightInt);
            }

            if (rightType == typeof(int) || (rightType != null && rightType.GetTypeInfo().IsEnum))
            {
                int leftInt = (int)context.ToObject(span, leftValue, typeof (int));
                return CalculateInt(op, span, leftInt, (int) rightValue);
            }

            if (leftType == typeof(bool))
            {
                bool rightBool = (bool)context.ToObject(span, rightValue, typeof(bool));
                return CalculateBool(op, span, (bool)leftValue, rightBool);
            }

            if (rightType == typeof(bool))
            {
                bool leftBool = (bool)context.ToObject(span, leftValue, typeof(bool));
                return CalculateBool(op, span, leftBool, (bool)rightValue);
            }

            if (leftType == typeof(DateTime) && rightType == typeof(DateTime))
            {
                return CalculateDateTime(op, span, (DateTime)leftValue, (DateTime)rightValue);
            }

            if (leftType == typeof(DateTime) && rightType == typeof(TimeSpan))
            {
                return CalculateDateTime(op, span, (DateTime)leftValue, (TimeSpan)rightValue);
            }

            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedTypeForBinaryOperation, op.ToText(), leftValue, leftType, rightValue, rightType));
        }

        private static object CalculateInt(ScriptBinaryOperator op, SourceSpan span, int left, int right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (float)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return left / right;
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
                case ScriptBinaryOperator.RangeInclude:
                    return RangeInclude(left, right);
                case ScriptBinaryOperator.RangeExclude:
                    return RangeExclude(left, right);
            }
            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "integer"));
        }

        private static object CalculateLong(ScriptBinaryOperator op, SourceSpan span, long left, long right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (double)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return left / right;
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
                case ScriptBinaryOperator.RangeInclude:
                    return RangeInclude(left, right);
                case ScriptBinaryOperator.RangeExclude:
                    return RangeExclude(left, right);
            }
            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "long"));
        }

        private static object CalculateDouble(ScriptBinaryOperator op, SourceSpan span, double left, double right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return left / right;
                case ScriptBinaryOperator.DivideRound:
                    return Math.Round(left / right);
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
            }
            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "double"));
        }

        private static object CalculateDecimal(ScriptBinaryOperator op, SourceSpan span, decimal left, decimal right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return left / right;
                case ScriptBinaryOperator.DivideRound:
                    return Math.Round(left / right);
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
            }
            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "decimal"));
        }

        private static object CalculateFloat(ScriptBinaryOperator op, SourceSpan span, float left, float right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (float)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return (double)(int)(left / right);
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
            }
            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "float"));
        }

        private static object CalculateDateTime(ScriptBinaryOperator op, SourceSpan span, DateTime left, DateTime right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
            }

            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "datetime"));
        }

        private static object CalculateDateTime(ScriptBinaryOperator op, SourceSpan span, DateTime left, TimeSpan right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
            }

            throw new ScriptRuntimeException(span, string.Format(RS.OperatorNotImplemented, op.ToText(), "datetime", "timespan"));
        }

        private static object CalculateBool(ScriptBinaryOperator op, SourceSpan span, bool left, bool right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
            }

            throw new ScriptRuntimeException(span, string.Format(RS.UnsupportedOperatorForType, op.ToText(), "bool"));
        }
    }
}