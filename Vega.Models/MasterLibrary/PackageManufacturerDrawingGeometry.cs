namespace Vega.Models.MasterLibrary;

/// <summary>Verified manufacturer geometry for one package definition and drawing revision.</summary>
public sealed class PackageManufacturerDrawingGeometry
{
    public int Id { get; set; }
    public int PackageDefinitionId { get; set; }

    public string Manufacturer { get; set; } = "";
    public string PackageSeries { get; set; } = "";
    public string SourceDocument { get; set; } = "";
    public string SourceRevision { get; set; } = "";
    public string SourcePage { get; set; } = "";
    public string VerificationStatus { get; set; } = "Unverified";
    public bool IsActive { get; set; } = true;
    public bool IsCurrent { get; set; }
    public string Notes { get; set; } = "";

    public List<ProjectionGeometry> Projections { get; set; } = [];
    public List<LeadGeometry> Leads { get; set; } = [];
    public List<DimensionTolerance> Dimensions { get; set; } = [];
    public List<Pin1Geometry> Pin1Markers { get; set; } = [];
}
