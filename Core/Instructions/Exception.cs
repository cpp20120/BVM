using VM.Core.Instructions;

namespace VM.Core.Exceptions
{
    /// <summary>
    /// Базовое исключение виртуальной машины
    /// </summary>
    public class VMException : Exception
    {
        public int LineNumber { get; }
        public int InstructionPointer { get; }

        public VMException(string message, int lineNumber = -1, int ip = -1) 
            : base(message)
        {
            LineNumber = lineNumber;
            InstructionPointer = ip;
        }
    }

    /// <summary>
    /// Ошибка несоответствия типов
    /// </summary>
    public class VMTypeException : VMException
    {
        public VMType ActualType { get; }
        public VMType ExpectedType { get; }

        public VMTypeException(VMType actual, VMType expected, int line = -1, int ip = -1)
            : base($"Type mismatch: expected {expected}, got {actual}", line, ip)
        {
            ActualType = actual;
            ExpectedType = expected;
        }

        public VMTypeException(string message, int line = -1, int ip = -1)
            : base(message, line, ip)
        {
            ActualType = VMType.Null;
            ExpectedType = VMType.Null;
        }
    }

    /// <summary>
    /// Ошибка работы со стеком
    /// </summary>
    public class VMStackException : VMException
    {
        public VMStackException(string message, int line = -1, int ip = -1)
            : base(message, line, ip) { }
    }

    /// <summary>
    /// Ошибка доступа к памяти
    /// </summary>
    public class VMMemoryException : VMException
    {
        public int Address { get; }

        public VMMemoryException(int address, string message, int line = -1, int ip = -1)
            : base($"Memory error at 0x{address:X4}: {message}", line, ip)
        {
            Address = address;
        }
    }
}