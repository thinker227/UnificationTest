using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnificationTest;

public sealed class Unification
{
    private readonly IReadOnlyDictionary<string, ISymbol> symbols;
    private readonly IReadOnlyDictionary<ISymbol, TypeVariable> vars;
    private readonly IReadOnlyList<Ast.Stmt> statements;
    private readonly List<Constraint> constraints;
    private readonly Dictionary<TypeVariable, IType> substitutions;



    private Unification(
        IReadOnlyDictionary<string, ISymbol> symbols,
        IReadOnlyDictionary<ISymbol, TypeVariable> vars,
        IReadOnlyList<Ast.Stmt> statements)
    {
        this.symbols = symbols;
        this.vars = vars;
        this.statements = statements;
        constraints = new();
        substitutions = new();
    }



    public static IReadOnlyDictionary<ISymbol, IType> Unify(
        IReadOnlyList<Ast.Stmt> statements,
        IReadOnlyDictionary<string, ISymbol> symbols)
    {
        var vars = GetTypeVariables(symbols.Values);

        Unification unifier = new(symbols, vars, statements);
        unifier.Unify();

        return unifier.vars
            .ToDictionary(
                var => var.Key,
                var => unifier.substitutions.TryGetValue(var.Value, out var sub) ? sub : new UnresolvableType());
    }

    private static IReadOnlyDictionary<ISymbol, TypeVariable> GetTypeVariables(
        IEnumerable<ISymbol> symbols)
    {
        Dictionary<ISymbol, TypeVariable> vars = new();
        int index = 0;

        foreach (var symbol in symbols)
        {
            vars.TryAdd(symbol, new()
            {
                Index = index++
            });
        }

        return vars;
    }

    private void Unify()
    {
        AddConstraints();

        // Function symbols already have their type known.
        foreach (var func in symbols.Values.OfType<FunctionSymbol>())
        {
            var funcTypeVar = vars[func];
            substitutions.Add(funcTypeVar, func.Type);
        }

        resolveFromStart:
        if (constraints.Count > 0)
        {
            foreach (var constraint in constraints)
            {
                var result = TryResolveConstraint(constraint);
                if (result is not null)
                {
                    // Add the new substitutions
                    var resultSubs = result?.Substitutions
                        ?? Array.Empty<Substitution>();
                    foreach (var resSub in resultSubs)
                    {
                        substitutions.Add(resSub.Variable, resSub.Type);
                    }

                    // Add the new constraints
                    var resultConsts = result?.Constraints
                        ?? Array.Empty<Constraint>();
                    constraints.AddRange(resultConsts);

                    constraints.Remove(constraint);
                    
                    // Go back and try resolve previous constraints again.
                    goto resolveFromStart;
                }
            }

            // Remaining constraints could not be resolved.
        }
    }

    // Adds constraints based on the symbols and statements.
    private void AddConstraints()
    {
        var decls = statements.OfType<Ast.Stmt.Declare>()
            .Where(decl => decl.Initializer is not null)
            .Select(decl => (
                symbols[decl.Identifier],
                decl.Initializer!));

        var assigns = statements.OfType<Ast.Stmt.Assign>()
            .Select(assign => (
                symbols[assign.Identifier],
                assign.Expression));

        var exprs = decls.Concat(assigns);

        foreach (var (symbol, expr) in exprs)
        {
            string otherIdentifier = expr switch
            {
                Ast.Expr.Invoke invocation => invocation.Identifier,
                Ast.Expr.Variable variable => variable.Identifier,
                _ => throw new InvalidOperationException()
            };

            var targetVar = vars[symbol];
            var otherVar = vars[symbols[otherIdentifier]];
            Constraint.Equatable constraint = new(targetVar, otherVar);
            constraints.Add(constraint);
        }
    }

    private ConstraintResolutionResult? TryResolveConstraint(Constraint constraint) => constraint switch
    {
        Constraint.Equatable equatable => TryResolveEquatableConstraint(equatable),
        _ => throw new InvalidOperationException("Unknown constraint."),
    };

    private ConstraintResolutionResult? TryResolveEquatableConstraint(Constraint.Equatable constraint)
    {
        if (!substitutions.TryGetValue(constraint.Other, out var otherType))
        {
            return null;
        }
        
        if (
            substitutions.TryGetValue(constraint.Target, out var targetType) &&
            !otherType.Equals(targetType))
        {
            throw new InvalidOperationException($"Invalid constraint '{targetType} = {otherType}'.");
        }

        Substitution substitution = new(constraint.Target, otherType);
        return new ConstraintResolutionResult(
            new Substitution[] { substitution },
            null);
    }



    private sealed record class TypeVariable
    {
        public required int Index { get; init; }

        [DebuggerStepThrough]
        public bool Equals(TypeVariable? other) =>
            Index == other?.Index;
        [DebuggerStepThrough]
        public override int GetHashCode() =>
            Index;

        public override string? ToString() => $"?T{Index}";
    }

    private abstract record class Constraint
    {
        public sealed record class Equatable(
            TypeVariable Target,
            TypeVariable Other) : Constraint
        {
            [DebuggerStepThrough]
            public override string ToString() =>
                $"{Target} = {Other}";
        }
    }

    private readonly record struct Substitution(
        TypeVariable Variable,
        IType Type);

    private readonly record struct ConstraintResolutionResult(
        IEnumerable<Substitution>? Substitutions,
        IEnumerable<Constraint>? Constraints);
}
