namespace VM.Core.ValueTypes;

public sealed class NullValue : IValue
{
    public static readonly NullValue Instance = new();

    private NullValue() { }

    public string TypeName => "null";
    public object Raw => null;

    public int AsInt() => 0;
    public float AsFloat() => 0f;
    public string AsString() => "";
    public bool AsBool() => false;

    public override string ToString() => "null";
}