using System.Text;

namespace VM.Core.Instructions
{
    public enum VmType : byte
    {
        INT,
        FLOAT,
        STRING,
        BOOL,
        ARRAY,
        STRUCT,
        NULL
    }

    public struct VmValue
    {
        public VmType Type;
        public object Value;

        public int AsInt() => Type == VmType.INT ? (int)Value : throw new VmTypeException(Type, VmType.INT);
        public float AsFloat() => Type == VmType.FLOAT ? (float)Value : throw new VmTypeException(Type, VmType.FLOAT);

        public string AsString() =>
            Type == VmType.STRING ? (string)Value : throw new VmTypeException(Type, VmType.STRING);

        public bool AsBool() => Type == VmType.BOOL ? (bool)Value : throw new VmTypeException(Type, VmType.BOOL);

        public VmArray AsArray() =>
            Type == VmType.ARRAY ? (VmArray)Value : throw new VmTypeException(Type, VmType.ARRAY);

        public static VmValue FromInt(int value) => new() { Type = VmType.INT, Value = value };
        public static VmValue FromFloat(float value) => new() { Type = VmType.FLOAT, Value = value };
        public static VmValue FromString(string value) => new() { Type = VmType.STRING, Value = value };
        public static VmValue FromBool(bool value) => new() { Type = VmType.BOOL, Value = value };
        public static VmValue FromArray(VmArray value) => new() { Type = VmType.ARRAY, Value = value };
    }

    public class DataStack
    {
        private readonly Stack<VmValue> _stack = new();
        public int Count => _stack.Count;
        public bool IsEmpty => _stack.Count == 0;
        public void Push(VmValue value) => _stack.Push(value);
        public VmValue Pop() => _stack.Count > 0 ? _stack.Pop() : throw new VmStackException("Pop from empty stack");
        public VmValue Peek() => _stack.Count > 0 ? _stack.Peek() : throw new VmStackException("Peek from empty stack");
        public void Clear() => _stack.Clear();

        public override string ToString() =>
            "[" + string.Join(", ", _stack.Reverse().Select(v => v.Value?.ToString() ?? "null")) + "]";
    }

    public class CallStack
    {
        private readonly Stack<int> _stack = new();
        public int Count => _stack.Count;
        public bool IsEmpty => _stack.Count == 0;
        public void Push(int value) => _stack.Push(value);
        public int Pop() => _stack.Count > 0 ? _stack.Pop() : throw new VmStackException("Pop from empty call stack");

        public int Peek() =>
            _stack.Count > 0 ? _stack.Peek() : throw new VmStackException("Peek from empty call stack");

        public void Clear() => _stack.Clear();
        public override string ToString() => "[" + string.Join(",", _stack.Reverse()) + "]";
    }

    public abstract class Instruction(OpCode code, string mnemonic, int operandSize = 0, string stackEffect = "")
    {
        public OpCode Code { get; } = code;
        public string Mnemonic { get; } = mnemonic;
        public int OperandSize { get; } = operandSize;
        public string StackEffect { get; } = stackEffect;

        public abstract void Execute(ExContext context);
    }

    public class PushInstruction() : Instruction(OpCode.PUSH, "PUSH", sizeof(int), "→ value")
    {
        public override void Execute(ExContext context)
        {
            var value = context.ReadInt();
            context.DataStack.Push(VmValue.FromInt(value));
        }
    }

    public class PrintInstruction() : Instruction(OpCode.PRINT, "PRINT", 0, "value →")
    {
        public override void Execute(ExContext context)
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

    public class PopInstruction() : Instruction(OpCode.POP, "POP", 0, "value →")
    {
        public override void Execute(ExContext context)
        {
            context.DataStack.Pop();
        }
    }

    public class DupInstruction() : Instruction(OpCode.DUP, "DUP", 0, "a → a a")
    {
        public override void Execute(ExContext context)
        {
            var value = context.DataStack.Peek();
            context.DataStack.Push(value);
        }
    }

