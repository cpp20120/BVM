using VM.Core;
using VM.Core.Instructions;
using VM.Parser;
using VM.Core.IR;
using VM.Core.IR.Nodes;

const string code = @"
INPUT A, B

LET X = 5
LET Y = 10
LET Z = X

PRINT X, Y, Z

LET S = A > 0
LET G = B > 0

PRINT S, G

IF S OR G THEN
    PRINT ""At least one is positive""
ELSE
    PRINT ""None are positive""
END IF

IF S AND G THEN
    PRINT ""Both are positive""
ELSE
    PRINT ""At least one is not""
END IF

IF X THEN
    PRINT ""X is true""
ELSE
    PRINT ""X is false""
END IF

FOR I = 1 TO 3
    PRINT I
NEXT I

WHILE X
    PRINT X
    LET X = 0
WEND

REPEAT
    PRINT ""In repeat""
UNTIL Z
";

// === TOKENIZER ===
var tokenizer = new Tokenizer(code);
var tokens = tokenizer.Tokenize().ToList();

Console.WriteLine("=== TOKENS ===");
foreach (var tok in tokens)
    Console.WriteLine($"{tok.Type}: '{tok.Text}' (Line {tok.Line})");

// === PARSER ===
var parser = new ProgramParser(tokens);
var ast = parser.ParseProgram();

Console.WriteLine("\n=== AST ===");
Console.WriteLine(ast);

// === IR ===
var compiler = new AstToIrCompiler();
List<IrNode?> ir = ast.Accept(compiler);

Console.WriteLine("\n=== IR ===");
foreach (var node in ir)
    Console.WriteLine($"{node?.GetType().Name} (Line {node?.Line})");

// === BYTECODE ===
var irCompiler = new IrToBytecodeCompiler();
var bytecode = irCompiler.Compile(ir);

Console.WriteLine("\n=== BYTECODE (hex) ===");
foreach (var t in bytecode)
    Console.Write($"{t:X2} ");

Console.WriteLine();

// === VM EXECUTION ===
var context = new ExContext();
context.LoadCode(bytecode);

var instructions = new Dictionary<OpCode, Instruction>
{
    { OpCode.POP, new PopInstruction() },
    { OpCode.DUP, new DupInstruction() },
    { OpCode.SWAP, new SwapInstruction() },
    { OpCode.OVER, new OverInstruction() },
    { OpCode.ADD, new AddInstruction() },
    { OpCode.SUB, new SubInstruction() },
    { OpCode.MUL, new MulInstruction() },
    { OpCode.DIV, new DivInstruction() },
    { OpCode.MOD, new ModInstruction() },
    { OpCode.CMP, new CmpInstruction() },
    { OpCode.EQ, new EqInstruction() },
    { OpCode.NEQ, new NeqInstruction() },
    { OpCode.NEG, new NegInstruction() },
    { OpCode.NOT, new NotInstruction() },
    { OpCode.LOAD, new LoadInstruction() },
    { OpCode.STORE, new StoreInstruction() },
    { OpCode.JMP, new JmpInstruction() },
    { OpCode.JZ, new JzInstruction() },
    { OpCode.JNZ, new JnzInstruction() },
    { OpCode.CALL, new CallInstruction() },
    { OpCode.RET, new RetInstruction() },
    { OpCode.INPUT, new InputInstruction() },
    { OpCode.PRINT, new PrintInstruction() },
    { OpCode.PUSH, new PushInstruction() },
    { OpCode.PUSHS, new PushStringInstruction() },
    { OpCode.HALT, new HaltInstruction() },
    { OpCode.OR, new OrInstruction() },
    { OpCode.AND, new AndInstruction() }
};

Console.WriteLine("\n=== EXECUTION ===");
try
{
    while (true)
    {
        var op = (OpCode)context.ReadByte();
        if (!instructions.TryGetValue(op, out var instr))
            throw new Exception($"Unknown opcode: {op}");

        Console.WriteLine($"Executing {instr.Mnemonic}");
        instr.Execute(context);
    }
}
catch (VmException e)
{
    Console.WriteLine($"VM halted: {e.Message}");
}
