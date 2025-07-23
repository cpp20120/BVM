using VM.Parser;
using VM.Core.IR.Nodes;

namespace VM.Core.IR
{
    public class AstToIrCompiler : IAstVisitor<List<IRNode>>
    {
        public List<IRNode> Visit(ProgramNode node)
        {
            var list = new List<IRNode>();
            foreach (var stmt in node.Statements)
                list.AddRange(stmt.Accept(this));
            return list;
        }

        public List<IRNode> Visit(PrintStmt node)
        {
            var expr = node.Expressions.Count > 0 ? CompileExpr(node.Expressions[0]) : new IRConst { Value = "", Type = "string" };
            return [ new IRPrint { Expr = expr, Line = node.Line } ];
        }

        public List<IRNode> Visit(LetStmt node)
        {
            return [ new IRLet
            {
                Name = node.Id!,
                Expr = CompileExpr(node.Expression!),
                Line = node.Line
            } ];
        }

        public List<IRNode> Visit(InputStmt node)
        {
            return [ new IRInput
            {
                VarNames = [..node.Ids],
                Line = node.Line
            } ];
        }

        public List<IRNode> Visit(IfStmt node)
        {
            var irIf = new IRIf
            {
                Condition = CompileExpr(node.Condition),
                ThenBlock = CompileBlock(node.ThenBranch),
                ElseBlock = node.ElseBranch.Count > 0 ? CompileBlock(node.ElseBranch) : null,
                Line = node.Line
            };
            return [ irIf ];
        }

        public List<IRNode> Visit(WhileStmt node)
        {
            return [ new IRWhile
            {
                Condition = CompileExpr(node.Condition!),
                Body = CompileBlock(node.Body),
                Line = node.Line
            } ];
        }

        public List<IRNode> Visit(RepeatStmt node)
        {
            return [ new IRRepeat
            {
                Body = CompileBlock(node.Body),
                Condition = CompileExpr(node.Condition!),
                Line = node.Line
            } ];
        }

        public List<IRNode> Visit(ForStmt node)
        {
            return [ new IRFor
            {
                VarName = node.Variable,
                From = CompileExpr(node.From),
                To = CompileExpr(node.To),
                Step = node.Step != null ? CompileExpr(node.Step) : null,
                Body = CompileBlock(node.Body),
                Line = node.Line
            } ];
        }

        public List<IRNode> Visit(ContinueStmt node)
        {
            return [ new IRGoto { Label = "__continue__", Line = node.Line } ];
        }

        public List<IRNode> Visit(ExitStmt node)
        {
            return [ new IRGoto { Label = "__break__", Line = node.Line } ];
        }

        public List<IRNode> Visit(BinaryExpr node)
        {
            throw new NotSupportedException("BinaryExpr должен обрабатываться как часть CompileExpr");
        }

        public List<IRNode> Visit(UnaryExpr node) => throw new NotSupportedException();
        public List<IRNode> Visit(NumberExpr node) => throw new NotSupportedException();
        public List<IRNode> Visit(StringExpr node) => throw new NotSupportedException();
        public List<IRNode> Visit(VarExpr node) => throw new NotSupportedException();
        public List<IRNode> Visit(FuncCallExpr node) => throw new NotSupportedException();

        private List<IRNode> CompileBlock(List<StatementNode> stmts)
        {
            var list = new List<IRNode>();
            foreach (var stmt in stmts)
                list.AddRange(stmt.Accept(this));
            return list;
        }

        private IRNode CompileExpr(ExprNode expr)
        {
            return expr switch
            {
                NumberExpr n => new IRConst { Value = n.Value, Type = "number", Line = n.Line },
                StringExpr s => new IRConst { Value = s.Value, Type = "string", Line = s.Line },
                VarExpr v => new IRVar { Name = v.Name, Line = v.Line },
                BinaryExpr b => new IRBinary
                {
                    Op = b.Operator.ToString(),
                    Left = CompileExpr(b.Left!),
                    Right = CompileExpr(b.Right!),
                    Line = b.Line
                },
                UnaryExpr u => new IRUnary
                {
                    Op = u.Operator.ToString(),
                    Operand = CompileExpr(u.Operand!),
                    Line = u.Line
                },
                FuncCallExpr f => new IRCall
                {
                    Name = f.Func.ToString().ToLower(),
                    Args = f.Arguments.ConvertAll(CompileExpr),
                    Line = f.Line
                },
                _ => throw new Exception($"Неизвестный тип выражения: {expr.GetType().Name}")
            };
        }
    }
}
