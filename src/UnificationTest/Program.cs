using System;
using System.Collections.Generic;
using UnificationTest;

/*
func foo(): int32;

var x;
var y = foo();
x = y;
*/

IReadOnlyList<Ast.Stmt> statements = new Ast.Stmt[]
{
    new Ast.Stmt.Declare(
        "x",
        null),
    
    new Ast.Stmt.Declare(
        "y",
        new Ast.Expr.Invoke(
            "foo", Array.Empty<Ast.Expr>())),
    
    new Ast.Stmt.Assign(
        "x",
        new Ast.Expr.Variable("y"))
};

var symbols = SymbolResolution.Resolve(statements);

var types = Unification.Unify(statements, symbols);

foreach (var (symbol, type) in types)
{
    Console.WriteLine($"'{symbol}' has type '{type}'");
}
