using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive.Journey.Tests.Utilities
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> s, string delimiter)
        {
            return string.Join(delimiter, s);
        }

        public static bool ContainsAll(this string s, params string[] expectedSubstrings)
        {
            foreach (var substring in expectedSubstrings)
            {
                if (!s.Contains(substring))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
