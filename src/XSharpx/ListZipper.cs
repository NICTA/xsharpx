using System;

namespace XSharpx {
  public struct ListZipper<A> {

    private readonly List<A> lefts;
    private readonly A focus;
    private readonly List<A> rights;

    internal ListZipper(List<A> lefts, A focus, List<A> rights) {
      this.lefts = lefts;
      this.focus = focus;
      this.rights = rights;
    }

    public Store<List<A>, ListZipper<A>> Lefts {
      get {
        var t = this;
        return lefts.StoreSet(l => new ListZipper<A>(l, t.focus, t.rights));
      }
    }

    public Store<A, ListZipper<A>> Focus {
      get {
        var t = this;
        return focus.StoreSet(x => new ListZipper<A>(t.lefts, x, t.rights));
      }
    }

    public Store<List<A>, ListZipper<A>> Rights {
      get {
        var t = this;
        return rights.StoreSet(r => new ListZipper<A>(t.lefts, t.focus, r));
      }
    }

    public List<A> ToList {
      get {
        return lefts.Reverse * (focus + rights);
      }
    }

    public NonEmptyList<A> ToNonEmptyList {
      get {
        var t = this;
        return lefts.Uncons(() => t.focus & t.rights, (hh, tt) => (hh & tt).Reverse * (t.focus & t.rights));
      }
    }

    public ListZipper<B> Select<B>(Func<A, B> f) {
      return new ListZipper<B>(lefts.Select(f), f(focus), rights.Select(f));
    }

    public ListZipper<ListZipper<A>> Duplicate {
      get {
        return Extend(q => q);
      }
    }

    public ListZipper<B> Extend<B>(Func<ListZipper<A>, B> f) {
      var l = this.UnfoldList(z => z.MoveLeft.Select(x => f(x).And(x)));
      var r = this.UnfoldList(z => z.MoveRight.Select(x => f(x).And(x)));
      return new ListZipper<B>(l, f(this), r);
    }

    public ListZipper<A> Update(Func<A, A> f) {
      return new ListZipper<A>(lefts, f(focus), rights);
    }

    public ListZipper<A> Set(A a) {
      return Update(_ => a);
    }

    public Option<ListZipper<A>> MoveLeft {
      get {
        var t = this;
        return lefts.HeadTail.Select(p => new ListZipper<A>(p._2.Get, p._1.Get, t.focus + t.rights));
      }
    }

    public Option<ListZipper<A>> MoveRight {
      get {
        var t = this;
        return rights.HeadTail.Select(p => new ListZipper<A>(t.focus + t.lefts, p._1.Get, p._2.Get));
      }
    }

    public ListZipper<A> MoveLeftOr(Func<ListZipper<A>> z) {
      return MoveLeft.ValueOr(z);
    }

    public ListZipper<A> MoveRightOr(Func<ListZipper<A>> z) {
      return MoveRight.ValueOr(z);
    }

    public ListZipper<A> TryLeft {
      get {
        var t = this;
        return MoveLeftOr(() => t);
      }
    }

    public ListZipper<A> TryRight {
      get {
        var t = this;
        return MoveRightOr(() => t);
      }
    }

    public ListZipper<A> InsertLeft(A a) {
      return new ListZipper<A>(lefts, a, focus + rights);
    }

    public ListZipper<A> InsertRight(A a) {
      return new ListZipper<A>(focus + lefts, a, rights);
    }

    public ListZipper<A> DeleteOthers {
      get {
        return new ListZipper<A>(List<A>.Empty, focus, List<A>.Empty);
      }
    }

    public ListZipper<A> DeleteLefts {
      get {
        return new ListZipper<A>(List<A>.Empty, focus, rights);
      }
    }

    public ListZipper<A> DeleteRights {
      get {
        return new ListZipper<A>(lefts, focus, List<A>.Empty);
      }
    }

    public Option<ListZipper<A>> DeletePullLeft {
      get {
        var t = this;
        return lefts.HeadTail.Select(p => new ListZipper<A>(p._2.Get, p._1.Get, t.rights));
      }
    }

    public Option<ListZipper<A>> DeletePullRight {
      get {
        var t = this;
        return rights.HeadTail.Select(p => new ListZipper<A>(t.lefts, p._1.Get, p._2.Get));
      }
    }

    public ListZipper<A> DeletePullLeftOr(Func<ListZipper<A>> z) {
      return DeletePullLeft.ValueOr(z);
    }

    public ListZipper<A> DeletePullRightOr(Func<ListZipper<A>> z) {
      return DeletePullRight.ValueOr(z);
    }

