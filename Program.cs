using System;
using System.Collections.Generic;
using System.Linq;
using VM.Core;
using VM.Core.Instructions;
using VM.Parser;
using VM.Core.IR;
using VM.Core.IR.Nodes;

class Program
{
    static void Main()
    {
        const string code = @"

LET A = ARRAY(5)
LET I = 0

REPEAT
    PRINT ""Enter number:""
    INPUT TMP
    LET A[I] = TMP
    LET I = I + 1
UNTIL I == 5

LET SUM = 0
LET I = 0

WHILE I < 5
    LET SUM = SUM + A[I]
    LET I = I + 1
WEND

LET AVG = SUM / 5
PRINT ""Average:""
PRINT AVG

LET I = 0
WHILE I < 5
    IF A[I] > AVG THEN
        PRINT A[I]
        PRINT ""Above average""
    ELSE
        IF A[I] == AVG THEN
            PRINT A[I]
            PRINT ""Equals average""
        ELSE
            PRINT A[I]
            PRINT ""Below average""
        END IF
    END IF
    LET I = I + 1
WEND

";

        // === TOKENIZE ===
        var tokenizer = new Tokenizer(code);
        var tokens = tokenizer.Tokenize().ToList();

        // === PARSE ===
        var parser = new ProgramParser(tokens);
        var ast = parser.ParseProgram();

        // === COMPILE TO IR ===
        var astToIr = new AstToIrCompiler();
        List<IrNode?> ir = ast.Accept(astToIr);

        // === COMPILE TO BYTECODE ===
        var irCompiler = new IrToBytecodeCompiler();
        byte[] bytecode = irCompiler.Compile(ir);

        // === RUN ON VM ===
        var vm = new VirtualMachine();
        vm.LoadProgram(bytecode);
        vm.Run();
    }
}