using System.Collections.Generic;

namespace UnificationTest;

public abstract record class Ast
{
    public abstract record class Stmt : Ast
    {
        public sealed record class Declare(
            string Identifier,
            Expr? Initializer) : Stmt;

        public sealed record class Assign(
            string Identifier,
            Expr Expression) : Stmt;
    }

    public abstract record class Expr : Ast
    {
        public sealed record class Invoke(
            string Identifier,
            IReadOnlyList<Expr> Args) : Expr;

        public sealed record class Variable(
            string Identifier) : Expr;
    }
}