﻿// -----------------------------------------------------------------------
// <copyright file="ScriptUnaryExpression.cs" repo="TextScript">
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
using System.IO;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    [ScriptSyntax("unary expression", "<operator> <expression>")]
    public class ScriptUnaryExpression : ScriptExpression
    {
        public ScriptUnaryOperator Operator { get; set; }

        public ScriptExpression Right { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            switch (Operator)
            {
                case ScriptUnaryOperator.Not:
                    {
                        var value = context.Evaluate(Right);
                        return !context.ToBool(Right.Span, value);
                    }
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                    {
                        var value = context.Evaluate(Right);

                        bool negate = Operator == ScriptUnaryOperator.Negate;

                        if (value != null)
                        {
                            if (value is int)
                                return negate ? -((int)value) : value;
                            else if (value is double)
                                return negate ? -((double)value) : value;
                            else if (value is float)
                                return negate ? -((float)value) : value;
                            else if (value is long)
                                return negate ? -((long)value) : value;
                            else if (value is decimal)
                                return negate ? -((decimal)value) : value;
                            else
                                throw new ScriptRuntimeException(this.Span, string.Format(RS.UnaryEvalFailed, value, value?.GetType()));
                        }
                    }
                    break;
                case ScriptUnaryOperator.FunctionAlias:
                    return context.Evaluate(Right, true);

                case ScriptUnaryOperator.FunctionParametersExpand:
                    // Function parameters expand is done at the function level, so here, we simply return the actual list
                    return context.Evaluate(Right);
            }

            throw new ScriptRuntimeException(Span, string.Format(RS.UnsupportedOperator, Operator));
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Operator.ToText());
            context.Write(Right);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", Operator, Right);
        }
    }
}