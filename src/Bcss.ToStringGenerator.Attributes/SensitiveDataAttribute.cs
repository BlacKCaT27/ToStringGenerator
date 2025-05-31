namespace Bcss.ToStringGenerator.Attributes
{
    #nullable enable
    /// <summary>
    /// An attribute for marking a field or property as sensitive. Sensitive members will not have their value output
    /// in the generated ToString() method. Instead, the provided `maskingValue` string is written. If no value is given,
    /// a default value is used.
    /// </summary>
    /// <param name="maskingValue">Sets the value that will be used in the ToString output instead of the member's actual value.</param>
    /// <remarks>
    /// <p>This attribute will be automatically loaded at compile time by the ToString source generator. You should not need to reference
    /// the project containing this attribute directly.</p>
    /// <br />
    /// <p>If your project exposes internal classes via [InternalsVisibleTo] and you reference the source generator package in multiple
    /// projects in one solution, you may end up with duplicate class definitions due to multiple generators being invoked. If this occurs,
    /// define the following constant in your projects .csproj file, then add a direct reference to the <c>Bcss.ToStringGenerator.Attributes</c>
    /// nuget package.</p>
    /// <br />
    /// <code><DefineConstants>TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES</DefineConstants></code>
    /// <br />
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SensitiveDataAttribute(string? maskingValue = null) : Attribute
    {
    }
    #nullable disable
}