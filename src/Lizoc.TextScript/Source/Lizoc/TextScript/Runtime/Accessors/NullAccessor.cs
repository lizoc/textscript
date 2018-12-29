// -----------------------------------------------------------------------
// <copyright file="NullAccessor.cs" repo="TextScript">
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

using System.Collections.Generic;
using Lizoc.TextScript.Parsing;

namespace Lizoc.TextScript.Runtime.Accessors
{
    public class NullAccessor : IObjectAccessor
    {
        public static readonly NullAccessor Default = new NullAccessor();

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return 0;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            yield break;
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return false;
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;
            return false;
        }
       
        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            return false;
        }
    }
}