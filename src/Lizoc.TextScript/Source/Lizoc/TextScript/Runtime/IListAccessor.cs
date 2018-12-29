// -----------------------------------------------------------------------
// <copyright file="IListAccessor.cs" repo="TextScript">
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

using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Generic interface used to access a list/array, used by <see cref="TemplateContext"/> via <see cref="TemplateContext.GetListAccessor"/>
    /// </summary>
    public interface IListAccessor
    {
        /// <summary>
        /// Gets the length of the specified target object
        /// </summary>
        /// <param name="context">The template context originating this call</param>
        /// <param name="span">The source span originating</param>
        /// <param name="target">The target list object</param>
        /// <returns>The length</returns>
        int GetLength(TemplateContext context, SourceSpan span, object target);

        /// <summary>
        /// Gets the element value at the specified index.
        /// </summary>
        /// <param name="context">The template context originating this call</param>
        /// <param name="span">The source span originating</param>
        /// <param name="target">The target list object</param>
        /// <param name="index">The index to retrieve a value</param>
        /// <returns>The value retrieved at the specified index for the target object</returns>
        object GetValue(TemplateContext context, SourceSpan span, object target, int index);

        /// <summary>
        /// Sets the element value at the specified index.
        /// </summary>
        /// <param name="context">The template context originating this call</param>
        /// <param name="span">The source span originating</param>
        /// <param name="target">The target list object</param>
        /// <param name="index">The index to set the value</param>
        /// <param name="value">The value to set at the specified index</param>
        void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value);
    }
}