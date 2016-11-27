using System;
using System.Collections;
using System.Collections.Generic;

namespace XSharpx {
    
  public struct Input<E> : IEnumerable<E> {
    internal readonly Option<Option<E>> val;

    internal Input(Option<Option<E>> val) {
      this.val = val;
    }

    public X Fold<X>(Func<X> empty, Func<X> eof, Func<E, X> element) =>
      val.Fold<X>(o => o.Fold(element, eof), empty);

    public bool IsEmpty => val.IsEmpty;

    public bool IsEof => val.Any(o => o.IsEmpty);

    public bool IsElement => val.Any(o => o.IsNotEmpty);

    public Option<E> InputElement => val.Flatten();

    public E InputElementOr(Func<E> f) => InputElement.ValueOr(f);

    public Input<E> OrElse(Func<Input<E>> i) => IsElement ? this : i();

    public Input<E> Append(Input<E> o, Semigroup<E> m) => m.Input.Op(this, o);

    public Iteratee<E, A> Done<A>(A a) => Iteratee<E, A>.Done(a, this);

    public Input<E> Where(Func<E, bool> p) {
      var z = InputElement;

      return IsEmpty || IsEof ? this : z.Any(p) ? this : Empty();
    }

    public Input<Input<E>> Duplicate => this.Select(a => Element(a));

    public Input<B> Extend<B>(Func<Input<E>, B> f) => this.Select(a => f(Element(a)));

    public Input<C> ZipWith<B, C>(Input<B> o, Func<E, Func<B, C>> f) => this.SelectMany(a => o.Select(b => f(a)(b)));

    public Input<Pair<E, B>> Zip<B>(Input<B> o) => ZipWith(o, Pair<E, B>.pairF());

    public bool Any(Func<E, bool> p) => val.Any(o => o.Any(p));

    public bool All(Func<E, bool> p) => val.All(o => o.All(p));

    public void ForEach(Action<E> a) {
      val.ForEach(o => o.ForEach(a));
    }

    public List<E> ToList => InputElement.ToList;

    public IEnumerator<E> GetEnumerator() => val.Flatten().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public List<Input<B>> TraverseList<B>(Func<E, List<B>> f) =>
      val.TraverseList(o => o.TraverseList(f)).Select(o => new Input<B>(o));

    public Option<Input<B>> TraverseOption<B>(Func<E, Option<B>> f) =>
      val.TraverseOption(o => o.TraverseOption(f)).Select(o => new Input<B>(o));

    public Terminal<Input<B>> TraverseTerminal<B>(Func<E, Terminal<B>> f) =>
      val.TraverseTerminal(o => o.TraverseTerminal(f)).Select(o => new Input<B>(o));

    public Input<Input<B>> TraverseInput<B>(Func<E, Input<B>> f) =>
      val.TraverseInput(o => o.TraverseInput(f)).Select(o => new Input<B>(o));

    public Either<X, Input<B>> TraverseEither<X, B>(Func<E, Either<X, B>> f) =>
      val.TraverseEither(o => o.TraverseEither(f)).Select(o => new Input<B>(o));

    public NonEmptyList<Input<B>> TraverseNonEmptyList<B>(Func<E, NonEmptyList<B>> f) =>
      val.TraverseNonEmptyList(o => o.TraverseNonEmptyList(f)).Select(o => new Input<B>(o));

    public Pair<X, Input<B>> TraversePair<X, B>(Func<E, Pair<X, B>> f, Monoid<X> m) =>
      val.TraversePair(o => o.TraversePair(f, m), m).Select(o => new Input<B>(o));

    public Func<X, Input<B>> TraverseFunc<X, B>(Func<E, Func<X, B>> f) =>
      val.TraverseFunc(o => o.TraverseFunc(f)).Select(o => new Input<B>(o));


    public Tree<Input<B>> TraverseTree<B>(Func<E, Tree<B>> f) =>
      val.TraverseTree(o => o.TraverseTree(f)).Select(o => new Input<B>(o));

    public static Input<E> Empty() => new Input<E>(Option.Empty);

