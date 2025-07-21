using System.Collections.Generic;

namespace VM.Parser
{
    public class ProgramParser(List<Token> tokens)
    {
        private readonly List<Token> tokens = tokens;
        private int position = 0;

        private Token Peek(int offset = 0) => position + offset < tokens.Count ? tokens[position + offset] : tokens[^1];
        private Token Next() => tokens[position++];
        private bool Match(params TokenType[] types)
        {
            if (types.Contains(Peek().Type))
            {
                position++;
                return true;
            }
            return false;
        }

        private void Expect(TokenType type)
        {
            if (!Match(type))
                throw new Exception($"ожидался токен {type}, но был {Peek().Type}");
        }

        public ProgramNode ParseProgram()
        {
            var program = new ProgramNode();
            while (!Match(TokenType.EOF))
            {
                Console.WriteLine($"[program] next token: {Peek().Type} '{Peek().Text}'");

                if (Peek().Type == TokenType.NEWLINE || Peek().Type == TokenType.COMMENT)
                {
                    Next();
                    continue;
                }

                var stmt = ParseStatement();
                program.Statements.Add(stmt);

                if (Peek().Type == TokenType.NEWLINE)
                    Next();
            }
            return program;
        }


        private StatementNode ParseStatement()
        {
            var token = Peek();

            switch (token.Type)
            {
                case TokenType.PRINT:
                    return ParsePrint();
                case TokenType.LET:
                    return ParseLet();
                case TokenType.IF:
                    return ParseIf;
                case TokenType.WHILE:
                    return ParseWhile;
                case TokenType.INPUT:
                    return ParseInput;
                case TokenType.CONTINUE:
                    Next();
                    return new ContinueStmt();
                case TokenType.EXIT:
                    Next();
                    return new ExitStmt();
                default:
                    throw new Exception($"неизвестное начало инструкции: {token.Type}");
            }
        }


        private StatementNode ParsePrint()
        {
            Next(); // skip PRINT
            var stmt = new PrintStmt();
            while (Peek().Type != TokenType.NEWLINE && Peek().Type != TokenType.EOF)
            {
                stmt.Expressions.Add(ParseExpr());
                if (!Match(TokenType.COMMA)) break;
            }
            return stmt;
        }

        private StatementNode ParseLet()
        {
            Next(); // skip LET
            var id = Next();
            Expect(TokenType.ASSIGN);
            var expr = ParseExpr();
            return new LetStmt { Id = id.Text, Expression = expr };
        }

        private StatementNode ParseIf
        {
            get
            {
                Next(); // skip IF
                var cond = ParseExpr();
                Expect(TokenType.THEN);
                if (Peek().Type == TokenType.NEWLINE) Next();

                var thenBlock = new List<StatementNode>();
                while (Peek().Type != TokenType.ELSE && !(Peek().Type == TokenType.END && Peek(1).Type == TokenType.IF))
                {
                    thenBlock.Add(ParseStatement());
                    if (Peek().Type == TokenType.NEWLINE) Next();
                }

                List<StatementNode> elseBlock = new();

                if (Match(TokenType.ELSE))
                {
                    if (Peek().Type == TokenType.NEWLINE) Next();
                    while (!(Peek().Type == TokenType.END && Peek(1).Type == TokenType.IF))
                    {
                        elseBlock.Add(ParseStatement());
                        if (Peek().Type == TokenType.NEWLINE) Next();
                    }
                }

                Expect(TokenType.END);
                Expect(TokenType.IF);

                return new IfStmt { Condition = cond, ThenBranch = thenBlock, ElseBranch = elseBlock };
            }
        }

        private StatementNode ParseWhile
        {
            get
            {
                Next(); // skip WHILE
                var cond = ParseExpr();
                if (Peek().Type == TokenType.NEWLINE) Next();

                var body = new List<StatementNode>();
                while (Peek().Type != TokenType.WEND)
                {
                    body.Add(ParseStatement());
                    if (Peek().Type == TokenType.NEWLINE) Next();
                }

                Expect(TokenType.WEND);
                return new WhileStmt { Condition = cond, Body = body };
            }
        }

        private StatementNode ParseInput
        {
            get
            {
                Next(); // skip INPUT
                var stmt = new InputStmt();
                var first = Next();
                if (first.Type != TokenType.ID)
                    throw new Exception($"ожидался идентификатор после INPUT, а не {first.Type}");
                stmt.Ids.Add(first.Text);

                while (Match(TokenType.COMMA))
                {
                    var id = Next();
                    if (id.Type != TokenType.ID)
                        throw new Exception($"ожидался идентификатор после ','");
                    stmt.Ids.Add(id.Text);
                }

                return stmt;
            }
        }

        private ExprNode ParseExpr()
        {
            var token = Next();
            return token.Type switch
            {
                TokenType.NUMBER => new NumberExpr { Value = token.Text },
                TokenType.STRING => new StringExpr { Value = token.Text },
                TokenType.ID => new VarExpr { Name = token.Text },
                _ => throw new Exception($"неожиданный токен в выражении: {token.Type}")
            };
        }

    }
}
