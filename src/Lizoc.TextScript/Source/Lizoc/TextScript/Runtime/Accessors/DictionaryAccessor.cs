// -----------------------------------------------------------------------
// <copyright file="DictionaryAccessor.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Runtime.Accessors
{
    public sealed class DictionaryAccessor : IObjectAccessor
    {
        public static readonly DictionaryAccessor Default = new DictionaryAccessor();

        private DictionaryAccessor()
        {
        }


        public static bool TryGet(Type type, out IObjectAccessor accessor)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                accessor = Default;
                return true;
            }

            Type dictionaryType = type.GetBaseOrInterface(typeof(IDictionary<,>));
            accessor = null;
            if (dictionaryType == null)
                return false;
            Type keyType = dictionaryType.GetTypeInfo().GetGenericArguments()[0];
            Type valueType = dictionaryType.GetTypeInfo().GetGenericArguments()[1];

            Type accessorType = typeof(GenericDictionaryAccessor<,>).GetTypeInfo().MakeGenericType(keyType, valueType);
            accessor = (IObjectAccessor)Activator.CreateInstance(accessorType);
            return true;
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return ((IDictionary) target).Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            foreach (var key in ((IDictionary) target).Keys)
            {
                yield return context.ToString(span, key);
            }
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return ((IDictionary) target).Contains(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;
            if (((IDictionary) target).Contains(member))
            {
                value = ((IDictionary)target)[member];
                return true;
            }
            return false;
        }
        
        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            ((IDictionary) target)[member] = value;
            return true;
        }
    }

    internal class GenericDictionaryAccessor<TKey, TValue> : IObjectAccessor
    {
        public GenericDictionaryAccessor()
        {
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return ((IDictionary<TKey, TValue>)target).Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            foreach (var key in ((IDictionary<TKey, TValue>)target).Keys)
            {
                yield return context.ToString(span, key);
            }
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object value, string member)
        {
            return ((IDictionary<TKey, TValue>) value).ContainsKey(TransformToKey(context, member));
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            TValue tvalue;
            bool result = ((IDictionary<TKey, TValue>) target).TryGetValue(TransformToKey(context, member), out tvalue);
            value = tvalue;
            return result;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            ((IDictionary<TKey, TValue>) value)[TransformToKey(context, member)] = (TValue)value;
            return true;
        }

        private TKey TransformToKey(TemplateContext context, string member)
        {
            return (TKey)context.ToObject(new SourceSpan(), member, typeof(TKey));
        }
    }
}