using System;

namespace XSharpx {
  public struct Input<E> {
    private readonly Option<Option<E>> val;

    private Input(Option<Option<E>> val) {
      this.val = val;
    }

    public bool IsEmpty {
      get {
        return val.IsEmpty;
      }
    }

    public bool IsEof {
      get {
        return val.Any(o => o.IsEmpty);
      }
    }

    public bool IsElement {
      get {
        return val.Any(o => o.IsNotEmpty);
      }
    }

    public Option<E> InputElement {
      get {
        return val.Flatten();
      }
    }

    public Iteratee<E, A> Done<A>(A a) {
      return Iteratee<E, A>.Done(a, this);
    }

    public static Input<E> Empty() {
      return new Input<E>(Option.Empty);
    }

    public static Input<E> Eof() {
      return new Input<E>(Option.Some(Option<E>.Empty));
    }

    public static Input<E> Element(E e) {
      return new Input<E>(Option.Some(Option.Some(e)));
    }
  }

  public struct Iteratee<E, A> {
    private readonly Either<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>> val;

    private Iteratee(Either<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>> val) {
      this.val = val;
    }

    public Option<Pair<A, Input<E>>> DoneT {
      get {
        return val.Swap.ToOption;
      }
    }

    public Option<A> DoneA {
      get {
        return DoneT.Select(x => x._1.Get);
      }
    }

    public Option<Input<E>> DoneI {
      get {
        return DoneT.Select(x => x._2.Get);
      }
    }

    public bool IsDone {
      get {
        return DoneI.IsNotEmpty;
      }
    }

    public bool IsDoneEmpty {
      get {
        return DoneI.Any(i => i.IsEmpty);
      }
    }

    public bool IsDoneEof {
      get {
        return DoneI.Any(i => i.IsEof);
      }
    }

    public bool isDoneElement {
      get {
        return DoneI.Any(i => i.IsElement);
      }
    }

    public Option<E> InputElement {
      get {
        return DoneI.SelectMany(i => i.InputElement);
      }
    }

    public Option<Func<Input<E>, Iteratee<E, A>>> ContT {
      get {
        return val.ToOption;
      }
    }

    public bool IsCont {
      get {
        return ContT.IsNotEmpty;
      }
    }

    public Option<Iteratee<E, A>> ApplyCont(Input<E> i) {
      return ContT.Select(f => f(i));
    }

    public static Iteratee<E, A> Done(A a, Input<E> i) {
      return new Iteratee<E, A>(Pair<A, Input<E>>.pair(a, i).Left<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>>());
    }

    public static Iteratee<E, A> Cont(Func<Input<E>, Iteratee<E, A>> c) {
      return new Iteratee<E, A>(c.Right<Pair<A, Input<E>>, Func<Input<E>, Iteratee<E, A>>>());
    }
  }

}
