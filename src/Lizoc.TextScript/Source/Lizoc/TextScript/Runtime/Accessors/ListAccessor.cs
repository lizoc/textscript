// -----------------------------------------------------------------------
// <copyright file="ListAccessor.cs" repo="TextScript">
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

using System.Collections;
using System.Collections.Generic;
using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Runtime.Accessors
{
    public class ListAccessor : IListAccessor, IObjectAccessor
    {
        public static ListAccessor Default = new ListAccessor();

        private ListAccessor()
        {
        }

        public int GetLength(TemplateContext context, SourceSpan span, object target)
        {
            return ((IList) target).Count;
        }

        public object GetValue(TemplateContext context, SourceSpan span, object target, int index)
        {
            IList list = ((IList)target);
            if (index < 0 || index >= list.Count)
                return null;

            return list[index];
        }

        public void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value)
        {
            IList list = ((IList)target);
            if (index < 0)
                return;

            // Auto-expand the array in case of accessing a range outside the current value
            for (int i = list.Count; i <= index; i++)
            {
                // TODO: If the array doesn't support null value, we shoud add a default value or throw an error?
                list.Add(null);
            }

            list[index] = value;
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            // size
            return 1;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            yield return "size";
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return member == "size";
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            if (member == "size")
            {
                value = GetLength(context, span, target);
                return true;
            }

            if (target is IScriptObject)
                return (((IScriptObject) target)).TryGetValue(context, span, member, out value);

            value = null;
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            if (member == "size")
                return false;

            if (target is IScriptObject)
                return (((IScriptObject)target)).TryGetValue(context, span, member, out value);

            return false;
        }
    }
}