namespace VM.Core.ValueTypes;

public class BoolValue(bool value) : IValue
{
    public bool Value { get; } = value;

    public string TypeName => "bool";
    public object Raw => Value;

    public int AsInt() => Value ? 1 : 0;

    public float AsFloat() => Value ? 1f : 0f;

    public string AsString() => Value ? "true" : "false";

    public bool AsBool() => Value;

    public override string ToString() => AsString();
}