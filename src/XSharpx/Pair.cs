using System;

namespace XSharpx {
  /// <summary>
  /// The conjunction of 2 values.
  /// </summary>
  /// <typeparam name="A">The element type of one of the two values.</typeparam>
  /// <typeparam name="B">The element type of one of the two values.</typeparam>
  /// <remarks>Also known as a pair.</remarks>
  public struct Pair<A, B> {
    private readonly A a;
    private readonly B b;

    private Pair(A a, B b) {
      this.a = a;
      this.b = b;
    }

    public Store<A, Pair<A, B>> _1 {
      get {
        var t = this;
        return a.StoreSet(aa => aa.And(t.b));
      }
    }

    public Store<B, Pair<A, B>> _2 {
      get {
        var t = this;
        return b.StoreSet(bb => t.a.And(bb));
      }
    }

    public Pair<A, X> Select<X>(Func<B, X> f) {
      return a.And(f(b));
    }

    public Pair<A, Pair<A, B>> Duplicate {
      get {
        return a.And(a.And(b));
      }
    }

    public Pair<A, X> Extend<X>(Func<Pair<A, B>, X> f) {
      return a.And(f(this));
    }

    public Pair<X, B> First<X>(Func<A, X> f) {
      return f(a).And(b);
    }

    public Pair<A, X> Second<X>(Func<B, X> f) {
      return a.And(f(b));
    }

    public Pair<X, Y> BinarySelect<X, Y>(Func<A, X> f, Func<B, Y> g) {
      return f(a).And(g(b));
    }

    public Pair<B, A> Swap {
      get {
        return b.And(a);
      }
    }

    public PairAndSemigroup<A, B> Constrain(Semigroup<A> s) {
      return new PairAndSemigroup<A, B>(this, s);
    }

    public PairAndMonoid<A, B> Constrain(Monoid<A> m) {
      return new PairAndMonoid<A, B>(this, m);
    }

    public Pair<X, Y> Swapped<X, Y>(Func<Pair<B, A>, Pair<Y, X>> f) {
      return f(Swap).Swap;
    }

    public X Fold<X>(Func<A, B, X> f) {
      return f(a, b);
    }

    public static Pair<A, B> pair(A a, B b) {
      return new Pair<A, B>(a, b);
    }

    public static Func<A, Func<B, Pair<A, B>>> pairF() {
      return a => b => new Pair<A, B>(a, b);
    }
  }

  public static class PairExtension {
    public static Pair<X, B> Select<X, A, B>(this Pair<X, A> ps, Func<A, B> f) {
      return ps._1.Get.And(f(ps._2.Get));
    }

    public static Pair<A, B> And<A, B>(this A a, B b) {
      return Pair<A, B>.pair(a, b);
    }
  }

  public struct PairAndSemigroup<A, B> {
    private readonly Pair<A, B> pair;
    private readonly Semigroup<A> s;

    internal PairAndSemigroup(Pair<A, B> pair, Semigroup<A> s) {
      this.pair = pair;
      this.s = s;
    }

    public A a {
      get {
        return pair._1.Get;
      }
    }

    public B b {
      get {
        return pair._2.Get;
      }
    }

    internal Semigroup<A> S {
      get {
        return s;
      }
    }

    public Pair<A, B> Pair {
      get {
        return pair;
      }
    }

    public PairAndSemigroup<A, D> ZipWith<C, D>(PairAndSemigroup<A, C> o, Func<B, Func<C, D>> f) {
      return this.SelectMany(a => o.Select(b => f(a)(b)));
    }

    public PairAndSemigroup<A, Pair<B, C>> Zip<C>(PairAndSemigroup<A, C> o) {
      return ZipWith<C, Pair<B, C>>(o, Pair<B, C>.pairF());
    }

    public Pair<X, B> First<X>(Func<A, X> f) {
      return pair.First(f);
    }

    public PairAndSemigroup<A, X> Second<X>(Func<B, X> f) {
      return new PairAndSemigroup<A, X>(pair.Second(f), s);
    }

