namespace VM.Parser
{
    public enum TokenType
    {
        PRINT, INPUT, LET, IF, THEN, ELSE, END, FOR, TO, STEP, NEXT,
        WHILE, WEND, REPEAT, UNTIL, CONTINUE, EXIT,
        LEN, VAL, ISNAN,
        EQ, NEQ, LT, LTE, GT, GTE, ADD, SUB, MUL, DIV, MOD, EXP,
        AND, OR, NOT, ASSIGN,
        LPAREN, RPAREN, COMMA,
        ID, NUMBER, STRING, NEWLINE, COMMENT, EOF
    }

    public record Token(TokenType Type, string Text, int Line);

    public class Tokenizer(string sourceCode)
    {
        private readonly string[] _lines = sourceCode.Replace("\r\n", "\n").Split('\n');
        private int _lineIndex = 0;
        private int _currentLine = 1;

        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            ["print"] = TokenType.PRINT,
            ["input"] = TokenType.INPUT,
            ["let"] = TokenType.LET,
            ["if"] = TokenType.IF,
            ["then"] = TokenType.THEN,
            ["else"] = TokenType.ELSE,
            ["end"] = TokenType.END,
            ["for"] = TokenType.FOR,
            ["to"] = TokenType.TO,
            ["step"] = TokenType.STEP,
            ["next"] = TokenType.NEXT,
            ["while"] = TokenType.WHILE,
            ["wend"] = TokenType.WEND,
            ["repeat"] = TokenType.REPEAT,
            ["until"] = TokenType.UNTIL,
            ["continue"] = TokenType.CONTINUE,
            ["exit"] = TokenType.EXIT,
            ["len"] = TokenType.LEN,
            ["val"] = TokenType.VAL,
            ["isnan"] = TokenType.ISNAN,
            ["and"] = TokenType.AND,
            ["or"] = TokenType.OR,
            ["not"] = TokenType.NOT,
        };

        private static readonly Dictionary<string, TokenType> Symbols = new()
        {
            ["=="] = TokenType.EQ,
            ["="] = TokenType.ASSIGN,
            ["!="] = TokenType.NEQ,
            ["<="] = TokenType.LTE,
            [">="] = TokenType.GTE,
            ["<"] = TokenType.LT,
            [">"] = TokenType.GT,
            ["+"] = TokenType.ADD,
            ["-"] = TokenType.SUB,
            ["*"] = TokenType.MUL,
            ["/"] = TokenType.DIV,
            ["%"] = TokenType.MOD,
            ["^"] = TokenType.EXP,
            ["("] = TokenType.LPAREN,
            [")"] = TokenType.RPAREN,
            [","] = TokenType.COMMA
        };

        public IEnumerable<Token> Tokenize()
        {
            while (_lineIndex < _lines.Length)
            {
                var line = _lines[_lineIndex++].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    yield return new Token(TokenType.NEWLINE, "", _currentLine++);
                    continue;
                }

                var i = 0;
                while (i < line.Length)
                {
                    if (char.IsWhiteSpace(line[i]))
                    {
                        i++;
                        continue;
                    }

                    if (line[i] == '\'')
                    {
                        yield return new Token(TokenType.COMMENT, line[i..], _currentLine++);
                        break;
                    }

                    if (line[i] == '"')
                    {
                        var end = i + 1;
                        while (end < line.Length && line[end] != '"') end++;
                        var str = line[(i + 1)..end];
                        yield return new Token(TokenType.STRING, str, _currentLine);
                        i = end + 1;
                        continue;
                    }

                    var matchedSymbol = false;
                    foreach (var (symbol, type) in Symbols.OrderByDescending(p => p.Key.Length))
                    {
                        if (!line[i..].StartsWith(symbol)) continue;
                        yield return new Token(type, symbol, _currentLine);
                        i += symbol.Length;
                        matchedSymbol = true;
                        break;
                    }
                    if (matchedSymbol) continue;

                    if (char.IsDigit(line[i]))
                    {
                        var  start = i;
                        while (i < line.Length && (char.IsDigit(line[i]) || line[i] == '.')) i++;
                        yield return new Token(TokenType.NUMBER, line[start..i], _currentLine);
                        continue;
                    }

                    if (char.IsLetter(line[i]))
                    {
                        var start = i;
                        while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_')) i++;
                        var text = line[start..i].ToLower();
                        if (Keywords.TryGetValue(text, out var type))
                        {
                            yield return new Token(type, text, _currentLine);
                        }
                        else
                        {
                            yield return new Token(TokenType.ID, text, _currentLine);
                        }
                        continue;
                    }
                    Console.WriteLine($"DEBUG: current line = '{line}', i = {i}, char = '{line[i]}' ({(int)line[i]})");
                    throw new Exception($"unexpected char '{line[i]}' at line {_currentLine}");
                }

                yield return new  Token(TokenType.NEWLINE, "", _currentLine++);
            }

            yield return new Token(TokenType.EOF, "", _currentLine);
        }
    }

}