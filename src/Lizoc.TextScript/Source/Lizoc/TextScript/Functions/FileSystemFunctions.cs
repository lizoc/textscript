// -----------------------------------------------------------------------
// <copyright file="FileSystemFunctions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// File system functions available through the builtin object `fs`.
    /// </summary>
    public class FileSystemFunctions : ScriptObject
    {
        /// <summary>
        /// Tests for the existance of a path.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="path">The path to test.</param>
        /// <param name="type">The type of path to test. May be one of the following: "leaf", "container" or "any". Defaults to "any".</param>
        /// <returns>If the path exists, `true`. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ '.\foo.txt' | fs.test }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool Test(TemplateContext context, SourceSpan span, string path, string type = "any")
        {
            if (string.IsNullOrEmpty(path))
                throw new ScriptRuntimeException(span, string.Format(RS.FSPathRequired, "fs.test"));

            PathType pathType = PathType.Any;

            if (string.IsNullOrEmpty(type))
                type = "any";
            else
                type = type.ToLowerInvariant();

            switch (type)
            {
                case "any":
                    break;
                case "container":
                    pathType = PathType.Container;
                    break;
                case "leaf":
                    pathType = PathType.Leaf;
                    break;
                default:
                    throw new ScriptRuntimeException(span, string.Format(RS.FSUnsupportedType, "fs.test", type));
            }

            ITemplateLoader templateLoader = context.TemplateLoader;
            if (templateLoader == null)
                throw new ScriptRuntimeException(span, string.Format(RS.NoTemplateLoader, "fs.test"));

            bool pathExists = false;
            try
            {
                pathExists = templateLoader.PathExists(context, span, path, pathType);
            }
            catch (Exception ex) when (!(ex is ScriptRuntimeException))
            {
            }

            return pathExists;
        }

        /// <summary>
        /// Returns items in a container path.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="path">The path to query. Wildcard is supported. For recursive search, use the syntax `**\foo.txt`.</param>
        /// <param name="type">The type of children items to return. May be one of the following: "leaf", "container" or "any". Defaults to "any".</param>
        /// <returns>A list of children items under the path specified.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ '**\fo?.txt' | fs.dir }}
        /// ```
        /// ```html
        /// [C:\foo.txt, C:\temp\foa.txt]
        /// ```
        /// </remarks>
        public static IEnumerable Dir(TemplateContext context, SourceSpan span, string path, string type = "any")
        {
            if (string.IsNullOrEmpty(path))
                throw new ScriptRuntimeException(span, string.Format(RS.FSPathRequired, "fs.dir"));

            PathType pathType = PathType.Any;

            if (string.IsNullOrEmpty(type))
                type = "any";
            else
                type = type.ToLowerInvariant();

            switch (type)
            {
                case "any":
                    break;
                case "container":
                    pathType = PathType.Container;
                    break;
                case "leaf":
                    pathType = PathType.Leaf;
                    break;
                default:
                    throw new ScriptRuntimeException(span, string.Format(RS.FSUnsupportedType, "fs.dir"));
            }

            ITemplateLoader templateLoader = context.TemplateLoader;
            if (templateLoader == null)
                throw new ScriptRuntimeException(span, string.Format(RS.NoTemplateLoader, "fs.dir"));

            return templateLoader.Enumerate(context, span, path, pathType);
        }
    }
}
