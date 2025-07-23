namespace VM.Core.Instructions
{
   public enum OpCode : byte
    {
        // Стек
        PUSH = 0x01,
        POP = 0x02,
        DUP = 0x03,
        SWAP = 0x04,
        OVER = 0x05,

        // Арифметика
        ADD = 0x10,
        SUB = 0x11,
        MUL = 0x12,
        DIV = 0x13,
        MOD = 0x14,
        NEG = 0x15,

        // Логика
        AND = 0x20,
        OR = 0x21,
        NOT = 0x22,
        CMP = 0x23,
        EQ = 0x24,
        NEQ = 0x25,

        // Переменные
        STORE = 0x30,
        LOAD = 0x31,
        GSTORE = 0x32,
        GLOAD = 0x33,

        // Управление
        JMP = 0x40,
        JZ = 0x41,
        JNZ = 0x42,
        CALL = 0x43,
        RET = 0x44,

        // Стандарт
        PRINT = 0x50,
        INPUT = 0x51,
        HALT = 0x52,

        // Строки
        CONCAT = 0x53,
        STRLEN = 0x54,
        STRVAL = 0x55,
        SUBSTR = 0x56,
        STRCHR = 0x57,
        STRORD = 0x58,
        PUSHS = 0x59,

        // Массивы
        ARRAY = 0x60,
        GETIDX = 0x61,
        SETIDX = 0x62,
        ARRLEN = 0x63,

        // Типы
        TYPE = 0x70,
        CAST = 0x71,
        ISNUM = 0x72,
        ISSTR = 0x73,

        // Файлы
        FSREAD = 0x80,
        FSWRITE = 0x81,
        FSEXISTS = 0x82,
        FSDELETE = 0x83,
        FSAPPEND = 0x84,
        
        //Массивы
        NEWARRAY = 0x85,
        GETINDEX = 0x86,
        SETINDEX = 0x87,
    }
}