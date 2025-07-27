namespace VM.Core.ValueTypes;

/// <summary>
/// Represents a value in the virtual machine's type system.
/// </summary>
/// <remarks>
/// Provides common operations and type conversions for all value types in the VM.
/// </remarks>
public interface IValue
{
    /// <summary>
    /// Gets the type name of this value.
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// Gets the raw underlying value as an object.
    /// </summary>
    object Raw { get; }

    /// <summary>
    /// Converts the value to an integer.
    /// </summary>
    /// <returns>The converted integer value.</returns>
    /// <exception cref="InvalidCastException">Thrown when conversion is not possible.</exception>
    int AsInt();

    /// <summary>
    /// Converts the value to a floating-point number.
    /// </summary>
    /// <returns>The converted float value.</returns>
    /// <exception cref="InvalidCastException">Thrown when conversion is not possible.</exception>
    float AsFloat();

    /// <summary>
    /// Converts the value to a string.
    /// </summary>
    /// <returns>The string representation of the value.</returns>
    string AsString();

    /// <summary>
    /// Converts the value to a boolean.
    /// </summary>
    /// <returns>The boolean representation of the value.</returns>
    bool AsBool();
}