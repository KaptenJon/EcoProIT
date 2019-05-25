using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelpClasses.Annotations;

namespace HelpClasses
{
    public static class Sumtions
    {


        public static T Sum<T>(this IEnumerable<T> source, object selector) where T : struct
        {
            return source.Aggregate(default(T), (current, number) => (dynamic)current + number);
        }
        public static T? Sum<T>(this IEnumerable<T?> source, object selector) where T : struct
        {
            return source.Where(nullable => nullable.HasValue)
                         .Aggregate(
                             default(T),
                             (current, nullable) => (dynamic)current + nullable.GetValueOrDefault());
        }
        public static V Sum<T, V>(this IEnumerable<T> source, Func<T, V> selector, object selector1) where V : struct
        {
            return source.Select(selector).Sum(selector1);
        }
        public static V? Sum<T, V>(this IEnumerable<T> source, Func<T, V?> selector, object selector1) where V : struct
        {
            return source.Select(selector).Sum(selector1);
        }

    }
}
