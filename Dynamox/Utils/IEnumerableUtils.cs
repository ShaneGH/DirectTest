using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class IEnumerableUtils
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> input, Action<T, int> action)
        {
            return input.Select((x, i) =>
            {
                action(x, i);
                return x;
            }).ToArray();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            return ForEach(input, (a, b) => action(a));
        }
    }
}