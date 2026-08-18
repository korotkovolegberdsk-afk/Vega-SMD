namespace Vega.Models.MasterLibrary;

public sealed class DimensionTolerance
{
    // DimensionKey is intentionally a string so manufacturer-specific labels
    // remain forward-compatible. Lead pitch semantics are axis-specific:
    // CHIP uses body dimensions, SOT uses pitch along a row, SOP/QFP also use
    // row spacing, QFN uses peripheral pitch, and BGA uses ball pitch.
    public const string LeadPitchAlongRow = "LeadPitchAlongRow";
    public const string LeadRowSpacing = "LeadRowSpacing";
    public const string LeadTerminalSpan = "LeadTerminalSpan";

    public int Id { get; set; }
    public int ManufacturerDrawingGeometryId { get; set; }

    public string DimensionKey { get; set; } = "";
    public string ProjectionType { get; set; } = "";
    public double? NominalValue { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string Symbol { get; set; } = "";
    public string Unit { get; set; } = "mm";
    public string SourceLabel { get; set; } = "";
}
