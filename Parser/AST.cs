namespace VM.Parser
{
    public abstract class AstNode
    {
        public abstract override string ToString();
    }

    public class ProgramNode : AstNode
    {
        public List<StatementNode> Statements = [];

        public override string ToString() =>
            string.Join("\n", Statements.Select(stmt => stmt.ToString()));
    }

    public abstract class StatementNode : AstNode { }

    public class PrintStmt : StatementNode
    {
        public List<ExprNode> Expressions = [];

        public override string ToString() =>
            Expressions.Count == 0
                ? "PRINT"
                : $"PRINT {string.Join(", ", Expressions)}";
    }

    public class LetStmt : StatementNode
    {
        public string Id;
        public ExprNode Expression;

        public override string ToString() => $"LET {Id} = {Expression}";
    }

    public class IfStmt : StatementNode
    {
        public required ExprNode Condition;
        public List<StatementNode> ThenBranch = [];
        public List<StatementNode> ElseBranch = []; 

        public override string ToString()
        {
            var thenStr = string.Join("\n", ThenBranch.Select(s => "  " + s));
            var elseStr = ElseBranch.Count > 0
                ? "\nELSE\n" + string.Join("\n", ElseBranch.Select(s => "  " + s))
                : "";

            return $"IF {Condition} THEN\n{thenStr}{elseStr}\nEND IF";
        }
    }

    public class WhileStmt : StatementNode
    {
        public ExprNode? Condition;
        public List<StatementNode> Body = [];

        public override string ToString()
        {
            var bodyStr = string.Join("\n", Body.Select(s => "  " + s));
            return $"WHILE {Condition}\n{bodyStr}\nWEND";
        }
    }
    public class InputStmt : StatementNode
    {
        public List<string> Ids = [];
        public override string ToString() => $"INPUT {string.Join(", ", Ids)}";
    }

    public class ContinueStmt : StatementNode
    {
        public override string ToString() => "CONTINUE";
    }

    public class ExitStmt : StatementNode
    {
        public override string ToString() => "EXIT";
    }

    // need to add other types

    public abstract class ExprNode : AstNode { }

    public class BinaryExpr : ExprNode
    {
        public ExprNode? Left;
        public TokenType Operator;
        public ExprNode? Right;

        public override string ToString() =>
            $"({Left} {TokenToString(Operator)} {Right})";

        private string TokenToString(TokenType type) => type switch
        {
            TokenType.ADD => "+",
            TokenType.SUB => "-",
            TokenType.MUL => "*",
            TokenType.DIV => "/",
            TokenType.MOD => "%",
            TokenType.EXP => "^",
            TokenType.EQ => "==",
            TokenType.NEQ => "!=",
            TokenType.LT => "<",
            TokenType.LTE => "<=",
            TokenType.GT => ">",
            TokenType.GTE => ">=",
            TokenType.AND => "AND",
            TokenType.OR => "OR",
            _ => type.ToString()
        };

    }

    public class UnaryExpr : ExprNode
    {
        public TokenType Operator;
        public ExprNode? Operand;

        public override string ToString()
        {
            var op = Operator switch
            {
                TokenType.SUB => "-",
                TokenType.NOT => "NOT",
                _ => Operator.ToString()
            };
            return $"{op}({Operand})";
        }
    }

    public class NumberExpr : ExprNode
    {
        public required string Value;
        public override string ToString() => Value;
    }

    public class StringExpr : ExprNode
    {
        public required string Value;
        public override string ToString() => $"\"{Value}\"";
    }

    public class VarExpr : ExprNode
    {
        public required string Name;
        public override string ToString() => Name;
    }
}
