namespace Vega.PnP.Models;

public class CorrectedPasteRecommendation
{
    public string RefDes { get; init; } = "";
    public ComponentPastePattern OriginalPattern { get; init; } = new();
    public ComponentPastePattern RecommendedPattern { get; init; } = new();
    public IReadOnlyList<string> Changes { get; init; } = Array.Empty<string>();
    public string Reason { get; init; } = "";
    public string Status { get; init; } = "";
}
