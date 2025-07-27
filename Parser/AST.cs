namespace VM.Parser
{
    /// <summary>
    /// Defines a visitor interface for traversing AST nodes.
    /// Used to separate processing logic from the AST structure itself (Visitor pattern).
    /// </summary>
    /// <typeparam name="T">The return type of the visitor methods.</typeparam>
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

    /// <summary>
    /// Base class for all AST nodes. Stores line number and provides an accept method for the visitor pattern.
    /// </summary>
    public abstract class AstNode
    {
        /// <summary>
        /// The line number in the source code where this node appears.
        /// </summary>
        public int Line { get; init; }

        /// <summary>
        /// Returns a string representation of the AST node for debugging.
        /// </summary>
        public abstract override string ToString();

        /// <summary>
        /// Accepts a visitor that returns a value.
        /// </summary>
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }

    /// <summary>
    /// Root node of the AST representing the entire program.
    /// Contains a sequence of top-level statements.
    /// </summary>
    public class ProgramNode : AstNode
    {
        public readonly List<StatementNode> Statements = [];

        public override string ToString() =>
            string.Join("\n", Statements.Select(stmt => stmt.ToString()));

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Base class for all statements (i.e., code that performs actions).
    /// </summary>
    public abstract class StatementNode : AstNode
    {
    }
    /// <summary>
    /// Statement representing a PRINT operation, which outputs expressions to the console.
    /// </summary>
    public class PrintStmt : StatementNode
    {
        public readonly List<ExprNode> Expressions = [];

        public override string ToString() =>
            Expressions.Count == 0
                ? "PRINT"
                : $"PRINT {string.Join(", ", Expressions)}";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Statement representing a LET assignment to a variable.
    /// </summary>
    public class LetStmt : StatementNode
    {
        public string? Id;
        public ExprNode? Expression;

        public override string ToString() => $"LET {Id} = {Expression}";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Statement representing a conditional IF block with optional ELSE branch.
    /// </summary>
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
    /// <summary>
    /// Statement representing a WHILE loop.
    /// </summary>
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
    /// <summary>
    /// Statement representing a REPEAT-UNTIL loop.
    /// </summary>
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
    /// <summary>
    /// Statement representing a FOR loop with optional STEP.
    /// </summary>
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
    /// <summary>
    /// Statement for accepting input into one or more variables.
    /// </summary>
    public class InputStmt : StatementNode
    {
        public readonly List<string> Ids = [];
        public override string ToString() => $"INPUT {string.Join(", ", Ids)}";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Statement that causes loop control to jump to the next iteration.
    /// </summary>
    public class ContinueStmt : StatementNode
    {
        public override string ToString() => "CONTINUE";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Statement that causes an immediate exit from the current loop or program.
    /// </summary>
    public class ExitStmt : StatementNode
    {
        public override string ToString() => "EXIT";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Base class for all expressions (i.e., values, operators, function calls).
    /// </summary>
    public abstract class ExprNode : AstNode
    {
    }
    /// <summary>
    /// Expression representing a binary operation (e.g., a + b).
    /// </summary>
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
    /// <summary>
    /// Expression representing a unary operation (e.g., -a or NOT a).
    /// </summary>
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
    /// <summary>
    /// Expression representing a numeric literal.
    /// </summary>
    public class NumberExpr : ExprNode
    {
        public required string Value;
        public override string ToString() => Value;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Expression representing a string literal.
    /// </summary>
    public class StringExpr : ExprNode
    {
        public required string Value;
        public override string ToString() => $"\"{Value}\"";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Expression representing a reference to a variable.
    /// </summary>
    public class VarExpr : ExprNode
    {
        public required string Name;
        public override string ToString() => Name;
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Expression representing a built-in function call.
    /// </summary>
    public class FuncCallExpr : ExprNode
    {
        public required TokenType Func;
        public List<ExprNode> Arguments = [];

        public override string ToString() =>
            $"{Func.ToString().ToLower()}({string.Join(", ", Arguments)})";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Expression representing an index access on an array (e.g., a[5]).
    /// </summary>
    public class IndexExpr : ExprNode
    {
        public required ExprNode Target;
        public required ExprNode Index;

        public override string ToString() => $"{Target}[{Index}]";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Statement representing assignment to an array element by index.
    /// </summary>
    public class AssignIndexStmt : StatementNode
    {
        public required string Target;
        public required ExprNode Index;
        public required ExprNode Value;

        public override string ToString() => $"LET {Target}[{Index}] = {Value}";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Expression representing a custom user-defined function call.
    /// </summary>
    public class CustomCallExpr : ExprNode
    {
        public required string Name;
        public required List<ExprNode> Args;

        public override string ToString() => $"{Name}({string.Join(", ", Args)})";

        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
    /// <summary>
    /// Expression representing array creation using ARRAY(n) syntax.
    /// </summary>
    public class NewArrayExpr : ExprNode
    {
        public required ExprNode Size { get; init; }
        public override string ToString() => $"ARRAY({Size})";
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}