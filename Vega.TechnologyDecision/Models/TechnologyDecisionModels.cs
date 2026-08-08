using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;

namespace Vega.TechnologyDecision.Models;

public enum TechnologyDecisionGoal
{
    StandardAssembly,
    FinePitch,
    VoidReduction,
    AntiSolderBall,
    HighReliability,
    LowPasteVolume
}

public enum TechnologyProcessCondition
{
    LeadFree,
    NoClean,
    HighTemperature,
    LowStandoff
}

public class TechnologyDecisionContext
{
    public int ComponentId { get; set; }
    public int PackageId { get; set; }
    public string PackageFamily { get; set; } = "";
    public IReadOnlyList<PastePrimitive> PastePattern { get; set; } = Array.Empty<PastePrimitive>();
    public double StencilThickness { get; set; }
    public TechnologyDecisionGoal TechnologyGoal { get; set; } = TechnologyDecisionGoal.StandardAssembly;
    public IReadOnlyCollection<TechnologyProcessCondition> ProcessCondition { get; set; } = Array.Empty<TechnologyProcessCondition>();
    public string CustomerRequirement { get; set; } = "";
    public IReadOnlyCollection<StencilDefectType> HistoricalDefects { get; set; } = Array.Empty<StencilDefectType>();
    public IReadOnlyCollection<ProcessExperienceInsight> HistoricalExperience { get; set; } = Array.Empty<ProcessExperienceInsight>();
    public IReadOnlyCollection<StencilTechnologyRule> AvailableRules { get; set; } = Array.Empty<StencilTechnologyRule>();
}

public class DecisionRuleScore
{
    public int? RuleId { get; set; }
    public string Source { get; set; } = "";
    public int Priority { get; set; }
    public double Confidence { get; set; }
    public double Score { get; set; }
    public string Reason { get; set; } = "";
}

public class TechnologyDecisionResult
{
    public ApertureStrategy SelectedStrategy { get; set; }
    public ApertureShapeType SelectedShape { get; set; }
    public IReadOnlyDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    public double Confidence { get; set; }
    public string Reason { get; set; } = "";
    public IReadOnlyList<TechnologySource> Sources { get; set; } = Array.Empty<TechnologySource>();
    public IReadOnlyList<DecisionRuleScore> Alternatives { get; set; } = Array.Empty<DecisionRuleScore>();
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}

public class TechnologyDecisionReport
{
    public string Component { get; set; } = "";
    public string Package { get; set; } = "";
    public string Goal { get; set; } = "";
    public string Selected { get; set; } = "";
    public IReadOnlyDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    public IReadOnlyList<string> Sources { get; set; } = Array.Empty<string>();
    public string Confidence { get; set; } = "";
    public string Explanation { get; set; } = "";
}