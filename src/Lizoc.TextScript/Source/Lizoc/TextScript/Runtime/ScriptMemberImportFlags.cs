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