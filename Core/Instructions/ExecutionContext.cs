using VM.Core.Exceptions;
using VM.Core.Instructions;

namespace VM.Core
{
    public class ExContext
    {
        public DataStack DataStack { get; } = new();
        public CallStack CallStack { get; } = new();
        public Dictionary<int, VMValue> LocalVariables = new();


        public int InstructionPointer { get; set; }

        private byte[] _code;
        public void LoadCode(byte[] bytecode)
        {
            _code = bytecode;
            InstructionPointer = 0;
        }

        public int ReadInt()
        {
            int result = BitConverter.ToInt32(_code, InstructionPointer);
            InstructionPointer += 4;
            return result;
        }

        public short ReadShort()
        {
            short result = BitConverter.ToInt16(_code, InstructionPointer);
            InstructionPointer += 2;
            return result;
        }

        // In ExContext.ReadByte():
        public byte ReadByte()
        {
            if (InstructionPointer >= _code.Length)
                throw new VMException("Attempt to read beyond bytecode boundary");
            return _code[InstructionPointer++];
        }
        public byte[] ReadBytes(int count)
        {
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = ReadByte();
            }
            return result;
        }

    }
}