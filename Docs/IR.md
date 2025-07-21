## IR (промежуточное представление)
Компилятор MyBasic сначала строит внутреннее промежуточное представление (IR), которое представляет высокоуровневые элементы программы. IR не является исполняемым кодом, но преобразуется в байткод VM.

### Основные узлы IR:

```text
IRNode =
  | Const(value, type)               
  | Var(name, type)                 
  | BinaryOp(op, left, right, type) 
  | UnaryOp(op, operand, type)      
  | Let(name, expr, type)           
  | Call(name, args, type)          
  | Print(expr)                     
  | Input(varName, type)            
  | If(cond, thenBody, elseBody?)   
  | While(cond, body)               
  | For(init, cond, step, body)     
  | Repeat(body, untilExpr)         
  | Block(statements[])             
  | Goto(label)                     
  | Label(name)                     
  | FunctionDecl(name, args, body, returnType)
  | Return(expr?)                   
  | NewArray(size, elementType)      
  | StructInit(typeName, fields)     
  | FieldAccess(obj, fieldName)      
```

IR создаётся на основе грамматики BASIC и затем транслируется в байткод (opcodes).