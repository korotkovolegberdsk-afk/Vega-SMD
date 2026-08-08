namespace Vega.Gerber.Models;

public enum AperturePatternType
{
    Single,
    RectangleReduction,
    WindowPane,
    Grid,
    Cross,
    HomePlate
}

public class OptimizedAperturePattern
{
    public string RefDes { get; init; } = string.Empty;
    public ApertureShapeType ApertureShapeType { get; init; } = ApertureShapeType.Rectangle;
    public ApertureGeometry? ApertureGeometry { get; init; }
    public AperturePatternType PatternType { get; init; }
    public double OriginalWidth { get; init; }
    public double OriginalHeight { get; init; }
    public double RecommendedWidth { get; init; }
    public double RecommendedHeight { get; init; }
    public int Rows { get; init; }
    public int Columns { get; init; }
    public double WebWidth { get; init; }
    public double CoveragePercent { get; init; }
    public string Reason { get; init; } = "";
}




