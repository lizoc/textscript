// -----------------------------------------------------------------------
// <copyright file="IScriptNamedArgumentContainer.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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