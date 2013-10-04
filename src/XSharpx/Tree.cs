using System;

namespace XSharpx {
  public struct Tree<A> {
    private readonly A root;
    private readonly List<Tree<A>> children;

    internal Tree(A root, List<Tree<A>> children) {
      this.root = root;
      this.children = children;
    }

    public Store<A, Tree<A>> Root {
      get {
        var t = this;
        return root.StoreSet(r => new Tree<A>(r, t.children));
      }
    }

    public Store<List<Tree<A>>, Tree<A>> Children {
      get {
        var t = this;
        return children.StoreSet(c => new Tree<A>(t.root, c));
      }
    }

    public Tree<Tree<A>> Duplicate {
      get {
        return this.UnfoldTree(t => t.And(t.children));
      }
    }

    public Tree<B> Extend<B>(Func<Tree<A>, B> f) {
      return this.UnfoldTree(t => f(t).And(t.children));
    }

    public Tree<C> ZipWith<B, C>(Tree<B> o, Func<A, Func<B, C>> f) {
      return this.SelectMany(a => o.Select(b => f(a)(b)));
    }

    public Tree<Pair<A, B>> Zip<B>(Tree<B> o) {
      return ZipWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());
    }

    public Tree<B> ScanRight<B>(Func<A, List<Tree<B>>, B> f) {
      var c = children.Select(t => t.ScanRight(f));
      return f(root, c).TreeNode(c);
    }

    private List<A> Squish(List<A> x) {
      return root + children.FoldRight((a, b) => a.Squish(b), x);
    }

    public List<A> PreOrder {
      get {
        return Squish(List<A>.Empty);
      }
    }

    public List<List<A>> BreadthFirst {
      get {
        return this.ListValue().UnfoldList<List<Tree<A>>, List<A>>(a =>
          a.IsEmpty ?
            Option.Empty :
            a.Select(q => q.Root.Get).And(a.SumMapRight(t => t.Children.Get, Monoid<Tree<A>>.List)).Some());
      }
    }

    public B FoldRight<B>(Func<A, B, B> f, B b) {
      return PreOrder.FoldRight(f, b);
    }

    public B FoldLeft<B>(Func<B, A, B> f, B b) {
      return PreOrder.FoldLeft(f, b);
    }

