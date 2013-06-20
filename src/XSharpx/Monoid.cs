using System;

namespace XSharpx {
  public struct Monoid<A> {
    private readonly Func<A, A, A> op;
    private readonly A id;

    private Monoid(Func<A, A, A> op, A id) {
      this.op = op;
      this.id = id;
    }

    public Semigroup<A> Semigroup {
      get {
        return Semigroup<A>.semigroup(op);
      }
    }

    public Func<A, A, A> Op {
      get {
        return op;
      }
    }

    public A Id {
      get {
        return id;
      }
    }

    public A Apply(A a1, A a2) {
      return Semigroup.Apply(a1, a2);
    }

    public Func<A, Func<A, A>> Curried {
      get {
        return Semigroup.Curried;
      }
    }

    public Func<A, A> Curried1(A a1) {
      return Semigroup.Curried1(a1);
    }

    public Monoid<A> Dual {
      get {
        return Semigroup.Dual.Monoid(id);
      }
    }

    public Func<A, A> Join {
      get {
        return Semigroup.Join;
      }
    }

    public Monoid<Pair<A, B>> Pair<B>(Monoid<B> s) {
      return Semigroup.Pair(s.Semigroup).Monoid(id.And(s.Id));
    }

    public Monoid<Option<A>> Option {
      get {
        return Semigroup.Option.Monoid(Option<A>.Empty);
      }
    }

    public Monoid<Input<A>> Input {
      get {
        return Semigroup.Input.Monoid(Input<A>.Empty());
      }
    }

    public Monoid<Func<B, A>> Pointwise<B>() {
      var t = this;
      return Semigroup.Pointwise<B>().Monoid(_ => t.id);
    }

    public Monoid<Func<B, C, A>> Pointwise2<B, C>() {
      var t = this;
      return Semigroup.Pointwise2<B, C>().Monoid((_, __) => t.id);
    }

    public Monoid<Func<B, C, D, A>> Pointwise3<B, C, D>() {
      var t = this;
      return Semigroup.Pointwise3<B, C, D>().Monoid((_, __, ___) => t.id);
    }

    public Monoid<B> XSelect<B>(Func<A, B> f, Func<B, A> g) {
      return Semigroup.XSelect(f, g).Monoid(f(id));
    }

    public static Monoid<A> monoid(Func<A, A, A> op, A id) {
      return new Monoid<A>(op, id);
    }

    public static Monoid<Option<A>> FirstOption {
      get {
        return Semigroup<A>.FirstOption.Monoid(Option<A>.Empty);
      }
    }

    public static Monoid<Option<A>> SecondOption {
      get {
        return Semigroup<A>.SecondOption.Monoid(Option<A>.Empty);
      }
    }

    public static Monoid<Func<A, A>> Endo {
      get {
        return Semigroup<A>.Endo.Monoid(a => a);
      }
    }

    public static Monoid<List<A>> List {
      get {
        return Semigroup<A>.List.Monoid(List<A>.Empty);
      }
    }
  }

  public static class Monoid {
    public static Monoid<bool> Or {
      get {
        return Semigroup.Or.Monoid(false);
      }
    }

    public static Monoid<bool> And {
      get {
        return Semigroup.And.Monoid(true);
      }
    }

    public static Monoid<string> String {
      get {
        return Semigroup.String.Monoid("");
      }
    }

    public static class Sum {
      public static Monoid<int> Integer {
        get {
          return Semigroup.Sum.Integer.Monoid(0);
        }
      }

      public static Monoid<byte> Byte {
        get {
          return Semigroup.Sum.Byte.Monoid(0);
        }
      }

      public static Monoid<short> Short {
        get {
          return Semigroup.Sum.Short.Monoid(0);
        }
      }

      public static Monoid<long> Long {
        get {
          return Semigroup.Sum.Long.Monoid(0);
        }
      }

      public static Monoid<char> Char {
        get {
          return Semigroup.Sum.Char.Monoid((char)0);
        }
      }
    }

    public static class Product {
      public static Monoid<int> Integer {
        get {
          return Semigroup.Product.Integer.Monoid(1);
        }
      }

      public static Monoid<byte> Byte {
        get {
          return Semigroup.Product.Byte.Monoid(1);
        }
      }

      public static Monoid<short> Short {
        get {
          return Semigroup.Product.Short.Monoid(1);
        }
      }

      public static Monoid<long> Long {
        get {
          return Semigroup.Product.Long.Monoid(1);
        }
      }

      public static Monoid<char> Char {
        get {
          return Semigroup.Product.Char.Monoid((char)1);
        }
      }
    }
  }
}

