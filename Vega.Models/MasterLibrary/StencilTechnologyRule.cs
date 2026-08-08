namespace Vega.Models.MasterLibrary;

public class StencilTechnologyRule
{
    public int Id { get; set; }
    public string PackageFamily { get; set; } = "";
    public string PackageName { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public string TechnologyGoal { get; set; } = "";
    public string PreferredShape { get; set; } = "";
    public string AlternativeShape { get; set; } = "";
    public double RecommendedThickness { get; set; }
    public double ReductionX { get; set; }
    public double ReductionY { get; set; }
    public double MinAreaRatio { get; set; }
    public double MinAspectRatio { get; set; }
    public double Coverage { get; set; }
    public string Source { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string DocumentReference { get; set; } = "";
    public string TechnologyReason { get; set; } = "";
    public string Notes { get; set; } = "";
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}
