using System;
using System.Collections.Generic;
using VM.Core.Instructions;

namespace VM.Core
{
        public class VirtualMachine
        {
            private readonly ExContext _context;
            private readonly InstructionSet _instructionSet;
            private readonly FrameStack _frames;

            public VirtualMachine()
            {
                _context = new ExContext();
                _instructionSet = new InstructionSet();
                _frames = new FrameStack();
            }

            public void LoadProgram(byte[] bytecode)
            {
                _context.LoadCode(bytecode);
                _frames.Push(new Frame { ReturnAddress = -1 });
            }

            public void Run()
            {
                try
                {
                    while (true)
                    {
                        var ipBefore = _context.InstructionPointer;
                        var opcode = (OpCode)_context.ReadByte();
                        var instr = _instructionSet.GetInstruction(opcode);

                        if (instr == null)
                            throw new VmException($"Неизвестный опкод: 0x{opcode:X2}", ip: ipBefore);

                        instr.Execute(_context, _frames);
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
                _frames.Clear();
            }
            
            public void DumpState()
            {
                Console.WriteLine("=== VM STATE ===");
                Console.WriteLine($"Bytecode: {_context.DataStack.Count}");
                Console.WriteLine($"IP: {_context.InstructionPointer}");
                Console.WriteLine($"Stack: {_context.DataStack}");
                Console.WriteLine($"CallStack: {_context.CallStack}");
                Console.WriteLine("=== FRAMES ===");
                _frames.Dump();
                Console.WriteLine("=== BYTECODE DUMP ===");
                var code = _context.Bytecode;
                for (int i = 0; i < code.Length; i++)
                {
                    if (i % 16 == 0)
                        Console.Write($"{i:X4}: ");
                    Console.Write($"{code[i]:X2} ");
                    if (i % 16 == 15 || i == code.Length - 1)
                        Console.WriteLine();
                }
            }
        }
    }