using System;

namespace Lizoc.TextScript.Parsing
{
    internal static class CharExtensions
    {
        public static bool IsHex(this char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }

        public static int HexToInt(this char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }
            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }
            throw new ArgumentOutOfRangeException(nameof(c), string.Format(RS.NotHexIntChar, c));
        }
    }
}