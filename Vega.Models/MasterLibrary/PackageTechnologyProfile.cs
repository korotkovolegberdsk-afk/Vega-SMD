namespace Vega.Models.MasterLibrary;

public sealed class PackageTechnologyProfile
{
    public int Id { get; set; }
    public int PackageDefinitionId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public int Revision { get; set; } = 1;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string SourceType { get; set; } = "Internal";
    public string SourceName { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
public sealed class PackageStencilRule { public int Id { get; set; } public int TechnologyProfileId { get; set; } public double StencilThickness { get; set; } public string ApertureShape { get; set; } = string.Empty; public double ReductionPercentX { get; set; } public double ReductionPercentY { get; set; } public double ReductionMmX { get; set; } public double ReductionMmY { get; set; } public double AreaRatioMin { get; set; } public double AspectRatioMin { get; set; } public bool WindowPaneEnabled { get; set; } public int WindowRows { get; set; } public int WindowColumns { get; set; } public double WindowWeb { get; set; } public bool HomePlateEnabled { get; set; } public bool SnubnoseEnabled { get; set; } public double PasteCoveragePercent { get; set; } public double Rotation { get; set; } public double OffsetX { get; set; } public double OffsetY { get; set; } public string Notes { get; set; } = string.Empty; }
public sealed class PackageAoiRule { public int Id { get; set; } public int TechnologyProfileId { get; set; } public string MirtecClass { get; set; } = string.Empty; public string InspectionMethod { get; set; } = string.Empty; public bool PolarityRequired { get; set; } public bool Pin1Required { get; set; } public bool BodyInspection { get; set; } public bool LeadInspection { get; set; } public bool SolderInspection { get; set; } public int MinLeadCount { get; set; } public int ExpectedPadCount { get; set; } public string LightingPreset { get; set; } = string.Empty; public double ToleranceX { get; set; } public double ToleranceY { get; set; } public double ToleranceRotation { get; set; } public string Notes { get; set; } = string.Empty; }
public sealed class PackagePlacementRule { public int Id { get; set; } public int TechnologyProfileId { get; set; } public string YamahaRecognitionType { get; set; } = string.Empty; public string NozzleName { get; set; } = string.Empty; public double PickupHeight { get; set; } public double PlacementHeight { get; set; } public bool VisionRequired { get; set; } public bool PolarityRequired { get; set; } public double MaxRotationCorrection { get; set; } public double PickupOffsetX { get; set; } public double PickupOffsetY { get; set; } public double PlacementOffsetX { get; set; } public double PlacementOffsetY { get; set; } public string Notes { get; set; } = string.Empty; }
