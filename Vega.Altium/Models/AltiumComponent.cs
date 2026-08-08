namespace Vega.Altium.Models;

public class AltiumComponent
{
    public string RefDes { get; init; } = "";
    public string Comment { get; init; } = "";
    public string Value { get; init; } = "";
    public string Description { get; init; } = "";
    public string Footprint { get; init; } = "";
    public string Manufacturer { get; init; } = "";
    public string ManufacturerPartNumber { get; init; } = "";
    public string Layer { get; init; } = "";
    public double X { get; init; }
    public double Y { get; init; }
    public double Rotation { get; init; }
    public int Quantity { get; init; } = 1;
}
