namespace XSharpx {
  /// <summary>
  /// A value with only one instance, <see cref="Unit.Value"/>.
  /// Unlike <see cref="System.Void"/>, can be legally used as a
  /// generic type parameter.
  /// </summary>
  ///
  /// <remarks>
  /// <para>If you have <see cref="Microsoft.FSharp.Core.Unit">F#
  /// Unit</see> available instead, use that; it's more
  /// convenient.</para>
  /// </remarks>
  public struct Unit {
    /// <summary>
    /// The only Unit value.  Make your own if you like; they're all
    /// the same.
    /// </summary>
    public static readonly Unit Value = new Unit();
  }

  public sealed class UnitInstances {
    private UnitInstances() { }

    /// <summary>
    /// The trivial monoid of any singleton universe.
    /// </summary>
    public static readonly Monoid<Unit> Monoid =
      Monoid<Unit>.monoid((a, b) => Unit.Value, Unit.Value);
  }
}
