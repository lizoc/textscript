// -----------------------------------------------------------------------
// <copyright file="MethodImplOptionsHelper.cs" repo="TextScript">
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

using System.Runtime.CompilerServices;

namespace Lizoc.TextScript
{
    /// <summary>
    /// Internal helper to allow to declare a method using AggressiveInlining without being .NET 4.0+
    /// </summary>
    internal static class MethodImplOptionsHelper
    {
        public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;
    }
}