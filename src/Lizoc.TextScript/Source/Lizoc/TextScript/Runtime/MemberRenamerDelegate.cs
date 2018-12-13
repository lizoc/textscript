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