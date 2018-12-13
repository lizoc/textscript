// -----------------------------------------------------------------------
// <copyright file="StringFunctions.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// String functions available through the builtin object `string`.
    /// </summary>
    public class StringFunctions : ScriptObject
    {
        /// <summary>
        /// Concatenates two strings.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="with">The text to append.</param>
        /// <returns>The two strings concatenated.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "Hello" | string.append " World" }}
        /// ```
        /// ```html
        /// Hello World
        /// ```
        /// </remarks>
        public static string Append(string text, string with)
        {
            return (text ?? string.Empty) + (with ?? string.Empty);
        }

        /// <summary>
        /// Returns a string based on whether a condition is evaluated to `true` or `false`.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="truthy">The string to return if the condition evaluates to `true`.</param>
        /// <param name="falsy">The string to return if the condition evaluates to `false`.</param>
        /// <returns>A string based on condition evaluated.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ true | string.bool "Got it!" "Not here" }}
        /// ```
        /// ```html
        /// Got it!
        /// ```
        /// </remarks>
        public static string Bool(bool condition, string truthy, string falsy)
        {
            return (condition ? truthy : falsy);
        }

        /// <summary>
        /// Converts the first character of the passed string to a upper case character.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The capitalized input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.capitalize }}
        /// ```
        /// ```html
        /// Test
        /// ```
        /// </remarks>
        public static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text) || char.IsUpper(text[0]))
                return text ?? string.Empty;

            StringBuilder builder = new StringBuilder(text);
            builder[0] = char.ToUpper(builder[0]);

            return builder.ToString();
        }

        /// <summary>
        /// Converts the first character of each word in the passed string to a upper case character.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The capitalized input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "This is easy" | string.capitalizewords }}
        /// ```
        /// ```html
        /// This Is Easy
        /// ```
        /// </remarks>
        public static string Capitalizewords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            StringBuilder builder = new StringBuilder(text.Length);
            bool previousSpace = true;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    previousSpace = true;
                }
                else if (previousSpace && char.IsLetter(c))
                {
                    // TODO: Handle culture
                    c = char.ToUpper(c);
                    previousSpace = false;
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string contains the specified substring.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="value">The substring to look for.</param>
        /// <returns>`true` if the substring was found in in input text. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "This is easy" | string.contains "easy" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool Contains(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
                return false;

            return text.Contains(value);
        }

        /// <summary>
        /// Converts the string to lower case.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The input string in lower case.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "TeSt" | string.downcase }}
        /// ```
        /// ```html
        /// test
        /// ```
        /// </remarks>
        public static string Downcase(string text)
        {
            return text?.ToLowerInvariant();
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string ends with the specified substring.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="value">The substring that should be the suffix of the input string.</param>
        /// <returns>`true` if the input string ends with the substring specified. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "This is easy" | string.ends_with "easy" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool EndsWith(string text, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(text))
                return false;

            return text.EndsWith(value);
        }

        /// <summary>
        /// Returns a url handle from the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>A url handle based on the input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ '100% M &amp; Ms!!!' | string.handleize }}
        /// ```
        /// ```html
        /// 100-m-ms
        /// ```
        /// </remarks>
        public static string Handleize(string text)
        {
            StringBuilder builder = new StringBuilder();
            char lastChar = (char) 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsLetterOrDigit(c))
                {
                    lastChar = c;
                    builder.Append(char.ToLowerInvariant(c));
                }
                else if (lastChar != '-')
                {
                    builder.Append('-');
                    lastChar = '-';
                }
            }

            if (builder.Length > 0 && builder[builder.Length - 1] == '-')
                builder.Length--;

            return builder.ToString();
        }

        /// <summary>
        /// Removes any whitespace characters on the **beginning** of the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The input string without any beginning whitespace characters.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ '   too many spaces           ' | string.lstrip  }}
        /// ```
        /// > Highlight to see the empty spaces to the right of the string
        /// ```html
        /// too many spaces           
        /// ```
        /// </remarks>
        public static string LStrip(string text)
        {
            return text?.TrimStart();
        }

        /// <summary>
        /// Outputs the singular or plural version of a string based on the value of a number. 
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <param name="singular">The singular string to return if number is equal to 1</param>
        /// <param name="plural">The plural string to return if number is not equal to 1</param>
        /// <returns>The singular or plural string based on number specified.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ products.size }} {{products.size | string.pluralize 'product' 'products' }}
        /// ```
        /// ```html
        /// 7 products
        /// ```
        /// </remarks>
        public static string Pluralize(int number, string singular, string plural)
        {
            return number == 1 ? singular : plural;
        }

        /// <summary>
        /// Concatenates two strings by prepending the input string with a prefix.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="by">The string to prepend to the input string.</param>
        /// <returns>The two strings concatenated.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "World" | string.prepend "Hello " }}
        /// ```
        /// ```html
        /// Hello World
        /// ```
        /// </remarks>
        public static string Prepend(string text, string by)
        {
            return (by ?? string.Empty) + (text ?? string.Empty);
        }

        /// <summary>
        /// Removes all occurrences of a substring from a string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="remove">The substring to remove.</param>
        /// <returns>The input string with the all occurence of a substring removed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "Hello, world. Goodbye, world." | string.remove "world" }}
        /// ```
        /// ```html
        /// Hello, . Goodbye, .
        /// ```
        /// </remarks>
        public static string Remove(string text, string remove)
        {
            if (string.IsNullOrEmpty(remove) || string.IsNullOrEmpty(text))
                return text;

            return text.Replace(remove, string.Empty);
        }

        /// <summary>
        /// Removes the first occurrence of a substring from a string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="remove">The first occurence of substring to remove.</param>
        /// <returns>The input string with the first occurence of a substring removed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "Hello, world. Goodbye, world." | string.remove_first "world" }}
        /// ```
        /// ```html
        /// Hello, . Goodbye, world.
        /// ```
        /// </remarks>
        public static string RemoveFirst(string text, string remove)
        {
            return ReplaceFirst(text, remove, string.Empty);
        }

        /// <summary>
        /// Replaces all occurrences of a substring with the value specified.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="match">The substring to find in the input string.</param>
        /// <param name="replace">The value that should be used to replace all occurances of the substring.</param>
        /// <returns>The input string replaced.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "Hello, world. Goodbye, world." | string.replace "world" "buddy" }}
        /// ```
        /// ```html
        /// Hello, buddy. Goodbye, buddy.
        /// ```
        /// </remarks>
        public static string Replace(string text, string match, string replace)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            match = match ?? string.Empty;
            replace = replace ?? string.Empty;

            return text.Replace(match, replace);
        }

        /// <summary>
        /// Substitutes a `null` or empty string with the specified string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="replace">The string to return if the input value is `null` or an empty string.</param>
        /// <param name="notEmpty">The string to return if the input value is a not an empty string. Defaults to the inputbstring itself.</param>
        /// <returns>A string based on whether the input string is `null` or an empty string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "" | string.replace_empty "its empty" "its full" }}
        /// ```
        /// ```html
        /// its empty
        /// ```
        /// </remarks>
        public static string ReplaceEmpty(string text, string replace, string notEmpty = null)
        {
            if (string.IsNullOrEmpty(text))
                return replace;

            return (notEmpty == null ? text : notEmpty);
        }

        /// <summary>
        /// Replaces the first occurrence of a substring with the value specified.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="match">The substring to find.</param>
        /// <param name="replace">The value used to replace the substring.</param>
        /// <returns>The input string replaced.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "Hello, world. Goodbye, world." | string.replace_first "world" "buddy" }}
        /// ```
        /// ```html
        /// Hello, buddy. Goodbye, world.
        /// ```
        /// </remarks>
        public static string ReplaceFirst(string text, string match, string replace)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (string.IsNullOrEmpty(match))
                return text;

            replace = replace ?? string.Empty;

            int indexOfMatch = text.IndexOf(match, StringComparison.OrdinalIgnoreCase);
            if (indexOfMatch < 0)
                return text;

            StringBuilder builder = new StringBuilder();
            builder.Append(text.Substring(0, indexOfMatch));
            builder.Append(replace);
            builder.Append(text.Substring(indexOfMatch + match.Length));

            return builder.ToString();
        }

        /// <summary>
        /// Removes any whitespace characters at the **end** of the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The input string without any whitespace characters at the end.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ '   too many spaces           ' | string.rstrip  }}
        /// ```
        /// > Highlight to see the empty spaces to the right of the string
        /// ```html
        ///    too many spaces
        /// ```
        /// </remarks>
        public static string RStrip(string text)
        {
            return text?.TrimEnd();
        }

        /// <summary>
        /// Returns the number of characters from the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The length of the input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.size }}
        /// ```
        /// ```html
        /// 4
        /// ```
        /// </remarks>
        public static int Size(string text)
        {
            return string.IsNullOrEmpty(text) ? 0 : text.Length;
        }

        /// <summary>
        /// The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring. 
        /// If no second parameter is given, a substring with the remaining characters will be returned.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="start">The starting index character where the slice should start from the input string.</param>
        /// <param name="length">The number of character. Default is 0, meaning that the remaining of the input string will be returned.</param>
        /// <returns>The input string sliced</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "hello" | string.slice 0 }}
        /// {{ "hello" | string.slice 1 }}
        /// {{ "hello" | string.slice 1 3 }}
        /// {{ "hello" | string.slice 1 length:3 }}
        /// ```
        /// ```html
        /// hello
        /// ello
        /// ell
        /// ell
        /// ```
        /// </remarks>
        public static string Slice(string text, int start, int length = 0)
        {
            if (string.IsNullOrEmpty(text) || start >= text.Length)
                return string.Empty;

            if (start < 0)
                start = start + text.Length;

            if (length <= 0)
                length = text.Length;

            if (start < 0)
            {
                if (start + length <= 0)
                    return string.Empty;

                length = length + start;
                start = 0;
            }

            if (start + length > text.Length)
                length = text.Length - start;

            return text.Substring(start, length);
        }

        /// <summary>
        /// The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring. 
        /// If no second parameter is given, the character at the specified index will be returned.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="start">The starting index where the slice should start from the input string.</param>
        /// <param name="length">The number of character to slice. Default is 1, meaning that only the first character at the starting position will be returned.</param>
        /// <returns>The input string sliced.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "hello" | string.slice1 0 }}
        /// {{ "hello" | string.slice1 1 }}
        /// {{ "hello" | string.slice1 1 3 }}
        /// {{ "hello" | string.slice1 1 length: 3 }}
        /// ```
        /// ```html
        /// h
        /// e
        /// ell
        /// ell
        /// ```
        /// </remarks>
        public static string Slice1(string text, int start, int length = 1)
        {
            if (string.IsNullOrEmpty(text) || start > text.Length || length <= 0)
                return string.Empty;

            if (start < 0)
                start = start + text.Length;

            if (start < 0)
            {
                length = length + start;
                start = 0;
            }

            if (start + length > text.Length)
                length = text.Length - start;

            return text.Substring(start, length);
        }

        /// <summary>
        /// Split a string into an array, using a substring as the delimiter.
        ///
        /// You can output different parts of the output array using various `array` functions.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="match">The substring used to split the input string.</param>
        /// <returns>An array consisting of the substrings of the input string between the delimiter.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ for word in "Hi, how are you today?" | string.split ' ' ~}}
        /// {{ word }}
        /// {{ end ~}}
        /// ```
        /// ```html
        /// Hi,
        /// how
        /// are
        /// you
        /// today?
        /// ```
        /// </remarks>
        public static IEnumerable Split(string text, string match)
        {
            if (string.IsNullOrEmpty(text))
                return Enumerable.Empty<string>();

            match = match ?? string.Empty;

            return text.Split(new[] { match }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string starts with the specified substring.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="value">The prefix to look for.</param>
        /// <returns>`true` if the input string starts with the prefix specified. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "This is easy" | string.starts_with "This" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool StartsWith(string text, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(text))
                return false;

            return text.StartsWith(value);
        }

        /// <summary>
        /// Removes any whitespace characters from the **start** and **end** of the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The input string without any starting or ending whitespace characters.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ '   too many spaces           ' | string.strip  }}
        /// ```
        /// > Highlight to see the empty spaces to the right of the string
        /// ```html
        /// too many spaces
        /// ```
        /// </remarks>
        public static string Strip(string text)
        {
            return text?.Trim();
        }

        /// <summary>
        /// Removes any line breaks/newlines from a string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The input string without any breaks/newlines characters.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "This is a string.^r^n With ^nanother ^rstring" | string.strip_newlines  }}
        /// ```
        /// ```html
        /// This is a string. With another string
        /// ```
        /// </remarks>
        public static string StripNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return Regex.Replace(text, @"\r\n|\r|\n", string.Empty);
        }

        /// <summary>
        /// Converts a string to a 32-bit integer.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string.</param>
        /// <returns>A 32-bit integer, or `null` if the conversion failed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "123" | string.to_int + 1 }}
        /// ```
        /// ```html
        /// 124
        /// ```
        /// </remarks>
        public static object ToInt(TemplateContext context, string text)
        {
            return int.TryParse(text, NumberStyles.Integer, context.CurrentCulture, out int result) 
                ? (object) result 
                : null;
        }

        /// <summary>
        /// Converts a string to a 64-bit integer.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string.</param>
        /// <returns>A 64-bit integer, or `null` if the conversion failed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "123678912345678" | string.to_long + 1 }}
        /// ```
        /// ```html
        /// 123678912345679
        /// ```
        /// </remarks>
        public static object ToLong(TemplateContext context, string text)
        {
            return long.TryParse(text, NumberStyles.Integer, context.CurrentCulture, out long result) 
                ? (object)result 
                : null;
        }

        /// <summary>
        /// Converts a string to a single precision floating point number.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string.</param>
        /// <returns>A 32-bit floating point number, or `null` if the conversion failed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "123.4" | string.to_float + 1 }}
        /// ```
        /// ```html
        /// 124.4
        /// ```
        /// </remarks>
        public static object ToFloat(TemplateContext context, string text)
        {
            return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, context.CurrentCulture, out float result) 
                ? (object)result 
                : null;
        }

        /// <summary>
        /// Converts a string to a double precision floating point number.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string.</param>
        /// <returns>A 64-bit floating point number, or `null` if the conversion failed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "123.4" | string.to_double + 1 }}
        /// ```
        /// ```html
        /// 124.4
        /// ```
        /// </remarks>
        public static object ToDouble(TemplateContext context, string text)
        {
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, context.CurrentCulture, out double result) 
                ? (object)result 
                : null;
        }

        /// <summary>
        /// Truncates a string down to the number of characters passed as the first parameter. An ellipsis is appended to the truncated string 
        /// and is included in the character count.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="length">The maximum length of the output string, including the length of the ellipsis.</param>
        /// <param name="ellipsis">The ellipsis to append to the end of the truncated string. Defaults to 3 dots (...).</param>
        /// <returns>The truncated input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "The cat came back the very next day" | string.truncate 13 }}
        /// ```
        /// ```html
        /// The cat ca...
        /// ```
        /// </remarks>
        public static string Truncate(string text, int length, string ellipsis = null)
        {
            ellipsis = ellipsis ?? "...";
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int lMinusTruncate = length - ellipsis.Length;
            if (text.Length > length)
            {
                StringBuilder builder = new StringBuilder(length);
                builder.Append(text, 0, lMinusTruncate < 0 ? 0 : lMinusTruncate);
                builder.Append(ellipsis);
                return builder.ToString();
            }
            return text;
        }

        /// <summary>
        /// Truncates a string down to the number of words passed as the first parameter. An ellipsis is appended to the truncated string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="count">The number of words to keep from the input string before appending the ellipsis.</param>
        /// <param name="ellipsis">The ellipsis to append to the end of the truncated string. Defaults to 3 dots (...).</param>
        /// <returns>The truncated input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "The cat came back the very next day" | string.truncatewords 4 }}
        /// ```
        /// ```html
        /// The cat came back...
        /// ```
        /// </remarks>
        public static string Truncatewords(string text, int count, string ellipsis = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            bool isFirstWord = true;
            foreach (string word in Regex.Split(text, @"\s+"))
            {
                if (count <= 0)
                    break;

                if (!isFirstWord)
                    builder.Append(' ');

                builder.Append(word);

                isFirstWord = false;
                count--;
            }
            builder.Append("...");
            return builder.ToString();
        }

        /// <summary>
        /// Converts the string to upper case.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The input string in upper case.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.upcase }}
        /// ```
        /// ```html
        /// TEST
        /// ```
        /// </remarks>
        public static string Upcase(string text)
        {
            return text?.ToUpperInvariant();
        }

        /// <summary>
        /// Pads a string with leading spaces, such that the total length of the result string is of the specified length.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="width">The number of characters that the result string should have.</param>
        /// <param name="paddingChar">The character to use for padding. Defaults to the space character.</param>
        /// <returns>The input string padded.</returns>
        /// <remarks>
        /// ```template-text
        /// hello{{ "world" | string.pad_left 10 }}
        /// ```
        /// ```html
        /// hello     world
        /// ```
        /// </remarks>
        public static string PadLeft(string text, int width, char paddingChar = ' ')
        {
            return (text ?? string.Empty).PadLeft(width, paddingChar);
        }

        /// <summary>
        /// Pads a string with trailing spaces, such that the total length of the result string is of the specified length.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="width">The number of characters in the resulting string</param>
        /// <param name="paddingChar">The character to use for padding. Defaults to the space character.</param>
        /// <returns>The input string padded.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "hello" | string.pad_right 10 }}world
        /// ```
        /// ```html
        /// hello     world
        /// ```
        /// </remarks> 
        public static string PadRight(string text, int width, char paddingChar = ' ')
        {
            return (text ?? string.Empty).PadRight(width, paddingChar);
        }

