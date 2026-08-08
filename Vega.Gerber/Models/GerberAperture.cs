namespace Vega.Gerber.Models;

public class GerberAperture
{
    public int ApertureId { get; init; }
    public string Shape { get; init; } = "";
    public double Width { get; init; }
    public double Height { get; init; }
    public double Diameter { get; init; }
}
