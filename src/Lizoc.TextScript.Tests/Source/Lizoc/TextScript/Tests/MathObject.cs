// -----------------------------------------------------------------------
// <copyright file="MathObject.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Lizoc.TextScript.Tests
{
    public static class MathObject
    {
        public const double PI = Math.PI;

        public static double Cos(double value)
        {
            return Math.Cos(value);
        }

        public static double Sin(double value)
        {
            return Math.Sin(value);
        }

        public static double Round(double value)
        {
            return Math.Round(value);
        }
    }
}