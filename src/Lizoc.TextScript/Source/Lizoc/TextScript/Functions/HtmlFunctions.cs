// -----------------------------------------------------------------------
// <copyright file="HtmlFunctions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// Html functions available through the builtin object `html`.
    /// </summary>
    public class HtmlFunctions : ScriptObject
    {
        // From https://stackoverflow.com/a/17668453/1356325
        private const string RegexMatchHtml = @"<script.*?</script>|<!--.*?-->|<style.*?</style>|<(?:[^>=]|='[^']*'|=""[^""]*""|=[^'""][^\s>]*)*>";

#if NETFX
        private static readonly Regex stripHtml = new Regex(RegexMatchHtml, RegexOptions.IgnoreCase | RegexOptions.Singleline);
#endif

        /// <summary>
        /// Removes any HTML tags from the input string.
        /// </summary>
        /// <param name="context">The template context (used for <see cref="TemplateContext.RegexTimeOut"/>).</param>
        /// <param name="text">The input string.</param>
        /// <returns>The input string with all HTML tags removed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "&lt;p&gt;This is a paragraph&lt;/p&gt;" | html.strip }}
        /// ```
        /// ```html
        /// This is a paragraph
        /// ```
        /// </remarks>
        public static string Strip(TemplateContext context, string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

#if NETSTANDARD
            var stripHtml = new Regex(RegexMatchHtml, RegexOptions.IgnoreCase | RegexOptions.Singleline, context.RegexTimeOut);
#endif

            return stripHtml.Replace(text, string.Empty);
        }

        /// <summary>
        /// Escapes a HTML input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The escaped string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "&lt;p&gt;This is a paragraph&lt;/p&gt;" | html.escape }}
        /// ```
        /// ```html
        /// &amp;lt;p&amp;gt;This is a paragraph&amp;lt;/p&amp;gt;
        /// ```
        /// </remarks>
        public static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

#if NETFX35
            return System.Web.HttpUtility.HtmlEncode(text);
#elif PORTABLE
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "html.escape"));
#else
            return System.Net.WebUtility.HtmlEncode(text);
#endif
        }

        /// <summary>
        /// Converts any URL-unsafe characters in a string into percent-encoded characters.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The url encoded string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "john@liquid.com" | html.url_encode }}
        /// ```
        /// ```html
        /// john%40liquid.com
        /// ```
        /// </remarks>
        public static string UrlEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

#if PORTABLE
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "html.url_encode"));
#else
            return Uri.EscapeDataString(text);
#endif
        }

        /// <summary>
        /// Identifies all characters in a string that are not allowed in URLS, and replaces the characters with their escaped variants.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The url escaped string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "&lt;hello&gt; &amp; &lt;world&gt;" | html.url_escape }}
        /// ```
        /// ```html
        /// %3Chello%3E%20&amp;%20%3Cworld%3E
        /// ```
        /// </remarks>
        public static string UrlEscape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
#if PORTABLE
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "html.url_escape"));
#else
            return Uri.EscapeUriString(text);
#endif
        }

        /// <summary>
        /// Escapes text for usage as XML attribute value.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The string escaped for usage as an XML attribute.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 'hello "my" world' | html.xmlattrib }}
        /// ```
        /// ```html
        /// hello &amp;quot;my&amp;quot; world
        /// ```
        /// </remarks>
        public static string Xmlattrib(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Xmltext(text).Replace("\"", "&quot;");
        }

        /// <summary>
        /// Escapes text for usage as XML text value.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The string escaped for usage as XML text.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ 'hello &lt;my&gt; world &amp; friends' | html.xmltext }}
        /// ```
        /// ```html
        /// hello &amp;lt;my&amp;gt; world &amp;amp; friends
        /// ```
        /// </remarks>
        public static string Xmltext(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // #todo if text.Trim() == ""

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\r", "&#x0D;")
                .Replace("\n", "&#x0A;")
                .Replace("\t", "&#x09;");
        }
    }
}
