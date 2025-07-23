using System;
using System.Collections.Generic;
using VM.Core.Instructions;

namespace VM.Core
{
    public class VirtualMachine
    {
        private readonly Dictionary<OpCode, Instruction> _instructions;
        private readonly ExContext _context;

        public VirtualMachine()
        {
            _context = new ExContext();
            _instructions = new Dictionary<OpCode, Instruction>
            {
                // Стековые
                { OpCode.PUSH, new PushInstruction() },
                { OpCode.POP, new PopInstruction() },
                { OpCode.DUP, new DupInstruction() },
                { OpCode.SWAP, new SwapInstruction() },
                { OpCode.OVER, new OverInstruction() },

                // Арифметика
                { OpCode.ADD, new AddInstruction() },
                { OpCode.SUB, new SubInstruction() },
                { OpCode.MUL, new MulInstruction() },
                { OpCode.DIV, new DivInstruction() },
                { OpCode.MOD, new ModInstruction() },
                { OpCode.NEG, new NegInstruction() },

                // Логика
                { OpCode.AND, new AndInstruction() },
                { OpCode.OR, new OrInstruction() },
                { OpCode.NOT, new NotInstruction() },
                { OpCode.CMP, new CmpInstruction() },
                { OpCode.EQ, new EqInstruction() },
                { OpCode.NEQ, new NeqInstruction() },

                // Переменные
                { OpCode.LOAD, new LoadInstruction() },
                { OpCode.STORE, new StoreInstruction() },
                // глобалки пока можно не добавлять

                // Управляющие
                { OpCode.JMP, new JmpInstruction() },
                { OpCode.JZ, new JzInstruction() },
                { OpCode.JNZ, new JnzInstruction() },
                { OpCode.CALL, new CallInstruction() },
                { OpCode.RET, new RetInstruction() },

                // Ввод/вывод
                { OpCode.PRINT, new PrintInstruction() },
                { OpCode.INPUT, new InputInstruction() },
                { OpCode.HALT, new HaltInstruction() },

                // Строки
                { OpCode.PUSHS, new PushStringInstruction() },

                // Массивы
                { OpCode.NEWARRAY, new NewArrayInstruction() },
                { OpCode.GETINDEX, new GetIndexInstruction() },
                { OpCode.SETINDEX, new SetIndexInstruction() },
            };
        }

        public void LoadProgram(byte[] bytecode)
        {
            _context.LoadCode(bytecode);
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    var ipBefore = _context.InstructionPointer;

                    var opcode = (OpCode)_context.ReadByte();

                    if (!_instructions.TryGetValue(opcode, out var instr))
                        throw new VmException($"Неизвестный опкод: 0x{opcode:X2}", ip: ipBefore);

                    instr.Execute(_context);
                }
            }
            catch (VmException ex)
            {
                Console.WriteLine($"[VM Error] {ex.Message} (Line: {ex.LineNumber}, IP: {ex.InstructionPointer})");
            }
            catch (VmHaltException)
            {
                Console.WriteLine("VM finished.");
            }
        }

        public void Reset()
        {
            _context.DataStack.Clear();
            _context.CallStack.Clear();
            _context.LocalVariables.Clear();
            _context.InstructionPointer = 0;
        }

        public void DumpState()
        {
            Console.WriteLine("=== VM STATE ===");
            Console.WriteLine($"IP: {_context.InstructionPointer}");
            Console.WriteLine($"Stack: {_context.DataStack}");
            Console.WriteLine($"CallStack: {_context.CallStack}");
        }
    }
}