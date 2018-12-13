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