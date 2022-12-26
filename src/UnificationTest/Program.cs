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

Unification.Unify(new TypeVariable(), new NameType("int32"));

var invokeExpr = (Ast.Expr.Invoke)((Ast.Stmt.Declare)statements[1]).Initializer!;
var funcType = new FunctionType(Array.Empty<IType>(), new NameType("int32")); // TypeOf(invokeExpr.Identifier)
var argsType = Array.Empty<IType>(); // invokeExpr.Args.Select(TypeOf)
var returnType = new TypeVariable();
var callSiteFuncType = new FunctionType(argsType, returnType);
Unification.Unify(funcType, callSiteFuncType);
var actualReturnType = returnType.Substitution;

var a = new FunctionType(new IType[] { new NameType("a"), new NameType("b") }, new NameType("c"));
var b = new FunctionType(new IType[] { new TypeVariable(), new NameType("b") }, new TypeVariable());
Unification.Unify(a, b);

Unification.Unify(new TypeVariable(), new TypeVariable());

var t = new TypeVariable();
Unification.Unify(t, t);
