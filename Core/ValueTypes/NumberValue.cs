namespace VM.Core.ValueTypes;

public class NumberValue : IValue
{
    private enum NumberKind
    {
        INT,
        FLOAT
    }

    private NumberKind Kind { get; }
    private object Value { get; }

    public NumberValue(int value)
    {
        Kind = NumberKind.INT;
        Value = value;
    }

    public NumberValue(float value)
    {
        Kind = NumberKind.FLOAT;
        Value = value;
    }

    public string TypeName => Kind == NumberKind.INT ? "int" : "float";
    public object Raw => Value;

    public int AsInt() =>
        Kind == NumberKind.INT ? (int)Value : throw new InvalidCastException("Not an int");

    public float AsFloat() =>
        Kind == NumberKind.FLOAT ? (float)Value : throw new InvalidCastException("Not a float");

    public string AsString() => Value.ToString();

    public bool AsBool() => Kind switch
    {
        NumberKind.INT => (int)Value != 0,
        NumberKind.FLOAT => Math.Abs((float)Value) > float.Epsilon,
        _ => false
    };

    public override string ToString() => Value.ToString();
}