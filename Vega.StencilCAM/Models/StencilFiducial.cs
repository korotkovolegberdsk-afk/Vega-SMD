namespace Vega.StencilCAM.Models;

public enum StencilFiducialType
{
    Pcb,
    Local
}

public class StencilFiducial
{
    public StencilFiducialType Type { get; init; }
    public string Shape { get; init; } = "Round";
    public double Diameter { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public string Layer { get; init; } = "Fiducial";
    public double Rotation { get; init; }
}
