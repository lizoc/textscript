using System.Collections.Generic;

namespace Lizoc.TextScript.Syntax
{
    /// <summary>
    /// Interfaces used by statements/expressions that have special trailing parameters (for, tablerow, include...)
    /// </summary>
    public interface IScriptNamedArgumentContainer
    {
        List<ScriptNamedArgument> NamedArguments { get; set; }
    }
}