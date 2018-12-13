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