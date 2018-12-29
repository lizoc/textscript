// -----------------------------------------------------------------------
// <copyright file="TypedObjectAccessor.cs" repo="TextScript">
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

namespace Lizoc.TextScript.Runtime.Accessors
{
    public class TypedObjectAccessor : IObjectAccessor
    {
        private readonly MemberFilterDelegate _filter;
        private readonly Type _type;
        private readonly MemberRenamerDelegate _renamer;
        private readonly Dictionary<string, MemberInfo> _members;

        public TypedObjectAccessor(Type targetType, MemberFilterDelegate filter, MemberRenamerDelegate renamer)
        {
            _type = targetType ?? throw new ArgumentNullException(nameof(targetType));
            _filter = filter;
            _renamer = renamer ?? StandardMemberRenamer.Default;
            _members = new Dictionary<string, MemberInfo>();
            PrepareMembers();
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return _members.Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            return _members.Keys;
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return _members.ContainsKey(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;
            MemberInfo memberAccessor;
            if (_members.TryGetValue(member, out memberAccessor))
            {
                var fieldAccessor = memberAccessor as FieldInfo;
                if (fieldAccessor != null)
                {
                    value = fieldAccessor.GetValue(target);
                    return true;
                }

                PropertyInfo propertyAccessor = (PropertyInfo)memberAccessor;
                value = propertyAccessor.GetValue(target);
                return true;
            }
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            MemberInfo memberAccessor;
            if (_members.TryGetValue(member, out memberAccessor))
            {
                var fieldAccessor = memberAccessor as FieldInfo;
                if (fieldAccessor != null)
                {
                    fieldAccessor.SetValue(target, value);
                }
                else
                {
                    PropertyInfo propertyAccessor = (PropertyInfo)memberAccessor;
                    propertyAccessor.SetValue(target, value);
                }
            }
            return true;
        }

        private void PrepareMembers()
        {
#if NETSTANDARD
            TypeInfo type = this._type.GetTypeInfo();
#else
            Type type = this._type.GetTypeInfo();
#endif

            while (type != null)
            {
                foreach (FieldInfo field in type.GetDeclaredFields())
                {
                    bool keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && !field.IsStatic && field.IsPublic && (_filter == null || _filter(field)))
                    {
                        string newFieldName = Rename(field);
                        if (string.IsNullOrEmpty(newFieldName))
                            newFieldName = field.Name;

                        if (!_members.ContainsKey(newFieldName))
                            _members.Add(newFieldName, field);
                    }
                }

                foreach (PropertyInfo property in type.GetDeclaredProperties())
                {
                    bool keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;

                    // workaround with .NETCore: extension method is not working (returning null)
#if NETFX
                    MethodInfo getMethod = property.GetGetMethod();
#else
                    MethodInfo getMethod = property.GetMethod;
#endif

                    if (keep && 
                        property.CanRead && 
                        !getMethod.IsStatic && 
                        getMethod.IsPublic && 
                        (_filter == null || _filter(property)))
                    {
                        string newPropertyName = Rename(property);
                        if (string.IsNullOrEmpty(newPropertyName))
                            newPropertyName = property.Name;

                        if (!_members.ContainsKey(newPropertyName))
                            _members.Add(newPropertyName, property);
                    }
                }

                if (type.BaseType == typeof(object))
                    break;

                type = type.BaseType.GetTypeInfo();
            }
        }

        private string Rename(MemberInfo member)
        {
            return _renamer(member);
        }
    }
}