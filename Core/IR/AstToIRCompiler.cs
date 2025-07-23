using VM.Parser;
using VM.Core.IR.Nodes;

namespace VM.Core.IR
{
    public class AstToIrCompiler : IAstVisitor<List<IrNode>>
    {
        public List<IrNode> Visit(ProgramNode node)
        {
            var list = new List<IrNode>();
            foreach (var stmt in node.Statements)
                list.AddRange(stmt.Accept(this));
            return list;
        }

        public List<IrNode> Visit(PrintStmt node)
        {
            var expr = node.Expressions.Count > 0 ? CompileExpr(node.Expressions[0]) : new IrConst { Value = "", Type = "string" };
            return [ new IrPrint { Expr = expr, Line = node.Line } ];
        }

        public List<IrNode> Visit(LetStmt node) =>
        [ new IrLet
        {
            Name = node.Id!,
            Expr = CompileExpr(node.Expression!),
            Line = node.Line
        } ];

        public List<IrNode> Visit(InputStmt node) =>
        [ new IrInput
        {
            VarNames = [..node.Ids],
            Line = node.Line
        } ];

        public List<IrNode> Visit(IfStmt node)
        {
            var irIf = new IrIf
            {
                Condition = CompileExpr(node.Condition),
                ThenBlock = CompileBlock(node.ThenBranch),
                ElseBlock = node.ElseBranch.Count > 0 ? CompileBlock(node.ElseBranch) : null,
                Line = node.Line
            };
            return [ irIf ];
        }

        public List<IrNode> Visit(WhileStmt node) =>
        [ new IrWhile
        {
            Condition = CompileExpr(node.Condition!),
            Body = CompileBlock(node.Body),
            Line = node.Line
        } ];

        public List<IrNode> Visit(RepeatStmt node)
        {
            return [ new IrRepeat
            {
                Body = CompileBlock(node.Body),
                Condition = CompileExpr(node.Condition!),
                Line = node.Line
            } ];
        }

        public List<IrNode> Visit(ForStmt node) =>
        [ new IrFor
        {
            VarName = node.Variable,
            From = CompileExpr(node.From),
            To = CompileExpr(node.To),
            Step = node.Step != null ? CompileExpr(node.Step) : null,
            Body = CompileBlock(node.Body),
            Line = node.Line
        } ];

        public List<IrNode> Visit(ContinueStmt node) => [ new IrGoto { Label = "__continue__", Line = node.Line } ];

        public List<IrNode> Visit(ExitStmt node) => [ new IrGoto { Label = "__break__", Line = node.Line } ];

        public List<IrNode> Visit(BinaryExpr node)
        {
            throw new NotSupportedException("BinaryExpr должен обрабатываться как часть CompileExpr");
        }

        public List<IrNode> Visit(UnaryExpr node) => throw new NotSupportedException();
        public List<IrNode> Visit(NumberExpr node) => throw new NotSupportedException();
        public List<IrNode> Visit(StringExpr node) => throw new NotSupportedException();
        public List<IrNode> Visit(VarExpr node) => throw new NotSupportedException();
        public List<IrNode> Visit(FuncCallExpr node) => throw new NotSupportedException();
        public List<IrNode> Visit(CustomCallExpr node) => throw new NotSupportedException();
        
        public List<IrNode> Visit(AssignIndexStmt node) =>
        [
            new IrStoreIndex
            {
                Target = new IrVar { Name = node.Target, Line = node.Line },
                Index = CompileExpr(node.Index),
                Value = CompileExpr(node.Value),
                Line = node.Line
            }
        ];

        public List<IrNode> Visit(NewArrayExpr node)
        {
            return [new IrNewArray() {
                Size = CompileExpr(node.Size),
                ElementType = "any",
                Line = node.Line
            }];
        }
        public List<IrNode> Visit(ExprNode node)
        {
            return node.Accept(this);
        }
        
        public List<IrNode> Visit(IndexExpr node) => throw new NotSupportedException("IndexExpr должен обрабатываться через CompileExpr");

        private List<IrNode> CompileBlock(List<StatementNode> stmts)
        {
            var list = new List<IrNode>();
            foreach (var stmt in stmts)
                list.AddRange(stmt.Accept(this));
            return list;
        }

        private static IrNode CompileExpr(ExprNode expr)
        {
            return expr switch
            {
                NumberExpr n => new IrConst { Value = n.Value, Type = "number", Line = n.Line },
                StringExpr s => new IrConst { Value = s.Value, Type = "string", Line = s.Line },
                VarExpr v => new IrVar { Name = v.Name, Line = v.Line },
                BinaryExpr b => new IrBinary
                {
                    Op = b.Operator.ToString(),
                    Left = CompileExpr(b.Left!),
                    Right = CompileExpr(b.Right!),
                    Line = b.Line
                },
                UnaryExpr u => new IrUnary
                {
                    Op = u.Operator.ToString(),
                    Operand = CompileExpr(u.Operand!),
                    Line = u.Line
                },
                FuncCallExpr f => new IrCall
                {
                    Name = f.Func.ToString().ToLower(),
                    Args = f.Arguments.ConvertAll(CompileExpr),
                    Line = f.Line
                },
                IndexExpr i => new IrIndex
                {
                    Target = CompileExpr(i.Target),
                    Index = CompileExpr(i.Index),
                    Line = i.Line
                },
                CustomCallExpr c => new IrCall
                {
                    Name = c.Name.ToLower(),
                    Args = c.Args.ConvertAll(CompileExpr),
                    Line = c.Line
                },
                NewArrayExpr a => new IrNewArray 
                {
                    Size = CompileExpr(a.Size),
                    ElementType = "any",
                    Line = a.Line
                },

                _ => throw new Exception($"Неизвестный тип выражения: {expr.GetType().Name}")
            };
        }
    }
}
