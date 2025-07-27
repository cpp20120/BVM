namespace VM.Core.ValueTypes;

/// <summary>
/// Represents a null value in the virtual machine.
/// </summary>
/// <remarks>
/// Implements the null object pattern for the VM's type system.
/// All conversions return default/empty values.
/// </remarks>
public sealed class NullValue : IValue
{
    /// <summary>
    /// The singleton instance of NullValue.
    /// </summary>
    public static readonly NullValue Instance = new();

    private NullValue()
    {
    }

    /// <inheritdoc/>
    public string TypeName => "null";

    /// <inheritdoc/>
    public object Raw => null;

    /// <inheritdoc/>
    public int AsInt() => 0;

    /// <inheritdoc/>
    public float AsFloat() => 0f;

    /// <inheritdoc/>
    public string AsString() => "";

    /// <inheritdoc/>
    public bool AsBool() => false;

    /// <summary>
    /// Returns the string "null".
    /// </summary>
    public override string ToString() => "null";
}