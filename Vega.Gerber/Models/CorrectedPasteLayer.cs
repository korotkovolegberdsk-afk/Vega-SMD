namespace Vega.Gerber.Models;

public class CorrectedPasteLayer
{
    public string OriginalFileName { get; init; } = "";
    public string Side { get; init; } = "";
    public int OriginalPrimitiveCount { get; init; }
    public int CorrectedPrimitiveCount { get; init; }
    public PasteLayer OriginalLayer { get; init; } = new();
    public IReadOnlyList<PastePrimitive> CorrectedPrimitives { get; init; }
        = Array.Empty<PastePrimitive>();
    public IReadOnlyList<PasteCorrectionChange> Changes { get; init; }
        = Array.Empty<PasteCorrectionChange>();
}
