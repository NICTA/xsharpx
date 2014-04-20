using System;
using System.Collections;
using System.Collections.Generic;

namespace XSharpx {
  // List with O(1) cons and snoc.
  public struct DiffList<A> : IEnumerable<A> {
    private readonly Func<List<A>, List<A>> endo;

    private DiffList(Func<List<A>, List<A>> endo) {
      this.endo = endo;
    }

    public List<A> Apply(List<A> a) {
      return endo(a);
    }

    public List<A> ToList {
      get {
        return endo(List<A>.Empty);
      }
    }

    public Option<A> Head {
      get {
        return ToList.Head;
      }
    }

    public Option<List<A>> Tail {
      get {
        return ToList.Tail;
      }
    }

    public A HeadOr(Func<A> a) {
      return Head.ValueOr(a);
    }

    public List<A> TailOr(Func<List<A>> a) {
      return Tail.ValueOr(a);
    }

    public bool IsEmpty {
      get {
        return ToList.IsEmpty;
      }
    }

    public bool IsNotEmpty {
      get {
        return !IsEmpty;
      }
    }

    public DiffList<A> Where(Func<A, bool> f) {
      return ToList.Where(f).ToDiffList;
    }

    public B FoldRight<B>(Func<A, B, B> f, B b) {
      return ToList.FoldRight(f, b);
    }

    public B FoldLeft<B>(Func<B, A, B> f, B b) {
      return ToList.FoldLeft(f, b);
    }

    public DiffList<C> ProductWith<B, C>(DiffList<B> o, Func<A, Func<B, C>> f) {
      return this.SelectMany(a => o.Select(b => f(a)(b)));
    }

    public DiffList<Pair<A, B>> Product<B>(DiffList<B> o) {
      return ProductWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());
    }

    public X Uncons<X>(Func<X> nil, Func<A, DiffList<A>, X> headTail) {
      return ToList.Uncons(nil, (a, l) => headTail(a, l.ToDiffList));
    }

    public static DiffList<A> diffList(params A[] a) {
      var k = DiffList<A>.Empty;

      for(int i = a.Length - 1; i >= 0; i--) {
        k = a[i] + k;
      }

      return k;
    }

    public static DiffList<A> operator +(A h, DiffList<A> t) {
      return new DiffList<A>(l => h + t.endo(l));
    }

    public static DiffList<A> operator +(DiffList<A> t, A r) {
      return new DiffList<A>(l => t.endo(r + l));
    }

    public static DiffList<A> operator *(DiffList<A> x, DiffList<A> y) {
      return new DiffList<A>(l => x.endo(y.endo(l)));
    }

    public static DiffList<A> diffList(Func<List<A>, List<A>> e) {
      return new DiffList<A>(e);
    }

    public static DiffList<A> Empty {
      get {
        return new DiffList<A>(l => l);
      }
    }

    private class DiffListEnumerator : IEnumerator<A> {
      private readonly IEnumerator<A> e;

      public DiffListEnumerator(IEnumerator<A> e) {
        this.e = e;
      }

      public void Dispose() {}

      public void Reset() {
        e.Reset();
      }

      public bool MoveNext() {
        return e.MoveNext();
      }

      A IEnumerator<A>.Current {
        get {
          return e.Current;
        }
      }

      public object Current {
        get {
          return e.Current;
        }
      }
    }

    private DiffListEnumerator Enumerate() {
      return new DiffListEnumerator((ToList as IEnumerable<A>).GetEnumerator());
    }

    IEnumerator<A> IEnumerable<A>.GetEnumerator() {
      return Enumerate();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Enumerate();
    }
  }

  public static class DiffListExtension {
    public static DiffList<B> Select<A, B>(this DiffList<A> ps, Func<A, B> f) {
      return ps.FoldRight((a, b) => f(a) + b, DiffList<B>.Empty);
    }

    public static DiffList<B> SelectMany<A, B>(this DiffList<A> ps, Func<A, DiffList<B>> f) {
       return ps.FoldRight((a, b) => f(a) * b, DiffList<B>.Empty);
    }

    public static DiffList<C> SelectMany<A, B, C>(this DiffList<A> ps, Func<A, DiffList<B>> p, Func<A, B, C> f) {
      return SelectMany(ps, a => Select(p(a), b => f(a, b)));
    }

    public static DiffList<B> Apply<A, B>(this DiffList<Func<A, B>> f, DiffList<A> o) {
      return f.ProductWith<A, B>(o, a => b => a(b));
    }

    public static DiffList<A> Flatten<A>(this DiffList<DiffList<A>> o) {
      return o.SelectMany(z => z);
    }

    public static Pair<DiffList<A>, DiffList<B>> Unzip<A, B>(this DiffList<Pair<A, B>> p) {
      return p.FoldRight<Pair<DiffList<A>, DiffList<B>>>(
        (x, y) => (x._1.Get + y._1.Get).And(x._2.Get + y._2.Get)
      , DiffList<A>.Empty.And(DiffList<B>.Empty)
      );
    }

    public static DiffList<A> DiffListValue<A>(this A a) {
      return DiffList<A>.diffList(l => a + l);
    }
  }
}
