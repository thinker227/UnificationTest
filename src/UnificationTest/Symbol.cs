using System;

namespace UnificationTest;

public interface ISymbol
{
    string Name { get; }
}

public sealed record class VariableSymbol(
    string Name,
    Ast.Stmt.Declare Declaration) : ISymbol
{
    public override string ToString() =>
        $"var {Name}";
}

public sealed record class FunctionSymbol(
    string Name,
    IType Type) : ISymbol
{
    public override string ToString() =>
        $"func {Name}(): {Type}";
}
