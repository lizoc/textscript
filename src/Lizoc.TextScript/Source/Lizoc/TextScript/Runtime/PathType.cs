// -----------------------------------------------------------------------
// <copyright file="PathType.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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
