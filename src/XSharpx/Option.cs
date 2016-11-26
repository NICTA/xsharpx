using System;
using System.Collections;
using System.Collections.Generic;

namespace XSharpx {
  public sealed class Option {
    private static readonly Option empty = new Option();
    private Option() { }
    public static Option Empty => empty;
    public static Option<A> Some<A>(A a) => Option<A>.Some(a);
  }

  /// <summary>
  /// An immutable list with a maximum length of 1.
  /// </summary>
  /// <typeparam name="A">The element type held by this homogenous structure.</typeparam>
  /// <remarks>This data type is also used in place of a nullable type.</remarks>
  public struct Option<A> : IEnumerable<A> {
    private readonly bool ne;
    private readonly A a;

    private Option(bool e, A a) {
      this.ne = !e;
      this.a = a;
    }

    public bool IsEmpty => !ne;

    public bool IsNotEmpty => ne;

    public X Fold<X>(Func<A, X> some, Func<X> empty) => IsEmpty ? empty() : some(a);

    public Option<Option<A>> Duplicate => this.Select(a => a.Some());

    public Option<B> Extend<B>(Func<Option<A>, B> f) => this.Select(a => f(a.Some()));

    public Option<C> ZipWith<B, C>(Option<B> o, Func<A, Func<B, C>> f) => this.SelectMany(a => o.Select(b => f(a)(b)));

    public Option<Pair<A, B>> Zip<B>(Option<B> o) => ZipWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());

    public void ForEach(Action<A> a) {
      foreach(A x in this) {
        a(x);
      }
    }

    public Option<A> Where(Func<A, bool> p) {
      var t = this;
      return Fold(a => p(a) ? t : Empty, () => Empty);
    }

    public A ValueOr(Func<A> or) => IsEmpty ? or() : a;

    public Option<A> OrElse(Func<Option<A>> o) => IsEmpty ? o() : this;

    public Option<A> Append(Option<A> o, Semigroup<A> m) => m.Option.Op(this, o);

    public bool All(Func<A, bool> f) => IsEmpty || f(a);

    public bool Any(Func<A, bool> f) => !IsEmpty && f(a);

    public List<A> ToList => IsEmpty ? List<A>.Empty : a.ListValue();

    public List<Option<B>> TraverseList<B>(Func<A, List<B>> f) => IsEmpty ? Option<B>.Empty.ListValue() : f(a).Select(q => q.Some());

    public Option<Option<B>> TraverseOption<B>(Func<A, Option<B>> f) => IsEmpty ? Option<B>.Empty.Some() : f(a).Select(q => q.Some());

    public Terminal<Option<B>> TraverseTerminal<B>(Func<A, Terminal<B>> f) =>
      IsEmpty ? Option<B>.Empty.TerminalValue() : f(a).Select(q => q.Some());

    public Input<Option<B>> TraverseInput<B>(Func<A, Input<B>> f) => IsEmpty ? Option<B>.Empty.InputElement() : f(a).Select(q => q.Some());

    public Either<X, Option<B>> TraverseEither<X, B>(Func<A, Either<X, B>> f) =>
      IsEmpty ? Option<B>.Empty.Right<X, Option<B>>() : f(a).Select(q => q.Some());

    public NonEmptyList<Option<B>> TraverseNonEmptyList<B>(Func<A, NonEmptyList<B>> f) =>
      IsEmpty ? Option<B>.Empty.NonEmptyListValue() : f(a).Select(q => q.Some());

    public Pair<X, Option<B>> TraversePair<X, B>(Func<A, Pair<X, B>> f, Monoid<X> m) =>
      IsEmpty ? m.Id.And(Option<B>.Empty) : f(a).Select(q => q.Some());

    public Func<X, Option<B>> TraverseFunc<X, B>(Func<A, Func<X, B>> f) {
      var z = this;
      return x =>
        z.IsEmpty ? Option<B>.Empty : f(z.a)(x).Some();
    }

    public Tree<Option<B>> TraverseTree<B>(Func<A, Tree<B>> f) =>
      IsEmpty ? Option<B>.Empty.TreeValue() : f(a).Select(q => q.Some());

    // Only instance of Option is Option.Empty. So return forall A.
    public static implicit operator Option<A>(Option o) => Empty;

    public static Option<A> Empty => new Option<A>(true, default(A));

    public static Option<A> Some(A t) => new Option<A>(false, t);

    private A Value {
      get {
        if(IsEmpty)
          throw new Exception("Value on empty Option");
        else
          return a;
      }
    }

    private IEnumerable<A> Enumerate() {
      if (IsNotEmpty) yield return a;
    }

    public IEnumerator<A> GetEnumerator() => Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();

  }

  public static class OptionExtension {
    public static Option<B> Select<A, B>(this Option<A> k, Func<A, B> f) =>
      k.Fold<Option<B>>(a => Option<B>.Some(f(a)), () => Option.Empty);

    public static Option<B> SelectMany<A, B>(this Option<A> k, Func<A, Option<B>> f) =>
      k.Fold(f, () => Option.Empty);

    public static Option<C> SelectMany<A, B, C>(this Option<A> k, Func<A, Option<B>> p, Func<A, B, C> f) =>
      SelectMany(k, a => Select(p(a), b => f(a, b)));

    public static Option<B> Apply<A, B>(this Option<Func<A, B>> f, Option<A> o) =>
      f.SelectMany(g => o.Select(p => g(p)));

    public static Option<A> Flatten<A>(this Option<Option<A>> o) =>
      o.SelectMany(z => z);

    public static Pair<Option<A>, Option<B>> Unzip<A, B>(this Option<Pair<A, B>> p) =>
      p.IsEmpty ?
        Option<A>.Empty.And(Option<B>.Empty) :
        Option<A>.Empty.And(Option<B>.Empty);

    public static Option<A> FromNull<A>(this A a) =>
      ReferenceEquals(null, a) ? Option<A>.Empty : Option<A>.Some(a);

    public static Option<A> Some<A>(this A a) => Option<A>.Some(a);

  }
}