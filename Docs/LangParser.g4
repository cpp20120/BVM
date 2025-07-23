parser grammar LangParser;

options {
    tokenVocab = LangLexer;
}

program: (line NEWLINE?)+ EOF;

line: 
    statement (NEWLINE | COMMENT)? 
    | COMMENT
    | NEWLINE
;

statement:
    printStmt
    | inputStmt
    | letStmt
    | ifStmt
    | forStmt
    | whileStmt
    | repeatStmt
    | continueStmt
    | exitStmt
;

printStmt: PRINT exprList?;
inputStmt: INPUT ID (COMMA ID)*;
letStmt: LET ID EQ expr;

ifStmt: 
    IF expr THEN NEWLINE? (statement NEWLINE?)+ 
    (ELSE NEWLINE? (statement NEWLINE?)+)? 
    END IF
;

forStmt: 
    FOR ID EQ expr TO expr (STEP expr)? NEWLINE?
    (statement NEWLINE?)+ 
    NEXT ID?
;

whileStmt:
    WHILE expr NEWLINE?
    (statement NEWLINE?)+
    WEND
;

repeatStmt:
    REPEAT NEWLINE?
    (statement NEWLINE?)+
    UNTIL expr
;

continueStmt: CONTINUE;
exitStmt: EXIT;

exprList: expr (COMMA expr)*;

expr:
      LPAREN expr RPAREN                             #parenExpr
    | op=(SUB | NOT) expr                            #unaryExpr
    | left=expr op=(EXP | MUL | DIV | MOD) right=expr #binaryExpr
    | left=expr op=(ADD | SUB) right=expr            #binaryExpr
    | left=expr op=(EQ | NEQ | LT | LTE | GT | GTE) right=expr #comparisonExpr
    | left=expr op=AND right=expr                    #logicalExpr
    | left=expr op=OR right=expr                     #logicalExpr
    | func=(LEN | VAL | ISNAN) LPAREN expr RPAREN    #funcCallExpr
    | ID LBRACKET expr RBRACKET                      #indexExpr
    | ID LPAREN expr RPAREN                          #customFuncCallExpr
    | ARRAY LPAREN expr RPAREN                       #arrayExpr
    | ID                                              #varExpr
    | NUMBER                                          #numExpr
    | STRINGLITERAL                                   #strExpr

;
