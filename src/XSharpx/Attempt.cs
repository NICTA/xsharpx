using System;

namespace XSharpx {
  public struct Attempt<R, A> {
    private readonly Func<R, Option<A>> runAttempt;

    public Attempt(Func<R, Option<A>> runAttempt) { this.runAttempt = runAttempt; }

    public Option<A> Run(R r) { return runAttempt(r); }

    public Attempt<R, B> Select<B>(Func<A, B> f) {
      var self = this;
      return new Attempt<R, B>(r => self.Run(r).Select(f));
    }

    public Attempt<R, B> SelectMany<B>(Func<A, Attempt<R, B>> f) {
      return SelectMany(f, (a, b) => b);
    }

    public Attempt<R, C> SelectMany<B, C>(Func<A, Attempt<R, B>> f, Func<A, B, C> selector) {
      var self = this;
      Func<R, Option<C>> func = r => {
        var maybeA = self.Run(r);
        var maybeB = maybeA.SelectMany(a => f(a).Run(r));
        return maybeA.Select(a => maybeB.Select(b => selector(a, b))).Flatten();
      };
      return new Attempt<R, C>(func);
    }
  }
}