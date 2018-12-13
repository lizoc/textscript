using System;

namespace Lizoc.TextScript.Runtime
{
    [AttributeUsage(AttributeTargets.Field| AttributeTargets.Property|AttributeTargets.Method)]
    public class ScriptMemberIgnoreAttribute : Attribute
    {
    }
}