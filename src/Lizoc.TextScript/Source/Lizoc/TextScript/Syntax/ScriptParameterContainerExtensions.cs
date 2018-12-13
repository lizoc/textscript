// -----------------------------------------------------------------------
// <copyright file="ScriptParameterContainerExtensions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Lizoc.TextScript.Syntax
{
    public static class ScriptParameterContainerExtensions
    {
        public static void AddParameter(this IScriptNamedArgumentContainer container, ScriptNamedArgument argument)
        {
            if (container.NamedArguments == null)
                container.NamedArguments = new List<ScriptNamedArgument>();

            container.NamedArguments.Add(argument);
        }

        public static void Write(this TemplateRewriterContext context, List<ScriptNamedArgument> parameters)
        {
            if (parameters == null)
                return;

            for (int i = 0; i < parameters.Count; i++)
            {
                ScriptNamedArgument option = parameters[i];
                context.ExpectSpace();
                context.Write(option);
            }
        }
    }
}