using System;

namespace XSharpx
{
    public struct Monoid<A>
    {
        private readonly Func<A, A, A> op;
        private readonly A id;

        private Monoid(Func<A, A, A> op, A id)
        {
            this.op = op;
            this.id = id;
        }

        public Semigroup<A> Semigroup => Semigroup<A>.semigroup(op);

        public Func<A, A, A> Op => op;

        public A Id => id;

        public A Apply(A a1, A a2) => Semigroup.Apply(a1, a2);

        public Func<A, Func<A, A>> Curried => Semigroup.Curried;

        public Func<A, A> Curried1(A a1) => Semigroup.Curried1(a1);

        public Monoid<A> Dual => Semigroup.Dual.Monoid(id);

        public Func<A, A> Join => Semigroup.Join;

        public Monoid<Pair<A, B>> Pair<B>(Monoid<B> s) => Semigroup.Pair(s.Semigroup).Monoid(id.And(s.Id));

        public Monoid<Option<A>> Option => Semigroup.Option.Monoid(Option<A>.Empty);

        public Monoid<Input<A>> Input => Semigroup.Input.Monoid(Input<A>.Empty());

        public Monoid<Func<B, A>> Pointwise<B>()
        {
            var t = this;
            return Semigroup.Pointwise<B>().Monoid(_ => t.id);
        }

        public Monoid<Func<B, C, A>> Pointwise2<B, C>()
        {
            var t = this;
            return Semigroup.Pointwise2<B, C>().Monoid((_, __) => t.id);
        }

        public Monoid<Func<B, C, D, A>> Pointwise3<B, C, D>()
        {
            var t = this;
            return Semigroup.Pointwise3<B, C, D>().Monoid((_, __, ___) => t.id);
        }

        public Monoid<B> XSelect<B>(Func<A, B> f, Func<B, A> g)
        {
            return Semigroup.XSelect(f, g).Monoid(f(id));
        }

        public static Monoid<A> monoid(Func<A, A, A> op, A id) => new Monoid<A>(op, id);

        public static Monoid<Option<A>> FirstOption => Semigroup<A>.FirstOption.Monoid(Option<A>.Empty);

        public static Monoid<Option<A>> SecondOption => Semigroup<A>.SecondOption.Monoid(Option<A>.Empty);

        public static Monoid<Func<A, A>> Endo => Semigroup<A>.Endo.Monoid(a => a);

        public static Monoid<List<A>> List => Semigroup<A>.List.Monoid(List<A>.Empty);

    }

    public static class Monoid
    {
        public static Monoid<bool> Or => Semigroup.Or.Monoid(false);

        public static Monoid<bool> And => Semigroup.And.Monoid(true);

        public static Monoid<string> String => Semigroup.String.Monoid("");

        public static class Sum
        {
            public static Monoid<int> Integer => Semigroup.Sum.Integer.Monoid(0);

            public static Monoid<byte> Byte => Semigroup.Sum.Byte.Monoid(0);

            public static Monoid<short> Short => Semigroup.Sum.Short.Monoid(0);

            public static Monoid<long> Long => Semigroup.Sum.Long.Monoid(0);
            public static Monoid<char> Char => Semigroup.Sum.Char.Monoid((char)0);

        }

        public static class Product
        {
            public static Monoid<int> Integer => Semigroup.Product.Integer.Monoid(1);

            public static Monoid<byte> Byte => Semigroup.Product.Byte.Monoid(1);

            public static Monoid<short> Short => Semigroup.Product.Short.Monoid(1);

            public static Monoid<long> Long => Semigroup.Product.Long.Monoid(1);

            public static Monoid<char> Char => Semigroup.Product.Char.Monoid((char)1);

        }
    }
}