    public List<Tree<B>> TraverseList<B>(Func<A, List<B>> f) {
      return f(root).ProductWith<List<Tree<B>>, Tree<B>>(
        children.TraverseList(w => w.TraverseList(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public Option<Tree<B>> TraverseOption<B>(Func<A, Option<B>> f) {
      return f(root).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraverseOption(w => w.TraverseOption(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public Terminal<Tree<B>> TraverseTerminal<B>(Func<A, Terminal<B>> f) {
      return f(root).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraverseTerminal(w => w.TraverseTerminal(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public Input<Tree<B>> TraverseInput<B>(Func<A, Input<B>> f) {
      return f(root).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraverseInput(w => w.TraverseInput(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public Either<X, Tree<B>> TraverseEither<X, B>(Func<A, Either<X, B>> f) {
      return f(root).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraverseEither(w => w.TraverseEither(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public NonEmptyList<Tree<B>> TraverseNonEmptyList<B>(Func<A, NonEmptyList<B>> f) {
      return f(root).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraverseNonEmptyList(w => w.TraverseNonEmptyList(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public Pair<X, Tree<B>> TraversePair<X, B>(Func<A, Pair<X, B>> f, Monoid<X> m) {
      return f(root).Constrain(m).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraversePair(w => w.TraversePair(f, m), m).Constrain(m)
      , b => bs => b.TreeNode(bs)
      ).Pair;
    }

    public Func<X, Tree<B>> TraverseFunc<X, B>(Func<A, Func<X, B>> f) {
      return f(root).ZipWith<B, List<Tree<B>>, Tree<B>, X>(
        children.TraverseFunc<X, Tree<B>>(w => w.TraverseFunc(f))
      , b => bs => b.TreeNode(bs)
      );
    }

    public Tree<Tree<B>> TraverseTree<B>(Func<A, Tree<B>> f) {
      return f(root).ZipWith<List<Tree<B>>, Tree<B>>(
        children.TraverseTree(w => w.TraverseTree(f))
      , b => bs => b.TreeNode(bs)
      );
    }

  }

  public static class TreeExtension {
    public static Tree<A> TreeValue<A>(this A a) {
      return new Tree<A>(a, List<Tree<A>>.Empty);
    }

    public static Tree<A> TreeNode<A>(this A a, List<Tree<A>> c) {
      return new Tree<A>(a, c);
    }

    public static Tree<B> Select<A, B>(this Tree<A> k, Func<A, B> f) {
      return f(k.Root.Get).TreeNode(k.Children.Get.Select(q => q.Select(f)));
    }

    public static Tree<B> SelectMany<A, B>(this Tree<A> k, Func<A, Tree<B>> f) {
      var r = f(k.Root.Get);
      return r.Root.Get.TreeNode(r.Children.Get * k.Children.Get.Select(q => q.SelectMany(f)));
    }

    public static Tree<C> SelectMany<A, B, C>(this Tree<A> k, Func<A, Tree<B>> p, Func<A, B, C> f) {
      return SelectMany(k, a => Select(p(a), b => f(a, b)));
    }

    public static Tree<B> Apply<A, B>(this Tree<Func<A, B>> f, Tree<A> o) {
      return f.SelectMany(g => o.Select(p => g(p)));
    }

    public static Tree<A> Flatten<A>(this Tree<Tree<A>> o) {
      return o.SelectMany(z => z);
    }

    public static Pair<Tree<A>, Tree<B>> Unzip<A, B>(this Tree<Pair<A, B>> p) {
      var c = p.Children.Get.Select(l => l.Unzip());
      var r1 = c.Select(q => q._1.Get);
      var r2 = c.Select(q => q._2.Get);
      return p.Root.Get._1.Get.TreeNode(r1).And(p.Root.Get._2.Get.TreeNode(r2));
    }

    public static List<Tree<B>> UnfoldChildren<A, B>(this List<A> a, Func<A, Pair<B, List<A>>> f) {
       return a.Select(z => z.UnfoldTree(f));
    }

    public static Tree<B> UnfoldTree<A, B>(this A a, Func<A, Pair<B, List<A>>> f) {
      var p = f(a);
      return p._1.Get.TreeNode(p._2.Get.UnfoldChildren(f));
    }
  }

  public struct TreeForest<A> {
    private readonly List<Tree<A>> forest;

    internal TreeForest(List<Tree<A>> forest) {
      this.forest = forest;
    }

    public Store<List<Tree<A>>, TreeForest<A>> Forest {
      get {
        return forest.StoreSet(f => new TreeForest<A>(f));
      }
    }

  }

  public static class TreeForestExtension {

    public static TreeForest<B> Select<A, B>(this TreeForest<A> k, Func<A, B> f) {
      return k.Forest.Get.Select(q => q.Select(f)).Forest();
    }

    public static TreeForest<B> SelectMany<A, B>(this TreeForest<A> k, Func<A, TreeForest<B>> f) {
      return k.Forest.Get.SelectMany(t => TreeT(a => f(a).Forest.Get, t)).Forest();
    }

    private static List<Tree<B>> TreeT<A, B>(Func<A, List<Tree<B>>> f, Tree<A> t) {
      return
        from r in f(t.Root.Get)
        from s in t.Children.Get.TraverseList(a => TreeT(f, a))
        select r.Root.Get.TreeNode(s.Append(r.Children.Get));
    }

    public static TreeForest<C> SelectMany<A, B, C>(this TreeForest<A> k, Func<A, TreeForest<B>> p, Func<A, B, C> f) {
      return SelectMany(k, a => Select(p(a), b => f(a, b)));
    }

    public static TreeForest<B> Apply<A, B>(this TreeForest<Func<A, B>> f, TreeForest<A> o) {
      return f.SelectMany(g => o.Select(p => g(p)));
    }

    public static TreeForest<A> Flatten<A>(this TreeForest<TreeForest<A>> o) {
      return o.SelectMany(z => z);
    }

    public static Pair<TreeForest<A>, TreeForest<B>> Unzip<A, B>(this TreeForest<Pair<A, B>> p) {
      return Pair<TreeForest<A>, TreeForest<B>>.pair(p.Select(a => a._1.Get), p.Select(a => a._2.Get));
    }

    public static TreeForest<A> Forest<A>(this List<Tree<A>> f) {
      return new TreeForest<A>(f);
    }
  }
}
