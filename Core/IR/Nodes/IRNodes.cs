namespace VM.Core.IR.Nodes
{
    public abstract class IrNode
    {
        public int Line { get; init; }
    }

    public class IrConst : IrNode
    {
        public object Value;
        public string? Type; // "int", "float", "string", "bool"
    }

    public class IrVar : IrNode
    {
        public string? Name;
    }

    public class IrLet : IrNode
    {
        public string? Name;
        public required IrNode? Expr;
    }

    public class IrPrint : IrNode
    {
        public required IrNode? Expr;
    }

    public class IrBinary : IrNode
    {
        public required string Op;
        public required IrNode? Left;
        public required IrNode? Right;
    }

    public class IrUnary : IrNode
    {
        public required string Op;
        public required IrNode? Operand;
    }

    public class IrCall : IrNode
    {
        public required string Name;
        public List<IrNode> Args = [];
    }

    public class IrBlock : IrNode
    {
        public readonly List<IrNode?> Statements = [];
    }

    public class IrIf : IrNode
    {
        public IrNode? Condition;
        public List<IrNode?> ThenBlock = [];
        public List<IrNode>? ElseBlock;
    }

    public class IrWhile : IrNode
    {
        public IrNode? Condition;
        public List<IrNode?> Body = [];
    }

    public class IrRepeat : IrNode
    {
        public List<IrNode?> Body = [];
        public IrNode? Condition;
    }

    public class IrFor : IrNode
    {
        public string VarName;
        public IrNode? From;
        public IrNode? To;
        public IrNode? Step;
        public List<IrNode?> Body = [];
    }

    public class IrInput : IrNode
    {
        public List<string?> VarNames = [];
    }

    public class IrGoto : IrNode
    {
        public required string Label;
    }

    public class IrLabel : IrNode
    {
        public required string Name;
    }

    public class IrReturn : IrNode
    {
        public IrNode? Expr;
    }

    public class IrFunctionDecl : IrNode
    {
        public required string Name;
        public List<string> Args = [];
        public List<IrNode> Body = [];
        public string ReturnType = "any";
    }

    public class IrNewArray : IrNode
    {
        public required IrNode Size { get; init; }
        public required string ElementType { get; init; }
        public override string ToString() => $"ARRAY({Size})";
    }

    public class IrStructInit : IrNode
    {
        public required string TypeName;
        public Dictionary<string, IrNode> Fields = new();
    }

    public class IrFieldAccess : IrNode
    {
        public required IrNode Target;
        public required string FieldName;
    }

    public class IrIndex : IrNode
    {
        public required IrNode Target;
        public required IrNode Index;
    }

    public class IrStoreIndex : IrNode
    {
        public required IrNode Target;
        public required IrNode Index;
        public required IrNode Value;
    }
}