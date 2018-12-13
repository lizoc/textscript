// -----------------------------------------------------------------------
// <copyright file="MemberFilterDelegate.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Allows to filter a member while importing a .NET object into a ScriptObject or while exposing a .NET instance through a ScriptObject, by returning <c>true</c> to keep the member; or false to discard it.
    /// </summary>
    /// <param name="member">A member info</param>
    /// <returns><c>true</c> to keep the member; otherwise <c>false</c> to remove the member</returns>
    public delegate bool MemberFilterDelegate(MemberInfo member);
}