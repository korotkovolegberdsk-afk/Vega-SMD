namespace Vega.StencilViewer.Models;

public class StencilPreviewData
{
    public double Zoom { get; set; } = 1;
    public double PanX { get; set; }
    public double PanY { get; set; }
    public double Rotation { get; set; }
    public IReadOnlyList<string> VisibleLayers { get; set; } = Array.Empty<string>();
}
