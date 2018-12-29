﻿// -----------------------------------------------------------------------
// <copyright file="ScriptMemberImportFlags.cs" repo="TextScript">
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