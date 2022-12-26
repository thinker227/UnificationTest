using System;
using System.Collections.Generic;
using System.Linq;

namespace UnificationTest;

public interface IType
{
}

public sealed record NameType(
    string Name) : IType
{
    public override string ToString() =>
        Name;
}

public sealed record FunctionType(
    IReadOnlyList<IType> Params,
    IType Return) : IType
{
    public override string ToString() =>
        $"({string.Join(", ", Params)}) -> {Return}";

    public bool Equals(FunctionType? other)
    {
        if (other is null) return false;

        if (Params.Count != other.Params.Count) return false;
        foreach (var (a, b) in Params.Zip(other.Params))
        {
            if (!a.Equals(b)) return false;
        }

        return Return.Equals(other.Return);
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new();

        foreach (var param in Params)
        {
            hashCode.Add(param);
        }

        hashCode.Add(Return);

        return hashCode.ToHashCode();
    }
}

public sealed record TypeVariable : IType
{
    private static int currentIndex = 0;
    private readonly int index = currentIndex++;
    private IType? substitution;

    public IType Substitution
    {
        get
        {
            if (substitution is null) return this;
            if (substitution is TypeVariable var) substitution = var.Substitution;
            return substitution;
        }
    }

    public void Substitute(IType substitution)
    {
        if (substitution is null) throw new ArgumentNullException(nameof(substitution));
        this.substitution = substitution;
    }

    public override string? ToString() => substitution switch
    {
        null => $"?T{index}",
        _ => substitution.ToString()
    };

    public bool Equals(TypeVariable? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other);
    }

    public override int GetHashCode() =>
        base.GetHashCode();
}

public sealed record UnresolvableType : IType
{
    public override string ToString() =>
        "<unresolvable>";

    public bool Equals(UnresolvableType? other) =>
        false;

    public override int GetHashCode() =>
        base.GetHashCode();
}
