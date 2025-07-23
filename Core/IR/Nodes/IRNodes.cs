namespace VM.Core.IR.Nodes
{
    public abstract class IRNode
    {
        public int Line { get; init; }
    }

    public class IRConst : IRNode
    {
        public object Value;
        public string Type; // "int", "float", "string", "bool"
    }

    public class IRVar : IRNode
    {
        public string Name;
    }

    public class IRLet : IRNode
    {
        public string Name;
        public IRNode Expr;
    }

    public class IRPrint : IRNode
    {
        public IRNode Expr;
    }

    public class IRBinary : IRNode
    {
        public string Op;
        public IRNode Left;
        public IRNode Right;
    }

    public class IRUnary : IRNode
    {
        public string Op;
        public IRNode Operand;
    }

    public class IRCall : IRNode
    {
        public string Name;
        public List<IRNode> Args = [];
    }

    public class IRBlock : IRNode
    {
        public List<IRNode> Statements = [];
    }

    public class IRIf : IRNode
    {
        public IRNode Condition;
        public List<IRNode> ThenBlock = [];
        public List<IRNode>? ElseBlock;
    }

    public class IRWhile : IRNode
    {
        public IRNode Condition;
        public List<IRNode> Body = [];
    }

    public class IRRepeat : IRNode
    {
        public List<IRNode> Body = [];
        public IRNode Condition;
    }

    public class IRFor : IRNode
    {
        public string VarName;
        public IRNode From;
        public IRNode To;
        public IRNode? Step;
        public List<IRNode> Body = [];
    }

    public class IRInput : IRNode
    {
        public List<string> VarNames = [];
    }

    public class IRGoto : IRNode
    {
        public string Label;
    }

    public class IRLabel : IRNode
    {
        public string Name;
    }

    public class IRReturn : IRNode
    {
        public IRNode? Expr;
    }

    public class IRFunctionDecl : IRNode
    {
        public string Name;
        public List<string> Args = [];
        public List<IRNode> Body = [];
        public string ReturnType = "any";
    }

    public class IRNewArray : IRNode
    {
        public IRNode Size;
        public string ElementType;
    }

    public class IRStructInit : IRNode
    {
        public string TypeName;
        public Dictionary<string, IRNode> Fields = new();
    }

    public class IRFieldAccess : IRNode
    {
        public IRNode Target;
        public string FieldName;
    }
}
