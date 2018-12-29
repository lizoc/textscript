// -----------------------------------------------------------------------
// <copyright file="LocalFileTemplateLoader.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Syntax;
using RS = Lizoc.PowerShell.TextScript.RS;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// A simple implementation of ITemplateLoader to load a file from disk.
    /// </summary>
    internal class LocalFileTemplateLoader : ITemplateLoader
    {
        private string _currentDirectory;

        public LocalFileTemplateLoader(string currentDirectory)
        {
            _currentDirectory = currentDirectory;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            if (Path.IsPathRooted(templateName))
                return templateName;

            string currentDirectory = GetCurrentDirectory();

            return Path.Combine(currentDirectory, templateName);
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            // Template path was produced by the `GetPath` method above in case the Template has 
            // not been loaded yet
            if (!File.Exists(templatePath))
                throw new ScriptRuntimeException(callerSpan, string.Format(RS.IncludeTemplateNotFound, templatePath));

            return File.ReadAllText(templatePath);
        }

        public bool PathExists(TemplateContext context, SourceSpan callerSpan, string templatePath, PathType type)
        {
            string targetPath = GetPath(context, callerSpan, templatePath);

            if (type == PathType.Container)
                return Directory.Exists(targetPath);
            else if (type == PathType.Leaf)
                return File.Exists(targetPath);
            else
                return (File.Exists(targetPath) || Directory.Exists(targetPath));
        }

        public IEnumerable Enumerate(TemplateContext context, SourceSpan callerSpan, string templatePath, PathType type)
        {
            string wildcard = "*";
            string parentPath = null;
            string parentFullPath = null;
            SearchOption recurseOption = SearchOption.TopDirectoryOnly;
            bool isWildcardQuery = false;

            // first check if the path contains wildcards
            if ((templatePath.IndexOf("*") != -1) || (templatePath.IndexOf("?") != -1))
            {
                isWildcardQuery = true;

                // first wildcard char
                int wildcardPosition = (templatePath.IndexOf("*") == -1) ? templatePath.IndexOf("?")
                    : (templatePath.IndexOf("?") == -1) ? templatePath.IndexOf("*")
                    : Math.Min(templatePath.IndexOf("?"), templatePath.IndexOf("*"));

                // last \ or / before the first wildcard char
                int pathSeparatorPosition = Math.Max(
                    templatePath.Substring(0, wildcardPosition).LastIndexOf(Path.DirectorySeparatorChar.ToString()),
                    templatePath.Substring(0, wildcardPosition).LastIndexOf(Path.AltDirectorySeparatorChar.ToString()));

                // there is no / or \ before the first wildcard -- e.g. foo*.txt
                if (pathSeparatorPosition == -1)
                {
                    // the **\xxx recurse syntax
                    if (templatePath.StartsWith("**" + Path.DirectorySeparatorChar.ToString()) ||
                        templatePath.StartsWith("**" + Path.AltDirectorySeparatorChar.ToString()))
                    {
                        recurseOption = SearchOption.AllDirectories;
                        wildcard = templatePath.Substring(3);
                    }
                    else
                    {
                        wildcard = templatePath;
                    }

                    parentPath = GetCurrentDirectory();
                }
                else
                {
                    parentPath = templatePath.Substring(0, pathSeparatorPosition);
                    wildcard = templatePath.Substring(pathSeparatorPosition + 1);

                    // the **\xxx recurse syntax
                    if (wildcard.StartsWith("**" + Path.DirectorySeparatorChar.ToString()) ||
                        wildcard.StartsWith("**" + Path.AltDirectorySeparatorChar.ToString()))
                    {
                        recurseOption = SearchOption.AllDirectories;
                        wildcard = wildcard.Substring(3);
                    }
                }

                // make sure there is no dir separator after the wildcard
                // we are not supporting container wildcards for now
                if (wildcard.Contains(Path.DirectorySeparatorChar.ToString()) || 
                    wildcard.Contains(Path.AltDirectorySeparatorChar.ToString()))
                {
                    throw new ScriptRuntimeException(callerSpan, string.Format(RS.ProviderDoesNotSupportWildcard, templatePath));
                }
            }
            else
            {
                // no wildcards. just use the path supplied!
                parentPath = templatePath;
            }

            // convert to rooted path
            parentFullPath = Path.GetFullPath(GetPath(context, callerSpan, parentPath));

            // trim off trailing \ or /
            if (parentFullPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                parentFullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                parentFullPath = parentFullPath.Substring(0, parentFullPath.Length - 1);
            }

            // return the full path if it is a file
            if (!isWildcardQuery && PathExists(context, callerSpan, parentFullPath, PathType.Leaf))
                return new string[] { parentFullPath };

            // check it exists!
            if (!PathExists(context, callerSpan, parentFullPath, PathType.Container))
                throw new ScriptRuntimeException(callerSpan, string.Format(RS.TemplatePathNotFound, templatePath));

            // enumerate the directory
            if (type == PathType.Container)
                return Directory.GetDirectories(parentFullPath, wildcard, recurseOption);
            else if (type == PathType.Leaf)
                return Directory.GetFiles(parentFullPath, wildcard, recurseOption);
            else
                return Directory.GetFileSystemEntries(parentFullPath, wildcard, recurseOption);
        }

        private string GetCurrentDirectory()
        {
            return _currentDirectory;
        }
    }
}
