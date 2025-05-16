namespace Bcss.ToStringGenerator.Generators;

internal readonly record struct MemberSymbolData(
    string MemberName,
    bool IsDictionary,
    bool IsEnumerable,
    bool IsNullableType,
    bool IsSensitive,
    string? Mask = null)
{
    public readonly string MemberName = MemberName;

    public readonly bool IsDictionary = IsDictionary;

    public readonly bool IsEnumerable = IsEnumerable;

    public readonly bool IsNullableType = IsNullableType;
    
    public readonly bool IsSensitive = IsSensitive;
    
    public readonly string? Mask = Mask;
}