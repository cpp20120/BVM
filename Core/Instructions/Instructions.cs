using System.Text;

namespace VM.Core.Instructions
{
    /// <summary>
    /// Represents the data types supported by the virtual machine.
    /// </summary>
    public enum VmType : byte
    {
        /// <summary>32-bit signed integer</summary>
        INT,
        /// <summary>32-bit floating point number</summary>
        FLOAT,
        /// <summary>UTF-8 encoded string</summary>
        STRING,
        /// <summary>Boolean value (true/false)</summary>
        BOOL,
        /// <summary>Array of values</summary>
        ARRAY,
        /// <summary>Structure/object type</summary>
        STRUCT,
        /// <summary>Null reference</summary>
        NULL
    }

    /// <summary>
    /// Represents a typed value in the virtual machine.
    /// </summary>
    public struct VmValue
    {
        /// <summary>The type of the value</summary>
        public VmType Type;
        
        /// <summary>The boxed value</summary>
        public object Value;

        /// <summary>Converts to integer with type checking</summary>
        /// <exception cref="VmTypeException">Thrown when type is not INT</exception>
        public int AsInt() => Type == VmType.INT ? (int)Value : throw new VmTypeException(Type, VmType.INT);
        
        /// <summary>Converts to float with type checking</summary>
        /// <exception cref="VmTypeException">Thrown when type is not FLOAT</exception>
        public float AsFloat() => Type == VmType.FLOAT ? (float)Value : throw new VmTypeException(Type, VmType.FLOAT);

        /// <summary>Converts to string with type checking</summary>
        /// <exception cref="VmTypeException">Thrown when type is not STRING</exception>
        public string AsString() =>
            Type == VmType.STRING ? (string)Value : throw new VmTypeException(Type, VmType.STRING);

        /// <summary>Converts to bool with type checking</summary>
        /// <exception cref="VmTypeException">Thrown when type is not BOOL</exception>
        public bool AsBool() => Type == VmType.BOOL ? (bool)Value : throw new VmTypeException(Type, VmType.BOOL);

        /// <summary>Converts to array with type checking</summary>
        /// <exception cref="VmTypeException">Thrown when type is not ARRAY</exception>
        public VmArray AsArray() =>
            Type == VmType.ARRAY ? (VmArray)Value : throw new VmTypeException(Type, VmType.ARRAY);

        /// <summary>Creates a new integer value</summary>
        public static VmValue FromInt(int value) => new() { Type = VmType.INT, Value = value };
        
        /// <summary>Creates a new float value</summary>
        public static VmValue FromFloat(float value) => new() { Type = VmType.FLOAT, Value = value };
        
        /// <summary>Creates a new string value</summary>
        public static VmValue FromString(string value) => new() { Type = VmType.STRING, Value = value };
        
        /// <summary>Creates a new boolean value</summary>
        public static VmValue FromBool(bool value) => new() { Type = VmType.BOOL, Value = value };
        
        /// <summary>Creates a new array value</summary>
        public static VmValue FromArray(VmArray value) => new() { Type = VmType.ARRAY, Value = value };
    }

    /// <summary>
    /// Represents the data stack for storing operands during execution.
    /// </summary>
    public class DataStack
    {
        private readonly Stack<VmValue> _stack = new();
        
        /// <summary>Gets the number of items on the stack</summary>
        public int Count => _stack.Count;
        
        /// <summary>Indicates whether the stack is empty</summary>
        public bool IsEmpty => _stack.Count == 0;
        
        /// <summary>Pushes a value onto the stack</summary>
        public void Push(VmValue value) => _stack.Push(value);
        
        /// <summary>Pops a value from the stack</summary>
        /// <exception cref="VmStackException">Thrown when stack is empty</exception>
        public VmValue Pop() => _stack.Count > 0 ? _stack.Pop() : throw new VmStackException("Pop from empty stack");
        
        /// <summary>Peeks at the top value without removing it</summary>
        /// <exception cref="VmStackException">Thrown when stack is empty</exception>
        public VmValue Peek() => _stack.Count > 0 ? _stack.Peek() : throw new VmStackException("Peek from empty stack");
        
        /// <summary>Clears all values from the stack</summary>
        public void Clear() => _stack.Clear();

