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