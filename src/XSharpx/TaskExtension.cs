using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace XSharpx
{
    public static class TaskExtension
    {
        public static Task<A> CompletedTask<A>(A a)
        {
            var ta = new TaskCompletionSource<A>();
            ta.SetResult(a);
            return ta.Task;
        }

        public static Task<B> Select<A, B>(this Task<A> t, Func<A, B> f)
        {
            return t.SelectMany(a => CompletedTask(f(a)), (a, b) => b);
        }

        public static Task<B> SelectMany<A, B>(this Task<A> t, Func<A, Task<B>> f)
        {
            return t.SelectMany(a => f(a), (a, b) => b);
        }

        public static Task<C> SelectMany<A, B, C>(this Task<A> ta, Func<A, Task<B>> f, Func<A, B, C> select)
        {
            return new Task<C>(() =>
            {
                if (ta.Status == TaskStatus.Created)
                    ta.Start(); //throws exception if already running, but if not, we want to 'thread' this how originally intended. 
                var a = ta.Result;
                var tb = f(a);
                if (tb.Status == TaskStatus.Created)
                    tb.Start();
                var b = tb.Result;
                return select(a, b);
            });
        }

        public static Task<List<A>> Sequence<A>(this List<Task<A>> li)
        {
            return li.FoldRight((t, tli) => t.SelectMany(a => tli.Select(nli => List<A>.Cons(a, nli))), new Task<List<A>>(() => List<A>.Empty));
        }
    }
}
