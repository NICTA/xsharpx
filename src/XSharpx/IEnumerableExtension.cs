using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSharpx
{
    public static class IEnumerableExtension
    {
        public static Option<A> FirstOption<A>(this IEnumerable<A> ie)
        {
            if (ie.Any())
                return Option.Some(ie.First());
            else
                return Option.Empty;
        }

        public static Option<A> LastOption<A>(this IEnumerable<A> ie)
        {
            if (ie.Any())
                return Option.Some(ie.Last());
            else
            {
                return Option.Empty;
            }
        }
    }
}
