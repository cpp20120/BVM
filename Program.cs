using VM.Parser;

var code = @"
LET x = 5
PRINT x
INPUT a, b, c
CONTINUE
EXIT

";

var tokenizer = new Tokenizer(code);
var tokens = tokenizer.Tokenize().ToList();

var parser = new ProgramParser(tokens);
var ast = parser.ParseProgram();
Console.WriteLine(ast);

Console.WriteLine("=== TOKENS ===");
foreach (var tok in tokens)
    Console.WriteLine($"{tok.Type}: '{tok.Text}' (Line {tok.Line})");

Console.WriteLine("\n=== AST ===");
Console.WriteLine(ast);
