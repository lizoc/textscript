// -----------------------------------------------------------------------
// <copyright file="StringHelper.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.Text;

namespace Lizoc.TextScript
{
    internal class StringHelper
    {
        public static string Join(string separator, IEnumerable items)
        {
            StringBuilder builder = new StringBuilder();
            bool isFirst = true;
            foreach (var item in items)
            {
                if (!isFirst)
                    builder.Append(separator);

                builder.Append(item);
                isFirst = false;
            }

            return builder.ToString();
        }
    }
}