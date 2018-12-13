using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.TextScript.Tests
{
    public class EqualityCompareResult
    {
        public bool IsEqual { get; set; }

        public List<string> VerboseMessage { get; set; }
    }

    /// <summary>
    /// Pretty text assert from https://gist.github.com/Haacked/1610603
    /// Modified version to only print +-10 characters around the first diff
    /// </summary>
    public static class TextAssert
    {
        public enum DiffStyle
        {
            Full,
            Minimal
        }

        public static EqualityCompareResult Equal(string expectedValue, string actualValue)
        {
            return Equal(expectedValue, actualValue, DiffStyle.Full);
        }

        public static EqualityCompareResult Equal(string expectedValue, string actualValue, DiffStyle diffStyle)
        {
            if (actualValue == null || expectedValue == null)
            {
                return new EqualityCompareResult()
                {
                    IsEqual = (expectedValue == actualValue)
                };
            }

            if (actualValue.Equals(expectedValue, StringComparison.Ordinal))
            {
                return new EqualityCompareResult()
                {
                    IsEqual = true
                };
            }

            List<string> verboseMessage = new List<string>();

            verboseMessage.Add("Index    Expected     Actual");
            verboseMessage.Add("----------------------------");
            int maxLen = Math.Max(actualValue.Length, expectedValue.Length);
            int minLen = Math.Min(actualValue.Length, expectedValue.Length);

            if (diffStyle != DiffStyle.Minimal)
            {
                int startDifferAt = 0;
                for (int i = 0; i < maxLen; i++)
                {
                    if (i >= minLen || actualValue[i] != expectedValue[i])
                    {
                        startDifferAt = i;
                        break;
                    }
                }

                var endDifferAt = Math.Min(startDifferAt + 10, maxLen);
                startDifferAt = Math.Max(startDifferAt - 10, 0);

                bool isFirstDiff = true;
                for (int i = startDifferAt; i < endDifferAt; i++)
                {
                    if (i >= minLen || actualValue[i] != expectedValue[i])
                    {
                        verboseMessage.Add(string.Format("{0,-3} {1,-3}    {2,-4} {3,-3}   {4,-4} {5,-3}",
                            i < minLen && actualValue[i] == expectedValue[i] ? " " : isFirstDiff ? ">>>" : "***",
                            // put a mark beside a differing row
                            i, // the index
                            i < expectedValue.Length ? ((int)expectedValue[i]).ToString() : "",
                            // character decimal value
                            i < expectedValue.Length ? expectedValue[i].ToSafeString() : "", // character safe string
                            i < actualValue.Length ? ((int)actualValue[i]).ToString() : "", // character decimal value
                            i < actualValue.Length ? actualValue[i].ToSafeString() : "" // character safe string
                            ));

                        isFirstDiff = false;
                    }
                }
            }

            return new EqualityCompareResult()
            {
                IsEqual = (expectedValue == actualValue),
                VerboseMessage = verboseMessage
            };
        }

        private static string ToSafeString(this char c)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
            {
                switch (c)
                {
                    case '\b':
                        return @"\b";
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\t':
                        return @"\t";
                    case '\a':
                        return @"\a";
                    case '\v':
                        return @"\v";
                    case '\f':
                        return @"\f";
                    default:
                        return $"\\u{(int)c:X};";
                }
            }
            return c.ToString(CultureInfo.InvariantCulture);
        }
    }
}