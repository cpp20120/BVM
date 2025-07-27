using System.Text;

namespace VM.Core.ValueTypes
{
    /// <summary>
    /// Represents an array value in the virtual machine, containing a collection of values of a specified type.
    /// </summary>
    /// <remarks>
    /// Implements the IValue interface to provide array-specific operations and conversions.
    /// Supports both initialization with existing values and creation of empty arrays of a specified size.
    /// </remarks>
    public class ArrayValue : IValue
    {
        private readonly List<IValue> _elements;

        /// <summary>
        /// Gets the type of elements stored in the array.
        /// </summary>
        /// <value>
        /// A string representing the element type (e.g., "int", "string", "any").
        /// </value>
        public string ElementType { get; }

        /// <summary>
        /// Initializes a new instance of the ArrayValue class with the specified values.
        /// </summary>
        /// <param name="values">The initial values to populate the array.</param>
        /// <param name="elementType">The type of elements in the array (default: "any").</param>
        public ArrayValue(IEnumerable<IValue> values, string elementType = "any")
        {
            _elements = values.ToList();
            ElementType = elementType;
        }

        /// <summary>
        /// Initializes a new instance of the ArrayValue class with the specified size.
        /// </summary>
        /// <param name="size">The number of elements in the array.</param>
        /// <param name="elementType">The type of elements in the array (default: "any").</param>
        /// <remarks>
        /// All elements are initialized to NullValue.Instance.
        /// </remarks>
        public ArrayValue(int size, string elementType = "any")
        {
            _elements = Enumerable.Repeat<IValue>(NullValue.Instance, size).ToList();
            ElementType = elementType;
        }

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public int Length => _elements.Count;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the index is less than 0 or greater than or equal to the array length.
        /// </exception>
        public IValue Get(int index)
        {
            if (index < 0 || index >= _elements.Count)
                throw new IndexOutOfRangeException($"Invalid array index: {index}");

            return _elements[index];
        }

        /// <summary>
        /// Sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to set.</param>
        /// <param name="value">The value to assign to the element.</param>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the index is less than 0 or greater than or equal to the array length.
        /// </exception>
        public void Set(int index, IValue value)
        {
            if (index < 0 || index >= _elements.Count)
                throw new IndexOutOfRangeException($"Invalid array index: {index}");

            _elements[index] = value;
        }

        /// <summary>
        /// Gets the type name of the array ("array").
        /// </summary>
        public string TypeName => "array";

        /// <summary>
        /// Gets the raw underlying list of values.
        /// </summary>
        public object Raw => _elements;

        /// <summary>
        /// Converts the array to an integer by returning its length.
        /// </summary>
        /// <returns>The number of elements in the array.</returns>
        public int AsInt() => Length;

        /// <summary>
        /// Converts the array to a float by returning its length.
        /// </summary>
        /// <returns>The number of elements in the array as a float.</returns>
        public float AsFloat() => Length;

        /// <summary>
        /// Converts the array to a string representation.
        /// </summary>
        /// <returns>A string showing all elements in the format [elem1, elem2, ...].</returns>
        public string AsString() => ToString();

        /// <summary>
        /// Converts the array to a boolean by checking if it's non-empty.
        /// </summary>
        /// <returns>true if the array has elements; otherwise, false.</returns>
        public bool AsBool() => Length > 0;

        /// <summary>
        /// Returns a string that represents the current array.
        /// </summary>
        /// <returns>A string showing all elements in the format [elem1, elem2, ...].</returns>
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
}