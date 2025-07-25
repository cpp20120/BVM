﻿using VM.Core.Instructions;

namespace VM.Core.Instructions
{
    public class VmException(string message, int lineNumber = -1, int ip = -1) : Exception(message)
    {
        public int LineNumber { get; } = lineNumber;
        public int InstructionPointer { get; } = ip;
    }

    public class VmTypeException : VmException
    {
        public VmType ActualType { get; }
        public VmType ExpectedType { get; }

        public VmTypeException(VmType actual, VmType expected, int line = -1, int ip = -1)
            : base($"Type mismatch: expected {expected}, got {actual}", line, ip)
        {
            ActualType = actual;
            ExpectedType = expected;
        }

        public VmTypeException(string message, int line = -1, int ip = -1)
            : base(message, line, ip)
        {
            ActualType = VmType.NULL;
            ExpectedType = VmType.NULL;
        }
    }

    public class VmStackException(string message, int line = -1, int ip = -1) : VmException(message, line, ip);

    public class VmMemoryException(int address, string message, int line = -1, int ip = -1)
        : VmException($"Memory error at 0x{address:X4}: {message}", line, ip)
    {
        public int Address { get; } = address;
    }
}