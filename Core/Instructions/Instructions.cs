using System;
using System.Collections.Generic;
using System.Threading;
using VM.Core.Exceptions;
using VM.Core.Instructions;



namespace VM.Core.Instructions
{
    public enum VMType : byte
    {
        Int,
        Float,
        String,
        Bool,
        Array,
        Struct,
        Null
    }

    public struct VMValue
    {
        public VMType Type;
        public object Value;

        public int AsInt() => Type == VMType.Int ? (int)Value : throw new VMTypeException(Type, VMType.Int);
        public float AsFloat() => Type == VMType.Float ? (float)Value : throw new VMTypeException(Type, VMType.Float);
        public string AsString() => Type == VMType.String ? (string)Value : throw new VMTypeException(Type, VMType.String);
        public bool AsBool() => Type == VMType.Bool ? (bool)Value : throw new VMTypeException(Type, VMType.Bool);

        public static VMValue FromInt(int value) => new() { Type = VMType.Int, Value = value };
        public static VMValue FromFloat(float value) => new() { Type = VMType.Float, Value = value };
        public static VMValue FromString(string value) => new() { Type = VMType.String, Value = value };
        public static VMValue FromBool(bool value) => new() { Type = VMType.Bool, Value = value };
    }

    public class DataStack
    {
        private readonly Stack<VMValue> _stack = new();
        public int Count => _stack.Count;
        public bool IsEmpty => _stack.Count == 0;
        public void Push(VMValue value) => _stack.Push(value);
        public VMValue Pop() => _stack.Count > 0 ? _stack.Pop() : throw new VMStackException("Pop from empty stack");
        public VMValue Peek() => _stack.Count > 0 ? _stack.Peek() : throw new VMStackException("Peek from empty stack");
    }

    public class CallStack
    {
        private readonly Stack<int> _stack = new();
        public int Count => _stack.Count;
        public bool IsEmpty => _stack.Count == 0;
        public void Push(int value) => _stack.Push(value);
        public int Pop() => _stack.Count > 0 ? _stack.Pop() : throw new VMStackException("Pop from empty call stack");
        public int Peek() => _stack.Count > 0 ? _stack.Peek() : throw new VMStackException("Peek from empty call stack");
    }

    public abstract class Instruction
    {
        public OpCode Code { get; }
        public string Mnemonic { get; }
        public int OperandSize { get; }
        public string StackEffect { get; }

        protected Instruction(OpCode code, string mnemonic, int operandSize = 0, string stackEffect = "")
        {
            Code = code;
            Mnemonic = mnemonic;
            OperandSize = operandSize;
            StackEffect = stackEffect;
        }

        public abstract void Execute(ExContext context);
    }
    public class PushInstruction : Instruction
    {
        public PushInstruction() : base(OpCode.PUSH, "PUSH", sizeof(int), "→ value") { }

        public override void Execute(ExContext context)
        {
            int value = context.ReadInt();
            context.DataStack.Push(VMValue.FromInt(value));
        }
    }

    public class PrintInstruction : Instruction
    {
        public PrintInstruction() : base(OpCode.PRINT, "PRINT", 0, "value →") { }

        public override void Execute(ExContext context)
        {
            var value = context.DataStack.Pop();
            Console.WriteLine($"[PRINT] {value.Value}");
        }
    }

    public class PopInstruction : Instruction
    {
        public PopInstruction() : base(OpCode.POP, "POP", 0, "value →") {}

        public override void Execute(ExContext context)
        {
            context.DataStack.Pop();
        }
    }

    public class DupInstruction : Instruction
    {
        public DupInstruction() : base(OpCode.DUP, "DUP", 0, "a → a a") {}

        public override void Execute(ExContext context)
        {
            var value = context.DataStack.Peek();
            context.DataStack.Push(value);
        }
    }

