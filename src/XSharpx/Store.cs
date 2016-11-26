using System;

namespace XSharpx {
  public struct Store<A, B> {
    private readonly Func<A, B> set;
    private readonly A get;

    internal Store(Func<A, B> set, A get) {
      this.set = set;
      this.get = get;
    }

    public Store<Func<A, B>, Store<A, B>> S {
      get {
        var t = this;
        return set.StoreSet(s => t.get.StoreSet(s));
      }
    }

    public Store<A, Store<A, B>> G {
      get {
        var t = this;
        return get.StoreSet(g => g.StoreSet(t.set));
      }
    }

    public Func<A, B> Set => set;

    public A Get => get;

    public Store<X, B> XSelect<X>(Func<A, X> f, Func<X, A> g) => new Store<X, B>(set.Compose(g), f(get));

    public Store<A, Store<A, B>> Duplicate => Extend(q => q);

    public Store<A, X> Extend<X>(Func<Store<A, B>, X> f) {
      var t = this;
      return new Store<A, X>(a => f(new Store<A, B>(t.set, a)), get);
    }

    public B Extract => set(get);

    public Store<Pair<A, C>, Pair<B, D>> Product<C, D>(Store<C, D> s) {
      var t = this;
      return new Store<Pair<A, C>, Pair<B, D>>(
        p => t.set(p._1.Get).And(s.set(p._2.Get))
      , get.And(s.get)
      );
    }

    public B Modify(Func<A, A> f) => set(f(get));

  }

  public static class StoreExtension {
    public static Store<X, B> Select<X, A, B>(this Store<X, A> s, Func<A, B> f) =>
      new Store<X, B>(f.Compose(s.Set), s.Get);

    public static Store<A, B> StoreSet<A, B>(this A a, Func<A, B> f) => new Store<A, B>(f, a);

  }
}
