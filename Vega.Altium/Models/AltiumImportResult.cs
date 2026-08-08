namespace Vega.Altium.Models;

public class AltiumImportResult
{
    public string ProjectName { get; init; } = "";
    public IReadOnlyList<AltiumComponent> Components { get; init; } = Array.Empty<AltiumComponent>();
    public IReadOnlyList<AltiumBomItem> Bom { get; init; } = Array.Empty<AltiumBomItem>();
    public IReadOnlyList<AltiumPnpItem> PickAndPlace { get; init; } = Array.Empty<AltiumPnpItem>();
}
