using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> s, string delimiter)
        {
            return string.Join(delimiter, s);
        }
    }
}
