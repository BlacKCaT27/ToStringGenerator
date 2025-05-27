namespace Bcss.ToStringGenerator;

/// <summary>
/// A struct containing information needed to properly generate the contents of a ToString() method for a class member.
/// </summary>
internal readonly struct MemberSymbolData : IEquatable<MemberSymbolData>
{
    public string MemberName { get; }
    public string MemberAccessibility { get; }
    public bool IsDictionary { get; }
    public bool IsEnumerable { get; }
    public bool IsNullableType { get; }
    public bool IsSensitive { get; }
    public bool IsStatic { get; }
    public string? Mask { get; }

    public MemberSymbolData(
        string memberName,
        string memberAccessibility,
        bool isDictionary,
        bool isEnumerable,
        bool isNullableType,
        bool isSensitive,
        bool isStatic,
        string? mask)
    {
        MemberName = memberName;
        MemberAccessibility = memberAccessibility;
        IsDictionary = isDictionary;
        IsEnumerable = isEnumerable;
        IsNullableType = isNullableType;
        IsSensitive = isSensitive;
        IsStatic = isStatic;
        Mask = mask;
    }

    public bool Equals(MemberSymbolData other)
    {
        return MemberName == other.MemberName &&
               MemberAccessibility == other.MemberAccessibility &&
               IsDictionary == other.IsDictionary &&
               IsEnumerable == other.IsEnumerable &&
               IsNullableType == other.IsNullableType &&
               IsSensitive == other.IsSensitive &&
               IsStatic == other.IsStatic &&
               Mask == other.Mask;
    }

    public override bool Equals(object? obj)
    {
        return obj is MemberSymbolData other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + (MemberName?.GetHashCode() ?? 0);
            hash = hash * 23 + (MemberAccessibility?.GetHashCode() ?? 0);
            hash = hash * 23 + IsDictionary.GetHashCode();
            hash = hash * 23 + IsEnumerable.GetHashCode();
            hash = hash * 23 + IsNullableType.GetHashCode();
            hash = hash * 23 + IsSensitive.GetHashCode();
            hash = hash * 23 + IsStatic.GetHashCode();
            hash = hash * 23 + (Mask?.GetHashCode() ?? 0);
            return hash;
        }
    }

    public static bool operator ==(MemberSymbolData left, MemberSymbolData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MemberSymbolData left, MemberSymbolData right)
    {
        return !left.Equals(right);
    }
}