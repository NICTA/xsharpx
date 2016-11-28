using System;

namespace XSharpx {
  public struct WriteOp<A> {
    private readonly char c;
    private readonly A val;
    private readonly bool o;

    internal WriteOp(char c, A val, bool o) {
      this.c = c;
      this.val = val;
      this.o = o;
    }

    public Store<char, WriteOp<A>> Character {
      get {
        var t = this;
        return c.StoreSet(x => new WriteOp<A>(x, t.val, t.o));
      }
    }

    public Store<A, WriteOp<A>> Value {
      get {
        var t = this;
        return val.StoreSet(x => new WriteOp<A>(t.c, x, t.o));
      }
    }

    public bool IsOut => o;

    public bool IsErr => !o;

    public WriteOp<A> SetOut => new WriteOp<A>(c, val, true);

    public WriteOp<A> SetErr => new WriteOp<A>(c, val, false);

    public WriteOp<A> Redirect => new WriteOp<A>(c, val, !o);

    public static WriteOp<A> Out(char c, A val) => new WriteOp<A>(c, val, true);

    public static WriteOp<A> Err(char c, A val) => new WriteOp<A>(c, val, false);

    public WriteOp<B> Extend<B>(Func<WriteOp<A>, B> f) {
      var b = f(this);
      return new WriteOp<B>(c, b, o);
    }

    public WriteOp<WriteOp<A>> Duplicate => Extend(z => z);

    public Op<A> Op => new Op<A>(this.Right<Either<Func<int, A>, Func<string, A>>, WriteOp<A>>());
  
  }

  public static class WriteOpExtension {
    public static WriteOp<B> Select<A, B>(this WriteOp<A> k, Func<A, B> f) =>
      new WriteOp<B>(k.Character.Get, f(k.Value.Get), k.IsOut);

  }

  public struct Op<A> {
    private readonly Either<Either<Func<int, A>, Func<string, A>>, WriteOp<A>> val;

    internal Op(Either<Either<Func<int, A>, Func<string, A>>, WriteOp<A>> val) {
      this.val = val;
    }

    public X Fold<X>(Func<Func<int, A>, X> r, Func<Func<string, A>, X> rl, Func<WriteOp<A>, X> p) =>
      val.Fold(d => d.Fold(r, rl), p);

    public bool IsWriteOut => val.Any(o => o.IsOut);

    public bool IsWriteErr => val.Any(o => o.IsErr);

    public bool IsWrite => val.IsRight;

    public bool IsRead => val.Swap.Any(o => o.IsLeft);

    public bool IsReadLine => val.Swap.Any(o => o.IsRight);

    public Option<WriteOp<A>> WriteOp => val.ToOption;

    public Option<char> WriteOpCharacter => WriteOp.Select(o => o.Character.Get);

    public Option<A> WriteOpValue => WriteOp.Select(o => o.Value.Get);

    public Option<Func<int, A>> ReadValue => val.Swap.ToOption.SelectMany(f => f.Swap.ToOption);

    public Option<Func<string, A>> ReadLineValue => val.Swap.ToOption.SelectMany(f => f.ToOption);

    public Terminal<A> Lift => new Terminal<A>(this.Select(a => Terminal<A>.Done(a)).Right<A, Op<Terminal<A>>>());

    public static Op<A> Read(Func<int, A> f) =>
      new Op<A>(f.Left<Func<int, A>, Func<string, A>>().Left<Either<Func<int, A>, Func<string, A>>, WriteOp<A>>()); /// new Op<A>(f.Left<Func<int, A>, WriteOp<A>>());


    public static Op<A> ReadLine(Func<string, A> f) =>
      new Op<A>(f.Right<Func<int, A>, Func<string, A>>().Left<Either<Func<int, A>, Func<string, A>>, WriteOp<A>>()); /// new Op<A>(f.Left<Func<int, A>, WriteOp<A>>());

    public static Op<A> WriteOut(char c, A a) =>
      new Op<A>(WriteOp<A>.Out(c, a).Right<Either<Func<int, A>, Func<string, A>>, WriteOp<A>>());

    public static Op<A> WriteErr(char c, A a) =>
      new Op<A>(WriteOp<A>.Err(c, a).Right<Either<Func<int, A>, Func<string, A>>, WriteOp<A>>());

  }

  public static class OpExtension {
    public static Op<B> Select<A, B>(this Op<A> k, Func<A, B> f) =>
      k.Fold(q => Op<B>.Read(f.Compose(q)), q => Op<B>.ReadLine(f.Compose(q)), o => o.Select(f).Op);

  }

  public struct Terminal<A> {
    internal readonly Either<A, Op<Terminal<A>>> val;

    internal Terminal(Either<A, Op<Terminal<A>>> val) {
      this.val = val;
    }

    public Terminal<C> ZipWith<B, C>(Terminal<B> o, Func<A, Func<B, C>> f) =>
      this.SelectMany(a => o.Select(b => f(a)(b)));

    public Terminal<Pair<A, B>> Zip<B>(Terminal<B> o) => ZipWith(o, Pair<A, B>.pairF());

    public static Terminal<A> Done(A a) => new Terminal<A>(a.Left<A, Op<Terminal<A>>>());

    internal static Terminal<A> More(Op<Terminal<A>> o) => new Terminal<A>(o.Right<A, Op<Terminal<A>>>());

    public static Terminal<Unit> WriteOut(char c) => Op<Unit>.WriteOut(c, Unit.Value).Lift;

    public static Terminal<Unit> WriteErr(char c) => Op<Unit>.WriteErr(c, Unit.Value).Lift;

    public static Terminal<int> Read => Op<int>.Read(c => c).Lift;

    public static Terminal<string> ReadLine => Op<string>.ReadLine(c => c).Lift;

    // CAUTION: unsafe (perform I/O)
    //
    // This function stands in for the interpreter of 'Terminal programs'.
    public A Run =>
      val.Swap.Reduce(o => o.Fold(
        r => {
          var c = Console.Read();
          return r(c).Run;
        }
        , r => {
          var s = Console.ReadLine();
          return r(s).Run;
        }
        , r => {
          if(r.IsOut) {
            Console.Write(r.Character);
            return r.Value.Get.Run;
          } else {
            Console.Error.Write(r.Character);
            return r.Value.Get.Run;
          }
        }
        ));
     
    // Select, SelectMany, Apply, Zip and friends, 8Monad functions
  }

  public static class TerminalExtension {
    public static Terminal<B> Select<A, B>(this Terminal<A> k, Func<A, B> f) =>
      k.val.Fold(
        a => Terminal<B>.Done(f(a))
      , o => Terminal<B>.More(o.Select(t => t.Select(f)))
      );

    public static Terminal<B> SelectMany<A, B>(this Terminal<A> k, Func<A, Terminal<B>> f) =>
      k.val.Fold(
        f
      , o => Terminal<B>.More(o.Select(t => t.SelectMany(f)))
      );

    public static Terminal<C> SelectMany<A, B, C>(this Terminal<A> k, Func<A, Terminal<B>> p, Func<A, B, C> f) =>
      SelectMany(k, a => Select(p(a), b => f(a, b)));

    public static Terminal<B> Apply<A, B>(this Terminal<Func<A, B>> f, Terminal<A> o) => f.SelectMany(g => o.Select(p => g(p)));

    public static Terminal<A> Flatten<A>(this Terminal<Terminal<A>> o) => o.SelectMany(z => z);

    public static Terminal<A> TerminalValue<A>(this A a) => Terminal<A>.Done(a);
  }
}
