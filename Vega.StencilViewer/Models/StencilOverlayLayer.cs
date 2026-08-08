namespace Vega.StencilViewer.Models;

public enum StencilOverlayLayerType
{
    OriginalPaste,
    CorrectedPaste,
    Frame,
    Fiducial,
    Marking
}

public enum StencilViewMode
{
    Original,
    Corrected,
    Overlay,
    Production
}

public class OverlayGeometry
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public double Rotation { get; init; }
    public string Shape { get; init; } = "Rectangle";
    public string Text { get; init; } = "";
}

public class StencilOverlayLayer
{
    public string Name { get; init; } = "";
    public bool Visible { get; set; } = true;
    public StencilOverlayLayerType LayerType { get; init; }
    public IReadOnlyList<OverlayGeometry> Geometry { get; init; } = Array.Empty<OverlayGeometry>();
}
