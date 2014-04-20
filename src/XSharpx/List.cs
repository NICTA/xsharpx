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

    public bool IsEmpty {
      get {
        return e;
      }
    }

    public bool IsNotEmpty {
      get {
        return !e;
      }
    }

    public List<List<A>> Duplicate {
      get {
        return Extend(q => q);
      }
    }

    public List<B> Extend<B>(Func<List<A>, B> f) {
      var b = ListBuffer<B>.Empty();
      var a = this;

      while(!a.IsEmpty) {
        b.Snoc(f(a));
        a = a.UnsafeTail;
      }

      return b.ToList;
    }

    public List<C> ProductWith<B, C>(List<B> o, Func<A, Func<B, C>> f) {
      return this.SelectMany(a => o.Select(b => f(a)(b)));
    }

    public List<Pair<A, B>> Product<B>(List<B> o) {
      return ProductWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());
    }

    public List<A> Append(List<A> x) {
      var b = ListBuffer<A>.Empty();

      foreach(var a in this)
        b.Snoc(a);

      foreach(var a in x)
        b.Snoc(a);

      return b.ToList;
    }

    public DiffList<A> ToDiffList {
      get {
        return DiffList<A>.diffList(r => this * r);
      }
    }
    public NonEmptyList<A> Append(NonEmptyList<A> x) {
      return IsEmpty ? x : new NonEmptyList<A>(UnsafeHead, UnsafeTail).Append(x);
    }

    public static List<A> operator +(A h, List<A> t) {
      return Cons(h, t);
    }

    public static NonEmptyList<A> operator &(A h, List<A> t) {
      return new NonEmptyList<A>(h, t);
    }

    public static List<A> operator *(List<A> a1, List<A> a2) {
      return a1.Append(a2);
    }

    public static List<A> Empty {
      get {
        return new List<A>(true, default(A), default(List<A>));
      }
    }

    public static List<A> Cons(A h, List<A> t) {
      return new List<A>(false, h, t);
    }

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

      A IEnumerator<A>.Current {
        get {
          return a.UnsafeHead;
        }
      }

      public object Current {
        get {
          return a.UnsafeHead;
        }
      }
    }

    private ListEnumerator Enumerate() {
      return new ListEnumerator(this);
    }

    IEnumerator<A> IEnumerable<A>.GetEnumerator() {
      return Enumerate();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Enumerate();
    }

    public ListBuffer<A> Buffer {
      get {
        var b = ListBuffer<A>.Empty();

        foreach(var a in this)
          b.Snoc(a);

        return b;
      }
    }

    public Option<A> Head {
      get {
        return IsEmpty ? Option.Empty : Option.Some(UnsafeHead);
      }
    }

    public Option<List<A>> Tail {
      get {
        return IsEmpty ? Option.Empty : Option.Some(UnsafeTail);
      }
    }

    public A HeadOr(Func<A> a) {
      return IsEmpty ? a() : UnsafeHead;
    }

    public List<A> TailOr(Func<List<A>> a) {
      return IsEmpty ? a() : UnsafeTail;
    }

    public X Uncons<X>(Func<X> nil, Func<A, List<A>, X> headTail) {
      return IsEmpty ? nil() : headTail(UnsafeHead, UnsafeTail);
    }

    public Option<Pair<A, List<A>>> HeadTail {
      get {
        return IsEmpty ? Option.Empty : Pair<A, List<A>>.pair(UnsafeHead, UnsafeTail).Some();
      }
    }

    public B FoldRight<B>(Func<A, B, B> f, B b) {
      return e ? b : f(UnsafeHead, UnsafeTail.FoldRight(f, b));
    }

    public B FoldLeft<B>(Func<B, A, B> f, B b) {
      var x = b;

      foreach(A z in this) {
        x = f(x, z);
      }

      return x;
    }

    public A SumRight(Monoid<A> m) {
      return FoldRight<A>(m.Op, m.Id);
    }

    public A SumLeft(Monoid<A> m) {
      return FoldLeft<A>(m.Op, m.Id);
    }

    public B SumMapRight<B>(Func<A, B> f, Monoid<B> m) {
      return FoldRight<B>((a, b) => m.Op(f(a), b), m.Id);
    }

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

    public List<A> Take(int n) {
      return n <= 0 || e
        ? List<A>.Empty
        : UnsafeHead + UnsafeTail.Take(n - 1);
    }

    public List<A> Drop(int n) {
      return n <= 0
        ? this
        : e
          ? List<A>.Empty
          : UnsafeTail.Drop(n - 1);
    }

    public List<A> TakeWhile(Func<A, bool> p) {
      return e
        ? this
        : p(UnsafeHead)
          ? UnsafeHead + UnsafeTail.TakeWhile(p)
          : List<A>.Empty;
    }

    public List<A> DropWhile(Func<A, bool> p) {
      var a = this;

      while(!a.IsEmpty && p(a.UnsafeHead)) {
        a = a.UnsafeTail;
      }

      return a;
    }

    public int Length {
      get {
        return FoldLeft((b, _) => b + 1, 0);
      }
    }

    public List<A> Reverse {
      get {
        return FoldLeft<List<A>>((b, a) => a + b, List<A>.Empty);
      }
    }

    public Option<A> this [int n] {
      get {
        return n < 0 || IsEmpty
          ? Option.Empty
          : n == 0
            ? Option.Some(UnsafeHead)
            : UnsafeTail[n - 1];
      }
    }

    public List<C> ZipWith<B, C>(List<B> bs, Func<A, Func<B, C>> f) {
      return IsEmpty && bs.IsEmpty
        ? List<C>.Empty
        : f(UnsafeHead)(bs.UnsafeHead) + UnsafeTail.ZipWith(bs.UnsafeTail, f);
    }

    public List<Pair<A, B>> Zip<B>(List<B> bs) {
      return ZipWith<B, Pair<A, B>>(bs, a => b => Pair<A, B>.pair(a, b));
    }

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

    public List<List<B>> TraverseList<B>(Func<A, List<B>> f) {
      return FoldRight<List<List<B>>>(
        (a, b) => f(a).ProductWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.ListValue()
      );
    }

    public Option<List<B>> TraverseOption<B>(Func<A, Option<B>> f) {
      return FoldRight<Option<List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.Some()
      );
    }

    public Terminal<List<B>> TraverseTerminal<B>(Func<A, Terminal<B>> f) {
      return FoldRight<Terminal<List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.TerminalValue()
      );
    }

    public Input<List<B>> TraverseInput<B>(Func<A, Input<B>> f) {
      return FoldRight<Input<List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.InputElement()
      );
    }

    public Either<X, List<B>> TraverseEither<X, B>(Func<A, Either<X, B>> f) {
      return FoldRight<Either<X, List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.Right<X, List<B>>()
      );
    }

    public NonEmptyList<List<B>> TraverseNonEmptyList<B>(Func<A, NonEmptyList<B>> f) {
      return FoldRight<NonEmptyList<List<B>>>(
        (a, b) => f(a).ProductWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.NonEmptyListValue()
      );
    }

    public Pair<X, List<B>> TraversePair<X, B>(Func<A, Pair<X, B>> f, Monoid<X> m) {
      return FoldRight<Pair<X, List<B>>>(
        (a, b) => f(a).Constrain(m).ZipWith<List<B>, List<B>>(b.Constrain(m), aa => bb => aa + bb).Pair
      , m.Id.And(List<B>.Empty)
      );
    }

    public Func<X, List<B>> TraverseFunc<X, B>(Func<A, Func<X, B>> f) {
      return x => this.Select(a => f(a)(x));
    }

    public Tree<List<B>> TraverseTree<B>(Func<A, Tree<B>> f) {
      return FoldRight<Tree<List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.TreeValue()
      );
    }

    public Option<ListZipper<A>> Zipper {
      get {
        return IsEmpty ? Option<ListZipper<A>>.Empty : (new ListZipper<A>(List<A>.Empty, UnsafeHead, UnsafeTail)).Some();
      }
    }
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

    public static List<C> SelectMany<A, B, C>(this List<A> ps, Func<A, List<B>> p, Func<A, B, C> f) {
      return SelectMany(ps, a => Select(p(a), b => f(a, b)));
    }

    public static List<B> Apply<A, B>(this List<Func<A, B>> f, List<A> o) {
      return f.ProductWith<A, B>(o, a => b => a(b));
    }

    public static List<B> ApplyZip<A, B>(this List<Func<A, B>> f, List<A> o) {
      return f.ZipWith<A, B>(o, a => b => a(b));
    }

    public static List<A> Flatten<A>(this List<List<A>> o) {
      return o.SelectMany(z => z);
    }

    public static Pair<List<A>, List<B>> Unzip<A, B>(this List<Pair<A, B>> p) {
      return p.FoldRight<Pair<List<A>, List<B>>>(
        (x, y) => (x._1.Get + y._1.Get).And(x._2.Get + y._2.Get)
      , List<A>.Empty.And(List<B>.Empty)
      );
    }

    public static List<A> ListValue<A>(this A a) {
      return List<A>.Cons(a, List<A>.Empty);
    }

    public static List<B> UnfoldList<A, B>(this A a, Func<A, Option<Pair<B, A>>> f) {
      return f(a).Fold<List<B>>(p => p._1.Get + p._2.Get.UnfoldList(f), () => List<B>.Empty);
    }
  }
}