    public static Input<E> Eof() => new Input<E>(Option.Some(Option<E>.Empty));

    public static Input<E> Element(E e) => new Input<E>(Option.Some(Option.Some(e)));

  }

  public static class InputExtension {
    public static Input<B> Select<A, B>(this Input<A> k, Func<A, B> f) =>
      new Input<B>(k.val.Select(o => o.Select(f)));

    public static Input<B> SelectMany<A, B>(this Input<A> k, Func<A, Input<B>> f) =>
      new Input<B>(k.val.SelectMany(o => o.SelectMany(a => f(a).val)));

    public static Input<C> SelectMany<A, B, C>(this Input<A> k, Func<A, Input<B>> p, Func<A, B, C> f) =>
      SelectMany(k, a => Select(p(a), b => f(a, b)));

    public static Input<B> Apply<A, B>(this Input<Func<A, B>> f, Input<A> o) =>
      f.SelectMany(g => o.Select(p => g(p)));

    public static Input<A> Flatten<A>(this Input<Input<A>> o) =>
      o.SelectMany(z => z);

    public static Input<A> InputElement<A>(this A a) => Input<A>.Element(a);

  }

  public struct Iteratee<E, A> {
    private readonly Either<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>> val;

    private Iteratee(Either<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>> val) {
      this.val = val;
    }

    public Either<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>> ToEither => val;

    public X Fold<X>(Func<A, Input<E>, X> done, Func<Func<Input<E>, Iteratee<E, A>>, X> cont) => 
      val.Fold(pair => pair.Fold(done), cont);

    public Iteratee<E, C> ZipWith<B, C>(Iteratee<E, B> o, Func<A, Func<B, C>> f) =>
      this.SelectMany(a => o.Select(b => f(a)(b)));

    public Iteratee<D, A> ContraSelect<D>(Func<D, E> f) =>
      Fold(
        (a, i) => Iteratee<D, A>.Done(a, i.IsEof ? Input<D>.Eof() : Input<D>.Empty())
      , r => Iteratee<D, A>.Cont(i => r(i.Select(f)).ContraSelect(f))
      );

    public Iteratee<E, Pair<A, B>> Zip<B>(Iteratee<E, B> o) => ZipWith(o, Pair<A, B>.pairF());

    public Option<Pair<A, Input<E>>> DoneT => val.Swap.ToOption;

    public Option<A> DoneA => DoneT.Select(x => x._1.Get);

    public Option<Input<E>> DoneI => DoneT.Select(x => x._2.Get);

    public bool IsDone => DoneI.IsNotEmpty;

    public bool IsDoneEmpty => DoneI.Any(i => i.IsEmpty);

    public bool IsDoneEof => DoneI.Any(i => i.IsEof);
    public bool isDoneElement => DoneI.Any(i => i.IsElement);

    public Option<E> InputElement => DoneI.SelectMany(i => i.InputElement);

    public Option<Func<Input<E>, Iteratee<E, A>>> ContT => val.ToOption;

    public bool IsCont => ContT.IsNotEmpty;

    public Option<Iteratee<E, A>> ApplyCont(Input<E> i) => ContT.Select(f => f(i));

    public static Iteratee<E, A> Done(A a, Input<E> i) =>
      new Iteratee<E, A>(Pair<A, Input<E>>.pair(a, i).Left<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>>());

    public static Iteratee<E, A> Cont(Func<Input<E>, Iteratee<E, A>> c) =>
      new Iteratee<E, A>(c.Right<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>>());

}

  public static class Iteratee {

    public static Iteratee<A, Option<A>> Head<A>() {
        Func<Input<A>, Iteratee<A, Option<A>>> step = default(Func<Input<A>, Iteratee<A, Option<A>>>);
        step = 
        i => i.Fold(
          () => Iteratee<A, Option<A>>.Cont(step)
        , () => Iteratee<A, Option<A>>.Done(Option.Empty, Input<A>.Eof())
        , e => Iteratee<A, Option<A>>.Done(Option.Some(e), Input<A>.Empty())
        );

      return Iteratee<A, Option<A>>.Cont(step);
    }

