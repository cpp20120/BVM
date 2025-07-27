namespace VM.Core.Instructions
{
    /// <summary>
    /// Represents the bytecode operation codes (opcodes) for the virtual machine.
    /// </summary>
    /// <remarks>
    /// Each opcode corresponds to a specific operation in the VM's instruction set.
    /// The numeric values are used in the bytecode representation of programs.
    /// </remarks>
    public enum OpCode : byte
    {
        // Stack operations
        /// <summary>Pushes a value onto the stack</summary>
        PUSH = 0x01,
        /// <summary>Removes the top value from the stack</summary>
        POP = 0x02,
        /// <summary>Duplicates the top stack value</summary>
        DUP = 0x03,
        /// <summary>Swaps the top two stack values</summary>
        SWAP = 0x04,
        /// <summary>Copies the second stack value to the top</summary>
        OVER = 0x05,

        // Arithmetic operations
        /// <summary>Adds the top two values</summary>
        ADD = 0x10,
        /// <summary>Subtracts the top value from the second value</summary>
        SUB = 0x11,
        /// <summary>Multiplies the top two values</summary>
        MUL = 0x12,
        /// <summary>Divides the second value by the top value</summary>
        DIV = 0x13,
        /// <summary>Computes modulus of the second value by the top value</summary>
        MOD = 0x14,
        /// <summary>Negates the top value</summary>
        NEG = 0x15,

        // Logical operations
        /// <summary>Performs logical AND on top two values</summary>
        AND = 0x20,
        /// <summary>Performs logical OR on top two values</summary>
        OR = 0x21,
        /// <summary>Performs logical NOT on top value</summary>
        NOT = 0x22,
        /// <summary>Compares top two values (-1, 0, or 1 result)</summary>
        CMP = 0x23,
        /// <summary>Checks if top two values are equal</summary>
        EQ = 0x24,
        /// <summary>Checks if top two values are not equal</summary>
        NEQ = 0x25,

        // Variable operations
        /// <summary>Stores a value in a local variable</summary>
        STORE = 0x30,
        /// <summary>Loads a value from a local variable</summary>
        LOAD = 0x31,
        /// <summary>Stores a value in a global variable</summary>
        GSTORE = 0x32,
        /// <summary>Loads a value from a global variable</summary>
        GLOAD = 0x33,

        // Control flow
        /// <summary>Unconditional jump</summary>
        JMP = 0x40,
        /// <summary>Jumps if top value is zero</summary>
        JZ = 0x41,
        /// <summary>Jumps if top value is not zero</summary>
        JNZ = 0x42,
        /// <summary>Calls a function at specified address</summary>
        CALL = 0x43,
        /// <summary>Returns from a function call</summary>
        RET = 0x44,

        // I/O operations
        /// <summary>Prints the top stack value</summary>
        PRINT = 0x50,
        /// <summary>Reads input from console</summary>
        INPUT = 0x51,
        /// <summary>Stops program execution</summary>
        HALT = 0x52,

        // String operations
        /// <summary>Concatenates two strings</summary>
        CONCAT = 0x53,
        /// <summary>Gets length of a string</summary>
        STRLEN = 0x54,
        /// <summary>Converts value to string</summary>
        STRVAL = 0x55,
        /// <summary>Extracts substring from string</summary>
        SUBSTR = 0x56,
        /// <summary>Gets character at index from string</summary>
        STRCHR = 0x57,
        /// <summary>Gets ASCII code of character</summary>
        STRORD = 0x58,
        /// <summary>Pushes a string constant onto the stack</summary>
        PUSHS = 0x59,

        // Type operations
        /// <summary>Gets type name of value</summary>
        TYPE = 0x70,
        /// <summary>Casts value to different type</summary>
        CAST = 0x71,
        /// <summary>Checks if value is numeric</summary>
        ISNUM = 0x72,
        /// <summary>Checks if value is a string</summary>
        ISSTR = 0x73,

        // File operations
        /// <summary>Reads content from file</summary>
        FSREAD = 0x80,
        /// <summary>Writes content to file</summary>
        FSWRITE = 0x81,
        /// <summary>Checks if file exists</summary>
        FSEXISTS = 0x82,
        /// <summary>Deletes a file</summary>
        FSDELETE = 0x83,
        /// <summary>Appends content to file</summary>
        FSAPPEND = 0x84,

        // Array operations
        /// <summary>Creates a new array</summary>
        NEWARRAY = 0x85,
        /// <summary>Gets element at index from array</summary>
        GETINDEX = 0x86,
        /// <summary>Sets element at index in array</summary>
        SETINDEX = 0x87,
    }
}