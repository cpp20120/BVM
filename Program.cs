using VM.Parser;

const string code = @"
INPUT A, B

LET X = 5
LET Y = 10
LET Z = X

PRINT X, Y, Z

IF X THEN
    PRINT ""X is true""
ELSE
    PRINT ""X is false""
END IF

FOR I = 1 TO 3
    PRINT I
NEXT I

FOR i = 1 TO 10 STEP 2
    PRINT i
    IF i > 5 THEN
       PRINT ""Over 5""
    ELSE
       PRINT ""5 or less""
    END IF
NEXT

WHILE X
    PRINT X
    LET X = 0
WEND

REPEAT
    PRINT ""In repeat""
UNTIL Z


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
