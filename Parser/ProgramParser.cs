namespace VM.Parser
{
    /// <summary>
    /// Exception thrown when a syntax error occurs during parsing
    /// </summary>
    /// <param name="line"></param>
    /// <param name="message"></param>
    public class ParseException(int line, string message) : Exception($"Line {line}: {message}")
    {
        public int Line { get; } = line;
    }
    /// <summary>
    /// Handwritten top-down LL(1) parser with operator precedence support
    /// for a BASIC-like language.
    /// </summary>
    /// <param name="tokens"></param>
    public partial class ProgramParser(List<Token> tokens)
    {
        private int _position;
        /// <summary>Reads the next token without advancing the cursor.</summary>
        private Token Peek(int offset = 0) =>
            _position + offset < tokens.Count ? tokens[_position + offset] : tokens[^1];

        /// <summary>Advances the cursor and returns the next token.</summary>
        private Token Next() => tokens[_position++];

        /// <summary>Checks if the current token matches any of the given types and advances if it does.</summary>
        private bool Match(params TokenType[] types)
        {
            if (!types.Contains(Peek().Type)) return false;
            _position++;
            return true;
        }

        /// <summary>Expects the given token type or throws a ParseException.</summary>
        private void Expect(TokenType type)
        {
            if (Match(type)) return;
            var next = Peek();
            throw new ParseException(next.Line,
                $"Expected {type} but found {next.Type} '{next.Text}'");
        }

        /// <summary>Skips all newline tokens.</summary>
        private void SkipNewlines()
        {
            while (Peek().Type == TokenType.NEWLINE)
                Next();
        }

        /// <summary>Begins parsing and returns the root program node.</summary>
        public ProgramNode ParseProgram()
        {
            var program = new ProgramNode();
            while (!Match(TokenType.EOF))
            {
                SkipNewlines();
                if (Peek().Type == TokenType.COMMENT)
                {
                    Next();
                    continue;
                }

                var stmt = ParseStatement();
                program.Statements.Add(stmt);
                SkipNewlines();
            }

            return program;
        }

        /// <summary>Parses a statement node.</summary>
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
                    return ParseIf();
                case TokenType.WHILE:
                    return ParseWhile();
                case TokenType.REPEAT:
                    return ParseRepeat();
                case TokenType.FOR:
                    return ParseFor();
                case TokenType.INPUT:
                    return ParseInput();
                case TokenType.CONTINUE:
                    Next();
                    return new ContinueStmt { Line = token.Line };
                case TokenType.EXIT:
                    Next();
                    return new ExitStmt { Line = token.Line };
                default:
                    throw new ParseException(token.Line,
                        $"Unknown statement beginning: {token.Type}");
            }
        }
        /// <summary>
        /// Parses a PRINT statement, optionally followed by comma-separated expressions.
        /// </summary>
        private StatementNode ParsePrint()
        {
            var printToken = Next();
            var stmt = new PrintStmt { Line = printToken.Line };

            if (Peek().Type == TokenType.NEWLINE || Peek().Type == TokenType.EOF)
                return stmt;

            stmt.Expressions.Add(ParseExpr());
            while (Match(TokenType.COMMA))
            {
                stmt.Expressions.Add(ParseExpr());
            }

            return stmt;
        }
        /// <summary>
        /// Parses a LET statement, which may assign a value to a variable or an array index.
        /// </summary>
        private StatementNode ParseLet()
        {
            var letToken = Next();
            var id = Next();

            if (id.Type != TokenType.ID)
                throw new ParseException(id.Line, $"Expected identifier after LET, got {id.Type}");

            if (Peek().Type == TokenType.LBRACKET)
            {
                Next(); // Skip '['
                var index = ParseExpr();
                Expect(TokenType.RBRACKET);
                Expect(TokenType.ASSIGN);
                var value = ParseExpr();

                return new AssignIndexStmt
                {
                    Target = id.Text,
                    Index = index,
                    Value = value,
                    Line = letToken.Line
                };
            }

            Expect(TokenType.ASSIGN);
            var expr = ParseExpr();

            return new LetStmt
            {
                Id = id.Text,
                Expression = expr,
                Line = letToken.Line
            };
        }
        /// <summary>
        /// Parses an IF statement including optional ELSE block and mandatory END IF.
        /// </summary>
        private StatementNode ParseIf()
        {
            var ifToken = Next();
            var cond = ParseExpr();
            Expect(TokenType.THEN);

            if (Peek().Type == TokenType.NEWLINE)
                Next();

            var thenBlock = new List<StatementNode>();
            while (Peek().Type != TokenType.ELSE &&
                   !(Peek().Type == TokenType.END && Peek(1).Type == TokenType.IF))
            {
                thenBlock.Add(ParseStatement());
                if (Peek().Type == TokenType.NEWLINE)
                    Next();
            }

            List<StatementNode> elseBlock = [];
            if (Match(TokenType.ELSE))
            {
                if (Peek().Type == TokenType.NEWLINE)
                    Next();

                while (!(Peek().Type == TokenType.END && Peek(1).Type == TokenType.IF))
                {
                    elseBlock.Add(ParseStatement());
                    if (Peek().Type == TokenType.NEWLINE)
                        Next();
                }
            }

            Expect(TokenType.END);
            Expect(TokenType.IF);

            return new IfStmt
            {
                Condition = cond,
                ThenBranch = thenBlock,
                ElseBranch = elseBlock,
                Line = ifToken.Line
            };
        }
        /// <summary>
        /// Parses a WHILE loop, which executes until the condition becomes false.
        /// </summary>
        private StatementNode ParseWhile()
        {
            var whileToken = Next();
            var cond = ParseExpr();

            if (Peek().Type == TokenType.NEWLINE)
                Next();

            var body = new List<StatementNode>();
            while (Peek().Type != TokenType.WEND)
            {
                body.Add(ParseStatement());
                if (Peek().Type == TokenType.NEWLINE)
                    Next();
            }

            Expect(TokenType.WEND);
            return new WhileStmt
            {
                Condition = cond,
                Body = body,
                Line = whileToken.Line
            };
        }
        /// <summary>
        /// Parses a REPEAT loop that continues until the UNTIL condition evaluates to true.
        /// </summary>
        private StatementNode ParseInput()
        {
            var inputToken = Next();
            var stmt = new InputStmt { Line = inputToken.Line };
            var first = Next();

            if (first.Type != TokenType.ID)
                throw new ParseException(first.Line,
                    $"Expected identifier after INPUT, not {first.Type}");

            stmt.Ids.Add(first.Text);

            while (Match(TokenType.COMMA))
            {
                var id = Next();
                if (id.Type != TokenType.ID)
                    throw new ParseException(id.Line,
                        $"Expected identifier after ','");
                stmt.Ids.Add(id.Text);
            }

            return stmt;
        }
        /// <summary>
        /// Parses a REPEAT loop that continues until the UNTIL condition evaluates to true.
        /// </summary>
        private StatementNode ParseRepeat()
        {
            var repeatToken = Next();
            if (Peek().Type == TokenType.NEWLINE)
                Next();

            var body = new List<StatementNode>();
            while (Peek().Type != TokenType.UNTIL)
            {
                body.Add(ParseStatement());
                if (Peek().Type == TokenType.NEWLINE)
                    Next();
            }

            Expect(TokenType.UNTIL);
            var condition = ParseExpr();

            return new RepeatStmt
            {
                Body = body,
                Condition = condition,
                Line = repeatToken.Line
            };
        }
        /// <summary>
        /// Parses a FOR loop with optional STEP, including initialization, condition, and body.
        /// </summary>
        private StatementNode ParseFor()
        {
            var forToken = Next();
            var id = Next();
            Expect(TokenType.ASSIGN);
            var fromExpr = ParseExpr();
            Expect(TokenType.TO);
            var toExpr = ParseExpr();

            ExprNode? stepExpr = null;
            if (Match(TokenType.STEP))
                stepExpr = ParseExpr();

            if (Peek().Type == TokenType.NEWLINE)
                Next();

            var body = new List<StatementNode>();
            while (Peek().Type != TokenType.NEXT)
            {
                body.Add(ParseStatement());
                if (Peek().Type == TokenType.NEWLINE)
                    Next();
            }

            Expect(TokenType.NEXT);
            if (Peek().Type == TokenType.ID)
                Next();

            return new ForStmt
            {
                Variable = id.Text,
                From = fromExpr,
                To = toExpr,
                Step = stepExpr,
                Body = body,
                Line = forToken.Line
            };
        }
        /// <summary>
        /// Parses any valid expression starting from the lowest precedence.
        /// </summary>
        private ExprNode ParseExpr() => ParseBinaryExpr(0);
        /// <summary>
        /// Parses binary expressions using precedence climbing algorithm.
        /// Supports chaining binary operators according to precedence.
        /// </summary>
        private ExprNode ParseBinaryExpr(int minPrecedence)
        {
            var left = ParseUnaryExpr();

            while (true)
            {
                var op = Peek();
                var precedence = GetPrecedence(op.Type);
                if (precedence < minPrecedence) break;

                if (!IsBinaryOperator(op.Type)) break;

                Next();

                var right = ParseBinaryExpr(precedence + 1);
                left = new BinaryExpr
                {
                    Left = left,
                    Operator = op.Type,
                    Right = right,
                    Line = op.Line
                };
            }

            return left;
        }
        /// <summary>
        /// Parses unary expressions like negation or logical NOT.
        /// </summary>
        private ExprNode ParseUnaryExpr()
        {
            if (!Match(TokenType.SUB, TokenType.NOT)) return ParsePrimaryExpr();
            var opToken = tokens[_position - 1];
            return new UnaryExpr
            {
                Operator = opToken.Type,
                Operand = ParseUnaryExpr(),
                Line = opToken.Line
            };
        }
        /// <summary>
        /// Parses primary expressions: literals, variables, function calls, and array creation/indexing.
        /// </summary>
        private ExprNode ParsePrimaryExpr()
        {
            if (Match(TokenType.LPAREN))
            {
                var lparen = tokens[_position - 1];
                var expr = ParseExpr();
                Expect(TokenType.RPAREN);
                return expr;
            }

            var token = Peek();
            if (token.Type is TokenType.LEN or TokenType.VAL or TokenType.ISNAN)
            {
                return ParseFuncCall();
            }

            if (token.Type == TokenType.ARRAY)
            {
                Next();
                Expect(TokenType.LPAREN);
                var sizeExpr = ParseExpr();
                Expect(TokenType.RPAREN);
                return new NewArrayExpr
                {
                    Size = sizeExpr,
                    Line = token.Line
                };
            }

            token = Next();
            switch (token.Type)
            {
                case TokenType.NUMBER:
                    return new NumberExpr { Value = token.Text, Line = token.Line };
                case TokenType.STRING:
                    return new StringExpr { Value = token.Text, Line = token.Line };
                case TokenType.ID:
                    if (Peek().Type != TokenType.LBRACKET) return new VarExpr { Name = token.Text, Line = token.Line };
                    Next(); // Skip '['
                    var index = ParseExpr();
                    Expect(TokenType.RBRACKET);
                    return new IndexExpr
                    {
                        Target = new VarExpr { Name = token.Text, Line = token.Line },
                        Index = index,
                        Line = token.Line
                    };
                default:
                    throw new ParseException(token.Line,
                        $"Unexpected token in expression: {token.Type}");
            }
        }
        /// <summary>
        /// Parses a built-in function call with one or more arguments.
        /// </summary>
        private ExprNode ParseFuncCall()
        {
            var funcToken = Next();
            Expect(TokenType.LPAREN);
            var args = new List<ExprNode>();

            if (Peek().Type != TokenType.RPAREN)
            {
                args.Add(ParseExpr());
                while (Match(TokenType.COMMA))
                {
                    args.Add(ParseExpr());
                }
            }

            Expect(TokenType.RPAREN);

            return new FuncCallExpr
            {
                Func = funcToken.Type,
                Arguments = args,
                Line = funcToken.Line
            };
        }
        /// <summary>
        /// Returns precedence value for a binary operator token.
        /// Higher values mean higher precedence.
        /// </summary>
        private static int GetPrecedence(TokenType type) => type switch
        {
            TokenType.OR => 1,
            TokenType.AND => 2,
            TokenType.EQ or TokenType.NEQ or TokenType.LT or TokenType.LTE or TokenType.GT or TokenType.GTE => 3,
            TokenType.ADD or TokenType.SUB => 4,
            TokenType.MUL or TokenType.DIV or TokenType.MOD => 5,
            TokenType.EXP => 6,
            _ => 0
        };
        /// <summary>
        /// Determines whether a token type represents a binary operator.
        /// </summary>
        private static bool IsBinaryOperator(TokenType type) => type switch
        {
            TokenType.ADD or TokenType.SUB or TokenType.MUL or TokenType.DIV or
                TokenType.MOD or TokenType.EXP or TokenType.EQ or TokenType.NEQ or
                TokenType.LT or TokenType.LTE or TokenType.GT or TokenType.GTE or
                TokenType.AND or TokenType.OR => true,
            _ => false
        };
    }
}