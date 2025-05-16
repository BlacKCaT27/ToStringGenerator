namespace Bcss.ToStringGenerator.Generators;

internal readonly record struct ClassSymbolData
{
    public readonly string ContainingNamespace;

    public readonly string ClassAccessibility;

    public readonly string ClassName;

    public readonly EquatableArray<MemberSymbolData> Members;

    public ClassSymbolData(
        string containingNamespace,
        string classAccessibility,
        string className,
        List<MemberSymbolData> members)
    {
        ContainingNamespace = containingNamespace;
        ClassAccessibility = classAccessibility;
        ClassName = className;
        Members = new EquatableArray<MemberSymbolData>(members.ToArray());
    }
}