// -----------------------------------------------------------------------
// <copyright file="ScriptSyntaxAttribute.cs" repo="TextScript">
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
using System.Reflection;

namespace Lizoc.TextScript.Syntax
{
    public class ScriptSyntaxAttribute : Attribute
    {
        private ScriptSyntaxAttribute()
        {
        }

        public ScriptSyntaxAttribute(string name, string example)
        {
            Name = name;
            Example = example;
        }

        public string Name { get; }

        public string Example { get; }

        public static ScriptSyntaxAttribute Get(object obj)
        {
            if (obj == null)
                return null;

            return Get(obj.GetType());
        }

        public static ScriptSyntaxAttribute Get(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ScriptSyntaxAttribute attribute = type.GetTypeInfo().GetCustomAttribute<ScriptSyntaxAttribute>() ?? new ScriptSyntaxAttribute(type.Name, "...");

            return attribute;
        }
    }
}