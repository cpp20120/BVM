namespace VM.Core.ValueTypes;

public class StringValue(string value) : IValue
{
    private string Value { get; } = value;

    public string TypeName => "string";
    public object Raw => Value;

    public int AsInt() =>
        int.TryParse(Value, out var i) ? i : throw new InvalidCastException("String is not an int");

    public float AsFloat() =>
        float.TryParse(Value, out var f) ? f : throw new InvalidCastException("String is not a float");

    public string AsString() => Value;

    public bool AsBool() => !string.IsNullOrEmpty(Value);

    public override string ToString() => Value;
}