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