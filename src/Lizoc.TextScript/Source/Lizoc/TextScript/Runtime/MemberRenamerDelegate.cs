// -----------------------------------------------------------------------
// <copyright file="MemberRenamerDelegate.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Allows to rename a member.
    /// </summary>
    /// <param name="member">A member info</param>
    /// <returns>The new name name of member</returns>
    public delegate string MemberRenamerDelegate(MemberInfo member);
}