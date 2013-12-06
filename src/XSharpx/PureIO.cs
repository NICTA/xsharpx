using System;

namespace PureIO {
  /*
    C# does not have proper sum types. They must be emulated.

    This data type is one of 4 possible values:
    - WriteOut, being a pair of a string and A
    - WriteErr, being a pair of a string and A
    - readLine, being a function from string to A
    - read, being a function from int to A

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

    /*
      This data type is a functor.
      This is all that is necessary to provide the grammar (Terminal).
      Note that Terminal uses only this `Select` method to implement `SelectMany`
    */
    public TerminalOperation<B> Select<B>(Func<A, B> f) {
      return Fold<TerminalOperation<B>>(
        (s, a) => new TerminalOperation<B>.WriteOut(s, f(a))
      , (s, a) => new TerminalOperation<B>.WriteErr(s, f(a))
      , g => new TerminalOperation<B>.ReadLine(s => f(g(s)))
      , g => new TerminalOperation<B>.Read(i => f(g(i)))
      );
    }

    private class WriteOut : TerminalOperation<A> {
      private string s;
      private A a;

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

    public static TerminalOperation<A> writeOut(string s, A a) {
      return new WriteOut(s, a);
    }

    private class WriteErr : TerminalOperation<A> {
      private string s;
      private A a;

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

    public static TerminalOperation<A> writeErr(string s, A a) {
      return new WriteErr(s, a);
    }

    private class ReadLine : TerminalOperation<A> {
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

    public static TerminalOperation<A> readLine(Func<string, A> f) {
      return new ReadLine(f);
    }

    private class Read : TerminalOperation<A> {
      private Func<int, A> f;

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

    public static TerminalOperation<A> read(Func<int, A> f) {
      return new Read(f);
    }
  }

  public abstract class Terminal<A> {
    public abstract X Fold<X>(
      Func<A, X> done
    , Func<TerminalOperation<Terminal<A>>, X> more
    );

    public Terminal<B> Select<B>(Func<A, B> f) {
      return Fold<Terminal<B>>(
        a => Terminal<B>.done(f(a))
      , a => Terminal<B>.more(a.Select(t => t.Select(f)))
      );
    }

    public Terminal<B> SelectMany<B>(Func<A, Terminal<B>> f) {
      return Fold<Terminal<B>>(
        f
      , a => Terminal<B>.more(a.Select(t => t.SelectMany(f)))
      );
    }

    private class Done : Terminal<A> {
      public A a;

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

    private class More : Terminal<A> {
      public TerminalOperation<Terminal<A>> a;

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

    /*
      CAUTION: This function is unsafe.
      It is the "end of the (Terminal) world" interpreter.

      Use this function to run the final terminal program.
      Ideally, this function would be hypothetical and unavailable
      to the programmer API (i.e. implemented in its own runtime).
    */
    public static A Interpret(Terminal<A> t) {
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
}
