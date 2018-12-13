// -----------------------------------------------------------------------
// <copyright file="DelegateCustomFunction.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// An implementation of <see cref="IScriptCustomFunction"/> using a function delegate.
    /// </summary>
    public class DelegateCustomFunction : IScriptCustomFunction
    {
        private readonly Func<TemplateContext, ScriptNode, ScriptArray, object> _customFunction;

        public DelegateCustomFunction(Func<TemplateContext, ScriptNode, ScriptArray, object> customFunction)
        {
            _customFunction = customFunction ?? throw new ArgumentNullException(nameof(customFunction));
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return _customFunction(context, callerContext, arguments);
        }
    }
}