    public Pair<X, Y> BinarySelect<X, Y>(Func<A, X> f, Func<B, Y> g) {
      return pair.BinarySelect(f, g);
    }

    public Pair<B, A> Swap {
      get {
        return pair.Swap;
      }
    }

    public Pair<X, Y> Swapped<X, Y>(Func<Pair<B, A>, Pair<Y, X>> f) {
      return pair.Swapped(f);
    }

    public X Fold<X>(Func<A, B, X> f) {
      return pair.Fold(f);
    }
  }

  public static class PairAndSemigroupExtension {
    public static PairAndSemigroup<X, B> Select<X, A, B>(this PairAndSemigroup<X, A> ps, Func<A, B> f) {
      return new PairAndSemigroup<X, B>(ps.Pair.Select(f), ps.S);
    }

    public static PairAndSemigroup<X, B> SelectMany<X, A, B>(this PairAndSemigroup<X, A> ps, Func<A, PairAndSemigroup<X, B>> f) {
      var r = f(ps.b);
      return new PairAndSemigroup<X, B>(ps.S.Op(ps.a, r.a).And(r.b), ps.S);
    }

    public static PairAndSemigroup<X, C> SelectMany<X, A, B, C>(this PairAndSemigroup<X, A> ps, Func<A, PairAndSemigroup<X, B>> p, Func<A, B, C> f) {
      return SelectMany(ps, a => Select(p(a), b => f(a, b)));
    }
  }

  public struct PairAndMonoid<A, B> {
    private readonly Pair<A, B> pair;
    private readonly Monoid<A> m;

    internal PairAndMonoid(Pair<A, B> pair, Monoid<A> m) {
      this.pair = pair;
      this.m = m;
    }

    public A a {
      get {
        return pair._1.Get;
      }
    }

    public B b {
      get {
        return pair._2.Get;
      }
    }

    internal Monoid<A> M {
      get {
        return m;
      }
    }

    public Pair<A, B> Pair {
      get {
        return pair;
      }
    }

    public PairAndMonoid<A, D> ZipWith<C, D>(PairAndMonoid<A, C> o, Func<B, Func<C, D>> f) {
      return this.SelectMany(a => o.Select(b => f(a)(b)));
    }

    public PairAndMonoid<A, Pair<B, C>> Zip<C>(PairAndMonoid<A, C> o) {
      return ZipWith<C, Pair<B, C>>(o, Pair<B, C>.pairF());
    }

    public Pair<X, B> First<X>(Func<A, X> f) {
      return pair.First(f);
    }

    public PairAndMonoid<A, X> Second<X>(Func<B, X> f) {
      return new PairAndMonoid<A, X>(pair.Second(f), m);
    }

    public Pair<X, Y> BinarySelect<X, Y>(Func<A, X> f, Func<B, Y> g) {
      return pair.BinarySelect(f, g);
    }

    public Pair<B, A> Swap {
      get {
        return pair.Swap;
      }
    }

    public Pair<X, Y> Swapped<X, Y>(Func<Pair<B, A>, Pair<Y, X>> f) {
      return pair.Swapped(f);
    }

    public X Fold<X>(Func<A, B, X> f) {
      return pair.Fold(f);
    }
  }

  public static class PairAndMonoidExtension {
    public static PairAndMonoid<X, B> Select<X, A, B>(this PairAndMonoid<X, A> ps, Func<A, B> f) {
      return new PairAndMonoid<X, B>(ps.Pair.Select(f), ps.M);
    }

    public static PairAndMonoid<X, B> SelectMany<X, A, B>(this PairAndMonoid<X, A> ps, Func<A, PairAndMonoid<X, B>> f) {
      var r = f(ps.b);
      return new PairAndMonoid<X, B>(Pair<X, B>.pair(ps.M.Op(ps.a, r.a), r.b), ps.M);
    }

    public static PairAndMonoid<X, C> SelectMany<X, A, B, C>(this PairAndMonoid<X, A> ps, Func<A, PairAndMonoid<X, B>> p, Func<A, B, C> f) {
      return SelectMany(ps, a => Select(p(a), b => f(a, b)));
    }
  }
}
