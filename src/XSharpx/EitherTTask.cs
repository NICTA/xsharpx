using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharpx
{
    public sealed class EitherTTask<A, B>
    {
        public readonly Task<Either<A, B>> _task;

        public EitherTTask(Task<Either<A, B>> t)
        {
            _task = t;
        }

        public EitherTTask(Func<Either<A, B>> f)
        {
            _task = new Task<Either<A, B>>(f);
        }

        public void Start()
        {
            _task.Start();
        }

        public Either<A, B> Result
        {
            get
            {
                return _task.Result;
            }
        }

        public void ForEach(Action<B> f)
        {
            _task.ContinueWith(t => t.Result.ForEach(b => f(b)));
        }
    }

    public static class EitherTTaskExtension
    {
        public static EitherTTask<A, C> Select<A, B, C>(this EitherTTask<A, B> te, Func<B, C> map)
        {
            return new EitherTTask<A, C>(te._task.Select(ea => ea.Select(b => map(b))));
        }

        public static EitherTTask<A, C> SelectMany<A, B, C>(this EitherTTask<A, B> te, Func<B, EitherTTask<A, C>> bind)
        {
            return te.SelectMany(b => bind(b), (b, c) => c);
        }

        public static EitherTTask<A, D> SelectMany<A, B, C, D>(this EitherTTask<A, B> te, Func<B, EitherTTask<A, C>> bind, Func<B, C, D> select)
        {
            return new EitherTTask<A, D>(te._task.SelectMany(ea =>
            {
                return ea.Fold<Task<Either<A, D>>>(a => TaskExtension.CompletedTask<Either<A, D>>(Either<A, D>.Left(a)),
                    b => bind(b).Select(c => select(b, c))._task);
            }));
        }
    }
}
