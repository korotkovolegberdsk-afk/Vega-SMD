using Vega.Gerber.Models;

namespace Vega.StencilCAM.Models;

public record StencilBounds(double MinX, double MinY, double MaxX, double MaxY)
{
    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
    public double CenterX => (MinX + MaxX) / 2;
    public double CenterY => (MinY + MaxY) / 2;
}

public class StencilPlacementResult
{
    public string FrameName { get; init; } = "";
    public StencilBounds PasteBounds { get; init; } = new(0, 0, 0, 0);
    public StencilBounds FinalBounds { get; init; } = new(0, 0, 0, 0);
    public double OffsetX { get; init; }
    public double OffsetY { get; init; }
    public double Rotation { get; init; }
    public bool IsFit { get; init; }
    public PasteLayer PlacedLayer { get; init; } = new();
}
