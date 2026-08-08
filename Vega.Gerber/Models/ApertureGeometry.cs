namespace Vega.Gerber.Models;

public enum ApertureShapeType
{
    Round,
    Square,
    Rectangle,
    Oblong,
    Ellipse,
    Triangle,
    Diamond,
    Array,
    HomePlate,
    InvertedHomePlate,
    MELF,
    Bullet,
    Snubnose,
    DogBone
}

public class ApertureGeometry
{
    public ApertureShapeType ShapeType { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public double Length { get; init; }
    public double Radius { get; init; }
    public double CornerRadius { get; init; }
    public double Rotation { get; init; }
    public int Rows { get; init; } = 1;
    public int Columns { get; init; } = 1;
    public double WebWidth { get; init; }
    public double Coverage { get; init; } = 100;
    public IReadOnlyDictionary<string, double> Parameters { get; init; }
        = new Dictionary<string, double>();
}
