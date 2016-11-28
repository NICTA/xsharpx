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

    public List<A> Apply(List<A> a) => endo(a);

    public List<A> ToList => endo(List<A>.Empty);

    public Option<A> Head => ToList.Head;

    public Option<List<A>> Tail => ToList.Tail;

    public A HeadOr(Func<A> a) => Head.ValueOr(a);

    public List<A> TailOr(Func<List<A>> a) =>Tail.ValueOr(a);

    public bool IsEmpty => ToList.IsEmpty;

    public bool IsNotEmpty => !IsEmpty;

    public DiffList<A> Where(Func<A, bool> f) => ToList.Where(f).ToDiffList;

    public B FoldRight<B>(Func<A, B, B> f, B b) => ToList.FoldRight(f, b);

    public B FoldLeft<B>(Func<B, A, B> f, B b) => ToList.FoldLeft(f, b);

    public DiffList<C> ProductWith<B, C>(DiffList<B> o, Func<A, Func<B, C>> f) =>
      this.SelectMany(a => o.Select(b => f(a)(b)));

    public DiffList<Pair<A, B>> Product<B>(DiffList<B> o) =>
      ProductWith(o, Pair<A, B>.pairF());

    public X Uncons<X>(Func<X> nil, Func<A, DiffList<A>, X> headTail) =>
      ToList.Uncons(nil, (a, l) => headTail(a, l.ToDiffList));

    public static DiffList<A> diffList(params A[] a) {
      var k = DiffList<A>.Empty;

      for(int i = a.Length - 1; i >= 0; i--) {
        k = a[i] + k;
      }

      return k;
    }

    public static DiffList<A> operator +(A h, DiffList<A> t) =>
      new DiffList<A>(l => h + t.endo(l));

    public static DiffList<A> operator +(DiffList<A> t, A r) =>
      new DiffList<A>(l => t.endo(r + l));

    public static DiffList<A> operator *(DiffList<A> x, DiffList<A> y) =>
      new DiffList<A>(l => x.endo(y.endo(l)));

    public static DiffList<A> diffList(Func<List<A>, List<A>> e) => new DiffList<A>(e);

    public static DiffList<A> Empty => new DiffList<A>(l => l);

    private class DiffListEnumerator : IEnumerator<A> {
      private readonly IEnumerator<A> e;

      public DiffListEnumerator(IEnumerator<A> e) {
        this.e = e;
      }

      public void Dispose() {}

      public void Reset() {
        e.Reset();
      }

      public bool MoveNext() => e.MoveNext();

      A IEnumerator<A>.Current => e.Current;
      public object Current => e.Current;
    }

    private DiffListEnumerator Enumerate() => new DiffListEnumerator((ToList as IEnumerable<A>).GetEnumerator());

    IEnumerator<A> IEnumerable<A>.GetEnumerator() => Enumerate();

    IEnumerator IEnumerable.GetEnumerator() => Enumerate();
  }

  public static class DiffListExtension {
    public static DiffList<B> Select<A, B>(this DiffList<A> ps, Func<A, B> f) =>
      ps.FoldRight((a, b) => f(a) + b, DiffList<B>.Empty);

    public static DiffList<B> SelectMany<A, B>(this DiffList<A> ps, Func<A, DiffList<B>> f) =>
       ps.FoldRight((a, b) => f(a) * b, DiffList<B>.Empty);

    public static DiffList<C> SelectMany<A, B, C>(this DiffList<A> ps, Func<A, DiffList<B>> p, Func<A, B, C> f) =>
      SelectMany(ps, a => Select(p(a), b => f(a, b)));
    

    public static DiffList<B> Apply<A, B>(this DiffList<Func<A, B>> f, DiffList<A> o) =>
      f.ProductWith<A, B>(o, a => b => a(b));

    public static DiffList<A> Flatten<A>(this DiffList<DiffList<A>> o) =>
       o.SelectMany(z => z);

    public static Pair<DiffList<A>, DiffList<B>> Unzip<A, B>(this DiffList<Pair<A, B>> p) =>
      p.FoldRight(
        (x, y) => (x._1.Get + y._1.Get).And(x._2.Get + y._2.Get)
      , DiffList<A>.Empty.And(DiffList<B>.Empty)
      );

    public static DiffList<A> DiffListValue<A>(this A a) =>
      DiffList<A>.diffList(l => a + l);
  }
}
