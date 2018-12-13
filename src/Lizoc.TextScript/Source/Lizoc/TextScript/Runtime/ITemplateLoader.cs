// -----------------------------------------------------------------------
// <copyright file="ITemplateLoader.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Interface used for loading a template.
    /// </summary>
    public interface ITemplateLoader
    {
        /// <summary>
        /// Gets an absolute path for the specified include template name. Note that it is not necessarely a path on a disk, 
        /// but an absolute path that can be used as a dictionary key for caching)
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templateName">The name of the template to load</param>
        /// <returns>An absolute path or unique key for the specified template name</returns>
        string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName);

        /// <summary>
        /// Loads a template using the specified template path/key.
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templatePath">The path/key previously returned by <see cref="GetPath"/></param>
        /// <returns>The content string loaded from the specified template path/key</returns>
        string Load(TemplateContext context, SourceSpan callerSpan, string templatePath);

        /// <summary>
        /// Test for the existance of a specified path or key.
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templatePath">The path/key to test.</param>
        /// <param name="type">Restricts the type of path to query.</param>
        /// <returns>A boolean value indicating whether the path/key exists.</returns>
        bool PathExists(TemplateContext context, SourceSpan callerSpan, string templatePath, PathType type);

        /// <summary>
        /// Returns a list of templates or sub-containers of a specified container.
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templatePath">The path/key to enumerate. Supports wildcards.</param>
        /// <param name="type">Restricts the type of child paths to return.</param>
        /// <returns>A list of child paths under the container specified.</returns>
        IEnumerable Enumerate(TemplateContext context, SourceSpan callerSpan, string templatePath, PathType type);
    }
}
