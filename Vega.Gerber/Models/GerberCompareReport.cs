namespace Vega.Gerber.Models;

public class GerberCompareReport
{
    public string OriginalFile { get; init; } = "";
    public string CorrectedFile { get; init; } = "";
    public int ModifiedCount { get; init; }
    public int AddedCount { get; init; }
    public int RemovedCount { get; init; }
}