        /// <summary>Returns string representation of stack contents</summary>
        public override string ToString() =>
            "[" + string.Join(", ", _stack.Reverse().Select(v => v.Value?.ToString() ?? "null")) + "]";
    }

    /// <summary>
    /// Represents the call stack for managing function calls and returns.
    /// </summary>
    public class CallStack
    {
        private readonly Stack<int> _stack = new();
        
        /// <summary>Gets the number of return addresses on stack</summary>
        public int Count => _stack.Count;
        
        /// <summary>Indicates whether the stack is empty</summary>
        public bool IsEmpty => _stack.Count == 0;
        
        /// <summary>Pushes a return address onto the stack</summary>
        public void Push(int value) => _stack.Push(value);
        
        /// <summary>Pops a return address from the stack</summary>
        /// <exception cref="VmStackException">Thrown when stack is empty</exception>
        public int Pop() => _stack.Count > 0 ? _stack.Pop() : throw new VmStackException("Pop from empty call stack");

        /// <summary>Peeks at the top return address without removing it</summary>
        /// <exception cref="VmStackException">Thrown when stack is empty</exception>
        public int Peek() =>
            _stack.Count > 0 ? _stack.Peek() : throw new VmStackException("Peek from empty call stack");

        /// <summary>Clears all return addresses from the stack</summary>
        public void Clear() => _stack.Clear();
        
        /// <summary>Returns string representation of call stack</summary>
        public override string ToString() => "[" + string.Join(",", _stack.Reverse()) + "]";
    }

    /// <summary>
    /// Abstract base class for all virtual machine instructions.
    /// </summary>
    public abstract class Instruction(OpCode code, string mnemonic, int operandSize = 0, string stackEffect = "")
    {
        /// <summary>Gets the opcode for this instruction</summary>
        public OpCode Code { get; } = code;
        
        /// <summary>Gets the human-readable mnemonic</summary>
        public string Mnemonic { get; } = mnemonic;
        
        /// <summary>Gets the size of operands in bytes</summary>
        public int OperandSize { get; } = operandSize;
        
        /// <summary>Gets the stack effect description</summary>
        public string StackEffect { get; } = stackEffect;

        /// <summary>
        /// Executes the instruction with the given context.
        /// </summary>
        /// <param name="context">The execution context</param>
        /// <param name="frames">The frame stack</param>
        public abstract void Execute(ExContext context, FrameStack frames);
    }

