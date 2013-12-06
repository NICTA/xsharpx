using System;

namespace PureIO {
  /*
    C# does not have proper sum types. They must be emulated.

    This data type is one of 4 possible values:
    - WriteOut, being a pair of a string and A
    - WriteErr, being a pair of a string and A
    - readLine, being a function from string to A
    - read, being a function from int to A

    It gives rise to a functor. See `Select` method.

    The Fold function deconstructs the data type into its one of 4 possibilities.
    The 4 static functions construct into one of the possibilities.
  */
  public abstract class TerminalOperation<A> {
    public abstract X Fold<X>(
      Func<string, A, X> writeOut
    , Func<string, A, X> writeErr
    , Func<Func<string, A>, X> readLine
    , Func<Func<int, A>, X> read
    );

    public Terminal<A> Lift {
      get {
        return Terminal<A>.more(this.Select<A, Terminal<A>>(Terminal<A>.done));
      }
    }

    internal class WriteOut : TerminalOperation<A> {
      private readonly string s;
      private readonly A a;

      public WriteOut(string s, A a) {
        this.s = s;
        this.a = a;
      }

      public override X Fold<X>(
        Func<string, A, X> writeOut
      , Func<string, A, X> writeErr
      , Func<Func<string, A>, X> readLine
      , Func<Func<int, A>, X> read
      ) {
        return writeOut(s, a);
      }
    }

    internal class WriteErr : TerminalOperation<A> {
      private readonly string s;
      private readonly A a;

      public WriteErr(string s, A a) {
        this.s = s;
        this.a = a;
      }

      public override X Fold<X>(
        Func<string, A, X> writeOut
      , Func<string, A, X> writeErr
      , Func<Func<string, A>, X> readLine
      , Func<Func<int, A>, X> read
      ) {
        return writeErr(s, a);
      }
    }

    internal class ReadLine : TerminalOperation<A> {
      private Func<string, A> f;

      public ReadLine(Func<string, A> f) {
        this.f = f;
      }

      public override X Fold<X>(
        Func<string, A, X> writeOut
      , Func<string, A, X> writeErr
      , Func<Func<string, A>, X> readLine
      , Func<Func<int, A>, X> read
      ) {
        return readLine(f);
      }
    }

    internal class Read : TerminalOperation<A> {
      private readonly Func<int, A> f;

      public Read(Func<int, A> f) {
        this.f = f;
      }

      public override X Fold<X>(
        Func<string, A, X> writeOut
      , Func<string, A, X> writeErr
      , Func<Func<string, A>, X> readLine
      , Func<Func<int, A>, X> read
      ) {
        return read(f);
      }
    }
  }

  public static class TerminalOperationFunctor {
    public static TerminalOperation<B> Select<A, B>(this TerminalOperation<A> o, Func<A, B> f) {
      /*
        The `TerminalOperation` data type is a functor.
        This is all that is necessary to provide the grammar (`Terminal`).
        Note that `Terminal` uses only `Select`
        (and no other `TerminalOperation` methods) to method to implement `SelectMany`
      */
      return o.Fold<TerminalOperation<B>>(
        (s, a) => new TerminalOperation<B>.WriteOut(s, f(a))
      , (s, a) => new TerminalOperation<B>.WriteErr(s, f(a))
      , g => new TerminalOperation<B>.ReadLine(s => f(g(s)))
      , g => new TerminalOperation<B>.Read(i => f(g(i)))
      );
    }
  }

  public abstract class Terminal<A> {
    public abstract X Fold<X>(
      Func<A, X> done
    , Func<TerminalOperation<Terminal<A>>, X> more
    );

    internal class Done : Terminal<A> {
      public readonly A a;

      public Done(A a) {
        this.a = a;
      }

      override public X Fold<X>(
        Func<A, X> done
      , Func<TerminalOperation<Terminal<A>>, X> more
      ) {
        return done(a);
      }
    }

    public static Terminal<A> done(A a) {
      return new Done(a);
    }

    internal class More : Terminal<A> {
      public readonly TerminalOperation<Terminal<A>> a;

      public More(TerminalOperation<Terminal<A>> a) {
        this.a = a;
      }

      override public X Fold<X>(
        Func<A, X> done
      , Func<TerminalOperation<Terminal<A>>, X> more
      ) {
        return more(a);
      }
    }

    public static Terminal<A> more(TerminalOperation<Terminal<A>> a) {
      return new More(a);
    }
  }

