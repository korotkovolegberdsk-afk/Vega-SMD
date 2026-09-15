namespace Vega.Models.MasterLibrary;

public sealed class ComponentHeightVariant
{
    public int Id { get; set; }
    public int ComponentDefinitionId { get; set; }
    public string VariantName { get; set; } = "";
    public string ConditionText { get; set; } = "";
    public double Height { get; set; }
    public string SourceDocument { get; set; } = "";
    public string SourceReference { get; set; } = "";
    public string VerificationStatus { get; set; } = "Unverified";
    public bool IsCurrent { get; set; } = true;
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