    public ListZipper<A> TryDeletePullLeft {
      get {
        var t = this;
        return DeletePullLeftOr(() => t);
      }
    }

    public ListZipper<A> TryDeletePullRight {
      get {
        var t = this;
        return DeletePullRightOr(() => t);
      }
    }

    public bool IsStart {
      get {
        return lefts.IsEmpty;
      }
    }

    public bool IsEnd {
      get {
        return rights.IsEmpty;
      }
    }

    public Option<ListZipper<A>> FindLeft(Func<A, bool> p) {
      var z = MoveLeft;

      while(z.Any(y => !p(y.focus))) {
        z = z.SelectMany(q => q.MoveLeft);
      }

      return z;
    }

    public Option<ListZipper<A>> FindRight(Func<A, bool> p) {
      var z = MoveRight;

      while(z.Any(y => !p(y.focus))) {
        z = z.SelectMany(q => q.MoveRight);
      }

      return z;
    }

    public ListZipper<A> FindLeftOr(Func<A, bool> p, Func<ListZipper<A>> z) {
      return FindLeft(p).ValueOr(z);
    }

    public ListZipper<A> FindRightOr(Func<A, bool> p, Func<ListZipper<A>> z) {
      return FindRight(p).ValueOr(z);
    }

    public ListZipper<A> TryFindLeft(Func<A, bool> p) {
      var t = this;
      return FindLeftOr(p, () => t);
    }

    public ListZipper<A> TryFindRight(Func<A, bool> p) {
      var t = this;
      return FindRightOr(p, () => t);
    }

    public Option<ListZipper<A>> MoveLeftN(int n) {
      return n == 0 ?
               this.Some() :
               n < 0 ?
               MoveRightN(Math.Abs(n)) :
               MoveLeft.SelectMany(z => z.MoveLeftN(n - 1));
    }

    public Option<ListZipper<A>> MoveRightN(int n) {
      return n == 0 ?
        this.Some() :
        n < 0 ?
        MoveLeftN(Math.Abs(n)) :
        MoveRight.SelectMany(z => z.MoveRightN(n - 1));
    }

    public ListZipper<A> Start {
      get {
        var z = this;

        while(!z.IsStart) {
          z = z.Start;
        }

        return z;
      }
    }

    public ListZipper<A> End {
      get {
        var z = this;

        while(!z.IsEnd) {
          z = z.End;
        }

        return z;
      }
    }

    public List<ListZipper<B>> TraverseList<B>(Func<A, List<B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseList(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseList(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public Option<ListZipper<B>> TraverseOption<B>(Func<A, Option<B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseOption(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseOption(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public Input<ListZipper<B>> TraverseInput<B>(Func<A, Input<B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseInput(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseInput(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public Either<X, ListZipper<B>> TraverseEither<X, B>(Func<A, Either<X, B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseEither(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseEither(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public NonEmptyList<ListZipper<B>> TraverseNonEmptyList<B>(Func<A, NonEmptyList<B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseNonEmptyList(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseNonEmptyList(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public Pair<X, ListZipper<B>> TraversePair<X, B>(Func<A, Pair<X, B>> f, Monoid<X> m) {
      var t = this;
      var r =
        from ll in lefts.Reverse.TraversePair(f, m).Constrain(m)
        from xx in f(t.focus).Constrain(m)
        from rr in t.rights.TraversePair(f, m).Constrain(m)
        select new ListZipper<B>(ll, xx, rr);
      return r.Pair;
    }

    public Func<X, ListZipper<B>> TraverseFunc<X, B>(Func<A, Func<X, B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseFunc(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseFunc(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public Tree<ListZipper<B>> TraverseTree<B>(Func<A, Tree<B>> f) {
      var t = this;
      return
        from ll in lefts.Reverse.TraverseTree(f)
        from xx in f(t.focus)
        from rr in t.rights.TraverseTree(f)
        select new ListZipper<B>(ll, xx, rr);
    }

    public static ListZipper<A> operator %(ListZipper<A> z, Func<A, A> f) {
      return z.Update(f);
    }
  }

  public static class ListZipperExtension {

    public static Pair<ListZipper<A>, ListZipper<B>> Unzip<A, B>(this ListZipper<Pair<A, B>> p) {
      return p.Select(q => q._1.Get).And(p.Select(q => q._2.Get));
    }

    public static ListZipper<A> ListZipperValue<A>(this A a) {
      return new ListZipper<A>(List<A>.Empty, a, List<A>.Empty);
    }
  }
}
