﻿using VM.Core.IR.Nodes;
using VM.Core.Instructions;

namespace VM.Core.IR
{
    public class IrToBytecodeCompiler
    {
        private readonly List<byte> _bytecode = new();
        private readonly Dictionary<string, int> _locals = new();
        private int _varCounter;

        private readonly Dictionary<string, int> _labels = new();
        private readonly List<(int Position, string Label)> _fixups = new();
        private int _labelId;

        public byte[] Compile(List<IrNode?> nodes)
        {
            foreach (var node in nodes)
                CompileNode(node);

            _bytecode.Add((byte)OpCode.HALT);

            ResolveLabels();
            return _bytecode.ToArray();
        }

        private void CompileNode(IrNode? node)
        {
            switch (node)
            {
                case IrConst c: CompileConst(c); break;
                case IrBinary b: CompileBinary(b); break;
                case IrUnary u: CompileUnary(u); break;
                case IrVar v: CompileVar(v); break;
                case IrLet l: CompileLet(l); break;
                case IrPrint p: CompilePrint(p); break;
                case IrInput i: CompileInput(i); break;
                case IrBlock b:
                    foreach (var stmt in b.Statements) CompileNode(stmt);
                    break;
                case IrIf iff: CompileIf(iff); break;
                case IrWhile wh: CompileWhile(wh); break;
                case IrRepeat rep: CompileRepeat(rep); break;
                case IrFor f: CompileFor(f); break;
                case IrNewArray a: CompileNewArray(a); break;
                case IrIndex i: CompileIndex(i); break;
                case IrStoreIndex s: CompileStoreIndex(s); break;
                default: throw new NotImplementedException($"Not implemented: {node?.GetType().Name}");
            }
        }

        private void CompileConst(IrConst c)
        {
            switch (c.Type)
            {
                case "int":
                case "float":
                case "bool":
                case "number":
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(Convert.ToInt32(c.Value)));
                    break;

                case "string":
                    var str = (string)c.Value;
                    var bytes = System.Text.Encoding.UTF8.GetBytes(str);
                    _bytecode.Add((byte)OpCode.PUSHS);
                    _bytecode.AddRange(BitConverter.GetBytes(bytes.Length));
                    _bytecode.AddRange(bytes);
                    break;

                default:
                    throw new Exception($"Unsupported constant type: {c.Type}");
            }
        }

        private void CompileBinary(IrBinary bin)
        {
            CompileNode(bin.Left);
            CompileNode(bin.Right);

            switch (bin.Op)
            {
                case "+": _bytecode.Add((byte)OpCode.ADD); break;
                case "-": _bytecode.Add((byte)OpCode.SUB); break;
                case "*": _bytecode.Add((byte)OpCode.MUL); break;
                case "/": _bytecode.Add((byte)OpCode.DIV); break;
                case "%": _bytecode.Add((byte)OpCode.MOD); break;
                case "==": _bytecode.Add((byte)OpCode.EQ); break;
                case "!=": _bytecode.Add((byte)OpCode.NEQ); break;
                case "AND": _bytecode.Add((byte)OpCode.AND); break;
                case "OR": _bytecode.Add((byte)OpCode.OR); break;

                case "<":
                case "LT":
                    _bytecode.Add((byte)OpCode.CMP);
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(-1));
                    _bytecode.Add((byte)OpCode.EQ);
                    break;

                case ">":
                case "GT":
                    _bytecode.Add((byte)OpCode.CMP);
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(1));
                    _bytecode.Add((byte)OpCode.EQ);
                    break;

