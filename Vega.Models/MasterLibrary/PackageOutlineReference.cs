namespace Vega.Models.MasterLibrary;

/// <summary>Copyright-safe metadata that records the source outline used for a vector drawing.</summary>
public sealed class PackageOutlineReference
{
    public int Id { get; init; }
    public int PackageDefinitionId { get; init; }
    public string Manufacturer { get; init; } = string.Empty;
    public string ReferencePackageName { get; init; } = string.Empty;
    public string StandardReference { get; init; } = string.Empty;
    public string SourceDocument { get; init; } = string.Empty;
    public string SourcePage { get; init; } = string.Empty;
    public string Revision { get; init; } = string.Empty;
    public string TopologyId { get; init; } = string.Empty;
    public string BodyShape { get; init; } = string.Empty;
    public string LeadStyle { get; init; } = string.Empty;
    public string LeadSides { get; init; } = string.Empty;
    public string LeadDistribution { get; init; } = string.Empty;
    public bool HasTab { get; init; }
    public bool HasExposedPad { get; init; }
    public bool HasBalls { get; init; }
    public bool HasPolarityMarker { get; init; }
    public bool HasPin1Marker { get; init; }
    public string Notes { get; init; } = string.Empty;
}