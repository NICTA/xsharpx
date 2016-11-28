using System;
using System.Collections;
using System.Collections.Generic;

namespace XSharpx
{
  /// <summary>
  /// An immutable in-memory single-linked list.
  /// </summary>
  /// <typeparam name="A">The element type held by this homogenous list.</typeparam>
  /// <remarks>Also known as a cons-list.</remarks>
  public sealed class List<A> : IEnumerable<A> {
    private readonly bool e;
    private readonly A h;
    private List<A> t;

    private List(bool e, A h, List<A> t) {
      this.e = e;
      this.h = h;
      this.t = t;
    }

    // To be used by ListBuffer only
    internal void UnsafeTailUpdate(List<A> t) {
      this.t = t;
    }

    // To be used by ListBuffer only
    internal A UnsafeHead {
      get {
        if(e)
          throw new Exception("Head on empty List");
        else
          return h;
      }
    }

    // To be used by ListBuffer only
    internal List<A> UnsafeTail {
      get {
        if(e)
          throw new Exception("Tail on empty List");
        else
          return t;
      }
    }

    public bool IsEmpty => e;

    public bool IsNotEmpty => !e;

    public List<List<A>> Duplicate => Extend(q => q);

    public List<B> Extend<B>(Func<List<A>, B> f) {
      var b = ListBuffer<B>.Empty();
      var a = this;

      while(!a.IsEmpty) {
        b.Snoc(f(a));
        a = a.UnsafeTail;
      }

      return b.ToList;
    }

    public List<C> ProductWith<B, C>(List<B> o, Func<A, Func<B, C>> f) =>
      this.SelectMany(a => o.Select(b => f(a)(b)));

    public List<Pair<A, B>> Product<B>(List<B> o) =>
      ProductWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());

    public List<A> Append(List<A> x) {
      var b = ListBuffer<A>.Empty();

      foreach(var a in this)
        b.Snoc(a);

      foreach(var a in x)
        b.Snoc(a);

      return b.ToList;
    }

    public DiffList<A> ToDiffList => DiffList<A>.diffList(r => this * r);

    public NonEmptyList<A> Append(NonEmptyList<A> x) =>
      IsEmpty ? x : new NonEmptyList<A>(UnsafeHead, UnsafeTail).Append(x);

    public static List<A> operator +(A h, List<A> t) =>
      Cons(h, t);

    public static NonEmptyList<A> operator &(A h, List<A> t) => new NonEmptyList<A>(h, t);

    public static List<A> operator *(List<A> a1, List<A> a2) => a1.Append(a2);

    public static List<A> Empty => new List<A>(true, default(A), default(List<A>));

    public static List<A> Cons(A h, List<A> t) => new List<A>(false, h, t);

    public static List<A> list(params A[] a) {
      var k = List<A>.Empty;

      for(int i = a.Length - 1; i >= 0; i--) {
        k = a[i] + k;
      }

      return k;
    }

    private class ListEnumerator : IEnumerator<A> {
      private bool z = true;
      private readonly List<A> o;
      private List<A> a;

      public ListEnumerator(List<A> o) {
        this.o = o;
      }

      public void Dispose() {}

      public void Reset() {
        z = true;
      }

      public bool MoveNext() {
        if(z) {
          a = o;
          z = false;
        } else
          a = a.UnsafeTail;

        return !a.IsEmpty;
      }

      A IEnumerator<A>.Current => a.UnsafeHead;

      public object Current => a.UnsafeHead;

    }

    private ListEnumerator Enumerate() => new ListEnumerator(this);

    IEnumerator<A> IEnumerable<A>.GetEnumerator() => Enumerate();

    IEnumerator IEnumerable.GetEnumerator() => Enumerate();

    public ListBuffer<A> Buffer {
      get {
        var b = ListBuffer<A>.Empty();

        foreach(var a in this)
          b.Snoc(a);

        return b;
      }
    }

    public Option<A> Head => IsEmpty ? Option.Empty : Option.Some(UnsafeHead);

    public Option<List<A>> Tail => IsEmpty ? Option.Empty : Option.Some(UnsafeTail);

    public A HeadOr(Func<A> a) => IsEmpty ? a() : UnsafeHead;

    public List<A> TailOr(Func<List<A>> a) => IsEmpty ? a() : UnsafeTail;

    public X Uncons<X>(Func<X> nil, Func<A, List<A>, X> headTail) => IsEmpty ? nil() : headTail(UnsafeHead, UnsafeTail);

    public Option<Pair<A, List<A>>> HeadTail =>
      IsEmpty ? Option.Empty : Pair<A, List<A>>.pair(UnsafeHead, UnsafeTail).Some();

    public B FoldRight<B>(Func<A, B, B> f, B b) =>
      e ? b : f(UnsafeHead, UnsafeTail.FoldRight(f, b));

    public B FoldLeft<B>(Func<B, A, B> f, B b) {
      var x = b;

      foreach(A z in this) {
        x = f(x, z);
      }

      return x;
    }

    public A SumRight(Monoid<A> m) => FoldRight(m.Op, m.Id);

    public A SumLeft(Monoid<A> m) => FoldLeft(m.Op, m.Id);

    public B SumMapRight<B>(Func<A, B> f, Monoid<B> m) => FoldRight((a, b) => m.Op(f(a), b), m.Id);

    public B SumMapLeft<B>(Func<A, B> f, Monoid<B> m) {
      return FoldLeft<B>((a, b) => m.Op(a, f(b)), m.Id);
    }

    public void ForEach(Action<A> a) {
      foreach(A x in this) {
        a(x);
      }
    }

