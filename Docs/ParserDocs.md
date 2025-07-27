## Basic language parser

Handwritten top down recursive descent parser (LL1) with operator precedence handling and typed AST generation targeting a virtual machine IR.

- **Input:** list of tokens from tokenizer
- **Output:** root node 'ProgramNode', with AST
- **Errors:** drops `ParseException` with token position

Expressions parses by Precedence Climbing (smth as Pratt-parser)

```mermaid
graph TD
    Start[Start of parsing] --> Program[Program]
    Program --> StatementList[StatementList]
    StatementList -->|Repeats| Statement[Statement]
    
    Statement --> Let[LetStatement]
    Statement --> Print[PrintStatement]
    Statement --> If[IfStatement]
    Statement --> While[WhileStatement]
    Statement --> For[ForStatement]
    Statement --> Repeat[RepeatStatement]
    Statement --> Input[InputStatement]
    Statement --> ExpressionStmt[ExpressionStatement]
    
    Let --> Expr1[Expression]
    Print --> Expr2[Expression]
    Input --> Var[VariableName]

    ExpressionStatement --> Expr3[Expression]

    If --> Cond1[Expression]
    If --> ThenBlock[Block]
    If --> ElseBlock[Block?]

    While --> Cond2[Expression]
    While --> Body1[Block]

    For --> Init[LetStatement]
    For --> Cond3[Expression]
    For --> Step[LetStatement?]
    For --> Body2[Block]

    Repeat --> Body3[Block]
    Repeat --> UntilCond[Expression]
    
    Expression --> BinaryExpr[BinaryExpression]
    Expression --> UnaryExpr[UnaryExpression]
    Expression --> FunctionCall[FunctionCall]
    Expression --> Literal[Literal or Variable]

    Block --> StatementList

    classDef stmt  stroke:#333,stroke-width:1px;
    class Statement,Let,Print,If,While,For,Repeat,Input,ExpressionStmt stmt;
```
