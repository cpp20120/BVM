namespace VM.Core.ValueTypes;

/// <summary>
/// Represents a string value in the virtual machine.
/// </summary>
/// <remarks>
/// Provides string-specific conversions and operations.
/// </remarks>
public class StringValue(string value) : IValue
{
    private string Value { get; } = value;

    /// <inheritdoc/>
    public string TypeName => "string";

    /// <inheritdoc/>
    public object Raw => Value;

    /// <inheritdoc/>
    public int AsInt() =>
        int.TryParse(Value, out var i) 
            ? i 
            : throw new InvalidCastException("String is not an int");

    /// <inheritdoc/>
    public float AsFloat() =>
        float.TryParse(Value, out var f) 
            ? f 
            : throw new InvalidCastException("String is not a float");

    /// <inheritdoc/>
    public string AsString() => Value;

    /// <inheritdoc/>
    public bool AsBool() => !string.IsNullOrEmpty(Value);

    /// <summary>
    /// Returns the underlying string value.
    /// </summary>
    public override string ToString() => Value;
}