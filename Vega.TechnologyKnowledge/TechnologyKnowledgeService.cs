using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.TechnologyKnowledge;

public class TechnologyKnowledgeService : ITechnologyKnowledgeProvider
{
    private readonly TechnologyRecommendationRepository _recommendations;
    private readonly TechnologySourceRepository _sources;
    private readonly StencilTechnologyRuleRepository _rules;

    public TechnologyKnowledgeService(
        TechnologyRecommendationRepository? recommendations = null,
        TechnologySourceRepository? sources = null,
        StencilTechnologyRuleRepository? rules = null)
    {
        _recommendations = recommendations ?? new TechnologyRecommendationRepository();
        _sources = sources ?? new TechnologySourceRepository();
        _rules = rules ?? new StencilTechnologyRuleRepository();
    }

    public TechnologyRecommendation? GetRecommendation(
        PackageDefinition package,
        string technologyGoal,
        ProcessCondition? processCondition = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        var goals = GoalCandidates(technologyGoal, processCondition);
        return _recommendations.GetByPackage(package.Id)
            .Where(item => goals.Contains(item.TechnologyGoal, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Priority)
            .FirstOrDefault();
    }

    public StencilTechnologyRule? GetBestRule(
        PackageDefinition package,
        string technologyGoal,
        ProcessCondition? processCondition = null)
    {
        var recommendation = GetRecommendation(package, technologyGoal, processCondition);
        if (recommendation?.RuleId is int ruleId) return _rules.GetById(ruleId);
        return new StencilTechnologyRuleService(_rules).GetRule(package, ToStrategy(technologyGoal));
    }

    public TechnologySource? GetSourceInformation(int sourceId) => _sources.GetById(sourceId);

    private static string[] GoalCandidates(string goal, ProcessCondition? condition)
    {
        var values = new List<string> { goal };
        if (goal.Equals("StandardPasteRelease", StringComparison.OrdinalIgnoreCase)) values.Add("StandardAssembly");
        if (condition?.RequiresVoidReduction == true) values.Add("VoidReduction");
        if (condition?.IsFinePitch == true) values.Add("FinePitch");
        return values.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static ApertureStrategy ToStrategy(string technologyGoal) => technologyGoal.ToUpperInvariant() switch
    {
        "STANDARDASSEMBLY" => ApertureStrategy.StandardPasteRelease,
        "FINEPITCH" => ApertureStrategy.FinePitch,
        "VOIDREDUCTION" => ApertureStrategy.VoidReduction,
        "ANTISOLDERBALL" => ApertureStrategy.AntiSolderBall,
        "HIGHRELIABILITY" => ApertureStrategy.AntiTombstone,
        "LOWSTANDOFF" => ApertureStrategy.FinePitch,
        _ => Enum.TryParse<ApertureStrategy>(technologyGoal, true, out var strategy)
            ? strategy : ApertureStrategy.StandardPasteRelease
    };
}