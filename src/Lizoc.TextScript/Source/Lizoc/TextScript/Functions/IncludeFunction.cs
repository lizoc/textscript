// -----------------------------------------------------------------------
// <copyright file="IncludeFunction.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// The include function available through the function `include`.
    /// </summary>
    public sealed class IncludeFunction : IScriptCustomFunction
    {
        public IncludeFunction()
        {
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            if (arguments.Count == 0)
                throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.BadFunctionInvokeArgEmpty, "include"));

            string templateName = context.ToString(callerContext.Span, arguments[0]);

            // If template name is empty, throw an exception
            if (string.IsNullOrEmpty(templateName))
            {
                // In a liquid template context, we let an include to continue without failing
                if (context is LiquidTemplateContext)
                    return null;

                throw new ScriptRuntimeException(callerContext.Span, RS.IncludeNameRequired);
            }

            ITemplateLoader templateLoader = context.TemplateLoader;
            if (templateLoader == null)
                throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.NoTemplateLoader, "include"));

            string templatePath;

            try
            {
                templatePath = templateLoader.GetPath(context, callerContext.Span, templateName);
            }
            catch (Exception ex) when (!(ex is ScriptRuntimeException))
            {
                throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.IncludeResolvePathError, templateName), ex);
            }

            // If template name is empty, throw an exception
            if (templatePath == null)
                throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.IncludePathNullError, templateName));

            // Compute a new parameters for the include
            ScriptArray newParameters = new ScriptArray(arguments.Count - 1);
            for (int i = 1; i < arguments.Count; i++)
            {
                newParameters[i] = arguments[i];
            }

            context.SetValue(ScriptVariable.Arguments, newParameters, true);

            Template template;

            if (!context.CachedTemplates.TryGetValue(templatePath, out template))
            {

                string templateText;
                try
                {
                    templateText = templateLoader.Load(context, callerContext.Span, templatePath);
                }
                catch (Exception ex) when (!(ex is ScriptRuntimeException))
                {
                    throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.IncludeLoadError, templateName, templatePath), ex);
                }

                if (templateText == null)
                    throw new ScriptRuntimeException(callerContext.Span, string.Format(RS.IncludeContentEmpty, templateName, templatePath));

                // Clone parser options
                ParserOptions parserOptions = context.TemplateLoaderParserOptions;
                LexerOptions lexerOptions = context.TemplateLoaderLexerOptions;
                template = Template.Parse(templateText, templatePath, parserOptions, lexerOptions);

                // If the template has any errors, throw an exception
                if (template.HasErrors)
                    throw new ScriptParserRuntimeException(callerContext.Span, string.Format(RS.IncludeParseError, templateName, templatePath), template.Messages);

                context.CachedTemplates.Add(templatePath, template);
            }

            // Make sure that we cannot recursively include a template

            context.PushOutput();
            object result = null;
            try
            {
                context.EnterRecursive(callerContext);
                result = template.Render(context);
                context.ExitRecursive(callerContext);
            }
            finally
            {
                context.PopOutput();
            }

            return result;
        }
    }
}