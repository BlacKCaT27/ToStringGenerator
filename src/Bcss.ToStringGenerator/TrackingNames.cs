namespace Bcss.ToStringGenerator;

/// <summary>
/// Class containing names for different stages of the ToString generator.
/// </summary>
/// <remarks>
/// This class is accessed reflectively within TestHelpers - do not rename, move, or remove
/// without updating there as well. Only add public constant string FIELDS, not properties, to this class.
/// </remarks>
internal static class TrackingNames
{
    public const string ReadConfig = nameof(ReadConfig);
    public const string InitialExtraction = nameof(InitialExtraction);
    public const string CombineProviders = nameof(CombineProviders);
}