namespace Bcss.ToStringGenerator;

/// <summary>
/// Class containing names for different stages of the ToString generator.
/// </summary>
/// <remarks>
/// This class is read in by Test Helper code via reflection to search for all stages that get added in the future.
/// Be sure to follow the same pattern as below when adding new stages.
/// </remarks>
public static class TrackingNames
{
    public const string ReadConfig = nameof(ReadConfig);
    public const string InitialExtraction = nameof(InitialExtraction);
    public const string CombineProviders = nameof(CombineProviders);
}