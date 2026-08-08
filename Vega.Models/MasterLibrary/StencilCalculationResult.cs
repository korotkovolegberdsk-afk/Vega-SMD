namespace Vega.Models.MasterLibrary;

public enum ApertureShape
{
    Rectangle,
    RoundedRectangle,
    Circle,
    Square
}

public enum CalculationStatus
{
    Good,
    Warning,
    Fail
}

public class StencilCalculationResult
{
    public double ApertureLength { get; init; }
    public double ApertureWidth { get; init; }
    public double AreaRatio { get; init; }
    public double AspectRatio { get; init; }
    public ApertureShape ApertureShape { get; init; }
    public CalculationStatus CalculationStatus { get; init; }
    public string Recommendation { get; init; } = "";
}