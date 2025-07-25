using System.Text;

namespace VM.Core.ValueTypes;

public class ArrayValue : IValue
{
    private readonly List<IValue> _elements;

    public string ElementType { get; }

    public ArrayValue(IEnumerable<IValue> values, string elementType = "any")
    {
        _elements = values.ToList();
        ElementType = elementType;
    }

    public ArrayValue(int size, string elementType = "any")
    {
        _elements = Enumerable.Repeat<IValue>(NullValue.Instance, size).ToList();
        ElementType = elementType;
    }

    public int Length => _elements.Count;

    public IValue Get(int index)
    {
        if (index < 0 || index >= _elements.Count)
            throw new IndexOutOfRangeException($"Invalid array index: {index}");

        return _elements[index];
    }

    public void Set(int index, IValue value)
    {
        if (index < 0 || index >= _elements.Count)
            throw new IndexOutOfRangeException($"Invalid array index: {index}");

        _elements[index] = value;
    }

    public string TypeName => "array";
    public object Raw => _elements;

    public int AsInt() => Length;
    public float AsFloat() => Length;
    public string AsString() => ToString();
    public bool AsBool() => Length > 0;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (var i = 0; i < _elements.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(_elements[i].ToString());
        }

        sb.Append(']');
        return sb.ToString();
    }
}