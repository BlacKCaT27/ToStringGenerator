namespace Bcss.ToStringGenerator;

/// <summary>
/// A struct containing all the data needed to generate a ToString() method for a class.
/// </summary>
internal readonly struct ClassSymbolData : IEquatable<ClassSymbolData>
{
    public string ContainingNamespace { get; }
    public string ClassAccessibility { get; }
    public string ClassName { get; }
    public List<MemberSymbolData> Members { get; }

    /// <summary>
    /// Instantiates a new instance of <see cref="ClassSymbolData"/>.
    /// </summary>
    /// <param name="containingNamespace">The namespace of the class whose ToString method is to be generated.</param>
    /// <param name="classAccessibility">The accessibility level of the class whose ToString method is to be generated.</param>
    /// <param name="className">The name of the class whose ToString method is to be generated.</param>
    /// <param name="members">A collection of <see cref="MemberSymbolData"/> containing information about each member to be recorded in the ToString method.</param>
    public ClassSymbolData(string containingNamespace, string classAccessibility, string className, List<MemberSymbolData> members)
    {
        ContainingNamespace = containingNamespace;
        ClassAccessibility = classAccessibility;
        ClassName = className;
        Members = members;
    }

    public bool Equals(ClassSymbolData other)
    {
        return ContainingNamespace == other.ContainingNamespace &&
               ClassAccessibility == other.ClassAccessibility &&
               ClassName == other.ClassName &&
               Members.SequenceEqual(other.Members);
    }

    public override bool Equals(object? obj)
    {
        return obj is ClassSymbolData other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + (ContainingNamespace?.GetHashCode() ?? 0);
            hash = hash * 23 + (ClassAccessibility?.GetHashCode() ?? 0);
            hash = hash * 23 + (ClassName?.GetHashCode() ?? 0);
            foreach (var member in Members)
            {
                hash = hash * 23 + member.GetHashCode();
            }
            return hash;
        }
    }

    public static bool operator ==(ClassSymbolData left, ClassSymbolData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ClassSymbolData left, ClassSymbolData right)
    {
        return !left.Equals(right);
    }
}