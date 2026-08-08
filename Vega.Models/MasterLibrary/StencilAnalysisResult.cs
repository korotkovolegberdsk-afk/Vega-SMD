namespace Vega.Models.MasterLibrary;

public class StencilAnalysisResult
{
    public string RefDes { get; init; } = "";
    public string PackageName { get; init; } = "";
    public double CurrentPasteArea { get; init; }
    public double ExpectedPasteArea { get; init; }
    public int PadCount { get; init; }
    public string Status { get; init; } = "";
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Recommendations { get; init; }
        = Array.Empty<string>();
}
