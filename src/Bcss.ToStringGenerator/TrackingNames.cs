namespace Bcss.ToStringGenerator;

/// <summary>
/// Class containing names for different stages of the ToString generator.
/// </summary>
/// <remarks>
/// This class is also defined in `TestHelper`, as we don't want these values part of the public api,
/// so it's marked internal here. Be sure to update TestHelper as well if new values are added here.
/// </remarks>
internal static class TrackingNames
{
    public const string ReadConfig = nameof(ReadConfig);
    public const string InitialExtraction = nameof(InitialExtraction);
    public const string CombineProviders = nameof(CombineProviders);
}