#if !PCL328 && !NETSTANDARD1_1
        /// <summary>
        /// Computes the `md5` hash of the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The `md5` hash of the input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.md5 }}
        /// ```
        /// ```html
        /// 098f6bcd4621d373cade4e832627b4f6
        /// ```
        /// </remarks>
        public static string Md5(string text)
        {
            text = text ?? string.Empty;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return Hash(md5, text);
            }
        }

        /// <summary>
        /// Computes the `sha1` hash of the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The `sha1` hash of the input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.sha1 }}
        /// ```
        /// ```html
        /// a94a8fe5ccb19ba61c4c0873d391e987982fbbd3
        /// ```
        /// </remarks>
        public static string Sha1(string text)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                return Hash(sha1, text);
            }
        }

        /// <summary>
        /// Computes the `sha256` hash of the input string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <returns>The `sha256` hash of the input string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.sha256 }}
        /// ```
        /// ```html
        /// 9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08
        /// ```
        /// </remarks>
        public static string Sha256(string text)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return Hash(sha256, text);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-1 hash using a hash message authentication code (HMAC). A secret key parameter is required.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The `SHA-1` hash of the input string using a hash message authentication code (HMAC).</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.hmac_sha1 "secret" }}
        /// ```
        /// ```html
        /// 1aa349585ed7ecbd3b9c486a30067e395ca4b356
        /// ```
        /// </remarks>
        public static string HmacSha1(string text, string secretKey)
        {
            using (var hsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)))
            {
                return Hash(hsha1, text);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash using a hash message authentication code (HMAC). A secret key parameter is required.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The `SHA-256` hash of the input string using a hash message authentication code (HMAC).</returns>
        /// <remarks>
        /// ```template-text
        /// {{ "test" | string.hmac_sha256 "secret" }}
        /// ```
        /// ```html
        /// 0329a06b62cd16b33eb6792be8c60b158d89a2ee3a876fce9a881ebb488c0914
        /// ```
        /// </remarks>
        public static string HmacSha256(string text, string secretKey)
        {
            using (var hsha256 = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)))
            {
                return Hash(hsha256, text);
            }
        }

        private static string Hash(System.Security.Cryptography.HashAlgorithm algo, string text)
        {
            text = text ?? string.Empty;
            var bytes = Encoding.UTF8.GetBytes(text);
            var hash = algo.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; i++)
            {
                var b = hash[i];
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
#else
        public static string Md5(string text)
        {
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "string.md5"));
        }

        public static string Sha1(string text)
        {
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "string.sha1"));
        }

        public static string Sha256(string text)
        {
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "string.sha256"));
        }

        public static string HmacSha1(string text, string secretKey)
        {
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "string.hmac_sha1"));
        }

        public static string HmacSha256(string text, string secretKey)
        {
            throw new NotSupportedException(string.Format(RS.UnsupportedFunctionOnPlatform, "string.sha256"));
        }
#endif
    }
}