// -----------------------------------------------------------------------
// <copyright file="PSCustomFileTemplateLoader.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using SC = System.Collections;
using SMA = System.Management.Automation;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Syntax;
using RS = Lizoc.PowerShell.TextScript.RS;

namespace Lizoc.TextScript.Runtime
{
    internal class PSCustomFileTemplateLoader : ITemplateLoader
    {
        private SMA.ScriptBlock _getPathScript;
        private SMA.ScriptBlock _loadTemplateScript;

        public PSCustomFileTemplateLoader(SMA.ScriptBlock getPathFunc, SMA.ScriptBlock loadFunc)
        {
            _getPathScript = getPathFunc;
            _loadTemplateScript = loadFunc;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            Collection<SMA.PSObject> psResult = _getPathScript.Invoke(templateName);
            return ScriptBlockResultToString(psResult);
        }

        public bool PathExists(TemplateContext context, SourceSpan callerSpan, string templatePath, PathType type)
        {
            return PathExists(templatePath, type);
        }

        private bool PathExists(string templatePath, PathType type)
        {
            Collection<SMA.PSObject> psResult = _getPathScript.Invoke(templatePath, type);
            string resultAsString = ScriptBlockResultToString(psResult);
            return resultAsString != null;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            // Template path was produced by the `GetPath` method above in case the Template has 
            // not been loaded yet
            if (!PathExists(templatePath, PathType.Leaf))
                throw new ScriptRuntimeException(callerSpan, string.Format(RS.IncludeTemplateNotFound, templatePath));

            Collection<SMA.PSObject> psResult = _loadTemplateScript.Invoke(templatePath);
            return ScriptBlockResultToString(psResult);
        }

        public SC.IEnumerable Enumerate(TemplateContext context, SourceSpan callerSpan, string templatePath, PathType type)
        {
            string wildcard = "*";
            string parentPath = null;
            SearchOption recurseOption = SearchOption.TopDirectoryOnly;
            bool isWildcardQuery = false;

            // first check if the path contains wildcards
            int indexOfAsterisk = templatePath.IndexOf('*');
            int indexOfQuestion = templatePath.IndexOf('?');
            if ((indexOfAsterisk != -1) || (indexOfQuestion != -1))
            {
                isWildcardQuery = true;

                // first wildcard char
                int wildcardPosition = (indexOfAsterisk == -1) ? indexOfQuestion
                    : (indexOfQuestion == -1) ? indexOfAsterisk
                    : Math.Min(indexOfQuestion, indexOfAsterisk);

                // last \ or / before the first wildcard char
                int pathSeparatorPosition = Math.Max(
                    templatePath.Substring(0, wildcardPosition).LastIndexOf(Path.DirectorySeparatorChar),
                    templatePath.Substring(0, wildcardPosition).LastIndexOf(Path.AltDirectorySeparatorChar));

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

                    parentPath = GetCurrentPath();
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
                // we are not supporting container wildcards for now --> like foo\bar\**\xxx\sdaf.txt
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

            // trim off trailing \ or /
            if (parentPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || 
                parentPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                parentPath = parentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            // return the full path if it is a file
            if (!isWildcardQuery && PathExists(context, callerSpan, parentPath, PathType.Leaf))
                return new string[] { parentPath };

            // check it exists!
            if (!PathExists(context, callerSpan, parentPath, PathType.Container))
                throw new ScriptRuntimeException(callerSpan, string.Format(RS.TemplatePathNotFound, templatePath));

            // enumerate the directory
            Collection<SMA.PSObject> psResult = _getPathScript.Invoke(parentPath, wildcard, recurseOption == SearchOption.TopDirectoryOnly);
            return ScriptBlockResultToStringArray(psResult);
        }

        private string GetCurrentPath()
        {
            Collection<SMA.PSObject> psResult = _getPathScript.Invoke();
            return ScriptBlockResultToString(psResult);
        }

        private string ScriptBlockResultToString(Collection<SMA.PSObject> psOutput)
        {
            if (psOutput.Count == 0)
                return null;

            if (psOutput.Count == 1)
                return psOutput[0].ToString();

            StringBuilder sb = new StringBuilder();
            foreach (SMA.PSObject psobj in psOutput)
            {
                sb.AppendLine(psobj.ToString());
            }

            return sb.ToString();
        }

        private string[] ScriptBlockResultToStringArray(Collection<SMA.PSObject> psOutput)
        {
            if (psOutput.Count == 0)
                return new string[] { };

            if (psOutput.Count == 1)
                return new string[] { psOutput[0].ToString() };

            string[] result = new string[psOutput.Count];
            for (int i = 0; i < psOutput.Count; i++)
            {
                result[i] = psOutput[i].ToString();
            }

            return result;
        }
    }
}
