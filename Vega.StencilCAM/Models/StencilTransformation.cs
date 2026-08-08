namespace Vega.StencilCAM.Models;

public class StencilTransformation
{
    public bool MirrorX { get; init; }
    public bool MirrorY { get; init; }
    public double RotationAngle { get; init; }
    public double OffsetX { get; init; }
    public double OffsetY { get; init; }
    public bool AutoCenter { get; init; }
}
