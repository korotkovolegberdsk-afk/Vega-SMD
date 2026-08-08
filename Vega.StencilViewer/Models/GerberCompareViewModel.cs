namespace Vega.StencilViewer.Models;

public class GerberCompareViewModel
{
    public int AddedApertures { get; init; }
    public int RemovedApertures { get; init; }
    public int ModifiedApertures { get; init; }
    public bool ChangedShape { get; init; }
    public IReadOnlyList<string> Changes { get; init; } = Array.Empty<string>();
}
