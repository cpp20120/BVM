using VM.Core.Instructions;

namespace VM.Core
{
    /// <summary>
    /// Maintains a mapping from operation codes (opcodes) to their corresponding executable instruction implementations.
    /// Acts as a registry and dispatcher for instruction behavior in the virtual machine.
    /// </summary>
    public class InstructionSet
    {
        private readonly Dictionary<OpCode, Instruction> _instructions = new();
        /// <summary>
        /// Initializes the instruction set with default core VM instructions.
        /// Categorized by purpose: stack manipulation, arithmetic, logic, control flow, variables, arrays, I/O, and string operations.
        /// </summary>
        public InstructionSet()
        {
            // Stack
            _instructions[OpCode.PUSH] = new PushInstruction();
            _instructions[OpCode.POP] = new PopInstruction();
            _instructions[OpCode.DUP] = new DupInstruction();
            _instructions[OpCode.SWAP] = new SwapInstruction();
            _instructions[OpCode.OVER] = new OverInstruction();

            // Arithmetic
            _instructions[OpCode.ADD] = new AddInstruction();
            _instructions[OpCode.SUB] = new SubInstruction();
            _instructions[OpCode.MUL] = new MulInstruction();
            _instructions[OpCode.DIV] = new DivInstruction();
            _instructions[OpCode.MOD] = new ModInstruction();
            _instructions[OpCode.NEG] = new NegInstruction();

            // Logic
            _instructions[OpCode.AND] = new AndInstruction();
            _instructions[OpCode.OR] = new OrInstruction();
            _instructions[OpCode.NOT] = new NotInstruction();
            _instructions[OpCode.CMP] = new CmpInstruction();
            _instructions[OpCode.EQ] = new EqInstruction();
            _instructions[OpCode.NEQ] = new NeqInstruction();

            // Variables
            _instructions[OpCode.LOAD] = new LoadInstruction();
            _instructions[OpCode.STORE] = new StoreInstruction();

            // Flow
            _instructions[OpCode.JMP] = new JmpInstruction();
            _instructions[OpCode.JZ] = new JzInstruction();
            _instructions[OpCode.JNZ] = new JnzInstruction();
            _instructions[OpCode.CALL] = new CallInstruction();
            _instructions[OpCode.RET] = new RetInstruction();

            // IO
            _instructions[OpCode.PRINT] = new PrintInstruction();
            _instructions[OpCode.INPUT] = new InputInstruction();
            _instructions[OpCode.HALT] = new HaltInstruction();

            // Strings
            _instructions[OpCode.PUSHS] = new PushStringInstruction();

            // Arrays
            _instructions[OpCode.NEWARRAY] = new NewArrayInstruction();
            _instructions[OpCode.GETINDEX] = new GetIndexInstruction();
            _instructions[OpCode.SETINDEX] = new SetIndexInstruction();
        }
        /// <summary>
        /// Retrieves the instruction implementation corresponding to the given opcode.
        /// Returns null if the opcode is not defined in this set.
        /// </summary>
        /// <param name="opcode">The operation code to look up.</param>
        /// <returns>The associated <see cref="Instruction"/> or null if not found.</returns>
        public Instruction? GetInstruction(OpCode opcode) =>
            _instructions.TryGetValue(opcode, out var instr) ? instr : null;
    }
}
