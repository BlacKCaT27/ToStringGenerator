namespace Bcss.ToStringGenerator;

/// <summary>
/// Contains configuration parameters for the ToString generator.
/// </summary>
public class ToStringGeneratorConfigOptions
{
    /// <summary>
    /// The default redaction value to use if no value is specified when using `SensitiveDataAttribute`.
    /// </summary>
    public string RedactionValue { get; set; } = "[REDACTED]";

    /// <summary>
    /// Whether private fields and properties should be hidden from the ToString output.
    /// Default is false.
    /// </summary>
    public bool IncludePrivateDataMembers { get; set; }
}