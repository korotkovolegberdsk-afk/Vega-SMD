namespace Vega.Gerber.Models;

public class PasteAnalysisResult
{
    public int PrimitiveCount { get; init; }
    public int ApertureCount { get; init; }
    public IReadOnlyDictionary<string, int> ShapeStatistics { get; init; }
        = new Dictionary<string, int>();
    public double MinApertureSize { get; init; }
    public double MaxApertureSize { get; init; }
    public int WarningCount { get; init; }
}