  public static class Terminal {
    public static Terminal<Unit> WriteOut(string s) {
      return new TerminalOperation<Unit>.WriteOut(s, Unit.Value).Lift;
    }

    public static Terminal<Unit> WriteErr(string s) {
      return new TerminalOperation<Unit>.WriteErr(s, Unit.Value).Lift;
    }

    public static Terminal<string> ReadLine {
      get {
        return new TerminalOperation<string>.ReadLine(s => s).Lift;
      }
    }

    public static Terminal<int> Read {
      get {
        return new TerminalOperation<int>.Read(i => i).Lift;
      }
    }

    public static Terminal<Unit> WriteOut() {
      return WriteOut("");
    }
  }

  public static class TerminalFunctor {
    public static Terminal<B> Select<A, B>(this Terminal<A> t, Func<A, B> f) {
      return t.Fold<Terminal<B>>(
        a => Terminal<B>.done(f(a))
      , a => Terminal<B>.more(a.Select(k => k.Select(f)))
      );
    }

    /*
      The monad for Terminal.

      Note that `TerminalOperation#Select` is the only method that is specific to `TerminalOperation`.
      More to the point, some other structure with a `Select` method could be
      substituted here to give rise to a different kind of behaviour.
    */
    public static Terminal<B> SelectMany<A, B>(this Terminal<A> t, Func<A, Terminal<B>> f) {
      return t.Fold<Terminal<B>>(
        f
      , a => Terminal<B>.more(a.Select(k => k.SelectMany(f)))
      );
    }

    public static Terminal<C> SelectMany<A, B, C>(this Terminal<A> t, Func<A, Terminal<B>> u, Func<A, B, C> f) {
      return SelectMany(t, a => Select(u(a), b => f(a, b)));
    }
  }

  public static class TerminalInterpreter {
    /*
      CAUTION: This function is unsafe.
      It is the "end of the (Terminal) world" interpreter.

      Use this function to run the final terminal program.
      Ideally, this function would be hypothetical and unavailable
      to the programmer API (i.e. implemented in its own runtime).
    */
    public static A Interpret<A>(this Terminal<A> t) {
      return t.Fold<A>(
        a => a
      , a => a.Fold<A>(
          (s, tt) => {
            Console.WriteLine(s);
            return Interpret(tt);
          }
        , (s, tt) => {
            Console.Error.WriteLine(s);
            return Interpret(tt);
          }
        , f => {
            var s = Console.ReadLine();
            return Interpret(f(s));
          }
        , f => {
            var i = Console.Read();
            return Interpret(f(i));
          }
        )
      );
    }
  }

  /*
    A data structure with only one possible value.
    It is similar to `void` but this can be used as a regular data type.
  */
  public struct Unit {
    public static readonly Unit Value = new Unit();
  }

  public class Previously {
    static void WriteOut(string s) { Console.WriteLine(s); }
    static void WriteOut() { WriteOut(""); }
    static void WriteErr(string s) { Console.Error.WriteLine(s); }
    static string ReadLine() { return Console.ReadLine(); }
    static int Read() { return Console.Read(); }

    public static int FormerlyMain() {
      WriteOut("Hello, let us begin");
      WriteOut("Please enter your name");
      var name = ReadLine();
      WriteOut("How old are you?");
      var age = ReadLine();
      WriteOut("Okey dokey, ready to tell the world?");
      WriteOut("0. No");
      WriteOut("1. Yes");
      var r = Read();
      WriteOut();
      if(r == '0')
        WriteErr(name + " is modest");
      else
        WriteOut(name + " is " + age + " years old");
      return r - 48;
    }
  }

  public class Demonstration {
    public static int Main() {
      Terminal<int> Program =
        from _1    in Terminal.WriteOut("Hello, let us begin")
        from _2    in Terminal.WriteOut("Please enter your name")
        from name  in Terminal.ReadLine
        from _3    in Terminal.WriteOut("How old are you?")
        from age   in Terminal.ReadLine
        from _4    in Terminal.WriteOut("Okey dokey, ready to tell the world?")
        from _5    in Terminal.WriteOut("0. No")
        from _6    in Terminal.WriteOut("1. Yes")
        from r     in Terminal.Read
        from _7    in Terminal.WriteOut()
        from _8    in r == '0' ?
                        Terminal.WriteErr(name + " is modest") :
                        Terminal.WriteOut(name + " is " + age + " years old")
        select r - 48;

      return Program.Interpret();
    }
  }

}
