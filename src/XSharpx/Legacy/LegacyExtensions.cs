using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSharpx.Legacy
{
    /**
     *  For dealing with unsafe APIs 
     **/

    public static class LegacyExtensions
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
                return Option.Empty;
        }

        //Named ToXList so we don't override IEnumerable's ToList
        public static List<A> ToXList<A>(this A[] a)
        {
            if (a == null)
                return List<A>.Empty;
            else
                return List<A>.list(a);
        }

        public static Option<A> ToOption<A>(this A a)
        {
            if (a == null)
                return Option<A>.Empty;
            else
                return Option<A>.Some(a);
        }
    }
}
