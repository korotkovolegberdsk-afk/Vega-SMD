namespace Vega.Models.MasterLibrary;

public enum TechnologySourceType
{
    IndustryStandard,
    ComponentManufacturer,
    SolderPasteManufacturer,
    EquipmentManufacturer,
    ProductionExperience
}

public class TechnologySource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public TechnologySourceType SourceType { get; set; }
    public string DocumentName { get; set; } = "";
    public string DocumentRevision { get; set; } = "";
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
}

public class TechnologyRecommendation
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public int? RuleId { get; set; }
    public int SourceId { get; set; }
    public string TechnologyGoal { get; set; } = "";
    public string RecommendationText { get; set; } = "";
    public string ParameterJson { get; set; } = "";
    public int Priority { get; set; }
}

public interface ITechnologyKnowledgeProvider
{
    TechnologyRecommendation? GetRecommendation(
        PackageDefinition package,
        string technologyGoal,
        ProcessCondition? processCondition = null);
}
public interface ITechnologyDecisionProvider
{
    ApertureStrategy SelectStrategy(
        ComponentDefinition component,
        PackageDefinition package,
        ProcessCondition processCondition);
}
public class ProcessExperienceInsight
{
    public int PackageId { get; set; }
    public string DefectType { get; set; } = "";
    public string RecommendedStrategy { get; set; } = "";
    public string Parameters { get; set; } = "";
    public double Confidence { get; set; }
    public string Reason { get; set; } = "";
}

public interface IProcessLearningProvider
{
    IReadOnlyList<ProcessExperienceInsight> GetInsights(
        int packageId,
        IReadOnlyCollection<StencilDefectType> defects);
}