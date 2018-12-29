// -----------------------------------------------------------------------
// <copyright file="ScriptForStatement.cs" repo="TextScript">
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
using System.Collections.Generic;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// A for in loop statement.
    /// </summary>
    [ScriptSyntax("for statement", "for <variable> in <expression> ... end")]
    public class ScriptForStatement : ScriptLoopStatementBase, IScriptNamedArgumentContainer
    {
        public ScriptExpression Variable { get; set; }

        public ScriptExpression Iterator { get; set; }

        public List<ScriptNamedArgument> NamedArguments { get; set; }

        internal ScriptNode IteratorOrLastParameter => NamedArguments != null && NamedArguments.Count > 0
            ? NamedArguments[NamedArguments.Count - 1]
            : Iterator;

        protected override void EvaluateImpl(TemplateContext context)
        {
            var loopIterator = context.Evaluate(Iterator);
            var list = loopIterator as IList;
            if (list == null)
            {
                var iterator = loopIterator as IEnumerable;
                if (iterator != null)
                    list = new ScriptArray(iterator);
            }

            if (list != null)
            {
                context.SetValue(ScriptVariable.LoopLength, list.Count);
                object previousValue = null;

                bool reversed = false;
                int startIndex = 0;
                int limit = list.Count;
                if (NamedArguments != null)
                {
                    foreach (ScriptNamedArgument option in NamedArguments)
                    {
                        switch (option.Name)
                        {
                            case "offset":
                                startIndex = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                                break;
                            case "reversed":
                                reversed = true;
                                break;
                            case "limit":
                                limit = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                                break;
                            default:
                                ProcessArgument(context, option);
                                break;
                        }
                    }
                }
                int endIndex = Math.Min(limit + startIndex, list.Count) - 1;

                int index = reversed ? endIndex : startIndex;
                int dir = reversed ? -1 : 1;
                bool isFirst = true;
                int i = 0;
                BeforeLoop(context);
                while (!reversed && index <= endIndex || reversed && index >= startIndex)
                {
                    if (!context.StepLoop(this))
                        return;

                    // We update on next run on previous value (in order to handle last)
                    var value = list[index];
                    bool isLast = reversed ? index == startIndex : index == endIndex;
                    context.SetValue(ScriptVariable.LoopLast, isLast);
                    context.SetValue(ScriptVariable.LoopChanged, isFirst || !Equals(previousValue, value));
                    context.SetValue(ScriptVariable.LoopRIndex, list.Count - index - 1);
                    context.SetValue(Variable, value);

                    if (!Loop(context, index, i, isLast))
                        break;

                    previousValue = value;
                    isFirst = false;
                    index += dir;
                    i++;
                }
                AfterLoop(context);

                context.SetValue(ScriptVariable.Continue, index);
            }
            else if (loopIterator != null)
            {
                throw new ScriptRuntimeException(Iterator.Span, string.Format(RS.InvalidIterator, loopIterator.GetType()));
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("for").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            if (!context.PreviousHasSpace)
                context.Write(" ");
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            context.Write(NamedArguments);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        protected virtual void ProcessArgument(TemplateContext context, ScriptNamedArgument argument)
        {
            throw new ScriptRuntimeException(argument.Span, string.Format(RS.InvalidArgInForLoop, argument.Name, this));
        }

        public override string ToString()
        {
            return string.Format("for {0} in {1} ... end", Variable, Iterator);
        }
    }
}