    /// <summary>
    /// Pushes an integer constant onto the stack.
    /// </summary>
    public class PushInstruction() : Instruction(OpCode.PUSH, "PUSH", sizeof(int), "→ value")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var value = context.ReadInt();
            context.DataStack.Push(VmValue.FromInt(value));
        }
    }

    /// <summary>
    /// Pops and prints the top stack value to console.
    /// </summary>
    public class PrintInstruction() : Instruction(OpCode.PRINT, "PRINT", 0, "value →")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var value = context.DataStack.Pop();
            string output = value.Type switch
            {
                VmType.INT => value.AsInt().ToString(),
                VmType.FLOAT => value.AsFloat().ToString(),
                VmType.STRING => value.AsString(),
                VmType.BOOL => value.AsBool().ToString(),
                VmType.ARRAY => value.AsArray().ToString(),
                VmType.NULL => "null",
                _ => throw new VmTypeException($"Cannot print type {value.Type}")
            };
            Console.WriteLine(output);
        }
    }

    /// <summary>
    /// Pops and discards the top stack value.
    /// </summary>
    public class PopInstruction() : Instruction(OpCode.POP, "POP", 0, "value →")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            context.DataStack.Pop();
        }
    }

    /// <summary>
    /// Duplicates the top stack value.
    /// </summary>
    public class DupInstruction() : Instruction(OpCode.DUP, "DUP", 0, "a → a a")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var value = context.DataStack.Peek();
            context.DataStack.Push(value);
        }
    }

    /// <summary>
    /// Swaps the top two stack values.
    /// </summary>
    public class SwapInstruction() : Instruction(OpCode.SWAP, "SWAP", 0, "a b → b a")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(b);
            context.DataStack.Push(a);
        }
    }

    /// <summary>
    /// Copies the second stack value to the top.
    /// </summary>
    public class OverInstruction() : Instruction(OpCode.OVER, "OVER", 0, "a b → a b a")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(a);
            context.DataStack.Push(b);
            context.DataStack.Push(a);
        }
    }

    /// <summary>
    /// Multiplies the top two stack values.
    /// </summary>
    public class MulInstruction() : Instruction(OpCode.MUL, "MUL", 0, "a b → (a*b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT && b.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(a.AsInt() * b.AsInt()));
            else if (a.Type == VmType.FLOAT && b.Type == VmType.FLOAT)
                context.DataStack.Push(VmValue.FromFloat(a.AsFloat() * b.AsFloat()));
            else
                throw new VmTypeException($"Cannot multiply types {a.Type} and {b.Type}");
        }
    }

    /// <summary>
    /// Divides the top two stack values.
    /// </summary>
    public class DivInstruction() : Instruction(OpCode.DIV, "DIV", 0, "a b → (a/b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT && b.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(a.AsInt() / b.AsInt()));
            else if (a.Type == VmType.FLOAT && b.Type == VmType.FLOAT)
                context.DataStack.Push(VmValue.FromFloat(a.AsFloat() / b.AsFloat()));
            else
                throw new VmTypeException($"Cannot divide types {a.Type} and {b.Type}");
        }
    }

    /// <summary>
    /// Computes modulus of the top two integer values.
    /// </summary>
    public class ModInstruction() : Instruction(OpCode.MOD, "MOD", 0, "a b → (a%b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT && b.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(a.AsInt() % b.AsInt()));
            else
                throw new VmTypeException("MOD requires integer operands");
        }
    }

    /// <summary>
    /// Negates the top numeric stack value.
    /// </summary>
    public class NegInstruction() : Instruction(OpCode.NEG, "NEG", 0, "a → (-a)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(-a.AsInt()));
            else if (a.Type == VmType.FLOAT)
                context.DataStack.Push(VmValue.FromFloat(-a.AsFloat()));
            else
                throw new VmTypeException("NEG requires numeric operand");
        }
    }

    /// <summary>
    /// Logically negates the top boolean stack value.
    /// </summary>
    public class NotInstruction() : Instruction(OpCode.NOT, "NOT", 0, "a → !a")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var a = context.DataStack.Pop();

            if (a.Type == VmType.BOOL)
                context.DataStack.Push(VmValue.FromBool(!a.AsBool()));
            else
                throw new VmTypeException("NOT requires boolean operand");
        }
    }

    /// <summary>
    /// Compares the top two stack values (-1, 0, or 1 result).
    /// </summary>
    public class CmpInstruction() : Instruction(OpCode.CMP, "CMP", 0, "a b → (a<=>b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            var result = a.Type switch
            {
                VmType.INT => a.AsInt().CompareTo(b.AsInt()),
                VmType.FLOAT => a.AsFloat().CompareTo(b.AsFloat()),
                VmType.STRING => string.Compare(a.AsString(), b.AsString(), StringComparison.Ordinal),
                _ => throw new VmTypeException("CMP requires comparable types")
            };

            context.DataStack.Push(VmValue.FromInt(result));
        }
    }

    /// <summary>
    /// Terminates virtual machine execution.
    /// </summary>
    public class HaltInstruction() : Instruction(OpCode.HALT, "HALT", 0, "→")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            throw new VmHaltException();
        }
    }

    /// <summary>
    /// Special exception thrown by HALT instruction to terminate execution.
    /// </summary>
    public class VmHaltException : Exception
    {
    }

    /// <summary>
    /// Returns from a function call.
    /// </summary>
    public class RetInstruction() : Instruction(OpCode.RET, "RET", 0, "ret_addr →")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            if (frames.Count == 0)
                throw new VmException("Frame stack is empty, cannot return");

            var frame = frames.Pop();
            context.InstructionPointer = frame.ReturnAddress;
        }
    }

    /// <summary>
    /// Calls a function at the specified address.
    /// </summary>
    public class CallInstruction() : Instruction(OpCode.CALL, "CALL", sizeof(int), "→ ret_addr")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var targetAddr = context.ReadInt();
            
            var argCount = context.DataStack.Pop().AsInt();

            var newFrame = new Frame
            {
                ReturnAddress = context.InstructionPointer,
                ArgumentCount = argCount
            };

            for (int i = argCount - 1; i >= 0; i--)
                newFrame.Locals[i] = context.DataStack.Pop();

            frames.Push(newFrame);
            context.InstructionPointer = targetAddr;
        }
    }

    /// <summary>
    /// Jumps if top stack value is not zero.
    /// </summary>
    public class JnzInstruction() : Instruction(OpCode.JNZ, "JNZ", sizeof(short), "cond →")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var offset = context.ReadShort();
            var condition = context.DataStack.Pop();

            var jump = condition.Type switch
            {
                VmType.INT => condition.AsInt() != 0,
                VmType.BOOL => condition.AsBool(),
                _ => throw new VmTypeException("Invalid type for JNZ condition")
            };

            if (jump)
                context.InstructionPointer += offset;
        }
    }

    /// <summary>
    /// Adds the top two stack values.
    /// </summary>
    public class AddInstruction() : Instruction(OpCode.ADD, "ADD", 0, "a b → (a+b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT && b.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(a.AsInt() + b.AsInt()));
            else if (a.Type == VmType.FLOAT && b.Type == VmType.FLOAT)
                context.DataStack.Push(VmValue.FromFloat(a.AsFloat() + b.AsFloat()));
            else
                throw new VmTypeException($"ADD not supported for {a.Type} and {b.Type}");
        }
    }

    /// <summary>
    /// Subtracts the top two stack values.
    /// </summary>
    public class SubInstruction() : Instruction(OpCode.SUB, "SUB", 0, "a b → (a-b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT && b.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(a.AsInt() - b.AsInt()));
            else if (a.Type == VmType.FLOAT && b.Type == VmType.FLOAT)
                context.DataStack.Push(VmValue.FromFloat(a.AsFloat() - b.AsFloat()));
            else
                throw new VmTypeException("SUB requires numeric operands");
        }
    }

    /// <summary>
    /// Tests for equality of top two stack values.
    /// </summary>
    public class EqInstruction() : Instruction(OpCode.EQ, "EQ", 0, "a b → (a==b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(VmValue.FromBool(Equals(a.Value, b.Value)));
        }
    }

    /// <summary>
    /// Tests for inequality of top two stack values.
    /// </summary>
    public class NeqInstruction() : Instruction(OpCode.NEQ, "NEQ", 0, "a b → (a!=b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(VmValue.FromBool(!Equals(a.Value, b.Value)));
        }
    }

    /// <summary>
    /// Loads a local variable onto the stack.
    /// </summary>
    public class LoadInstruction() : Instruction(OpCode.LOAD, "LOAD", sizeof(int), "→ value")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var index = context.ReadInt();
            var frame = frames.Peek();
            
            if (!frame.Locals.TryGetValue(index, out var val))
                throw new VmException($"LLOAD: локальной переменной с индексом {index} нет");
            context.DataStack.Push(val);
        }
    }

    /// <summary>
    /// Stores the top stack value into a local variable.
    /// </summary>
    public class StoreInstruction() : Instruction(OpCode.STORE, "STORE", sizeof(int), "value →")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var index = context.ReadInt();
            var value = context.DataStack.Pop();
            var frame = frames.Peek();
            frame.Locals[index] = value;
            context.LocalVariables[index] = value;
        }
    }

    /// <summary>
    /// Performs an unconditional jump.
    /// </summary>
    public class JmpInstruction() : Instruction(OpCode.JMP, "JMP", sizeof(short), "→")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var offset = context.ReadShort();
            context.InstructionPointer += offset;
        }
    }

    /// <summary>
    /// Jumps if top stack value is zero.
    /// </summary>
    public class JzInstruction() : Instruction(OpCode.JZ, "JZ", sizeof(short), "cond →")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var offset = context.ReadShort();
            var condition = context.DataStack.Pop();

            var isZero = condition.Type switch
            {
                VmType.INT => condition.AsInt() == 0,
                VmType.BOOL => !condition.AsBool(),
                _ => throw new VmTypeException("Invalid type for JZ condition")
            };

            if (isZero)
                context.InstructionPointer += offset;
        }
    }

    /// <summary>
    /// Reads input from console and pushes onto stack.
    /// </summary>
    public class InputInstruction() : Instruction(OpCode.INPUT, "INPUT", 0, "→ value")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            Console.Write("INPUT > ");
            var line = Console.ReadLine();

            context.DataStack.Push(int.TryParse(line, out var intVal)
                ? VmValue.FromInt(intVal)
                : VmValue.FromString(line ?? ""));
        }
    }

    /// <summary>
    /// Pushes a string constant onto the stack.
    /// </summary>
    public class PushStringInstruction() : Instruction(OpCode.PUSHS, "PUSHS", -1, "→ value")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var len = context.ReadInt();
            var bytes = context.ReadBytes(len);
            var str = System.Text.Encoding.UTF8.GetString(bytes);
            context.DataStack.Push(VmValue.FromString(str));
        }
    }

    /// <summary>
    /// Performs logical OR on top two boolean values.
    /// </summary>
    public class OrInstruction() : Instruction(OpCode.OR, "OR", 0, "a b → (a || b)")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type != VmType.BOOL || b.Type != VmType.BOOL)
                throw new VmTypeException("OR requires boolean operands");

            context.DataStack.Push(VmValue.FromBool(a.AsBool() || b.AsBool()));
        }
    }

    /// <summary>
    /// Performs logical AND on top two boolean values.
    /// </summary>
    public class AndInstruction() : Instruction(OpCode.AND, "AND", 2, "a, b → a AND b")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            var result = a.AsBool() && b.AsBool();
            context.DataStack.Push(VmValue.FromBool(result));
        }
    }
    
    /// <summary>
    /// Represents an array of values in the virtual machine.
    /// </summary>
    public class VmArray
    {
        private readonly VmValue[] _elements;
        
        /// <summary>Gets the element type of the array</summary>
        public string ElementType { get; }

        /// <summary>
        /// Creates a new array of specified size.
        /// </summary>
        /// <param name="size">Number of elements</param>
        /// <param name="elementType">Type description of elements</param>
        public VmArray(int size, string elementType = "any")
        {
            _elements = new VmValue[size];
            ElementType = elementType;
            for (int i = 0; i < size; i++)
                _elements[i] = new VmValue { Type = VmType.NULL, Value = null };
        }

        /// <summary>
        /// Gets an element at the specified index.
        /// </summary>
        /// <exception cref="VmException">Thrown when index is out of bounds</exception>
        public VmValue Get(int index)
        {
            if (index < 0 || index >= _elements.Length)
                throw new VmException($"Array index out of range: {index}");
            return _elements[index];
        }

        /// <summary>
        /// Sets an element at the specified index.
        /// </summary>
        /// <exception cref="VmException">Thrown when index is out of bounds</exception>
        public void Set(int index, VmValue value)
        {
            if (index < 0 || index >= _elements.Length)
                throw new VmException($"Array index out of range: {index}");
            _elements[index] = value;
        }

        /// <summary>Gets the length of the array</summary>
        public int Length => _elements.Length;

        /// <summary>Returns string representation of array contents</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < _elements.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_elements[i].Value?.ToString() ?? "null");
            }

            sb.Append(']');
            return sb.ToString();
        }
    }

    /// <summary>
    /// Creates a new array and pushes it onto the stack.
    /// </summary>
    public class NewArrayInstruction() : Instruction(OpCode.NEWARRAY, "NEWARRAY", 0, "size → array")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var size = context.DataStack.Pop().AsInt();
            if (size < 0)
                throw new VmException("Array size cannot be negative");
            var array = new VmArray(size, "any");
            context.DataStack.Push(VmValue.FromArray(array));
        }
    }

    /// <summary>
    /// Gets an array element at specified index.
    /// </summary>
    public class GetIndexInstruction() : Instruction(OpCode.GETINDEX, "GETINDEX", 0, "array index → value")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var index = context.DataStack.Pop().AsInt();
            var array = context.DataStack.Pop().AsArray();
            var value = array.Get(index);
            context.DataStack.Push(value);
        }
    }

    /// <summary>
    /// Sets an array element at specified index.
    /// </summary>
    public class SetIndexInstruction() : Instruction(OpCode.SETINDEX, "SETINDEX", 0, "array index value → array")
    {
        /// <inheritdoc/>
        public override void Execute(ExContext context, FrameStack frames)
        {
            var value = context.DataStack.Pop();
            var index = context.DataStack.Pop().AsInt();
            var array = context.DataStack.Pop().AsArray();
            array.Set(index, value);
        }
    }
}