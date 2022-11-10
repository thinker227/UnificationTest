namespace UnificationTest.Tests;

public class UnificationTests
{
    [Fact]
    public void UnifiesVariableChain()
    {
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

        types[symbols["x"]].ShouldBe(new NameType("int32"));
        types[symbols["y"]].ShouldBe(new NameType("int32"));
        types[symbols["foo"]].ShouldBe(new NameType("int32"));
    }

    [Fact]
    public void UnifiesDirectFunctionCall()
    {
        /*
        func foo(): int32;

        var x = foo();
        */

        IReadOnlyList<Ast.Stmt> statements = new Ast.Stmt[]
        {
            new Ast.Stmt.Declare(
                "x",
                new Ast.Expr.Invoke(
                    "foo", Array.Empty<Ast.Expr>())),
        };
        
        var symbols = SymbolResolution.Resolve(statements);

        var types = Unification.Unify(statements, symbols);

        types[symbols["x"]].ShouldBe(new NameType("int32"));
    }

    [Fact]
    public void StandaloneVariableShouldBeUnresolvable()
    {
        /*
        var x;
        */

        IReadOnlyList<Ast.Stmt> statements = new Ast.Stmt[]
        {
            new Ast.Stmt.Declare(
                "x",
                null),
        };
        
        var symbols = SymbolResolution.Resolve(statements);

        var types = Unification.Unify(statements, symbols);

        types[symbols["x"]].ShouldBeOfType<UnresolvableType>();
    }

    [Fact]
    public void AssignmentToUnresolvableShouldBeUnresolvable()
    {
        /*
        var x;
        var y = x;
        */

        IReadOnlyList<Ast.Stmt> statements = new Ast.Stmt[]
        {
            new Ast.Stmt.Declare(
                "x",
                null),

            new Ast.Stmt.Declare(
                "y",
                new Ast.Expr.Variable(
                    "x"))
        };
        
        var symbols = SymbolResolution.Resolve(statements);

        var types = Unification.Unify(statements, symbols);

        types[symbols["x"]].ShouldBeOfType<UnresolvableType>();
        types[symbols["y"]].ShouldBeOfType<UnresolvableType>();
    }

    [Fact]
    public void AssignmentToSelfShouldBeUnresolvable()
    {
        /*
        var x = x;
        */

        IReadOnlyList<Ast.Stmt> statements = new Ast.Stmt[]
        {
            new Ast.Stmt.Declare(
                "x",
                new Ast.Expr.Variable(
                    "x")),
        };
        
        var symbols = SymbolResolution.Resolve(statements);

        var types = Unification.Unify(statements, symbols);

        types[symbols["x"]].ShouldBeOfType<UnresolvableType>();
    }

    [Fact]
    public void InvalidAssignmentShouldThrow()
    {
        /*
        var x = foo();
        var y = bar();
        x = y;
        */

        IReadOnlyList<Ast.Stmt> statements = new Ast.Stmt[]
        {
            new Ast.Stmt.Declare(
                "x",
                new Ast.Expr.Invoke(
                    "foo", Array.Empty<Ast.Expr>())),
            
            new Ast.Stmt.Declare(
                "y",
                new Ast.Expr.Invoke(
                    "bar", Array.Empty<Ast.Expr>())),

            new Ast.Stmt.Assign(
                "x",
                new Ast.Expr.Variable(
                    "y")),
        };
        
        var symbols = SymbolResolution.Resolve(statements);

        Should.Throw<InvalidOperationException>(
            () => Unification.Unify(statements, symbols));
    }
}