    public static Iteratee<A, Option<A>> Peek<A>() {
        Func<Input<A>, Iteratee<A, Option<A>>> step = default(Func<Input<A>, Iteratee<A, Option<A>>>);
        step = i => i.Fold(
          () => Iteratee<A, Option<A>>.Cont(step)
        , () => Iteratee<A, Option<A>>.Done(Option.Empty, Input<A>.Eof())
        , e => Iteratee<A, Option<A>>.Done(Option.Some(e), i)
        );

      return Iteratee<A, Option<A>>.Cont(step);
    }

    public static Iteratee<A, Unit> Drop<A>(int n) {
      Iteratee<A, Unit> eof = Iteratee<A, Unit>.Done(default(Unit), Input<A>.Eof());
      Iteratee<A, Unit> empty = Iteratee<A, Unit>.Done(default(Unit), Input<A>.Empty());
      Func<Input<A>, Iteratee<A, Unit>> step = default(Func<Input<A>, Iteratee<A, Unit>>);
        step =
        i => i.Fold<Iteratee<A, Unit>>(
          () => Iteratee<A, Unit>.Cont(step)
        , () => eof
        , _ => Drop<A>(n - 1)
        );

      return n == 0 ?
        empty :
        Iteratee<A, Unit>.Cont(step);
    }

    public static Iteratee<A, Unit> DropWhile<A>(Func<A, bool> p) {
      Func<Input<A>, Iteratee<A, Unit>> v = i => Iteratee<A, Unit>.Done(default(Unit), i);
      var eof = v(Input<A>.Eof());
      Func<Input<A>, Iteratee<A, Unit>> step = default(Func<Input<A>, Iteratee<A, Unit>>);
        step = 
        i => i.Fold<Iteratee<A, Unit>>(
          () => Iteratee<A, Unit>.Cont(step)
        , () => eof
        , e => p(e) ? DropWhile(p) : v(i)
        );

      return Iteratee<A, Unit>.Cont(step);
    }

    public static Iteratee<E, A> Fold<E, A>(Func<A, E, A> f, A z) {
        Func<A, Input<E>, Iteratee<E, A>> step = default(Func<A, Input<E>, Iteratee<E, A>>);
        step = (acc, i) => i.Fold(
          () => Iteratee<E, A>.Cont(j => step(acc, j))
        , () => Iteratee<E, A>.Done(acc, Input<E>.Eof())
        , e => Iteratee<E, A>.Cont(j => step(f(acc, e), j))
        );

      return Iteratee<E, A>.Cont(i => step(z, i));
    }

    public static Iteratee<A, A> Sum<A>(Monoid<A> m) => Fold(m.Op, m.Id);

  }

  public static class IterateeExtension {
    public static Iteratee<E, B> Select<E, A, B>(this Iteratee<E, A> k, Func<A, B> f) =>
      k.SelectMany(a => Iteratee<E, B>.Done(f(a), Input<E>.Empty()));

    public static Iteratee<E, B> SelectMany<E, A, B>(this Iteratee<E, A> k, Func<A, Iteratee<E, B>> f) =>
      k.Fold(
        (a, i) => i.IsEmpty ?
                  f(a) :
                  f(a).Fold(
                    (b, _) => Iteratee<E, B>.Done(b, i)
                    , s => s(i)
                    )
        , r => Iteratee<E, B>.Cont(u => r(u).SelectMany(f))
      );

    public static Iteratee<E, C> SelectMany<E, A, B, C>(this Iteratee<E, A> k, Func<A, Iteratee<E, B>> p, Func<A, B, C> f) =>
      SelectMany(k, a => Select(p(a), b => f(a, b)));

    public static Iteratee<E, B> Apply<E, A, B>(this Iteratee<E, Func<A, B>> f, Iteratee<E, A> o) =>
      f.SelectMany(g => o.Select(p => g(p)));

    public static Iteratee<E, A> Flatten<E, A>(this Iteratee<E, Iteratee<E, A>> o) => o.SelectMany(z => z);

  }
}
