using System;
using System.Collections.Generic;
using System.Linq;

namespace Util
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<B> SelectWhere<A,B>(this IEnumerable<A> enumerable, Func<A,B?> selector)
        where B : struct
        {
            return enumerable.Select(selector).Where(_ => _.HasValue).Select(_ => _.Value);
        }

        public static T PopRandom<T>(this List<T> l)
        {
            var index = UnityEngine.Random.Range(0, l.Count);
            var result = l[index];
            l.RemoveAt(index);
            return result;
        }
    }
}