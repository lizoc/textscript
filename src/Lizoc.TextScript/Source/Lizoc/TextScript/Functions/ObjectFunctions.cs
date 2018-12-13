using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// Object functions available through the builtin object `object`.
    /// </summary>
    public class ObjectFunctions : ScriptObject
    {
        /// <summary>
        /// The `default` value is returned if the input `value` is `null` or an empty string. A string containing whitespace characters 
        /// will not resolve to the default value.
        /// </summary>
        /// <param name="value">The input value to check if it is null or an empty string.</param>
        /// <param name="defaultValue">The default value to return if the input value is `null` or an empty string.</param>
        /// <returns>
        /// The value specified by `default` is returned if the input value is `null` or an empty string. Otherwise, the input value is 
        /// returned.
        /// </returns>
        /// <remarks>
        /// ```template-text
        /// {{ undefined_var | object.default "Yo" }}
        /// ```
        /// ```html
        /// Yo
        /// ```
        /// </remarks>
        public static object Default(object value, object defaultValue)
        {
            return value == null || (value is string && string.IsNullOrEmpty((string)value)) ? defaultValue : value;
        }

        /// <summary>
        /// Formats an object using the specified format string.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input value. The input value must implement the <see cref="IFormattable"/> interface.</param>
        /// <param name="format">The format string that defines how <paramref name="value"/> should be formatted.</param>
        /// <param name="culture">The culture as a string (e.g `en-US`). By default the culture from <see cref="TemplateContext.CurrentCulture"/> is used.</param>
        /// <remarks>
        /// ```template-text
        /// {{ 255 | object.format "X4" }}
        /// {{ 1523 | object.format "N" "fr-FR" }}
        /// ```
        /// ```html
        /// 00FF
        /// 1 523,00
        /// ```
        /// </remarks>
        public static string Format(TemplateContext context, SourceSpan span, object value, string format, string culture = null)
        {
            if (value == null)
                return string.Empty;

            format = format ?? string.Empty;
            if (!(value is IFormattable formattable))
                throw new ScriptRuntimeException(span, string.Format(RS.NotFormatObjectError, value));

            return formattable.ToString(format, culture != null ? new CultureInfo(culture) : context.CurrentCulture);
        }

        /// <summary>
        /// Checks if the specified object contains the specified property.
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <param name="key">The name of the property to check.</param>
        /// <returns>`true` if the input object contains the specified property. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ product | object.has_key "title" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// To check that the property value is not `null`, use the `has_value` function.
        /// </remarks>
        public static bool HasKey(IDictionary<string, object> value, string key)
        {
            if (value == null || key == null)
                return false;

            return value.ContainsKey(key);
        }

        /// <summary>
        /// Checks if the specified object contains the specified property, and that the property value is not `null`.
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <param name="key">The name of the property to check.</param>
        /// <returns>`true` if the input object contains the specified property and the property value is not `null`. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ product | object.has_value "title" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool HasValue(IDictionary<string, object> value, string key)
        {
            if (value == null || key == null)
                return false;

            return value.ContainsKey(key) && value[key] != null;
        }

        /// <summary>
        /// Return all property names of an object.
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <returns>A list of property names of the object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ product | object.keys | array.sort }}
        /// ```
        /// ```html
        /// [title, type]
        /// ```
        /// </remarks>
        public new static ScriptArray Keys(IDictionary<string, object> value)
        {
            return value == null ? new ScriptArray() : new ScriptArray(value.Keys);
        }

        /// <summary>
        /// Returns the size of the input object. 
        /// - If the input object is a string, it will return the length.
        /// - If the input is a list, it will return the number of elements.
        ///
        /// All unsupported types will return -1.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="value">The input object.</param>
        /// <returns>The size of the input object, or -1.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, 2, 3] | object.size }}
        /// ```
        /// ```html
        /// 3
        /// ```
        /// </remarks>
        public static int Size(TemplateContext context, SourceSpan span, object value)
        {
            if (value is string)
                return StringFunctions.Size((string)value);

            if (value is IEnumerable)
                return ArrayFunctions.Size((IEnumerable)value);

            // Should we throw an exception?
            return -1;
        }

        /// <summary>
        /// This is similar to the Powershell command `ConvertFrom-StringData`, but uses the character "`" instead 
        /// of "\"" as the escape character.
        ///
        /// For a list of characters that must be escaped, refer to 
        /// the [MSDN documentation](https://msdn.microsoft.com/library/system.text.regularexpressions.regex.unescape)
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="text">Text in the string data syntax.</param>
        /// <returns>
        /// An object representing key-value pairs of strings.
        /// </returns>
        /// <remarks>
        /// ```template-text
        /// {{ localized = include 'localization.txt' | object.from_string
        /// localized.foo
        /// }}
        /// ```
        /// ```html
        /// bar!
        /// ```
        /// </remarks>
        public static ScriptObject FromString(TemplateContext context, SourceSpan span, string text)
        {
            if (string.IsNullOrEmpty(text))
                return new ScriptObject();

            // char ` is our escape char
            // we need to escape `` so it turns out as `
            const string escapeChar = "`";
            const string reservedSequence = escapeChar + escapeChar;

            // encodeAs is just an arbitary string. you don't need to escape encodeAs but choosing 
            // something unique helps with the speed
            const string encodeAs = "^~^";

            string inputEscaped = text.Replace(encodeAs, encodeAs + "2").Replace(reservedSequence, encodeAs + "1");
            string inputRegexEscaped = inputEscaped.Replace("\\", "\\\\").Replace(escapeChar, "\\");

            ScriptObject dataObj = new ScriptObject();
            string[] textLines = inputRegexEscaped.Split('\n');
            foreach (string textLine in textLines)
            {
                if (string.IsNullOrEmpty(textLine))
                    continue;

                string line = textLine.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;
                else if (line[0] == '#')
                    continue;

                const string kvpDelimiter = "=";
                int kvpDelimiterIndex = line.IndexOf(kvpDelimiter);

                if (kvpDelimiterIndex <= 0)
                    throw new ScriptRuntimeException(span, RS.InvalidStringDataLine);

                string itemName = line.Substring(0, kvpDelimiterIndex).Trim();
                // +1 is ok, b'cos we already know there is a = somewhere
                string itemRawValue = line.Substring(kvpDelimiterIndex + 1).TrimStart();
                string itemValue = Regex.Unescape(itemRawValue).Replace(
                    encodeAs + "1", reservedSequence).Replace(
                    encodeAs + "2", encodeAs).Replace(
                    reservedSequence, escapeChar);

                if (dataObj.ContainsKey(itemName))
                    dataObj[itemName] = itemValue;
                else
                    dataObj.Add(itemName, itemValue);
            }

            return dataObj;
        }

        /// <summary>
        /// Returns a string representing the type of the input object. Supported types are:
        /// - string
        /// - boolean
        /// - number
        /// - array
        /// - iterator
        /// - object
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <remarks>
        /// ```template-text
        /// {{ null | object.typeof }}
        /// {{ true | object.typeof }}
        /// {{ 1 | object.typeof }}
        /// {{ 1.0 | object.typeof }}
        /// {{ "text" | object.typeof }}
        /// {{ 1..5 | object.typeof }}
        /// {{ [1,2,3,4,5] | object.typeof }}
        /// {{ {} | object.typeof }}
        /// {{ object | object.typeof }}
        /// ```
        /// ```html
        /// 
        /// boolean
        /// number
        /// number
        /// string
        /// iterator
        /// array
        /// object
        /// object
        /// ```
        /// </remarks>
        public static string Typeof(object value)
        {
            if (value == null)
                return null;

            Type type = value.GetType();

#if NETSTANDARD
            TypeInfo typeInfo = type.GetTypeInfo();
#else
            Type typeInfo = type.GetTypeInfo();
#endif

            if (type == typeof(string))
                return "string";

            if (type == typeof(bool))
                return "boolean";

            // We assume that we are only using int/double/long for integers and shortcut to IsPrimitive
            if (type.IsPrimitiveOrDecimal())
                return "number";

            // Test first IList, then IEnumerable
            if (typeof(IList).GetTypeInfo().IsAssignableFrom(typeInfo))
                return "array";

            if ((!typeof(ScriptObject).GetTypeInfo().IsAssignableFrom(typeInfo) && !typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeInfo)) &&
                typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return "iterator";
            }

            return "object";
        }

        /// <summary>
        /// Returns the value of each property of an object as an array.
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <returns>An array consisting of the value of each property of the input object.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ product | object.values | array.sort }}
        /// ```
        /// ```html
        /// [fruit, Orange]
        /// ```
        /// </remarks>
        public new static ScriptArray Values(IDictionary<string, object> value)
        {
            return value == null ? new ScriptArray() : new ScriptArray(value.Values);
        }
    }
}