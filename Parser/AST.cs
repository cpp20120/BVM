namespace VM.Parser
{
    public interface IAstVisitor<out T>
    {
        T Visit(ProgramNode node);
        T Visit(PrintStmt node);
        T Visit(LetStmt node);
        T Visit(IfStmt node);
        T Visit(WhileStmt node);
        T Visit(RepeatStmt node);
        T Visit(ForStmt node);
        T Visit(InputStmt node);
        T Visit(ContinueStmt node);
        T Visit(ExitStmt node);
        T Visit(BinaryExpr node);
        T Visit(UnaryExpr node);
        T Visit(NumberExpr node);
        T Visit(StringExpr node);
        T Visit(VarExpr node);
        T Visit(FuncCallExpr node);
        T Visit(IndexExpr node);
        T Visit(AssignIndexStmt node);
        T Visit(CustomCallExpr node);
        T Visit(NewArrayExpr node);
        T Visit(ExprNode node);
    }

    public abstract class AstNode
    {
        public int Line { get; init; }
        public abstract override string ToString();
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }

    public class ProgramNode : AstNode
    {
        public readonly List<StatementNode> Statements = [];

        public override string ToString() =>
            string.Join("\n", Statements.Select(stmt => stmt.ToString()));

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public abstract class StatementNode : AstNode
    {
    }

    public class PrintStmt : StatementNode
    {
        public readonly List<ExprNode> Expressions = [];

        public override string ToString() =>
            Expressions.Count == 0
                ? "PRINT"
                : $"PRINT {string.Join(", ", Expressions)}";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class LetStmt : StatementNode
    {
        public string? Id;
        public ExprNode? Expression;

        public override string ToString() => $"LET {Id} = {Expression}";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class IfStmt : StatementNode
    {
        public required ExprNode Condition;
        public List<StatementNode> ThenBranch = [];
        public List<StatementNode> ElseBranch = [];

        public override string ToString()
        {
            var thenStr = string.Join("\n", ThenBranch.Select(s => $"  {s}"));
            var elseStr = ElseBranch.Count > 0
                ? $"\nELSE\n{string.Join("\n", ElseBranch.Select(s => $"  {s}"))}"
                : "";

            return $"IF {Condition} THEN\n{thenStr}{elseStr}\nEND IF";
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class WhileStmt : StatementNode
    {
        public ExprNode? Condition;
        public List<StatementNode> Body = [];

        public override string ToString()
        {
            var bodyStr = string.Join("\n", Body.Select(s => $"  {s}"));
            return $"WHILE {Condition}\n{bodyStr}\nWEND";
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class RepeatStmt : StatementNode
    {
        public ExprNode? Condition;
        public List<StatementNode> Body = [];

        public override string ToString()
        {
            var bodyStr = string.Join("\n", Body.Select(s => $"  {s}"));
            return $"REPEAT\n{bodyStr}\nUNTIL {Condition}";
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ForStmt : StatementNode
    {
        public required string Variable;
        public required ExprNode From;
        public required ExprNode To;
        public ExprNode? Step;
        public List<StatementNode> Body = [];

        public override string ToString()
        {
            var stepStr = Step != null ? $@" STEP {Step}" : "";
            var bodyStr = string.Join("\n", Body.Select(s => $"  {s}"));
            return $"FOR {Variable} = {From} TO {To}{stepStr}\n{bodyStr}\nNEXT {Variable}";
        }

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class InputStmt : StatementNode
    {
        public readonly List<string> Ids = [];
        public override string ToString() => $"INPUT {string.Join(", ", Ids)}";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ContinueStmt : StatementNode
    {
        public override string ToString() => "CONTINUE";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ExitStmt : StatementNode
    {
        public override string ToString() => "EXIT";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public abstract class ExprNode : AstNode
    {
    }

    public class BinaryExpr : ExprNode
    {
        public ExprNode? Left;
        public TokenType Operator;
        public ExprNode? Right;

        public override string ToString() =>
            $"({Left} {TokenToString(Operator)} {Right})";

        private static string TokenToString(TokenType type) => type switch
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

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
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

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class NumberExpr : ExprNode
    {
        public required string Value;
        public override string ToString() => Value;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class StringExpr : ExprNode
    {
        public required string Value;
        public override string ToString() => $"\"{Value}\"";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class VarExpr : ExprNode
    {
        public required string Name;
        public override string ToString() => Name;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class FuncCallExpr : ExprNode
    {
        public required TokenType Func;
        public List<ExprNode> Arguments = [];

        public override string ToString() =>
            $"{Func.ToString().ToLower()}({string.Join(", ", Arguments)})";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class IndexExpr : ExprNode
    {
        public required ExprNode Target;
        public required ExprNode Index;

        public override string ToString() => $"{Target}[{Index}]";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class AssignIndexStmt : StatementNode
    {
        public required string Target;
        public required ExprNode Index;
        public required ExprNode Value;

        public override string ToString() => $"LET {Target}[{Index}] = {Value}";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }


    public class CustomCallExpr : ExprNode
    {
        public required string Name;
        public required List<ExprNode> Args;

        public override string ToString() => $"{Name}({string.Join(", ", Args)})";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }

    public class NewArrayExpr : ExprNode
    {
        public ExprNode Size { get; init; }
        public override string ToString() => $"ARRAY({Size})";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}