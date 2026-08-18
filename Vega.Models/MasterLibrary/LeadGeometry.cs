namespace Vega.Models.MasterLibrary;

public sealed class LeadGeometry
{
    public int Id { get; set; }
    public int ManufacturerDrawingGeometryId { get; set; }

    public int LeadNumber { get; set; }
    public string Side { get; set; } = "";
    public string ProjectionType { get; set; } = "";
    public double RootX { get; set; }
    public double RootY { get; set; }
    public double ContactX { get; set; }
    public double ContactY { get; set; }
    public string ProfileGeometry { get; set; } = "";
    public double Width { get; set; }
    public double Thickness { get; set; }
    // Legacy field retained for backward compatibility. It must not be used
    // as a universal pitch: package families define different pitch axes.
    public double Pitch { get; set; }

    /// <summary>Pitch between adjacent leads in the same row.</summary>
    public double? PitchAlongRow { get; set; }

    /// <summary>Distance between opposing lead rows.</summary>
    public double? RowSpacing { get; set; }

    /// <summary>Distance between the outermost terminals.</summary>
    public double? TerminalSpan { get; set; }
    public bool IsPin1 { get; set; }
}