                case "<=":
                case "LTE":
                    _bytecode.Add((byte)OpCode.CMP);
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(-1));
                    _bytecode.Add((byte)OpCode.EQ);
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(0));
                    _bytecode.Add((byte)OpCode.EQ);
                    _bytecode.Add((byte)OpCode.OR);
                    break;

                case ">=":
                case "GTE":
                    _bytecode.Add((byte)OpCode.CMP);
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(1));
                    _bytecode.Add((byte)OpCode.EQ);
                    _bytecode.Add((byte)OpCode.PUSH);
                    _bytecode.AddRange(BitConverter.GetBytes(0));
                    _bytecode.Add((byte)OpCode.EQ);
                    _bytecode.Add((byte)OpCode.OR);
                    break;

                default:
                    throw new Exception($"Unknown binary operator: {bin.Op}");
            }
        }

        private void CompileUnary(IrUnary u)
        {
            CompileNode(u.Operand);
            _bytecode.Add(u.Op switch
            {
                "-" => (byte)OpCode.NEG,
                "!" => (byte)OpCode.NOT,
                _ => throw new Exception($"Unknown unary operator: {u.Op}")
            });
        }

        private void CompileVar(IrVar v)
        {
            var index = GetLocalIndex(v.Name);
            _bytecode.Add((byte)OpCode.LOAD);
            _bytecode.AddRange(BitConverter.GetBytes(index));
        }

        private void CompileLet(IrLet l)
        {
            CompileNode(l.Expr);
            var index = GetOrCreateLocalIndex(l.Name);
            _bytecode.Add((byte)OpCode.STORE);
            _bytecode.AddRange(BitConverter.GetBytes(index));
        }

        private void CompileInput(IrInput input)
        {
            foreach (var name in input.VarNames)
            {
                _bytecode.Add((byte)OpCode.INPUT);
                var index = GetOrCreateLocalIndex(name);
                _bytecode.Add((byte)OpCode.STORE);
                _bytecode.AddRange(BitConverter.GetBytes(index));
            }
        }

        private void CompilePrint(IrPrint p)
        {
            CompileNode(p.Expr);
            _bytecode.Add((byte)OpCode.PRINT);
        }

        private void CompileIf(IrIf iff)
        {
            CompileNode(iff.Condition);
            var elseLabel = NewLabel("else");
            var endLabel = NewLabel("endif");

            _bytecode.Add((byte)OpCode.JZ);
            AddFixup(elseLabel);

            foreach (var stmt in iff.ThenBlock)
                CompileNode(stmt);

            _bytecode.Add((byte)OpCode.JMP);
            AddFixup(endLabel);

            PlaceLabel(elseLabel);
            if (iff.ElseBlock != null)
                foreach (var stmt in iff.ElseBlock)
                    CompileNode(stmt);

            PlaceLabel(endLabel);
        }

        private void CompileWhile(IrWhile w)
        {
            var start = NewLabel("while_start");
            var end = NewLabel("while_end");

            PlaceLabel(start);
            CompileNode(w.Condition);
            _bytecode.Add((byte)OpCode.JZ);
            AddFixup(end);

            foreach (var stmt in w.Body)
                CompileNode(stmt);

            _bytecode.Add((byte)OpCode.JMP);
            AddFixup(start);
            PlaceLabel(end);
        }

        private void CompileRepeat(IrRepeat r)
        {
            var start = NewLabel("repeat_start");

            PlaceLabel(start);
            foreach (var stmt in r.Body)
                CompileNode(stmt);

            CompileNode(r.Condition);
            _bytecode.Add((byte)OpCode.JZ);
            AddFixup(start); // repeat until → пока false → jump back
        }

        private void CompileFor(IrFor f)
        {
            var start = NewLabel("for_start");
            var end = NewLabel("for_end");

            var index = GetOrCreateLocalIndex(f.VarName);

            CompileNode(f.From);
            _bytecode.Add((byte)OpCode.STORE);
            _bytecode.AddRange(BitConverter.GetBytes(index));

            PlaceLabel(start);

            _bytecode.Add((byte)OpCode.LOAD);
            _bytecode.AddRange(BitConverter.GetBytes(index));
            CompileNode(f.To);
            _bytecode.Add((byte)OpCode.CMP);
            _bytecode.Add((byte)OpCode.JZ);
            AddFixup(end);

            foreach (var stmt in f.Body)
                CompileNode(stmt);

            _bytecode.Add((byte)OpCode.LOAD);
            _bytecode.AddRange(BitConverter.GetBytes(index));

            if (f.Step != null)
            {
                CompileNode(f.Step);
            }
            else
            {
                _bytecode.Add((byte)OpCode.PUSH);
                _bytecode.AddRange(BitConverter.GetBytes(1)); // default step = 1
            }

            _bytecode.Add((byte)OpCode.ADD);
            _bytecode.Add((byte)OpCode.STORE);
            _bytecode.AddRange(BitConverter.GetBytes(index));

            _bytecode.Add((byte)OpCode.JMP);
            AddFixup(start);

            PlaceLabel(end);
        }

        private void CompileNewArray(IrNewArray a)
        {
            CompileNode(a.Size); //Putting the array size to stack
            _bytecode.Add((byte)OpCode.NEWARRAY);
        }

        private void CompileIndex(IrIndex i)
        {
            CompileNode(i.Target); // Putting the array on the stack
            CompileNode(i.Index); // Putting the index on the stack
            _bytecode.Add((byte)OpCode.GETINDEX);
        }

        private void CompileStoreIndex(IrStoreIndex s)
        {
            CompileNode(s.Target); // Putting the array on the stack
            CompileNode(s.Index);  // Putting the index on the stack
            CompileNode(s.Value); // Putting the value  on the stack
            _bytecode.Add((byte)OpCode.SETINDEX);
        }

        // --- Helpers ---
        private int GetOrCreateLocalIndex(string name)
        {
            if (!_locals.TryGetValue(name, out var index))
                _locals[name] = index = _varCounter++;
            return index;
        }

        private int GetLocalIndex(string name)
        {
            if (!_locals.TryGetValue(name, out var index))
                throw new Exception($"Undefined variable: {name}");
            return index;
        }

        private string NewLabel(string prefix) => $"{prefix}_{_labelId++}";

        private void PlaceLabel(string label)
        {
            _labels[label] = _bytecode.Count;
        }

        private void AddFixup(string label)
        {
            var pos = _bytecode.Count;
            _fixups.Add((pos, label));
            _bytecode.AddRange(new byte[2]); // placeholder for short offset
        }

        private void ResolveLabels()
        {
            foreach (var (pos, label) in _fixups)
            {
                if (!_labels.TryGetValue(label, out var target))
                    throw new Exception($"Unresolved label: {label}");

                var offset = (short)(target - (pos + 2));
                var bytes = BitConverter.GetBytes(offset);
                _bytecode[pos] = bytes[0];
                _bytecode[pos + 1] = bytes[1];
            }
        }
    }
}