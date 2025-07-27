namespace VM.Core.ValueTypes;

/// <summary>
/// Represents a boolean value in the virtual machine, implementing the IValue interface.
/// </summary>
/// <remarks>
/// Encapsulates a boolean value and provides type conversion methods to other primitive types.
/// </remarks>
public class BoolValue(bool value) : IValue
{
    /// <summary>
    /// Gets the underlying boolean value.
    /// </summary>
    public bool Value { get; } = value;

    /// <summary>
    /// Gets the type name of this value ("bool").
    /// </summary>
    public string TypeName => "bool";

    /// <summary>
    /// Gets the raw boolean value as an object.
    /// </summary>
    public object Raw => Value;

    /// <summary>
    /// Converts the boolean value to an integer (1 for true, 0 for false).
    /// </summary>
    /// <returns>1 if true; otherwise 0.</returns>
    public int AsInt() => Value ? 1 : 0;

    /// <summary>
    /// Converts the boolean value to a float (1.0 for true, 0.0 for false).
    /// </summary>
    /// <returns>1.0 if true; otherwise 0.0.</returns>
    public float AsFloat() => Value ? 1f : 0f;

    /// <summary>
    /// Converts the boolean value to a string ("true" or "false").
    /// </summary>
    /// <returns>"true" if true; otherwise "false".</returns>
    public string AsString() => Value ? "true" : "false";

    /// <summary>
    /// Returns the boolean value directly.
    /// </summary>
    /// <returns>The underlying boolean value.</returns>
    public bool AsBool() => Value;

    /// <summary>
    /// Returns a string representation of the boolean value.
    /// </summary>
    /// <returns>"true" if true; otherwise "false".</returns>
    public override string ToString() => AsString();
}