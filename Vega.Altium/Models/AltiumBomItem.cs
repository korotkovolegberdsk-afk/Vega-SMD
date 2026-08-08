namespace Vega.Altium.Models;

public class AltiumBomItem
{
    public string PartNumber { get; init; } = "";
    public string Description { get; init; } = "";
    public string Manufacturer { get; init; } = "";
    public string Package { get; init; } = "";
    public int Quantity { get; init; }
    public IReadOnlyList<string> Components { get; init; } = Array.Empty<string>();
}
