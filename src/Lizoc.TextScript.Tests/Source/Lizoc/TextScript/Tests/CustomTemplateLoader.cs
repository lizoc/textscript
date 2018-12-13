// -----------------------------------------------------------------------
// <copyright file="CustomTemplateLoader.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Tests
{
    internal class CustomTemplateLoader : ITemplateLoader
    {
        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            if (templateName == "null")
            {
                return null;
            }

            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            switch (templatePath)
            {
                case "invalid":
                    return "Invalid script with syntax error: {{ 1 + }}";

                case "invalid2":
                    return null;

                case "arguments":
                    return "{{ $1 }} + {{ $2 }}";

                case "product":
                    return "product: {{ product.title }}";

                case "nested_templates":
                    return "{{ include 'header' }} {{ include 'body' }} {{ include 'footer' }}";

                case "body":
                    return "body {{ include 'body_detail' }}";

                case "header":
                    return "This is a header";

                case "body_detail":
                    return "This is a body_detail";

                case "footer":
                    return "This is a footer";

                case "recursive_nested_templates":
                    return "{{$1}}{{ x = x ?? 0; x = x + 1; if x < 5; include 'recursive_nested_templates' ($1 + 1); end }}";

                default:
                    return templatePath;
            }
        }

        public bool PathExists(TemplateContext context, SourceSpan callerSpan, string templateName, PathType type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Enumerate(TemplateContext context, SourceSpan callerSpan, string templateName, PathType type)
        {
            throw new NotImplementedException();
        }
    }

    internal class LiquidCustomTemplateLoader : ITemplateLoader
    {
        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            switch (templatePath)
            {
                case "arguments":
                    return "{{ var1 }} + {{ var2 }}";

                case "with_arguments":
                    return "{{ with_arguments }} : {{ var1 }} + {{ var2 }}";

                case "with_product":
                    return "with_product: {{ with_product.title }}";

                case "for_product":
                    return "for_product: {{ for_product.title }} ";

                default:
                    return templatePath;
            }
        }

        public bool PathExists(TemplateContext context, SourceSpan callerSpan, string templateName, PathType type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Enumerate(TemplateContext context, SourceSpan callerSpan, string templateName, PathType type)
        {
            throw new NotImplementedException();
        }
    }
}
