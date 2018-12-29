﻿// -----------------------------------------------------------------------
// <copyright file="IScriptCustomFunction.cs" repo="TextScript">
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

using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Allows to create a custom function object.
    /// </summary>
    public interface IScriptCustomFunction
    {
        /// <summary>
        /// Calls the custom function object.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="callerContext">The script node originating this call</param>
        /// <param name="arguments">The parameters of the call</param>
        /// <param name="blockStatement">The current block statement this call is made</param>
        /// <returns>The result of the call</returns>
        object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);
    }
}