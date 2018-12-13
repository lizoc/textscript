// -----------------------------------------------------------------------
// <copyright file="ScriptMemberImportFlags.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Lizoc.TextScript.Runtime
{
    [Flags]
    public enum ScriptMemberImportFlags
    {
        Field = 1,

        Property = 2,

        Method = 4,

        [Obsolete("Importing Method Instance is actually not supported - This flag will be removed in a future release")]
        MethodInstance = 8,

        All = Field | Property | Method
    }
}