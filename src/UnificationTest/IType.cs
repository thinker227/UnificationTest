namespace UnificationTest;

public interface IType
{
    string Name { get; }
}

public sealed record NameType(
    string Name) : IType
{
    public override string ToString() =>
        Name;
}

public sealed record UnresolvableType : IType
{
    public string Name => "<unresolvable>";

    public override string ToString() =>
        Name;

    public bool Equals(UnresolvableType? other) =>
        false;

    public override int GetHashCode() =>
        base.GetHashCode();
}
