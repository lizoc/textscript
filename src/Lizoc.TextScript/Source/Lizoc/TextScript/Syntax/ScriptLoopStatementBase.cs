﻿// -----------------------------------------------------------------------
// <copyright file="ScriptLoopStatementBase.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// Base class for a loop statement
    /// </summary>
    public abstract class ScriptLoopStatementBase : ScriptStatement
    {
        public ScriptBlockStatement Body { get; set; }


        protected virtual void BeforeLoop(TemplateContext context)
        {
        }

        /// <summary>
        /// Base implementation for a loop single iteration
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="index">The index in the loop</param>
        /// <param name="localIndex"></param>
        /// <param name="isLast"></param>
        /// <returns></returns>
        protected virtual bool Loop(TemplateContext context, int index, int localIndex, bool isLast)
        {
            // Setup variable
            context.SetValue(ScriptVariable.LoopFirst, index == 0);
            bool even = (index & 1) == 0;
            context.SetValue(ScriptVariable.LoopEven, even);
            context.SetValue(ScriptVariable.LoopOdd, !even);
            context.SetValue(ScriptVariable.LoopIndex, index);

            context.Evaluate(Body);

            // Return must bubble up to call site
            if (context.FlowState == ScriptFlowState.Return)
                return false;

            // If we need to break, restore to none state
            bool result = context.FlowState != ScriptFlowState.Break;
            context.FlowState = ScriptFlowState.None;
            return result;
        }

        protected virtual void AfterLoop(TemplateContext context)
        {
        }

        public override object Evaluate(TemplateContext context)
        {
            // Notify the context that we enter a loop block (used for variable with scope Loop)
            context.EnterLoop(this);            
            try
            {
                EvaluateImpl(context);
            }
            finally
            {
                // Level scope block
                context.ExitLoop(this);

                // Revert to flow state to none unless we have a return that must be handled at a higher level
                if (context.FlowState != ScriptFlowState.Return)
                    context.FlowState = ScriptFlowState.None;
            }
            return null;
        }
        protected abstract void EvaluateImpl(TemplateContext context);
    }
}