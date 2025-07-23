using VM.Core;
using VM.Parser;
using VM.Core.IR;
using VM.Core.IR.Nodes;

namespace VM
{
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

            var tokenizer = new Tokenizer(code);
            var tokens = tokenizer.Tokenize().ToList();

            var parser = new ProgramParser(tokens);
            var ast = parser.ParseProgram();

            var astToIr = new AstToIrCompiler();
            List<IrNode?> ir = ast.Accept(astToIr);

            var irCompiler = new IrToBytecodeCompiler();
            var bytecode = irCompiler.Compile(ir);

            var vm = new VirtualMachine();
            vm.LoadProgram(bytecode);
            vm.DumpState();
            vm.Run();
            vm.DumpState();
        }
    }
}