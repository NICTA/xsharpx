using System;

namespace XSharpx {
  public struct Semigroup<A> {
    private readonly Func<A, A, A> op;

    private Semigroup(Func<A, A, A> op) {
      this.op = op;
    }

    public Func<A, A, A> Op {
      get {
        return op;
      }
    }

    public A Apply(A a1, A a2) {
      return op(a1, a2);
    }

    public Func<A, Func<A, A>> Curried {
      get {
        return op.Curry();
      }
    }

    public Func<A, A> Curried1(A a1) {
      return Curried(a1);
    }

    public Semigroup<A> Dual {
      get {
        var t = this;
        return semigroup((a1, a2) => t.op(a2, a1));
      }
    }

    public Func<A, A> Join {
      get {
        var t = this;
        return a => t.op(a, a);
      }
    }

    public Semigroup<Pair<A, B>> Pair<B>(Semigroup<B> s) {
      var t = this;
      return new Semigroup<Pair<A, B>>((p1, p2) => t.op(p1._1.Get, p2._1.Get).And(s.op(p1._2.Get, p2._2.Get)));
    }

    public Semigroup<Option<A>> Option {
      get {
        var t = this;
        return new Semigroup<Option<A>>((o1, o2) => o1.Fold<Option<A>>(a1 => o2.OrElse(() => o1).Select(a2 => t.op(a1, a2)), () => o2));
      }
    }

    public Semigroup<Input<A>> Input {
      get {
        var t = this;
        return new Semigroup<Input<A>>((i1, i2) => new Input<A>(t.Option.Option.Op(i1.val, i2.val)));
      }
    }

    public Semigroup<Func<B, A>> Pointwise<B>() {
      var t = this;
      return new Semigroup<Func<B, A>>((f1, f2) => b => t.op(f1(b), f2(b)));
    }

    public Semigroup<Func<B, C, A>> Pointwise2<B, C>() {
      var t = this;
      return new Semigroup<Func<B, C, A>>((f1, f2) => (b, c) => t.op(f1(b, c), f2(b, c)));
    }

    public Semigroup<Func<B, C, D, A>> Pointwise3<B, C, D>() {
      var t = this;
      return new Semigroup<Func<B, C, D, A>>((f1, f2) => (b, c, d) => t.op(f1(b, c, d), f2(b, c, d)));
    }

    public Semigroup<B> XSelect<B>(Func<A, B> f, Func<B, A> g) {
      var t = this;
      return new Semigroup<B>((b1, b2) => f(t.op(g(b1), g(b2))));
    }

    public Monoid<A> Monoid(A id) {
      return Monoid<A>.monoid(op, id);
    }

    public Reducer<Q, A> Reducer<Q>(Func<Q, A> unit) {
      return Reducer<Q, A>.reducer(op, unit);
    }

    public Reducer<A, A> IdReducer {
      get {
        return Reducer<A>(a => a);
      }
    }

    public static Semigroup<A> semigroup(Func<A, A, A> op) {
      return new Semigroup<A>(op);
    }

    public static Semigroup<A> Constant(Func<A> a) {
      return semigroup((a1, a2) => a());
    }

    public static Semigroup<A> Split1(Func<A, A> f) {
      return semigroup((a1, _) => f(a1));
    }

    public static Semigroup<A> Split2(Func<A, A> f) {
      return semigroup((_, a2) => f(a2));
    }

    public static Semigroup<A> First {
      get {
        return semigroup((a1, _) => a1);
      }
    }

    public static Semigroup<A> Last {
      get {
        return semigroup((_, a2) => a2);
      }
    }

    public static Semigroup<Option<A>> FirstOption {
      get {
        return new Semigroup<Option<A>>((o1, o2) => o1.OrElse(() => o2));
      }
    }

    public static Semigroup<Option<A>> SecondOption {
      get {
        return new Semigroup<Option<A>>((o1, o2) => o2.OrElse(() => o1));
      }
    }

    public static Semigroup<Func<A, A>> Endo {
      get {
        return new Semigroup<Func<A, A>>((f1, f2) => f1.Compose(f2));
      }
    }

    public static Semigroup<List<A>> List {
      get {
        return new Semigroup<List<A>>((s1, s2) => s1.Append(s2));
      }
    }
  }

  public static class Semigroup {
    public static Semigroup<bool> Or {
      get {
        return Semigroup<bool>.semigroup((p, q) => p || q);
      }
    }

    public static Semigroup<bool> And {
      get {
        return Semigroup<bool>.semigroup((p, q) => p && q);
      }
    }

    public static Semigroup<string> String {
      get {
        return Semigroup<string>.semigroup((s1, s2) => s1 + s2);
      }
    }

    public static class Sum {
      public static Semigroup<int> Integer {
        get {
          return Semigroup<int>.semigroup((x, y) => x + y);
        }
      }

      public static Semigroup<byte> Byte {
        get {
          return Semigroup<byte>.semigroup((x, y) => (byte)(x + y));
        }
      }

      public static Semigroup<short> Short {
        get {
          return Semigroup<short>.semigroup((x, y) => (short)(x + y));
        }
      }

      public static Semigroup<long> Long {
        get {
          return Semigroup<long>.semigroup((x, y) => x + y);
        }
      }

      public static Semigroup<char> Char {
        get {
          return Semigroup<char>.semigroup((x, y) => (char)(x + y));
        }
      }
    }

    public static class Product {
      public static Semigroup<int> Integer {
        get {
          return Semigroup<int>.semigroup((x, y) => x * y);
        }
      }

      public static Semigroup<byte> Byte {
        get {
          return Semigroup<byte>.semigroup((x, y) => (byte)(x * y));
        }
      }

      public static Semigroup<short> Short {
        get {
          return Semigroup<short>.semigroup((x, y) => (short)(x * y));
        }
      }

      public static Semigroup<long> Long {
        get {
          return Semigroup<long>.semigroup((x, y) => x * y);
        }
      }

      public static Semigroup<char> Char {
        get {
          return Semigroup<char>.semigroup((x, y) => (char)(x * y));
        }
      }
    }
  }
}