    public List<A> Where(Func<A, bool> f) {
      var b = ListBuffer<A>.Empty();

      foreach(var a in this)
        if(f(a))
          b.Snoc(a);

      return b.ToList;
    }

    public List<A> Take(int n) =>
      n <= 0 || e
        ? List<A>.Empty
        : UnsafeHead + UnsafeTail.Take(n - 1);

    public List<A> Drop(int n) =>
      n <= 0
        ? this
        : e
          ? List<A>.Empty
          : UnsafeTail.Drop(n - 1);

    public List<A> TakeWhile(Func<A, bool> p) =>
      e
        ? this
        : p(UnsafeHead)
          ? UnsafeHead + UnsafeTail.TakeWhile(p)
          : List<A>.Empty;

    public List<A> DropWhile(Func<A, bool> p) {
      var a = this;

      while(!a.IsEmpty && p(a.UnsafeHead)) {
        a = a.UnsafeTail;
      }

      return a;
    }

    public int Length =>FoldLeft((b, _) => b + 1, 0);

    public List<A> Reverse => FoldLeft<List<A>>((b, a) => a + b, List<A>.Empty);
    public Option<A> this [int n] =>
      n < 0 || IsEmpty
        ? Option.Empty
        : n == 0
          ? Option.Some(UnsafeHead)
          : UnsafeTail[n - 1];
      

    public List<C> ZipWith<B, C>(List<B> bs, Func<A, Func<B, C>> f) =>
      IsEmpty && bs.IsEmpty
        ? List<C>.Empty
        : f(UnsafeHead)(bs.UnsafeHead) + UnsafeTail.ZipWith(bs.UnsafeTail, f);


    public List<Pair<A, B>> Zip<B>(List<B> bs) =>
      ZipWith<B, Pair<A, B>>(bs, a => b => Pair<A, B>.pair(a, b));

    public bool All(Func<A, bool> f) {
      var x = true;

      foreach(var t in this) {
        if(!f(t))
          return false;
      }

      return x;
    }

    public bool Any(Func<A, bool> f) {
      var x = false;

      foreach(var t in this) {
        if(f(t))
          return true;
      }

      return x;
    }

    public List<List<B>> TraverseList<B>(Func<A, List<B>> f) =>
      FoldRight(
        (a, b) => f(a).ProductWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.ListValue()
      );

    public Option<List<B>> TraverseOption<B>(Func<A, Option<B>> f) =>
      FoldRight(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.Some()
      );

    public Terminal<List<B>> TraverseTerminal<B>(Func<A, Terminal<B>> f) =>
      FoldRight(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.TerminalValue()
      );

    public Input<List<B>> TraverseInput<B>(Func<A, Input<B>> f) =>
      FoldRight(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.InputElement()
      );

    public Either<X, List<B>> TraverseEither<X, B>(Func<A, Either<X, B>> f) =>
      FoldRight<Either<X, List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.Right<X, List<B>>()
      );

    public NonEmptyList<List<B>> TraverseNonEmptyList<B>(Func<A, NonEmptyList<B>> f) =>
      FoldRight(
        (a, b) => f(a).ProductWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.NonEmptyListValue()
      );


    public Pair<X, List<B>> TraversePair<X, B>(Func<A, Pair<X, B>> f, Monoid<X> m) =>
      FoldRight(
        (a, b) => f(a).Constrain(m).ZipWith<List<B>, List<B>>(b.Constrain(m), aa => bb => aa + bb).Pair
      , m.Id.And(List<B>.Empty)
      );

    public Func<X, List<B>> TraverseFunc<X, B>(Func<A, Func<X, B>> f) => x => this.Select(a => f(a)(x));

    public Tree<List<B>> TraverseTree<B>(Func<A, Tree<B>> f) =>
      FoldRight(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.TreeValue()
      );

    public Option<ListZipper<A>> Zipper =>
      IsEmpty ? Option<ListZipper<A>>.Empty : (new ListZipper<A>(List<A>.Empty, UnsafeHead, UnsafeTail)).Some();
  }

  public static class ListExtension {
    public static List<B> Select<A, B>(this List<A> ps, Func<A, B> f) {
      var b = ListBuffer<B>.Empty();

      foreach(var a in ps)
        b.Snoc(f(a));

      return b.ToList;
    }

    public static List<B> SelectMany<A, B>(this List<A> ps, Func<A, List<B>> f) {
      var b = ListBuffer<B>.Empty();

      foreach(var a in ps)
        b.Append(f(a));

      return b.ToList;
    }

    public static List<C> SelectMany<A, B, C>(this List<A> ps, Func<A, List<B>> p, Func<A, B, C> f) =>
      SelectMany(ps, a => Select(p(a), b => f(a, b)));
    

    public static List<B> Apply<A, B>(this List<Func<A, B>> f, List<A> o) =>
      f.ProductWith<A, B>(o, a => b => a(b));

    public static List<B> ApplyZip<A, B>(this List<Func<A, B>> f, List<A> o) =>
      f.ZipWith<A, B>(o, a => b => a(b));

    public static List<A> Flatten<A>(this List<List<A>> o) =>
      o.SelectMany(z => z);

    public static Pair<List<A>, List<B>> Unzip<A, B>(this List<Pair<A, B>> p) =>
      p.FoldRight(
        (x, y) => (x._1.Get + y._1.Get).And(x._2.Get + y._2.Get)
      , List<A>.Empty.And(List<B>.Empty)
      );

    public static List<A> ListValue<A>(this A a) =>
      List<A>.Cons(a, List<A>.Empty);

    public static List<B> UnfoldList<A, B>(this A a, Func<A, Option<Pair<B, A>>> f) =>
      f(a).Fold(p => p._1.Get + p._2.Get.UnfoldList(f), () => List<B>.Empty);

  }
}

