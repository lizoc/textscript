// -----------------------------------------------------------------------
// <copyright file="HtmlFunctions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
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
            Regex stripHtml = new Regex(RegexMatchHtml, RegexOptions.IgnoreCase | RegexOptions.Singleline, context.RegexTimeOut);
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
            return HtmlHelper.HtmlEncode(text);
        }

        /// <summary>
        /// Decode a HTML-escaped input string back to HTML.
        /// </summary>
        /// <param name="text">The HTML-escaped input string.</param>
        /// <returns>The decoded string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "&amp;lt;p&amp;gt;This is a paragraph&amp;lt;/p&amp;gt;" | html.unescape }}
        /// ```
        /// ```html
        /// &lt;p&gt;This is a paragraph&lt;/p&gt;
        /// ```
        /// </remarks>
        public static string Unescape(string text)
        {
            return HtmlHelper.HtmlDecode(text);
        }

        /// <summary>
        /// Converts any URL-unsafe characters in a string into percent-encoded characters.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The url encoded string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "john@liquid.com" | html.url_escape }}
        /// ```
        /// ```html
        /// john%40liquid.com
        /// ```
        /// </remarks>
        public static string UrlEscape(string text)
        {
            return HtmlHelper.UrlEncode(text);
        }

        /// <summary>
        /// Converts an URL that is percent-encoded back to a regular URL.
        /// </summary>
        /// <param name="text">The input URL that is percent-encoded.</param>
        /// <returns>The regular url string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "john%40liquid.com" | html.url_unescape }}
        /// ```
        /// ```html
        /// john@liquid.com
        /// ```
        /// </remarks>
        public static string UrlUnescape(string text)
        {
            return HtmlHelper.UrlDecode(text);
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
