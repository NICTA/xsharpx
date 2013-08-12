using System;

namespace XSharpx {
  public struct Possible<R, A> {
    private readonly Func<R, Option<A>> runReader;

    public Possible(Func<R, Option<A>> runReader) { this.runReader = runReader; }

    public Option<A> Run(R r) { return runReader(r); }

    public Possible<R, B> Select<B>(Func<A, B> f) {
      var self = this;
      return new Possible<R, B>(r => self.Run(r).Select(f));
    }

    public Possible<R, B> SelectMany<B>(Func<A, Possible<R, B>> f) {
      return SelectMany(f, (a, b) => b);
    }

    public Possible<R, C> SelectMany<B, C>(Func<A, Possible<R, B>> f, Func<A, B, C> selector) {
      var self = this;
      Func<R, Option<C>> func = r => {
        var maybeA = self.Run(r);
        var maybeB = maybeA.SelectMany(a => f(a).Run(r));
        return maybeA.Select(a => maybeB.Select(b => selector(a, b))).Flatten();
      };
      return new Possible<R, C>(func);
    }
  }
}