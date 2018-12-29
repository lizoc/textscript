// -----------------------------------------------------------------------
// <copyright file="ScriptObjectExtensions.cs" repo="TextScript">
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
using System.Reflection;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime.Accessors;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Extensions attached to an <see cref="IScriptObject"/>.
    /// </summary>
    public static class ScriptObjectExtensions
    {
        /// <summary>
        /// Asserts that the specified script object is not readonly or throws a <see cref="ScriptRuntimeException"/>
        /// </summary>
        /// <param name="scriptObject">The script object.</param>
        /// <exception cref="ScriptRuntimeException">If the object is not readonly</exception>
        public static void AssertNotReadOnly(this IScriptObject scriptObject)
        {
            if (scriptObject.IsReadOnly)
                throw new InvalidOperationException(RS.ReadOnlyObjectError);
        }

        /// <summary>
        /// Imports the specified object intto this <see cref="ScriptObject"/> context. See remarks.
        /// </summary>
        /// <param name="scriptObject">The script object to import into</param>
        /// <param name="obj">The object.</param>
        /// <param name="filter">Optional member filterer</param>
        /// <param name="renamer">Optional renamer</param>
        /// <remarks>
        /// <ul>
        /// <li>If <paramref name="obj"/> is a <see cref="Type"/>, this method will import only the static field/properties of the specified object.</li>
        /// <li>If <paramref name="obj"/> is a <see cref="ScriptObject"/>, this method will import the members of the specified object into the new object.</li>
        /// <li>If <paramref name="obj"/> is a plain object, this method will import the public fields/properties of the specified object into the <see cref="ScriptObject"/>.</li>
        /// </ul>
        /// </remarks>
        public static void Import(this IScriptObject scriptObject, object obj, MemberFilterDelegate filter = null, MemberRenamerDelegate renamer = null)
        {
            if (obj is IScriptObject)
            {
                // TODO: Add support for filter, member renamer
                scriptObject.Import((IScriptObject)obj);
                return;
            }

            if (obj is IDictionary)
            {
                // TODO: Add support for filter, member renamer
                scriptObject.ImportDictionary((IDictionary)obj);
                return;
            }

            scriptObject.Import(obj, ScriptMemberImportFlags.All, filter, renamer);
        }

        public static bool TryGetValue(this IScriptObject scriptObject, string key, out object value)
        {
            return scriptObject.TryGetValue(null, new SourceSpan(), key, out value);
        }

        /// <summary>
        /// Tries to set the value and readonly state of the specified member.
        /// </summary>
        /// <param name="scriptObject">The script object</param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        /// <returns><c>true</c> if the value could be set; <c>false</c> if a value already exist an is readonly</returns>
        public static bool TrySetValue(this IScriptObject scriptObject, string member, object value, bool readOnly)
        {
            if (!scriptObject.CanWrite(member))
                return false;

            scriptObject.SetValue(null, new SourceSpan(), member, value, readOnly);
            return true;
        }

        public static void SetValue(this IScriptObject scriptObject, string member, object value, bool readOnly)
        {
            scriptObject.SetValue(null, new SourceSpan(), member, value, readOnly);
        }

        /// <summary>
        /// Imports the specified <see cref="ScriptObject"/> into this instance by copying the member values into this object.
        /// </summary>
        /// <param name="scriptObject">The script object to import into</param>
        /// <param name="other">The other <see cref="ScriptObject"/>.</param>
        public static void Import(this IScriptObject scriptObject, IScriptObject other)
        {
            if (other == null)
                return;

            ScriptObject thisScript = scriptObject.GetScriptObject();
            AssertNotReadOnly(thisScript);
            ScriptObject otherScript = other.GetScriptObject();

            foreach (var keyValue in otherScript.Store)
            {
                string member = keyValue.Key;
                if (!thisScript.CanWrite(member))
                    continue;

                thisScript.Store[keyValue.Key] = keyValue.Value;
            }
        }

        private static void ImportDictionary(this IScriptObject scriptObject, IDictionary dictionary)
        {
            if (dictionary == null)
                return;

            foreach (DictionaryEntry entry in dictionary)
            {
                string member = entry.Key?.ToString();
                if (!scriptObject.CanWrite(member))
                    continue;

                ScriptObject thisScript = scriptObject.GetScriptObject();
                AssertNotReadOnly(thisScript);
                thisScript[member] = entry.Value;
            }
        }

        /// <summary>
        /// Gets the script object attached to the specified instance.
        /// </summary>
        /// <param name="scriptObject">The script object proxy.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Expecting ScriptObject or ScriptArray instance</exception>
        public static ScriptObject GetScriptObject(this IScriptObject scriptObject)
        {
            var script = scriptObject as ScriptObject;
            if (script == null)
            {
                var scriptArray = scriptObject as ScriptArray;
                if (scriptArray == null)
                    throw new ArgumentException(RS.ScriptObjectOrArrayOnly, nameof(scriptObject));

                script = scriptArray.ScriptObject;
            }
            return script;
        }


        /// <summary>
        /// Imports a specific member from the specified object.
        /// </summary>
        /// <param name="scriptObject">The script object to import into</param>
        /// <param name="obj">The object.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="exportName">Name of the member name replacement. If null, use the default renamer will be used.</param>
        public static void ImportMember(this IScriptObject scriptObject, object obj, string memberName, string exportName = null)
        {
            scriptObject.Import(obj, ScriptMemberImportFlags.All, member => member.Name == memberName, exportName != null ? name => exportName: (MemberRenamerDelegate)null);
        }

        /// <summary>
        /// Imports the specified object.
        /// </summary>
        /// <param name="scriptObject">The script object to import into</param>
        /// <param name="obj">The object.</param>
        /// <param name="flags">The import flags.</param>
        /// <param name="filter">A filter applied on each member</param>
        /// <param name="renamer">The member renamer.</param>
        /// <exception cref="ArgumentOutOfRangeException">The object is not importable.</exception>
        public static void Import(this IScriptObject scriptObject, object obj, ScriptMemberImportFlags flags, MemberFilterDelegate filter = null, MemberRenamerDelegate renamer = null)
        {
            if (obj == null)
                return;

            if (!ScriptObject.IsImportable(obj))
                throw new ArgumentOutOfRangeException(nameof(obj), string.Format(RS.InvalidObjectType, obj.GetType()));

#if NETSTANDARD
            TypeInfo typeInfo = (obj as Type ?? obj.GetType()).GetTypeInfo();
#else
            Type typeInfo = (obj as Type ?? obj.GetType()).GetTypeInfo();
#endif

            bool useStatic = false;
            bool useInstance = false;
            if (obj is Type)
            {
                useStatic = true;
                obj = null;
            }
            else
            {
                useInstance = true;
            }

            renamer = renamer ?? StandardMemberRenamer.Default;

            while (typeInfo != null)
            {
                if ((flags & ScriptMemberImportFlags.Field) != 0)
                {
                    foreach (FieldInfo field in typeInfo.GetDeclaredFields())
                    {
                        if (!field.IsPublic)
                            continue;
                        if (filter != null && !filter(field))
                            continue;

                        bool keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                        if (keep && ((field.IsStatic && useStatic) || useInstance))
                        {
                            string newFieldName = renamer(field);
                            if (string.IsNullOrEmpty(newFieldName))
                                newFieldName = field.Name;

                            // If field is init only or literal, it cannot be set back so we mark it as read-only
                            scriptObject.SetValue(null, new SourceSpan(), newFieldName, field.GetValue(obj), field.IsInitOnly || field.IsLiteral);
                        }
                    }
                }

                if ((flags & ScriptMemberImportFlags.Property) != 0)
                {
                    foreach (PropertyInfo property in typeInfo.GetDeclaredProperties())
                    {
                        // Workaround with .NET Core, extension method is not working (retuning null)
#if NETFX
                        MethodInfo getMethod = property.GetGetMethod();
#else
                        MethodInfo getMethod = property.GetMethod;
#endif

                        if (!property.CanRead || !getMethod.IsPublic)
                            continue;

                        if (filter != null && !filter(property))
                            continue;

                        bool keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                        if (keep && ((getMethod.IsStatic && useStatic) || useInstance))
                        {
                            string newPropertyName = renamer(property);
                            if (string.IsNullOrEmpty(newPropertyName))
                                newPropertyName = property.Name;

                            // Initially, we were setting readonly depending on the precense of a set method, but this is not compatible with liquid implems, so we remove readonly restriction
                            //script.SetValue(null, new SourceSpan(), newPropertyName, property.GetValue(obj), property.GetSetMethod() == null || !property.GetSetMethod().IsPublic);
                            scriptObject.SetValue(null, new SourceSpan(), newPropertyName, property.GetValue(obj), false);
                        }
                    }
                }

                if ((flags & ScriptMemberImportFlags.Method) != 0 && useStatic)
                {
                    foreach (MethodInfo method in typeInfo.GetDeclaredMethods())
                    {
                        if (filter != null && !filter(method))
                            continue;

                        bool keep = method.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                        if (keep && method.IsPublic && method.IsStatic && !method.IsSpecialName)
                        {
                            string newMethodName = renamer(method);
                            if (string.IsNullOrEmpty(newMethodName))
                                newMethodName = method.Name;

                            scriptObject.SetValue(null, new SourceSpan(), newMethodName, DynamicCustomFunction.Create(obj, method), true);
                        }
                    }
                }

                if (typeInfo.BaseType == typeof(object))
                    break;

                typeInfo = typeInfo.BaseType.GetTypeInfo();
            }
        }

        /// <summary>
        /// Imports the delegate to the specified member.
        /// </summary>
        /// <param name="scriptObject">The script object to import into</param>
        /// <param name="member">The member.</param>
        /// <param name="function">The function delegate.</param>
        /// <exception cref="ArgumentNullException">if member or function are null</exception>
        public static void Import(this IScriptObject scriptObject, string member, Delegate function)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            scriptObject.SetValue(null, new SourceSpan(), member, DynamicCustomFunction.Create(function.Target, function.GetMethodInfo()), true);
        }
    }
}