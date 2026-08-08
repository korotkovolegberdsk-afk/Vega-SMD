namespace Vega.Models.MasterLibrary;

public class StencilRecommendationRule
{
    public int Id { get; set; }
    public string PackageFamily { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public double RecommendedStencilThickness { get; set; }
    public ApertureShape ApertureShape { get; set; }
    public double ReductionX { get; set; }
    public double ReductionY { get; set; }
    public string ThermalPadRule { get; set; } = "";
    public double AreaRatioMinimum { get; set; }
    public double AspectRatioMinimum { get; set; }
    public string TechnologyGoal { get; set; } = "";
    public string PreferredShape { get; set; } = "";
    public string AlternativeShape { get; set; } = "";
    public int Priority { get; set; }
    public string RuleSource { get; set; } = "";
    public string Notes { get; set; } = "";
}

