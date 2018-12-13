namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Type of path.
    /// </summary>
    public enum PathType
    {
        /// <summary>
        /// Either container or leaf.
        /// </summary>
        Any,

        /// <summary>
        /// A container that may contain sub-containers or leafs.
        /// </summary>
        Container,

        /// <summary>
        /// A leaf is the smallest unit.
        /// </summary>
        Leaf
    }
}
