namespace Vega.StencilCAM.Models;

public class StencilFrame
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string PrinterModel { get; init; } = "";
    public double FrameWidth { get; init; }
    public double FrameHeight { get; init; }
    public double StencilWidth { get; init; }
    public double StencilHeight { get; init; }
    public double OriginX { get; init; }
    public double OriginY { get; init; }
    public string GerberTemplateFile { get; init; } = "";
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; } = true;
    public int SortOrder { get; init; }
    public string Notes { get; init; } = "";
}
