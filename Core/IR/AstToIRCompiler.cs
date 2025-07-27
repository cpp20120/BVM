using VM.Parser;
using VM.Core.IR.Nodes;

namespace VM.Core.IR
{
    /// <summary>
    /// Compiles an Abstract Syntax Tree (AST) into Intermediate Representation (IR) nodes.
    /// </summary>
    /// <remarks>
    /// Implements the visitor pattern to traverse the AST and generate corresponding IR nodes.
    /// Handles all major language constructs including statements, expressions, and control flow.
    /// </remarks>
    public class AstToIrCompiler : IAstVisitor<List<IrNode>>
    {
        /// <summary>
        /// Visits a ProgramNode and compiles all its statements into IR nodes.
        /// </summary>
        /// <param name="node">The program node to compile.</param>
        /// <returns>A list of IR nodes representing the program.</returns>
        public List<IrNode> Visit(ProgramNode node)
        {
            var list = new List<IrNode>();
            foreach (var stmt in node.Statements)
                list.AddRange(stmt.Accept(this));
            return list;
        }

        /// <summary>
        /// Compiles a print statement into IR nodes.
        /// </summary>
        /// <param name="node">The print statement node to compile.</param>
        /// <returns>A list containing a single IrPrint node.</returns>
        public List<IrNode> Visit(PrintStmt node)
        {
            var expr = node.Expressions.Count > 0
                ? CompileExpr(node.Expressions[0])
                : new IrConst { Value = "", Type = "string" };
            return [new IrPrint { Expr = expr, Line = node.Line }];
        }

        /// <summary>
        /// Compiles a variable declaration (let statement) into IR nodes.
        /// </summary>
        /// <param name="node">The let statement node to compile.</param>
        /// <returns>A list containing a single IrLet node.</returns>
        public List<IrNode> Visit(LetStmt node) =>
        [
            new IrLet
            {
                Name = node.Id!,
                Expr = CompileExpr(node.Expression!),
                Line = node.Line
            }
        ];

        /// <summary>
        /// Compiles an input statement into IR nodes.
        /// </summary>
        /// <param name="node">The input statement node to compile.</param>
        /// <returns>A list containing a single IrInput node.</returns>
        public List<IrNode> Visit(InputStmt node) =>
        [
            new IrInput
            {
                VarNames = [..node.Ids],
                Line = node.Line
            }
        ];

        /// <summary>
        /// Compiles an if statement into IR nodes.
        /// </summary>
        /// <param name="node">The if statement node to compile.</param>
        /// <returns>A list containing a single IrIf node.</returns>
        public List<IrNode> Visit(IfStmt node)
        {
            var irIf = new IrIf
            {
                Condition = CompileExpr(node.Condition),
                ThenBlock = CompileBlock(node.ThenBranch),
                ElseBlock = node.ElseBranch.Count > 0 ? CompileBlock(node.ElseBranch) : null,
                Line = node.Line
            };
            return [irIf];
        }

        /// <summary>
        /// Compiles a while loop into IR nodes.
        /// </summary>
        /// <param name="node">The while statement node to compile.</param>
        /// <returns>A list containing a single IrWhile node.</returns>
        public List<IrNode> Visit(WhileStmt node) =>
        [
            new IrWhile
            {
                Condition = CompileExpr(node.Condition!),
                Body = CompileBlock(node.Body),
                Line = node.Line
            }
        ];

        /// <summary>
        /// Compiles a repeat-until loop into IR nodes.
        /// </summary>
        /// <param name="node">The repeat statement node to compile.</param>
        /// <returns>A list containing a single IrRepeat node.</returns>
        public List<IrNode> Visit(RepeatStmt node)
        {
            return
            [
                new IrRepeat
                {
                    Body = CompileBlock(node.Body),
                    Condition = CompileExpr(node.Condition!),
                    Line = node.Line
                }
            ];
        }

        /// <summary>
        /// Compiles a for loop into IR nodes.
        /// </summary>
        /// <param name="node">The for statement node to compile.</param>
        /// <returns>A list containing a single IrFor node.</returns>
        public List<IrNode> Visit(ForStmt node) =>
        [
            new IrFor
            {
                VarName = node.Variable,
                From = CompileExpr(node.From),
                To = CompileExpr(node.To),
                Step = node.Step != null ? CompileExpr(node.Step) : null,
                Body = CompileBlock(node.Body),
                Line = node.Line
            }
        ];

        /// <summary>
        /// Compiles a continue statement into IR nodes.
        /// </summary>
        /// <param name="node">The continue statement node to compile.</param>
        /// <returns>A list containing a single IrGoto node with "__continue__" label.</returns>
        public List<IrNode> Visit(ContinueStmt node) => [new IrGoto { Label = "__continue__", Line = node.Line }];

        /// <summary>
        /// Compiles a break statement into IR nodes.
        /// </summary>
        /// <param name="node">The break statement node to compile.</param>
        /// <returns>A list containing a single IrGoto node with "__break__" label.</returns>
        public List<IrNode> Visit(ExitStmt node) => [new IrGoto { Label = "__break__", Line = node.Line }];

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

        /// <summary>
        /// Compiles an array index assignment into IR nodes.
        /// </summary>
        /// <param name="node">The index assignment node to compile.</param>
        /// <returns>A list containing a single IrStoreIndex node.</returns>
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

        /// <summary>
        /// Compiles a new array expression into IR nodes.
        /// </summary>
        /// <param name="node">The new array expression node to compile.</param>
        /// <returns>A list containing a single IrNewArray node.</returns>
        public List<IrNode> Visit(NewArrayExpr node)
        {
            return
            [
                new IrNewArray()
                {
                    Size = CompileExpr(node.Size),
                    ElementType = "any",
                    Line = node.Line
                }
            ];
        }

        /// <summary>
        /// Visits an expression node by delegating to its specific Accept method.
        /// </summary>
        /// <param name="node">The expression node to visit.</param>
        /// <returns>The result of visiting the specific expression type.</returns>
        public List<IrNode> Visit(ExprNode node)
        {
            return node.Accept(this);
        }

        /// <summary>
        /// Throws NotSupportedException as IndexExpr should be handled by CompileExpr.
        /// </summary>
        public List<IrNode> Visit(IndexExpr node) =>
            throw new NotSupportedException("IndexExpr должен обрабатываться через CompileExpr");

        /// <summary>
        /// Compiles a block of statements into a list of IR nodes.
        /// </summary>
        /// <param name="stmts">The list of statements to compile.</param>
        /// <returns>A list of IR nodes representing the block.</returns>
        private List<IrNode> CompileBlock(List<StatementNode> stmts)
        {
            var list = new List<IrNode>();
            foreach (var stmt in stmts)
                list.AddRange(stmt.Accept(this));
            return list;
        }

        /// <summary>
        /// Compiles an expression node into an IR node.
        /// </summary>
        /// <param name="expr">The expression node to compile.</param>
        /// <returns>An IR node representing the expression.</returns>
        /// <exception cref="Exception">Thrown when encountering an unknown expression type.</exception>
        private static IrNode CompileExpr(ExprNode expr)
        {
            return expr switch
            {
                NumberExpr n => new IrConst { Value = n.Value, Type = "number", Line = n.Line },
                StringExpr s => new IrConst { Value = s.Value, Type = "string", Line = s.Line },
                VarExpr v => new IrVar { Name = v.Name, Line = v.Line },
                BinaryExpr b => new IrBinary
                {
                    Op = TokenUtils.TokenToString(b.Operator),
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