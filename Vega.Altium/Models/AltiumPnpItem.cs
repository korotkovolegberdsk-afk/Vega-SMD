namespace Vega.Altium.Models;

public class AltiumPnpItem
{
    public string RefDes { get; init; } = "";
    public double X { get; init; }
    public double Y { get; init; }
    public double Rotation { get; init; }
    public string Layer { get; init; } = "";
    public string Footprint { get; init; } = "";
    public string Comment { get; init; } = "";
}
