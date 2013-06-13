using System;

namespace XSharpx {
  public static class FuncExtension {
    // 0-arity
    public static Func<B> Select<A, B>(this Func<A> a, Func<A, B> f) {
      return () => f(a());
    }

    public static Func<B> SelectMany<A, B>(this Func<A> a, Func<A, Func<B>> f) {
      return f(a());
    }

    public static Func<C> SelectMany<A, B, C>(this Func<A> k, Func<A, Func<B>> p, Func<A, B, C> f) {
      return SelectMany<A, C>(k, a => Select<B, C>(p(a), b => f(a, b)));
    }

    public static Func<B> Apply<A, B>(this Func<Func<A, B>> a, Func<A> f) {
      return () => a()(f());
    }

    public static Func<A> Flatten<A>(this Func<Func<A>> f) {
      return SelectMany(f, z => z);
    }

    // 1-arity
    public static Func<A, C> Select<A, B, C>(this Func<A, B> f, Func<B, C> g) {
      return a => g(f(a));
    }

    public static Func<C, B> SelectMany<A, B, C>(this Func<C, A> f, Func<A, Func<C, B>> g) {
      return a => g(f(a))(a);
    }

    public static Func<C, D> SelectMany<A, B, C, D>(this Func<C, A> f, Func<A, Func<C, B>> p, Func<A, B, D> k) {
      return SelectMany<A, D, C>(f, b => Select<C, B, D>(p(b), x => k(b, x)));
    }

    public static Func<A, C> Apply<A, B, C>(this Func<A, Func<B, C>> f, Func<A, B> g) {
      return a => f(a)(g(a));
    }

    public static Func<A, B> Flatten<A, B>(this Func<A, Func<A, B>> f) {
      return SelectMany(f, z => z);
    }

    public static Func<X, C> ZipWith<A, B, C, X>(this Func<X, A> a, Func<X, B> b, Func<A, Func<B, C>> f) {
      return x => f(a(x))(b(x));
    }

    public static Func<X, Pair<A, B>> Zip<A, B, X>(this Func<X, A> a, Func<X, B> b) {
      return ZipWith<A, B, Pair<A, B>, X>(a, b, Pair<A, B>.pairF());
    }

    public static Func<B, A, C> Flip<A, B, C>(this Func<A, B, C> f) {
      return (b, a) => f(a, b);
    }

    public static Func<A, Func<B, C>> Curry<A, B, C>(this Func<A, B, C> f) {
      return a => b => f(a, b);
    }

    public static Func<A, B, C> UnCurry<A, B, C>(this Func<A, Func<B, C>> f) {
      return (a, b) => f(a)(b);
    }

    public static Func<A, C> Compose<A, B, C>(this Func<B, C> f, Func<A, B> g) {
      return a => f(g(a));
    }

    public static Func<A, B> Constant<A, B>(this Func<B> f) {
      return _ => f();
    }

    public static Func<A, Func<B>> Promote<A, B>(this Func<A, B> f) {
      return a => () => f(a);
    }

  }
}

