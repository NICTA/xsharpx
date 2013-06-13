using System.Collections;
using System.Collections.Generic;

namespace XSharpx
{
  /// <summary>
  /// A mutable in-memory linked list.
  /// </summary>
  /// <typeparam name="A">The element type held by this homogenous list.</typeparam>
  /// <remarks>Constant time append.</remarks>
  public sealed class ListBuffer<A> : IEnumerable<A> {
    private List<A> start = List<A>.Empty;
    private List<A> tail = default(List<A>);
    private bool exported;

    private ListBuffer() {}

    public ListBuffer<A> Snoc(A a) {
      if(exported)
        Copy();

      var t = a.ListValue();

      if(start.IsEmpty)
        start = t;
      else
        tail.UnsafeTailUpdate(t);

      tail = t;

      return this;
    }

    private void Copy() {
      if(start.IsNotEmpty) {
        var s = start;
        var t = tail;
        start = List<A>.Empty;
        exported = false;

        while(s != t) {
          Snoc(s.UnsafeHead);
          s = s.UnsafeTail;
        }

        Snoc(t.UnsafeHead);
      }
    }

    public ListBuffer<A> Append(List<A> a) {
      for(List<A> xs = a; xs.IsNotEmpty; xs = xs.UnsafeTail)
        Snoc(xs.UnsafeHead);

      return this;
    }

    public List<A> ToList {
      get {
        exported = start.IsNotEmpty;
        return start;
      }
    }

    public static ListBuffer<A> Empty() {
      return new ListBuffer<A>();
    }

    private class ListBufferEnumerator : IEnumerator<A> {
      private bool z = true;
      private readonly List<A> o;
      private List<A> a;

      public ListBufferEnumerator(ListBuffer<A> o) {
        this.o = o.start;
      }

      public void Dispose() {}

      public void Reset() {
        z = true;
      }

      public bool MoveNext() {
        if(z) {
          a = o;
          z = false;
        } else
          a = a.UnsafeTail;

        return !a.IsEmpty;
      }

      A IEnumerator<A>.Current {
        get {
          return a.UnsafeHead;
        }
      }

      public object Current {
        get {
          return a.UnsafeHead;
        }
      }
    }

    private ListBufferEnumerator Enumerate() {
      return new ListBufferEnumerator(this);
    }

    IEnumerator<A> IEnumerable<A>.GetEnumerator() {
      return Enumerate();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Enumerate();
    }
  }
}

