namespace Vega.Gerber.Models;

public class PastePrimitive
{
    public ApertureShapeType? ShapeType { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Rotation { get; init; }
    public int ApertureId { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public double Area { get; init; }
    public double Perimeter { get; init; }
}

