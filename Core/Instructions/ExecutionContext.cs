using VM.Core.Instructions;

namespace VM.Core
{
    /// <summary>
    /// Represents the execution context of the virtual machine, maintaining the state during program execution.
    /// </summary>
    /// <remarks>
    /// The execution context includes:
    /// - Data stack for operand storage
    /// - Call stack for function calls
    /// - Local variables storage
    /// - Bytecode being executed
    /// - Instruction pointer tracking current execution position
    /// </remarks>
    public class ExContext
    {
        /// <summary>
        /// Gets the data stack used for storing operands during execution.
        /// </summary>
        public DataStack DataStack { get; } = new();

        /// <summary>
        /// Gets the call stack used for managing function calls and returns.
        /// </summary>
        public CallStack CallStack { get; } = new();

        /// <summary>
        /// Gets the dictionary of local variables available in the current scope.
        /// </summary>
        /// <remarks>
        /// Local variables are stored with their integer identifiers as keys.
        /// </remarks>
        public Dictionary<int, VmValue> LocalVariables = new();

        /// <summary>
        /// Gets the current bytecode being executed by the virtual machine.
        /// </summary>
        public byte[] Bytecode => _code;

        /// <summary>
        /// Gets or sets the current instruction pointer (IP) position in the bytecode.
        /// </summary>
        /// <remarks>
        /// The instruction pointer indicates the next byte to be read from the bytecode.
        /// </remarks>
        public int InstructionPointer { get; set; }

        private byte[] _code;

        /// <summary>
        /// Loads new bytecode into the execution context and resets the instruction pointer.
        /// </summary>
        /// <param name="bytecode">The bytecode program to load.</param>
        public void LoadCode(byte[] bytecode)
        {
            _code = bytecode;
            InstructionPointer = 0;
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the current position in the bytecode.
        /// </summary>
        /// <returns>The 32-bit signed integer read from the bytecode.</returns>
        /// <remarks>
        /// Advances the instruction pointer by 4 bytes after reading.
        /// </remarks>
        public int ReadInt()
        {
            var result = BitConverter.ToInt32(_code, InstructionPointer);
            InstructionPointer += 4;
            return result;
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the current position in the bytecode.
        /// </summary>
        /// <returns>The 16-bit signed integer read from the bytecode.</returns>
        /// <remarks>
        /// Advances the instruction pointer by 2 bytes after reading.
        /// </remarks>
        public short ReadShort()
        {
            var result = BitConverter.ToInt16(_code, InstructionPointer);
            InstructionPointer += 2;
            return result;
        }

        /// <summary>
        /// Reads a single byte from the current position in the bytecode.
        /// </summary>
        /// <returns>The byte read from the bytecode.</returns>
        /// <exception cref="VmException">Thrown when attempting to read beyond the bytecode boundary.</exception>
        /// <remarks>
        /// Advances the instruction pointer by 1 byte after reading.
        /// </remarks>
        public byte ReadByte()
        {
            if (InstructionPointer >= _code.Length)
                throw new VmException("Attempt to read beyond bytecode boundary");
            return _code[InstructionPointer++];
        }

        /// <summary>
        /// Reads a specified number of bytes from the current position in the bytecode.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>An array containing the requested bytes.</returns>
        /// <exception cref="VmException">Thrown when attempting to read beyond the bytecode boundary.</exception>
        /// <remarks>
        /// Advances the instruction pointer by the number of bytes read.
        /// </remarks>
        public byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = ReadByte();
            }
            return result;
        }
    }
}