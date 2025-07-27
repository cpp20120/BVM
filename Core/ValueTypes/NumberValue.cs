namespace VM.Core.ValueTypes;

/// <summary>
/// Represents a numeric value (integer or floating-point) in the virtual machine.
/// </summary>
/// <remarks>
/// Can store either int or float values internally and provides appropriate conversions.
/// </remarks>
public class NumberValue : IValue
{
    private enum NumberKind
    {
        INT,
        FLOAT
    }

    private NumberKind Kind { get; }
    private object Value { get; }

    /// <summary>
    /// Initializes a new integer NumberValue.
    /// </summary>
    /// <param name="value">The integer value to store.</param>
    public NumberValue(int value)
    {
        Kind = NumberKind.INT;
        Value = value;
    }

    /// <summary>
    /// Initializes a new floating-point NumberValue.
    /// </summary>
    /// <param name="value">The float value to store.</param>
    public NumberValue(float value)
    {
        Kind = NumberKind.FLOAT;
        Value = value;
    }

    /// <inheritdoc/>
    public string TypeName => Kind == NumberKind.INT ? "int" : "float";

    /// <inheritdoc/>
    public object Raw => Value;

    /// <inheritdoc/>
    public int AsInt() =>
        Kind == NumberKind.INT 
            ? (int)Value 
            : throw new InvalidCastException("Not an int");

    /// <inheritdoc/>
    public float AsFloat() =>
        Kind == NumberKind.FLOAT 
            ? (float)Value 
            : throw new InvalidCastException("Not a float");

    /// <inheritdoc/>
    public string AsString() => Value.ToString();

    /// <inheritdoc/>
    public bool AsBool() => Kind switch
    {
        NumberKind.INT => (int)Value != 0,
        NumberKind.FLOAT => Math.Abs((float)Value) > float.Epsilon,
        _ => false
    };

    /// <summary>
    /// Returns the string representation of the number.
    /// </summary>
    public override string ToString() => Value.ToString();
}