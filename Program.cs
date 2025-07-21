using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace VM
{
    public enum InstructionType
    {
        Mov,
        Add, Sub, Mul, Div, Mod,
        And, Or, Not, Neg,
        Eq, Neq, Less, LessEq, Greater, GreaterEq,
        Jmp, JmpFalse,
        Print, Read, Call,
        ScopeOut,
        Label,
        Ret
    }

    public record Instruction(InstructionType Type, object[] Operands);

    public record Label(string Name)
    {
        public static Label Parse(string s)
        {
            s = s.Trim();
            if (s.StartsWith("$$") && s.EndsWith("$$"))
            {
                return new Label(s);
            }
            throw new FormatException($"Invalid label: {s}");
        }

        public override string ToString() => Name;
    }

    public abstract record Operand
    {
        public static Operand Parse(string s)
        {
            s = s.Trim();
            if (s == "pop") return new PopOperand();
            if (s.StartsWith('_') && s.EndsWith('_')) return new IdOperand(s);
            return ValueOperand.Parse(s);
        }

        public abstract Value GetValue(Dictionary<string, Value> variables, Stack<Value> stack);
    }

    public record IdOperand(string Id) : Operand
    {
        public override Value GetValue(Dictionary<string, Value> variables, Stack<Value> stack)
        {
            if (variables.TryGetValue(Id, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException($"Variable {Id} not found in scope");
        }
    }

    public record ValueOperand(Value Value) : Operand
    {
        public static new ValueOperand Parse(string s)
        {
            return new ValueOperand(Value.Parse(s));
        }

        public override Value GetValue(Dictionary<string, Value> variables, Stack<Value> stack)
        {
            return Value;
        }
    }

    public record PopOperand : Operand
    {
        public override Value GetValue(Dictionary<string, Value> variables, Stack<Value> stack)
        {
            if (stack.TryPop(out var value))
            {
                return value;
            }
            throw new InvalidOperationException("Cannot pop. The value stack is empty");
        }
    }

    public abstract record Target
    {
        public static Target Parse(string s)
        {
            s = s.Trim();
            if (s == "push") return new PushTarget();
            if (s.StartsWith('_') && s.EndsWith('_')) return new IdTarget(s);
            throw new FormatException($"Invalid target: {s}");
        }

        public abstract void SetValue(Value value, Dictionary<string, Value> variables, Stack<Value> stack);
    }

    public record IdTarget(string Id) : Target
    {
        public override void SetValue(Value value, Dictionary<string, Value> variables, Stack<Value> stack)
        {
            variables[Id] = value;
        }
    }

    public record PushTarget : Target
    {
        public override void SetValue(Value value, Dictionary<string, Value> variables, Stack<Value> stack)
        {
            stack.Push(value);
        }
    }

    public abstract record Value
    {
        public static Value Parse(string s)
        {
            s = s.Trim();
            if ((s.StartsWith('"') && s.EndsWith('"')) || (s.StartsWith('\'') && s.EndsWith('\'')))
            {
                return new StringValue(s[1..^1]);
            }
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            {
                return new NumberValue(num);
            }
            if (s == "true") return new NumberValue(1);
            if (s == "false") return new NumberValue(0);
            throw new FormatException($"Invalid value: {s}");
        }

        public abstract Value Add(Value other);
        public abstract Value Sub(Value other);
        public abstract Value Mul(Value other);
        public abstract Value Div(Value other);
        public abstract Value Mod(Value other);
        public abstract Value And(Value other);
        public abstract Value Or(Value other);
        public abstract Value Not();
        public abstract Value Neg();
        public abstract Value Eq(Value other);
        public abstract Value Neq(Value other);
        public abstract Value Less(Value other);
        public abstract Value LessEq(Value other);
        public abstract Value Greater(Value other);
        public abstract Value GreaterEq(Value other);
        public abstract bool IsTruthy();
    }

    public record NumberValue(double Value) : Value
    {
        public override Value Add(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value + n.Value),
            StringValue s => new StringValue(Value.ToString(CultureInfo.InvariantCulture) + s.Value),
            _ => throw new InvalidOperationException($"Cannot add {this} and {other}")
        };

        public override Value Sub(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value - n.Value),
            _ => throw new InvalidOperationException($"Cannot subtract {this} and {other}")
        };

        public override Value Mul(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value * n.Value),
            _ => throw new InvalidOperationException($"Cannot multiply {this} and {other}")
        };

        public override Value Div(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value / n.Value),
            _ => throw new InvalidOperationException($"Cannot divide {this} and {other}")
        };

        public override Value Mod(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value % n.Value),
            _ => throw new InvalidOperationException($"Cannot modulo {this} and {other}")
        };

        public override Value And(Value other) => other switch
        {
            NumberValue n => new NumberValue(IsTruthy() && n.IsTruthy() ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot AND {this} and {other}")
        };

        public override Value Or(Value other) => other switch
        {
            NumberValue n => new NumberValue(IsTruthy() || n.IsTruthy() ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot OR {this} and {other}")
        };

        public override Value Not() => new NumberValue(IsTruthy() ? 0 : 1);
        public override Value Neg() => new NumberValue(-Value);
        public override Value Eq(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value == n.Value ? 1 : 0),
            _ => throw new InvalidOperationException()
        };

        public override Value Neq(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value != n.Value ? 1 : 0),
            _ => throw new InvalidOperationException()
        };

        public override Value Less(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value < n.Value ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value LessEq(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value <= n.Value ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value Greater(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value > n.Value ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value GreaterEq(Value other) => other switch
        {
            NumberValue n => new NumberValue(Value >= n.Value ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override bool IsTruthy() => Value != 0;
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public record StringValue(string Value) : Value
    {
        public override Value Add(Value other) => new StringValue(Value + other.ToString());
        public override Value Sub(Value other) => throw new InvalidOperationException($"Cannot subtract strings");
        public override Value Mul(Value other) => throw new InvalidOperationException($"Cannot multiply strings");
        public override Value Div(Value other) => throw new InvalidOperationException($"Cannot divide strings");
        public override Value Mod(Value other) => throw new InvalidOperationException($"Cannot modulo strings");
        public override Value And(Value other) => throw new InvalidOperationException($"Cannot AND strings");
        public override Value Or(Value other) => throw new InvalidOperationException($"Cannot OR strings");
        public override Value Not() => throw new InvalidOperationException($"Cannot NOT strings");
        public override Value Neg() => throw new InvalidOperationException($"Cannot negate strings");

        public override Value Eq(Value other) => other switch
        {
            StringValue s => new NumberValue(Value == s.Value ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value Neq(Value other) => other switch
        {
            StringValue s => new NumberValue(Value != s.Value ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value Less(Value other) => other switch
        {
            StringValue s => new NumberValue(string.CompareOrdinal(Value, s.Value) < 0 ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value LessEq(Value other) => other switch
        {
            StringValue s => new NumberValue(string.CompareOrdinal(Value, s.Value) <= 0 ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value Greater(Value other) => other switch
        {
            StringValue s => new NumberValue(string.CompareOrdinal(Value, s.Value) > 0 ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override Value GreaterEq(Value other) => other switch
        {
            StringValue s => new NumberValue(string.CompareOrdinal(Value, s.Value) >= 0 ? 1 : 0),
            _ => throw new InvalidOperationException($"Cannot compare {this} and {other}")
        };

        public override bool IsTruthy() => !string.IsNullOrEmpty(Value);
        public override string ToString() => Value;
    }

    public class Program
    {
        public List<Instruction> Instructions { get; } = [];
        public Dictionary<string, int> LabelMap { get; } = [];

        public static Program Parse(string code)
        {
            var program = new Program();
            var lines = code.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            // First pass: build label map
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var tokens = Tokenize(line);
                if (tokens.Count == 0) continue;

                var instruction = tokens[0].ToLower();
                if (instruction == "lbl" && tokens.Count == 2)
                {
                    var label = Label.Parse(tokens[1]);
                    program.LabelMap[label.Name] = i;
                }
            }

            // Second pass: parse instructions
            foreach (var line in lines)
            {
                var instruction = ParseInstruction(line);
                if (instruction != null)
                {
                    program.Instructions.Add(instruction);
                }
            }

            return program;
        }

        private static Instruction? ParseInstruction(string line)
        {
            var tokens = Tokenize(line);
            if (tokens.Count == 0) return null;

            var instruction = tokens[0].ToLower();
            tokens.RemoveAt(0);

            try
            {
                return instruction switch
                {
                    "mov" when tokens.Count == 2 =>
                        new Instruction(InstructionType.Mov, [Target.Parse(tokens[0]), Operand.Parse(tokens[1])]),

                    "+" or "-" or "*" or "/" or "%" or "&" or "|" or "==" or "!=" or "<" or "<=" or ">" or ">=" when tokens.Count == 3 =>
                        new Instruction(GetInstructionType(instruction), [Operand.Parse(tokens[0]), Operand.Parse(tokens[1]), Target.Parse(tokens[2])]),

                    "!" or "neg" when tokens.Count == 2 =>
                        new Instruction(GetInstructionType(instruction), [Operand.Parse(tokens[0]), Target.Parse(tokens[1])]),

                    "jmp" when tokens.Count == 1 =>
                        new Instruction(InstructionType.Jmp, [Label.Parse(tokens[0])]),

                    "jf" when tokens.Count == 2 =>
                        new Instruction(InstructionType.JmpFalse, [Label.Parse(tokens[0]), Operand.Parse(tokens[1])]),

                    "prn" when tokens.Count == 1 =>
                        new Instruction(InstructionType.Print, [Operand.Parse(tokens[0])]),

                    "read" when tokens.Count == 0 =>
                        new Instruction(InstructionType.Read, []),

                    "call" when tokens.Count == 1 =>
                        new Instruction(InstructionType.Call, [Label.Parse(tokens[0])]),

                    "out" when tokens.Count == 0 =>
                        new Instruction(InstructionType.ScopeOut, []),

                    "ret" when tokens.Count == 0 =>
                        new Instruction(InstructionType.Ret, []),

                    "lbl" when tokens.Count == 1 =>
                        new Instruction(InstructionType.Label, [Label.Parse(tokens[0])]),

                    _ => throw new FormatException($"Invalid instruction: {instruction}")
                };
            }
            catch (Exception ex)
            {
                throw new FormatException($"Error parsing instruction '{line}': {ex.Message}");
            }
        }

        private static InstructionType GetInstructionType(string op) => op switch
        {
            "+" => InstructionType.Add,
            "-" => InstructionType.Sub,
            "*" => InstructionType.Mul,
            "/" => InstructionType.Div,
            "%" => InstructionType.Mod,
            "&" => InstructionType.And,
            "|" => InstructionType.Or,
            "!" => InstructionType.Not,
            "neg" => InstructionType.Neg,
            "==" => InstructionType.Eq,
            "!=" => InstructionType.Neq,
            "<" => InstructionType.Less,
            "<=" => InstructionType.LessEq,
            ">" => InstructionType.Greater,
            ">=" => InstructionType.GreaterEq,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };

        private static List<string> Tokenize(string line)
        {
            var tokens = new List<string>();
            bool inQuotes = false;
            var currentToken = new System.Text.StringBuilder();

            foreach (var c in line.Trim())
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    currentToken.Append(c);
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                }
                else
                {
                    currentToken.Append(c);
                }
            }

            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }

            return tokens;
        }

        public int FindLabel(Label label)
        {
            if (LabelMap.TryGetValue(label.Name, out var position))
            {
                return position;
            }
            throw new KeyNotFoundException($"Label not found: {label.Name}");
        }
    }

    public class Vm
    {
        private readonly Stack<Value> _valueStack = new();
        private readonly Stack<CallFrame> _callStack = new();
        private readonly Stack<Dictionary<string, Value>> _scopeStack = new();
        private const int MaxCallDepth = 10000;
        private const int MaxStackSize = 100000;

        public class CallFrame(int returnIp, Dictionary<string, Value> scope)
        {
            public int ReturnIp { get; } = returnIp;
            public Dictionary<string, Value> Scope { get; } = scope;
        }

        public Vm()
        {
            _scopeStack.Push([]);
        }

        public Value Run(Program program)
        {
            var ip = 0; // instruction pointer

            while (ip < program.Instructions.Count)
            {
                CheckResourceLimits();

                var instruction = program.Instructions[ip];
                var result = ExecuteInstruction(program, instruction, ip);

                if (result.Done)
                {
                    return result.Value!;
                }

                ip = result.NextIp;
            }

            throw new InvalidOperationException("Program terminated without returning a value");
        }

        private void CheckResourceLimits()
        {
            if (_callStack.Count >= MaxCallDepth)
            {
                throw new StackOverflowException($"Maximum call depth exceeded ({MaxCallDepth})");
            }

            if (_valueStack.Count >= MaxStackSize)
            {
                throw new StackOverflowException($"Maximum stack size exceeded ({MaxStackSize})");
            }
        }

        private (bool Done, Value? Value, int NextIp) ExecuteInstruction(Program program, Instruction instruction, int ip)
        {
            var scope = _scopeStack.Peek();

            try
            {
                switch (instruction.Type)
                {
                    case InstructionType.Mov:
                        ExecuteMov(instruction, scope);
                        return (false, null, ip + 1);

                    case InstructionType.Add:
                    case InstructionType.Sub:
                    case InstructionType.Mul:
                    case InstructionType.Div:
                    case InstructionType.Mod:
                    case InstructionType.And:
                    case InstructionType.Or:
                    case InstructionType.Eq:
                    case InstructionType.Neq:
                    case InstructionType.Less:
                    case InstructionType.LessEq:
                    case InstructionType.Greater:
                    case InstructionType.GreaterEq:
                        ExecuteBinaryOperation(instruction, scope);
                        return (false, null, ip + 1);

                    case InstructionType.Not:
                    case InstructionType.Neg:
                        ExecuteUnaryOperation(instruction, scope);
                        return (false, null, ip + 1);

                    case InstructionType.Jmp:
                        return ExecuteJump(program, instruction);

                    case InstructionType.JmpFalse:
                        return ExecuteConditionalJump(program, instruction, scope, ip);

                    case InstructionType.Print:
                        ExecutePrint(instruction, scope);
                        return (false, null, ip + 1);

                    case InstructionType.Read:
                        ExecuteRead(scope);
                        return (false, null, ip + 1);

                    case InstructionType.Call:
                        return ExecuteCall(program, instruction, ip);

                    case InstructionType.ScopeOut:
                        return HandleScopeOut();

                    case InstructionType.Ret:
                        return HandleReturn();

                    case InstructionType.Label:
                        return (false, null, ip + 1);

                    default:
                        throw new InvalidOperationException($"Unknown instruction type: {instruction.Type}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing instruction at IP {ip} ({instruction.Type}): {ex.Message}", ex);
            }
        }

        private void ExecuteMov(Instruction instruction, Dictionary<string, Value> scope)
        {
            var target = (Target)instruction.Operands[0];
            var operand = (Operand)instruction.Operands[1];
            var value = operand.GetValue(scope, _valueStack);
            target.SetValue(value, scope, _valueStack);
        }

        private void ExecuteBinaryOperation(Instruction instruction, Dictionary<string, Value> scope)
        {
            var op1 = (Operand)instruction.Operands[0];
            var op2 = (Operand)instruction.Operands[1];
            var target = (Target)instruction.Operands[2];

            var val1 = op1.GetValue(scope, _valueStack);
            var val2 = op2.GetValue(scope, _valueStack);

            var result = instruction.Type switch
            {
                InstructionType.Add => val1.Add(val2),
                InstructionType.Sub => val1.Sub(val2),
                InstructionType.Mul => val1.Mul(val2),
                InstructionType.Div => val1.Div(val2),
                InstructionType.Mod => val1.Mod(val2),
                InstructionType.And => val1.And(val2),
                InstructionType.Or => val1.Or(val2),
                InstructionType.Eq => val1.Eq(val2),
                InstructionType.Neq => val1.Neq(val2),
                InstructionType.Less => val1.Less(val2),
                InstructionType.LessEq => val1.LessEq(val2),
                InstructionType.Greater => val1.Greater(val2),
                InstructionType.GreaterEq => val1.GreaterEq(val2),
                _ => throw new InvalidOperationException("Unknown binary operation")
            };

            target.SetValue(result, scope, _valueStack);
        }

        private void ExecuteUnaryOperation(Instruction instruction, Dictionary<string, Value> scope)
        {
            var operand = (Operand)instruction.Operands[0];
            var target = (Target)instruction.Operands[1];
            var value = operand.GetValue(scope, _valueStack);

            var result = instruction.Type switch
            {
                InstructionType.Not => value.Not(),
                InstructionType.Neg => value.Neg(),
                _ => throw new InvalidOperationException("Unknown unary operation")
            };

            target.SetValue(result, scope, _valueStack);
        }

        private static (bool Done, Value? Value, int NextIp) ExecuteJump(Program program, Instruction instruction)
        {
            var label = (Label)instruction.Operands[0];
            var jumpTo = program.FindLabel(label);
            return (false, null, jumpTo);
        }

        private (bool Done, Value? Value, int NextIp) ExecuteConditionalJump(
            Program program, Instruction instruction, Dictionary<string, Value> scope, int ip)
        {
            var label = (Label)instruction.Operands[0];
            var operand = (Operand)instruction.Operands[1];
            var value = operand.GetValue(scope, _valueStack);

            if (!value.IsTruthy())
            {
                var jumpTo = program.FindLabel(label);
                return (false, null, jumpTo);
            }
            return (false, null, ip + 1);
        }

        private void ExecutePrint(Instruction instruction, Dictionary<string, Value> scope)
        {
            var operand = (Operand)instruction.Operands[0];
            var value = operand.GetValue(scope, _valueStack);
            Console.WriteLine(value);
        }

        private void ExecuteRead(Dictionary<string, Value> scope)
        {
            var input = Console.ReadLine() ?? "";
            var value = Value.Parse(input);
            new PushTarget().SetValue(value, scope, _valueStack);
        }

        private (bool Done, Value? Value, int NextIp) ExecuteCall(Program program, Instruction instruction, int ip)
        {
            var label = (Label)instruction.Operands[0];

            // Save current state
            var currentScope = new Dictionary<string, Value>(_scopeStack.Peek());
            _callStack.Push(new CallFrame(ip + 1, currentScope));

            // create new scope
            _scopeStack.Push([]);

            // jump to function
            var jumpTo = program.FindLabel(label);
            return (false, null, jumpTo);
        }

        private (bool Done, Value? Value, int NextIp) HandleScopeOut()
        {
            if (_scopeStack.Count == 1)
            {
                // program exit
                var value = _valueStack.Count > 0 ? _valueStack.Pop() : new NumberValue(0);
                return (true, value, -1);
            }

            // normal scope exit
            _scopeStack.Pop();
            return (false, null, _callStack.Pop().ReturnIp);
        }

        private (bool Done, Value? Value, int NextIp) HandleReturn()
        {
            if (_callStack.Count == 0)
            {
                // Return from main
                var value = _valueStack.Count > 0 ? _valueStack.Pop() : new NumberValue(0);
                return (true, value, -1);
            }

            // Return from function
            var frame = _callStack.Pop();
            _scopeStack.Pop(); // Remove current scope
            _scopeStack.Push(frame.Scope); // Restore caller's scope

            return (false, null, frame.ReturnIp);
        }
    }

    class ProgramRunner
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: vm <file>");
                return;
            }

            try
            {
                var code = File.ReadAllText(args[0]);
                var program = VM.Program.Parse(code);
                var vm = new VM.Vm();

                Console.WriteLine("Starting execution...");
                var result = vm.Run(program);
                Console.WriteLine($"Program completed with result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}