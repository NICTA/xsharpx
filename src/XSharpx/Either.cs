using System;
using System.Collections;
using System.Collections.Generic;

namespace XSharpx {
  /// <summary>
  /// A disjoint union type representing 1 of 2 possible values.
  /// </summary>
  ///  <typeparam name="A">The type of the potential left value.</typeparam>
  ///  <typeparam name="B">The type of the potential right value.</typeparam>
  /// <remarks>Either is often used to emulate checked exceptions.</remarks>
  public class Either<A, B> : IEnumerable<B> {
    private readonly bool l;
    private readonly A a;
    private readonly B b;

    private Either(bool l, A a, B b) {
      this.l = l;
      this.a = a;
      this.b = b;
    }

    public bool IsLeft => l;

    public bool IsRight => !l;

    public Either<C, D> BinarySelect<C, D>(Func<A, C> f, Func<B, D> g) =>
      Fold(a => f(a).Left<C, D>(), b => g(b).Right<C, D>());

    public Either<A, Either<A, B>> Duplicate =>
      this.Select(b => b.Right<A, B>());

    public Either<A, X> Extend<X>(Func<Either<A, B>, X> f) =>
      this.Select(b => f(b.Right<A, B>()));

    public Either<A, D> ZipWith<C, D>(Either<A, C> o, Func<B, Func<C, D>> f) =>
      this.SelectMany(a => o.Select(b => f(a)(b)));

    public Either<A, B> OrElse(Func<Either<A, B>> o) =>
      IsLeft ? o() : this;

    public bool All(Func<B, bool> f) => IsLeft || f(b);

    public bool Any(Func<B, bool> f) => IsRight && f(b);

    public List<B> ToList => IsLeft ? List<B>.Empty : b.ListValue();

    public Either<B, A> Swap => l ? Either<B, A>.Right(a) : Either<B, A>.Left(b);

    public Either<A, B> Swapped(Func<Either<B, A>, Either<B, A>> f) =>
      f(Swap).Swap;

    public X Fold<X>(Func<A, X> left, Func<B, X> right) => l ? left(a) : right(b);

    public void ForEach(Action<B> x) {
      if(IsRight)
        x(b);
    }

    public B ValueOr(Func<B> x) => IsLeft ? x() : b;

    public B Reduce(Func<A, B> f) => IsLeft ? f(a) : b;

    public Either<A, B> Ensure(Func<B, bool> p, Func<A> or) =>
      this.SelectMany(b => p(b) ? b.Right<A, B>() : or().Left<A, B>());

    public Option<B> ToOption => IsLeft ? Option<B>.Empty : b.Some();

    public List<Either<A, X>> TraverseList<X>(Func<B, List<X>> f) =>
      Fold(a => a.Left<A, X>().ListValue(), b => f(b).Select(x => x.Right<A, X>()));

    public Option<Either<A, X>> TraverseOption<X>(Func<B, Option<X>> f) =>
      Fold(a => a.Left<A, X>().Some(), b => f(b).Select(x => x.Right<A, X>()));

    public Terminal<Either<A, X>> TraverseTerminal<X>(Func<B, Terminal<X>> f) =>
      Fold(a => a.Left<A, X>().TerminalValue(), b => f(b).Select(x => x.Right<A, X>()));

    public Input<Either<A, X>> TraverseInput<X>(Func<B, Input<X>> f) =>
      Fold(a => a.Left<A, X>().InputElement(), b => f(b).Select(x => x.Right<A, X>()));

    public Either<W, Either<A, X>> TraverseEither<W, X>(Func<B, Either<W, X>> f) =>
      Fold(a => a.Left<A, X>().Right<W, Either<A, X>>(), b => f(b).Select(x => x.Right<A, X>()));

    public NonEmptyList<Either<A, X>> TraverseNonEmptyList<X>(Func<B, NonEmptyList<X>> f) =>
      Fold(a => a.Left<A, X>().NonEmptyListValue(), b => f(b).Select(x => x.Right<A, X>()));

    public Pair<W, Either<A, X>> TraversePair<W, X>(Func<B, Pair<W, X>> f, Monoid<W> m) =>
      Fold(a => m.Id.And(a.Left<A, X>()), b => f(b).Select(x => x.Right<A, X>()));

    public Func<W, Either<A, X>> TraverseFunc<W, X>(Func<B, Func<W, X>> f) =>
      Fold(a => _ => a.Left<A, X>(), b => f(b).Select(x => x.Right<A, X>()));

    public Tree<Either<A, X>> TraverseTree<X>(Func<B, Tree<X>> f) =>
      Fold(a => a.Left<A, X>().TreeValue(), b => f(b).Select(x => x.Right<A, X>()));

    public List<Either<X, Y>> BinaryTraverseList<X, Y>(Func<A, List<X>> f, Func<B, List<Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));

    public Option<Either<X, Y>> BinaryTraverseOption<X, Y>(Func<A, Option<X>> f, Func<B, Option<Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));

    public Either<W, Either<X, Y>> BinaryTraverseEither<W, X, Y>(Func<A, Either<W, X>> f, Func<B, Either<W, Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));

    public NonEmptyList<Either<X, Y>> BinaryTraverseNonEmptyList<X, Y>(Func<A, NonEmptyList<X>> f, Func<B, NonEmptyList<Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));

    public Pair<W, Either<X, Y>> BinaryTraversePair<W, X, Y>(Func<A, Pair<W, X>> f, Func<B, Pair<W, Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));

    public Func<W, Either<X, Y>> BinaryTraverseFunc<W, X, Y>(Func<A, Func<W, X>> f, Func<B, Func<W, Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));

    public Tree<Either<X, Y>> BinaryTraverseTree<X, Y>(Func<A, Tree<X>> f, Func<B, Tree<Y>> g) =>
      Fold(a => f(a).Select(x => x.Left<X, Y>()), b => g(b).Select(y => y.Right<X, Y>()));
    IEnumerator<B> IEnumerable<B>.GetEnumerator() =>
      ((IEnumerable<B>) ToOption).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
      ((IEnumerable) ToOption).GetEnumerator();

    public static Either<A, B> Left(A a) =>
      new Either<A, B>(true, a, default(B));

    public static Either<A, B> Right(B b) =>
      new Either<A, B>(false, default(A), b);
  }

  public static class EitherExtension {
    public static Either<X, B> Select<X, A, B>(this Either<X, A> k, Func<A, B> f) =>
      k.Fold(x => Either<X, B>.Left(x), a => Either<X, B>.Right(f(a)));

    public static Either<X, B> SelectMany<X, A, B>(this Either<X, A> k, Func<A, Either<X, B>> f) =>
      k.Fold(x => Either<X, B>.Left(x), f);

    public static Either<X, C> SelectMany<X, A, B, C>(this Either<X, A> k, Func<A, Either<X, B>> p, Func<A, B, C> f) =>
      SelectMany(k, a => Select(p(a), b => f(a, b)));

    public static Either<X, B> Apply<X, A, B>(this Either<X, Func<A, B>> f, Either<X, A> o) =>
      f.SelectMany(g => o.Select(p => g(p)));

    public static Either<X, A> Flatten<X, A>(this Either<X, Either<X, A>> o) => o.SelectMany(z => z);

    public static Pair<Either<X, A>, Either<X, B>> Unzip<X, A, B>(this Either<X, Pair<A, B>> p) => p.Select(q => q._1.Get).And(p.Select(q => q._2.Get));

    public static Either<A, B> Left<A, B>(this A a) => Either<A, B>.Left(a);

    public static Either<A, B> Right<A, B>(this B b) => Either<A, B>.Right(b);
  }
}