    public class SwapInstruction : Instruction
    {
        public SwapInstruction() : base(OpCode.SWAP, "SWAP", 0, "a b → b a") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(b);
            context.DataStack.Push(a);
        }
    }

    public class OverInstruction : Instruction
    {
        public OverInstruction() : base(OpCode.OVER, "OVER", 0, "a b → a b a") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(a);
            context.DataStack.Push(b);
            context.DataStack.Push(a);
        }
    }

    public class MulInstruction : Instruction
    {
        public MulInstruction() : base(OpCode.MUL, "MUL", 0, "a b → (a*b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Int && b.Type == VMType.Int)
                context.DataStack.Push(VMValue.FromInt(a.AsInt() * b.AsInt()));
            else if (a.Type == VMType.Float && b.Type == VMType.Float)
                context.DataStack.Push(VMValue.FromFloat(a.AsFloat() * b.AsFloat()));
            else
                throw new VMTypeException($"Cannot multiply types {a.Type} and {b.Type}");
        }
    }

    public class DivInstruction : Instruction
    {
        public DivInstruction() : base(OpCode.DIV, "DIV", 0, "a b → (a/b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Int && b.Type == VMType.Int)
                context.DataStack.Push(VMValue.FromInt(a.AsInt() / b.AsInt()));
            else if (a.Type == VMType.Float && b.Type == VMType.Float)
                context.DataStack.Push(VMValue.FromFloat(a.AsFloat() / b.AsFloat()));
            else
                throw new VMTypeException($"Cannot divide types {a.Type} and {b.Type}");
        }
    }

    public class ModInstruction : Instruction
    {
        public ModInstruction() : base(OpCode.MOD, "MOD", 0, "a b → (a%b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Int && b.Type == VMType.Int)
                context.DataStack.Push(VMValue.FromInt(a.AsInt() % b.AsInt()));
            else
                throw new VMTypeException("MOD requires integer operands");
        }
    }

    public class NegInstruction : Instruction
    {
        public NegInstruction() : base(OpCode.NEG, "NEG", 0, "a → (-a)") {}

        public override void Execute(ExContext context)
        {
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Int)
                context.DataStack.Push(VMValue.FromInt(-a.AsInt()));
            else if (a.Type == VMType.Float)
                context.DataStack.Push(VMValue.FromFloat(-a.AsFloat()));
            else
                throw new VMTypeException("NEG requires numeric operand");
        }
    }

    public class NotInstruction : Instruction
    {
        public NotInstruction() : base(OpCode.NOT, "NOT", 0, "a → !a") {}

        public override void Execute(ExContext context)
        {
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Bool)
                context.DataStack.Push(VMValue.FromBool(!a.AsBool()));
            else
                throw new VMTypeException("NOT requires boolean operand");
        }
    }

    public class CmpInstruction : Instruction
    {
        public CmpInstruction() : base(OpCode.CMP, "CMP", 0, "a b → (a<=>b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            int result = a.Type switch
            {
                VMType.Int => a.AsInt().CompareTo(b.AsInt()),
                VMType.Float => a.AsFloat().CompareTo(b.AsFloat()),
                VMType.String => string.Compare(a.AsString(), b.AsString(), StringComparison.Ordinal),
                _ => throw new VMTypeException("CMP requires comparable types")
            };

            context.DataStack.Push(VMValue.FromInt(result));
        }
    }

    public class HaltInstruction : Instruction
    {
        public HaltInstruction() : base(OpCode.HALT, "HALT", 0, "→") {}

        public override void Execute(ExContext context)
        {
            throw new VMException("Execution halted by HALT instruction");
        }
    }

    public class RetInstruction : Instruction
    {
        public RetInstruction() : base(OpCode.RET, "RET", 0, "ret_addr →") {}

        public override void Execute(ExContext context)
        {
            if (context.CallStack.IsEmpty)
                throw new VMException("Call stack is empty, cannot return");

            context.InstructionPointer = context.CallStack.Pop();
        }
    }

    public class CallInstruction : Instruction
    {
        public CallInstruction() : base(OpCode.CALL, "CALL", sizeof(int), "→ ret_addr") {}

        public override void Execute(ExContext context)
        {
            int targetAddr = context.ReadInt();
            context.CallStack.Push(context.InstructionPointer);
            context.InstructionPointer = targetAddr;
        }
    }

    public class JnzInstruction : Instruction
    {
        public JnzInstruction() : base(OpCode.JNZ, "JNZ", sizeof(short), "cond →") {}

        public override void Execute(ExContext context)
        {
            short offset = context.ReadShort();
            var condition = context.DataStack.Pop();

            bool jump = condition.Type switch
            {
                VMType.Int => condition.AsInt() != 0,
                VMType.Bool => condition.AsBool(),
                _ => throw new VMTypeException("Invalid type for JNZ condition")
            };

            if (jump)
                context.InstructionPointer += offset;
        }
    }
    public class AddInstruction : Instruction
    {
        public AddInstruction() : base(OpCode.ADD, "ADD", 0, "a b → (a+b)") {}

        public override void  Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Int && b.Type == VMType.Int)
                context.DataStack.Push(VMValue.FromInt(a.AsInt() + b.AsInt()));
            else if (a.Type == VMType.Float && b.Type == VMType.Float)
                context.DataStack.Push(VMValue.FromFloat(a.AsFloat() + b.AsFloat()));
            else
                throw new VMTypeException($"ADD not supported for {a.Type} and {b.Type}");
        }
    }

    public class SubInstruction : Instruction
    {
        public SubInstruction() : base(OpCode.SUB, "SUB", 0, "a b → (a-b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type == VMType.Int && b.Type == VMType.Int)
                context.DataStack.Push(VMValue.FromInt(a.AsInt() - b.AsInt()));
            else if (a.Type == VMType.Float && b.Type == VMType.Float)
                context.DataStack.Push(VMValue.FromFloat(a.AsFloat() - b.AsFloat()));
            else
                throw new VMTypeException("SUB requires numeric operands");
        }
    }
    public class EqInstruction : Instruction
    {
        public EqInstruction() : base(OpCode.EQ, "EQ", 0, "a b → (a==b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(VMValue.FromBool(Equals(a.Value, b.Value)));
        }
    }

    public class NeqInstruction : Instruction
    {
        public NeqInstruction() : base(OpCode.NEQ, "NEQ", 0, "a b → (a!=b)") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();
            context.DataStack.Push(VMValue.FromBool(!Equals(a.Value, b.Value)));
        }
    }
    public class LoadInstruction : Instruction
    {
        
        public LoadInstruction() : base(OpCode.LOAD, "LOAD", sizeof(int), "→ value") {}

        public override void Execute(ExContext context)
        {
            int index = context.ReadInt();
            context.DataStack.Push(context.LocalVariables[index]);
        }
    }

    public class StoreInstruction : Instruction
    {
        public StoreInstruction() : base(OpCode.STORE, "STORE", sizeof(int), "value →") {}

        public override void Execute(ExContext context)
        {
            int index = context.ReadInt();
            var value = context.DataStack.Pop();
            context.LocalVariables[index] = value;
        }
    }
    public class JmpInstruction : Instruction
    {
        public JmpInstruction() : base(OpCode.JMP, "JMP", sizeof(short), "→") {}

        public override void Execute(ExContext context)
        {
            short offset = context.ReadShort();
            context.InstructionPointer += offset;
        }
    }
    public class JzInstruction : Instruction
    {
        public JzInstruction() : base(OpCode.JZ, "JZ", sizeof(short), "cond →") {}

        public override void Execute(ExContext context)
        {
            short offset = context.ReadShort();
            var condition = context.DataStack.Pop();

            bool isZero = condition.Type switch
            {
                VMType.Int => condition.AsInt() == 0,
                VMType.Bool => !condition.AsBool(),
                _ => throw new VMTypeException("Invalid type for JZ condition")
            };

            if (isZero)
                context.InstructionPointer += offset;
        }
    }
    public class InputInstruction : Instruction
    {
        public InputInstruction() : base(OpCode.INPUT, "INPUT", 0, "→ value") {}

        public override void Execute(ExContext context)
        {
            Console.Write("INPUT > ");
            string? line = Console.ReadLine();

            if (int.TryParse(line, out var intVal))
                context.DataStack.Push(VMValue.FromInt(intVal));
            else
                context.DataStack.Push(VMValue.FromString(line ?? ""));
        }
    }
    public class PushStringInstruction : Instruction
    {
        public PushStringInstruction() : base(OpCode.PUSHS, "PUSHS", -1, "→ value") {}

        public override void Execute(ExContext context)
        {
            int len = context.ReadInt();
            byte[] bytes = context.ReadBytes(len);
            string str = System.Text.Encoding.UTF8.GetString(bytes);
            context.DataStack.Push(VMValue.FromString(str));
        }
    }
    
    public class OrInstruction : Instruction
    {
        public OrInstruction() : base(OpCode.OR, "OR", 0, "a b → (a || b)") { }

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            if (a.Type != VMType.Bool || b.Type != VMType.Bool)
                throw new VMTypeException("OR requires boolean operands");

            context.DataStack.Push(VMValue.FromBool(a.AsBool() || b.AsBool()));
        }
    }

    public class AndInstruction : Instruction
    {
        public AndInstruction() : base(OpCode.AND, "AND", 2, "a, b → a AND b") {}

        public override void Execute(ExContext context)
        {
            var b = context.DataStack.Pop();
            var a = context.DataStack.Pop();

            bool result = a.AsBool() && b.AsBool();
            context.DataStack.Push(VMValue.FromBool(result));
        }
    }
} 
