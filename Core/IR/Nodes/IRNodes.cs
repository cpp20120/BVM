namespace VM.Core.IR.Nodes
{
    /// <summary>
    /// Abstract base class for all Intermediate Representation (IR) nodes.
    /// </summary>
    public abstract class IrNode
    {
        /// <summary>
        /// Gets or sets the source line number associated with this node.
        /// </summary>
        public int Line { get; init; }
    }

    /// <summary>
    /// Represents a constant value in the IR.
    /// </summary>
    public class IrConst : IrNode
    {
        /// <summary>
        /// Gets or sets the constant value.
        /// </summary>
        public object Value;

        /// <summary>
        /// Gets or sets the type of the constant ("int", "float", "string", "bool").
        /// </summary>
        public string? Type;
    }

    /// <summary>
    /// Represents a variable reference in the IR.
    /// </summary>
    public class IrVar : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        public string? Name;
    }

    /// <summary>
    /// Represents a variable assignment (let binding) in the IR.
    /// </summary>
    public class IrLet : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the variable being assigned.
        /// </summary>
        public string? Name;

        /// <summary>
        /// Gets or sets the expression being assigned to the variable.
        /// </summary>
        public required IrNode? Expr;
    }

    /// <summary>
    /// Represents a print statement in the IR.
    /// </summary>
    public class IrPrint : IrNode
    {
        /// <summary>
        /// Gets or sets the expression to be printed.
        /// </summary>
        public required IrNode? Expr;
    }

    /// <summary>
    /// Represents a binary operation in the IR.
    /// </summary>
    public class IrBinary : IrNode
    {
        /// <summary>
        /// Gets or sets the binary operator (e.g., "+", "-", "*", "/", "==", etc.).
        /// </summary>
        public required string Op;

        /// <summary>
        /// Gets or sets the left operand of the binary operation.
        /// </summary>
        public required IrNode? Left;

        /// <summary>
        /// Gets or sets the right operand of the binary operation.
        /// </summary>
        public required IrNode? Right;
    }

    /// <summary>
    /// Represents a unary operation in the IR.
    /// </summary>
    public class IrUnary : IrNode
    {
        /// <summary>
        /// Gets or sets the unary operator (e.g., "-", "!", etc.).
        /// </summary>
        public required string Op;

        /// <summary>
        /// Gets or sets the operand of the unary operation.
        /// </summary>
        public required IrNode? Operand;
    }

    /// <summary>
    /// Represents a function call in the IR.
    /// </summary>
    public class IrCall : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the function being called.
        /// </summary>
        public required string Name;

        /// <summary>
        /// Gets the list of arguments being passed to the function.
        /// </summary>
        public List<IrNode> Args = [];
    }

    /// <summary>
    /// Abstract base class for block statements in the IR.
    /// </summary>
    public abstract class IrBlock : IrNode
    {
        /// <summary>
        /// Gets the list of statements contained in this block.
        /// </summary>
        public readonly List<IrNode?> Statements = [];
    }

    /// <summary>
    /// Represents an if statement in the IR.
    /// </summary>
    public class IrIf : IrNode
    {
        /// <summary>
        /// Gets or sets the condition expression for the if statement.
        /// </summary>
        public IrNode? Condition;

        /// <summary>
        /// Gets the list of statements in the 'then' block.
        /// </summary>
        public List<IrNode?> ThenBlock = [];

        /// <summary>
        /// Gets or sets the optional list of statements in the 'else' block.
        /// </summary>
        public List<IrNode>? ElseBlock;
    }

    /// <summary>
    /// Represents a while loop in the IR.
    /// </summary>
    public class IrWhile : IrNode
    {
        /// <summary>
        /// Gets or sets the condition expression for the while loop.
        /// </summary>
        public IrNode? Condition;

        /// <summary>
        /// Gets the list of statements in the loop body.
        /// </summary>
        public List<IrNode?> Body = [];
    }

    /// <summary>
    /// Represents a repeat-until loop in the IR.
    /// </summary>
    public class IrRepeat : IrNode
    {
        /// <summary>
        /// Gets the list of statements in the loop body.
        /// </summary>
        public List<IrNode?> Body = [];

        /// <summary>
        /// Gets or sets the condition expression checked after each iteration.
        /// </summary>
        public IrNode? Condition;
    }

    /// <summary>
    /// Represents a for loop in the IR.
    /// </summary>
    public class IrFor : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the loop variable.
        /// </summary>
        public string VarName;

        /// <summary>
        /// Gets or sets the expression specifying the starting value.
        /// </summary>
        public IrNode? From;

        /// <summary>
        /// Gets or sets the expression specifying the ending value.
        /// </summary>
        public IrNode? To;

        /// <summary>
        /// Gets or sets the optional expression specifying the step value.
        /// </summary>
        public IrNode? Step;

        /// <summary>
        /// Gets the list of statements in the loop body.
        /// </summary>
        public List<IrNode?> Body = [];
    }

    /// <summary>
    /// Represents an input statement in the IR.
    /// </summary>
    public class IrInput : IrNode
    {
        /// <summary>
        /// Gets the list of variable names to store input values.
        /// </summary>
        public List<string?> VarNames = [];
    }

    /// <summary>
    /// Represents a goto statement in the IR.
    /// </summary>
    public class IrGoto : IrNode
    {
        /// <summary>
        /// Gets or sets the label to jump to.
        /// </summary>
        public required string Label;
    }

    /// <summary>
    /// Represents a label in the IR.
    /// </summary>
    public class IrLabel : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the label.
        /// </summary>
        public required string Name;
    }

    /// <summary>
    /// Represents a return statement in the IR.
    /// </summary>
    public class IrReturn : IrNode
    {
        /// <summary>
        /// Gets or sets the optional return value expression.
        /// </summary>
        public IrNode? Expr;
    }

    /// <summary>
    /// Represents a function declaration in the IR.
    /// </summary>
    public class IrFunctionDecl : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        public required string Name;

        /// <summary>
        /// Gets the list of parameter names.
        /// </summary>
        public List<string> Args = [];

        /// <summary>
        /// Gets the list of statements in the function body.
        /// </summary>
        public List<IrNode> Body = [];

        /// <summary>
        /// Gets or sets the return type of the function.
        /// </summary>
        public string ReturnType = "any";
    }

    /// <summary>
    /// Represents an array creation operation in the IR.
    /// </summary>
    public class IrNewArray : IrNode
    {
        /// <summary>
        /// Gets the expression specifying the size of the array.
        /// </summary>
        public required IrNode Size { get; init; }

        /// <summary>
        /// Gets the type of elements in the array.
        /// </summary>
        public required string ElementType { get; init; }

        /// <summary>
        /// Returns a string representation of the array creation.
        /// </summary>
        public override string ToString() => $"ARRAY({Size})";
    }

    /// <summary>
    /// Represents a structure initialization in the IR.
    /// </summary>
    public class IrStructInit : IrNode
    {
        /// <summary>
        /// Gets or sets the name of the structure type.
        /// </summary>
        public required string TypeName;

        /// <summary>
        /// Gets the dictionary of field names to their initialization expressions.
        /// </summary>
        public Dictionary<string, IrNode> Fields = new();
    }

    /// <summary>
    /// Represents a field access operation in the IR.
    /// </summary>
    public class IrFieldAccess : IrNode
    {
        /// <summary>
        /// Gets or sets the target object whose field is being accessed.
        /// </summary>
        public required IrNode Target;

        /// <summary>
        /// Gets or sets the name of the field being accessed.
        /// </summary>
        public required string FieldName;
    }

    /// <summary>
    /// Represents an array/indexer access operation in the IR.
    /// </summary>
    public class IrIndex : IrNode
    {
        /// <summary>
        /// Gets or sets the target object being indexed.
        /// </summary>
        public required IrNode Target;

        /// <summary>
        /// Gets or sets the index expression.
        /// </summary>
        public required IrNode Index;
    }

    /// <summary>
    /// Represents an array/indexer store operation in the IR.
    /// </summary>
    public class IrStoreIndex : IrNode
    {
        /// <summary>
        /// Gets or sets the target object being indexed.
        /// </summary>
        public required IrNode Target;

        /// <summary>
        /// Gets or sets the index expression.
        /// </summary>
        public required IrNode Index;

        /// <summary>
        /// Gets or sets the value to store at the specified index.
        /// </summary>
        public required IrNode Value;
    }
}