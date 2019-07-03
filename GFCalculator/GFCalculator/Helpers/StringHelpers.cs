using System.Collections.Generic;
using System.Linq;

namespace GFCalculator.Helpers
{
    public static class StringHelpers
    {
        public static int[] FindAllIndexOf(this string values, string val)
        {
            return values.ToCharArray().Select(o => o.ToString()).FindAllIndexOf(val);
        }

        public static int[] FindAllIndexOf(this string values, char val)
        {
            return values.ToCharArray().FindAllIndexOf(val);
        }

        public static int[] FindAllIndexOf<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }
    }
}
