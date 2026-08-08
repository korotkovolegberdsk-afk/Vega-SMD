namespace Vega.StencilCAM.Models;

public class StencilMarking
{
    public string Text { get; init; } = "";
    public double PositionX { get; init; }
    public double PositionY { get; init; }
    public double Height { get; init; }
    public string Font { get; init; } = "";
    public bool Mirror { get; init; }
    public double Rotation { get; init; }
}
