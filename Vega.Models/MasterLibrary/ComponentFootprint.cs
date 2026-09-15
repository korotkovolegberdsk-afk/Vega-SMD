namespace Vega.Models.MasterLibrary;

/// <summary>
/// Footprint tied to one exact manufacturer part number. It must not be used
/// as a default footprint for every component sharing the package.
/// </summary>
public sealed class ComponentFootprint
{
    public int Id { get; set; }
    public int ComponentDefinitionId { get; set; }
    public string PatternName { get; set; } = "";
    public int PadCount { get; set; }
    public double PadLength { get; set; }
    public double PadWidth { get; set; }
    public double PadPitch { get; set; }
    public string PasteLayer { get; set; } = "";
    public string SourceSystem { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string SourceVariant { get; set; } = "";
    public string VerificationStatus { get; set; } = "";
    public string Notes { get; set; } = "";
}
