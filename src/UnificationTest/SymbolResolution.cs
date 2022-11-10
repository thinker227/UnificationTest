using System.Collections.Generic;
using System.Linq;

namespace UnificationTest;

public sealed class SymbolResolution
{
    private SymbolResolution() { }

    public static IReadOnlyDictionary<string, ISymbol> Resolve(IReadOnlyList<Ast.Stmt> statements)
    {
        Dictionary<string, ISymbol> symbols = new();

        foreach (var decl in statements.OfType<Ast.Stmt.Declare>())
        {
            VariableSymbol symbol = new(decl.Identifier, decl);
            symbols.Add(symbol.Name, symbol);
        }

        // Equivalent to 'func foo(): int32;'.
        symbols.Add("foo", new FunctionSymbol("foo", new NameType("int32")));
        symbols.Add("bar", new FunctionSymbol("bar", new NameType("uint64")));

        return symbols;
    }    
}