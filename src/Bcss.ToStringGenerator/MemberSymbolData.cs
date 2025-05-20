namespace Bcss.ToStringGenerator;

public readonly struct MemberSymbolData : IEquatable<MemberSymbolData>
{
    public string MemberName { get; }
    public bool IsDictionary { get; }
    public bool IsEnumerable { get; }
    public bool IsNullableType { get; }
    public bool IsSensitive { get; }
    public string? Mask { get; }

    public MemberSymbolData(
        string memberName,
        bool isDictionary,
        bool isEnumerable,
        bool isNullableType,
        bool isSensitive,
        string? mask)
    {
        MemberName = memberName;
        IsDictionary = isDictionary;
        IsEnumerable = isEnumerable;
        IsNullableType = isNullableType;
        IsSensitive = isSensitive;
        Mask = mask;
    }

    public bool Equals(MemberSymbolData other)
    {
        return MemberName == other.MemberName &&
               IsDictionary == other.IsDictionary &&
               IsEnumerable == other.IsEnumerable &&
               IsNullableType == other.IsNullableType &&
               IsSensitive == other.IsSensitive &&
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
            hash = hash * 23 + IsDictionary.GetHashCode();
            hash = hash * 23 + IsEnumerable.GetHashCode();
            hash = hash * 23 + IsNullableType.GetHashCode();
            hash = hash * 23 + IsSensitive.GetHashCode();
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