// -----------------------------------------------------------------------
// <copyright file="BuiltinFunctions.cs" repo="TextScript">
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

using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Functions
{
    public class BuiltinFunctions : ScriptObject
    {
        /// <summary>
        /// This object is readonly, should not be modified by any other objects internally.
        /// </summary>
        internal static readonly ScriptObject Default = new DefaultBuiltins();

        public BuiltinFunctions() : base(10)
        {
            ((ScriptObject)Default.Clone(true)).CopyTo(this);
        }

        /// <summary>
        /// Use an internal object to create all default builtins just once to avoid allocations of delegates/IScriptCustomFunction
        /// </summary>
        private class DefaultBuiltins : ScriptObject
        {
            public DefaultBuiltins() : base(10, false)
            {
                SetValue("array", new ArrayFunctions(), true);
                SetValue("empty", EmptyScriptObject.Default, true);
                SetValue("include", new IncludeFunction(), true);
                SetValue("where", new WhereFunction(), true);
                SetValue(DateTimeFunctions.DateVariable.Name, new DateTimeFunctions(), true);
                SetValue("html", new HtmlFunctions(), true);
                SetValue("math", new MathFunctions(), true);
                SetValue("object", new ObjectFunctions(), true);
                SetValue("regex", new RegexFunctions(), true);
                SetValue("string", new StringFunctions(), true);
                SetValue("timespan", new TimeSpanFunctions(), true);
                SetValue("fs", new FileSystemFunctions(), true);
            }
        }
    }
}