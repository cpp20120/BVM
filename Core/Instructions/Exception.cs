using VM.Core.Instructions;

namespace VM.Core.Instructions
{
    /// <summary>
    /// The base exception class for all virtual machine runtime errors.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="lineNumber">The source line number where the error occurred (-1 if unknown).</param>
    /// <param name="ip">The instruction pointer value when the error occurred (-1 if unknown).</param>
    public class VmException(string message, int lineNumber = -1, int ip = -1) : Exception(message)
    {
        /// <summary>
        /// Gets the source line number where the error occurred.
        /// </summary>
        /// <value>-1 if the line number is unknown.</value>
        public int LineNumber { get; } = lineNumber;

        /// <summary>
        /// Gets the instruction pointer value at the time of the error.
        /// </summary>
        /// <value>-1 if the instruction pointer is unknown.</value>
        public int InstructionPointer { get; } = ip;
    }

    /// <summary>
    /// Represents errors that occur when type checking fails in the virtual machine.
    /// </summary>
    public class VmTypeException : VmException
    {
        /// <summary>
        /// Gets the actual type encountered during execution.
        /// </summary>
        public VmType ActualType { get; }

        /// <summary>
        /// Gets the expected type that should have been encountered.
        /// </summary>
        public VmType ExpectedType { get; }

        /// <summary>
        /// Initializes a new instance of the VmTypeException class with the actual and expected types.
        /// </summary>
        /// <param name="actual">The actual type encountered.</param>
        /// <param name="expected">The expected type.</param>
        /// <param name="line">The source line number where the error occurred (-1 if unknown).</param>
        /// <param name="ip">The instruction pointer value when the error occurred (-1 if unknown).</param>
        public VmTypeException(VmType actual, VmType expected, int line = -1, int ip = -1)
            : base($"Type mismatch: expected {expected}, got {actual}", line, ip)
        {
            ActualType = actual;
            ExpectedType = expected;
        }

        /// <summary>
        /// Initializes a new instance of the VmTypeException class with a custom message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="line">The source line number where the error occurred (-1 if unknown).</param>
        /// <param name="ip">The instruction pointer value when the error occurred (-1 if unknown).</param>
        public VmTypeException(string message, int line = -1, int ip = -1)
            : base(message, line, ip)
        {
            ActualType = VmType.NULL;
            ExpectedType = VmType.NULL;
        }
    }

    /// <summary>
    /// Represents errors that occur during stack operations in the virtual machine.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="line">The source line number where the error occurred (-1 if unknown).</param>
    /// <param name="ip">The instruction pointer value when the error occurred (-1 if unknown).</param>
    public class VmStackException(string message, int line = -1, int ip = -1) : VmException(message, line, ip);

    /// <summary>
    /// Represents errors that occur during memory access operations in the virtual machine.
    /// </summary>
    /// <param name="address">The memory address where the error occurred.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="line">The source line number where the error occurred (-1 if unknown).</param>
    /// <param name="ip">The instruction pointer value when the error occurred (-1 if unknown).</param>
    public class VmMemoryException(int address, string message, int line = -1, int ip = -1)
        : VmException($"Memory error at 0x{address:X4}: {message}", line, ip)
    {
        /// <summary>
        /// Gets the memory address where the error occurred.
        /// </summary>
        public int Address { get; } = address;
    }
}