using Vega.Gerber.Models;

namespace Vega.PnP.Models;

public class ComponentPastePattern
{
    public string RefDes { get; init; } = "";
    public string PackageName { get; init; } = "";
    public IReadOnlyList<PastePrimitive> PastePrimitives { get; init; }
        = Array.Empty<PastePrimitive>();
    public int PadCount { get; init; }
    public double TotalArea { get; init; }
    public double MinX { get; init; }
    public double MaxX { get; init; }
    public double MinY { get; init; }
    public double MaxY { get; init; }
    public double CenterX { get; init; }
    public double CenterY { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public double Rotation { get; init; }
}
