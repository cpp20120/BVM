namespace VM.Core.ValueTypes;

public class NumberValue : IValue
{
    public enum NumberKind { Int, Float }

    public NumberKind Kind { get; }
    public object Value { get; }

    public NumberValue(int value)
    {
        Kind = NumberKind.Int;
        Value = value;
    }

    public NumberValue(float value)
    {
        Kind = NumberKind.Float;
        Value = value;
    }

    public string TypeName => Kind == NumberKind.Int ? "int" : "float";
    public object Raw => Value;

    public int AsInt() =>
        Kind == NumberKind.Int ? (int)Value : throw new InvalidCastException("Not an int");

    public float AsFloat() =>
        Kind == NumberKind.Float ? (float)Value : throw new InvalidCastException("Not a float");

    public string AsString() => Value.ToString();

    public bool AsBool() => Kind switch
    {
        NumberKind.Int => (int)Value != 0,
        NumberKind.Float => Math.Abs((float)Value) > float.Epsilon,
        _ => false
    };

    public override string ToString() => Value.ToString();
}