    public class SwapInstruction() : Instruction(OpCode.SWAP, "SWAP", 0, "a b → b a")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(b);
            context.DataStack.Push(a);
        }
    }

    public class OverInstruction() : Instruction(OpCode.OVER, "OVER", 0, "a b → a b a")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(a);
            context.DataStack.Push(b);
            context.DataStack.Push(a);
        }
    }

    public class MulInstruction() : Instruction(OpCode.MUL, "MUL", 0, "a b → (a*b)")
    {
        public override void Execute(ExContext context)
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

    public class DivInstruction() : Instruction(OpCode.DIV, "DIV", 0, "a b → (a/b)")
    {
        public override void Execute(ExContext context)
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

    public class ModInstruction() : Instruction(OpCode.MOD, "MOD", 0, "a b → (a%b)")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VmType.INT && b.Type == VmType.INT)
                context.DataStack.Push(VmValue.FromInt(a.AsInt() % b.AsInt()));
            else
                throw new VmTypeException("MOD requires integer operands");
        }
    }

    public class NegInstruction() : Instruction(OpCode.NEG, "NEG", 0, "a → (-a)")
    {
        public override void Execute(ExContext context)
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

    public class NotInstruction() : Instruction(OpCode.NOT, "NOT", 0, "a → !a")
    {
        public override void Execute(ExContext context)
        {
            var a = context.DataStack.Pop();

            if (a.Type == VmType.BOOL)
                context.DataStack.Push(VmValue.FromBool(!a.AsBool()));
            else
                throw new VmTypeException("NOT requires boolean operand");
        }
    }

    public class CmpInstruction() : Instruction(OpCode.CMP, "CMP", 0, "a b → (a<=>b)")
    {
        public override void Execute(ExContext context)
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

    public class HaltInstruction() : Instruction(OpCode.HALT, "HALT", 0, "→")
    {
        public override void Execute(ExContext context)
        {
            throw new VmHaltException();
        }
    }

    public class VmHaltException : Exception
    {
    }


    public class RetInstruction() : Instruction(OpCode.RET, "RET", 0, "ret_addr →")
    {
        public override void Execute(ExContext context)
        {
            if (context.CallStack.IsEmpty)
                throw new VmException("Call stack is empty, cannot return");

            context.InstructionPointer = context.CallStack.Pop();
        }
    }

    public class CallInstruction() : Instruction(OpCode.CALL, "CALL", sizeof(int), "→ ret_addr")
    {
        public override void Execute(ExContext context)
        {
            var targetAddr = context.ReadInt();
            context.CallStack.Push(context.InstructionPointer);
            context.InstructionPointer = targetAddr;
        }
    }

    public class JnzInstruction() : Instruction(OpCode.JNZ, "JNZ", sizeof(short), "cond →")
    {
        public override void Execute(ExContext context)
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

    public class AddInstruction() : Instruction(OpCode.ADD, "ADD", 0, "a b → (a+b)")
    {
        public override void Execute(ExContext context)
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

    public class SubInstruction() : Instruction(OpCode.SUB, "SUB", 0, "a b → (a-b)")
    {
        public override void Execute(ExContext context)
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

    public class EqInstruction() : Instruction(OpCode.EQ, "EQ", 0, "a b → (a==b)")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(VmValue.FromBool(Equals(a.Value, b.Value)));
        }
    }

    public class NeqInstruction() : Instruction(OpCode.NEQ, "NEQ", 0, "a b → (a!=b)")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(VmValue.FromBool(!Equals(a.Value, b.Value)));
        }
    }

    public class LoadInstruction() : Instruction(OpCode.LOAD, "LOAD", sizeof(int), "→ value")
    {
        public override void Execute(ExContext context)
        {
            var index = context.ReadInt();
            context.DataStack.Push(context.LocalVariables[index]);
        }
    }

    public class StoreInstruction() : Instruction(OpCode.STORE, "STORE", sizeof(int), "value →")
    {
        public override void Execute(ExContext context)
        {
            var index = context.ReadInt();
            var value = context.DataStack.Pop();
            context.LocalVariables[index] = value;
        }
    }

    public class JmpInstruction() : Instruction(OpCode.JMP, "JMP", sizeof(short), "→")
    {
        public override void Execute(ExContext context)
        {
            var offset = context.ReadShort();
            context.InstructionPointer += offset;
        }
    }

    public class JzInstruction() : Instruction(OpCode.JZ, "JZ", sizeof(short), "cond →")
    {
        public override void Execute(ExContext context)
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

    public class InputInstruction() : Instruction(OpCode.INPUT, "INPUT", 0, "→ value")
    {
        public override void Execute(ExContext context)
        {
            Console.Write("INPUT > ");
            var line = Console.ReadLine();

            context.DataStack.Push(int.TryParse(line, out var intVal)
                ? VmValue.FromInt(intVal)
                : VmValue.FromString(line ?? ""));
        }
    }

    public class PushStringInstruction() : Instruction(OpCode.PUSHS, "PUSHS", -1, "→ value")
    {
        public override void Execute(ExContext context)
        {
            var len = context.ReadInt();
            var bytes = context.ReadBytes(len);
            var str = System.Text.Encoding.UTF8.GetString(bytes);
            context.DataStack.Push(VmValue.FromString(str));
        }
    }

    public class OrInstruction() : Instruction(OpCode.OR, "OR", 0, "a b → (a || b)")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type != VmType.BOOL || b.Type != VmType.BOOL)
                throw new VmTypeException("OR requires boolean operands");

            context.DataStack.Push(VmValue.FromBool(a.AsBool() || b.AsBool()));
        }
    }

    public class AndInstruction() : Instruction(OpCode.AND, "AND", 2, "a, b → a AND b")
    {
        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            var result = a.AsBool() && b.AsBool();
            context.DataStack.Push(VmValue.FromBool(result));
        }
    }

    public class VmArray
    {
        private readonly VmValue[] _elements;
        public string ElementType { get; }

        public VmArray(int size, string elementType = "any")
        {
            _elements = new VmValue[size];
            ElementType = elementType;
            for (int i = 0; i < size; i++)
                _elements[i] = new VmValue { Type = VmType.NULL, Value = null };
        }

        public VmValue Get(int index)
        {
            if (index < 0 || index >= _elements.Length)
                throw new VmException($"Array index out of range: {index}");
            return _elements[index];
        }

        public void Set(int index, VmValue value)
        {
            if (index < 0 || index >= _elements.Length)
                throw new VmException($"Array index out of range: {index}");
            _elements[index] = value;
        }

        public int Length => _elements.Length;

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

    public class NewArrayInstruction : Instruction
    {
        public NewArrayInstruction() : base(OpCode.NEWARRAY, "NEWARRAY", 0, "size → array")
        {
        }

        public override void Execute(ExContext context)
        {
            var size = context.DataStack.Pop().AsInt();
            if (size < 0)
                throw new VmException("Array size cannot be negative");
            var array = new VmArray(size, "any");
            context.DataStack.Push(VmValue.FromArray(array));
        }
    }

    public class GetIndexInstruction : Instruction
    {
        public GetIndexInstruction() : base(OpCode.GETINDEX, "GETINDEX", 0, "array index → value")
        {
        }

        public override void Execute(ExContext context)
        {
            var index = context.DataStack.Pop().AsInt();
            var array = context.DataStack.Pop().AsArray();
            var value = array.Get(index);
            context.DataStack.Push(value);
        }
    }

    public class SetIndexInstruction : Instruction
    {
        public SetIndexInstruction() : base(OpCode.SETINDEX, "SETINDEX", 0, "array index value → array")
        {
        }

        public override void Execute(ExContext context)
        {
            var value = context.DataStack.Pop();
            var index = context.DataStack.Pop().AsInt();
            var array = context.DataStack.Pop().AsArray();
            array.Set(index, value);
            context.DataStack.Push(VmValue.FromArray(array));
        }
    }
}