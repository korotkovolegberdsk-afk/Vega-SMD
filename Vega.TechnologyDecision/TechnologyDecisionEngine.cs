using System.Text.Json;
using Vega.Data.MasterLibrary.Repository;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.TechnologyDecision.Models;
using Vega.TechnologyKnowledge;

namespace Vega.TechnologyDecision;

public class TechnologyDecisionEngine : ITechnologyDecisionProvider
{
    private readonly PackageDefinitionRepository _packages;
    private readonly TechnologyRecommendationRepository _recommendations;
    private readonly TechnologySourceRepository _sources;
    private readonly StencilTechnologyRuleRepository _rules;
    private readonly TechnologyKnowledgeService _knowledge;

    public TechnologyDecisionEngine(
        PackageDefinitionRepository? packages = null,
        TechnologyRecommendationRepository? recommendations = null,
        TechnologySourceRepository? sources = null,
        StencilTechnologyRuleRepository? rules = null,
        TechnologyKnowledgeService? knowledge = null)
    {
        _packages = packages ?? new PackageDefinitionRepository();
        _recommendations = recommendations ?? new TechnologyRecommendationRepository();
        _sources = sources ?? new TechnologySourceRepository();
        _rules = rules ?? new StencilTechnologyRuleRepository();
        _knowledge = knowledge ?? new TechnologyKnowledgeService(_recommendations, _sources, _rules);
    }

    public TechnologyDecisionResult Evaluate(TechnologyDecisionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var package = _packages.GetById(context.PackageId)
            ?? throw new InvalidOperationException($"Package {context.PackageId} was not found in MasterLibrary.");
        var candidates = BuildCandidates(context, package).OrderByDescending(item => item.Score.Score).ToList();
        if (candidates.Count == 0)
        {
            return new TechnologyDecisionResult
            {
                SelectedStrategy = TechnologyKnowledgeService.ToStrategy(context.TechnologyGoal.ToString()),
                SelectedShape = ApertureShapeType.Rectangle,
                Confidence = 0.25,
                Reason = "No matching technology recommendation was found; the default rectangle rule was selected.",
                Warnings = ["No package-specific technology recommendation is available."]
            };
        }

        var selected = candidates[0];
        var parameters = ReadParameters(selected.Recommendation?.ParameterJson);
        var shape = SelectShape(selected.Rule, parameters, context.PastePattern);
        var sources = selected.Source is null ? Array.Empty<TechnologySource>() : [selected.Source];
        var confidence = Math.Clamp(selected.Score.Confidence + selected.Score.Score / 10000000000d, 0.0, 1.0);
        var warnings = new List<string>();
        if (selected.Rule is null) warnings.Add("A linked stencil rule was not found; recommendation parameters were used.");
        if (selected.Rule?.RecommendedThickness > 0 && Math.Abs(context.StencilThickness - selected.Rule.RecommendedThickness) > .03) warnings.Add("Stencil thickness differs from the selected rule recommendation.");
        if (context.PastePattern.Count == 0) warnings.Add("Paste geometry is not available; the decision is based on package rules.");
        return new TechnologyDecisionResult
        {
            SelectedStrategy = TechnologyKnowledgeService.ToStrategy(context.TechnologyGoal.ToString()),
            SelectedShape = shape,
            Parameters = parameters,
            Confidence = confidence,
            Reason = selected.Score.Reason,
            Sources = sources,
            Alternatives = candidates.Skip(1).Select(item => item.Score).ToArray(),
            Warnings = warnings
        };
    }

    public ApertureStrategy SelectStrategy(TechnologyDecisionContext context) => Evaluate(context).SelectedStrategy;

    public IReadOnlyList<DecisionRuleScore> CompareAlternatives(TechnologyDecisionContext context) =>
        BuildCandidates(context, Package(context)).OrderByDescending(item => item.Score.Score).Select(item => item.Score).ToArray();

    public string ExplainDecision(TechnologyDecisionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var sourceText = result.Sources.Count == 0 ? "default rule" : string.Join(", ", result.Sources.Select(item => item.Name));
        var shapeText = result.Parameters.TryGetValue("shape", out var shape) ? shape : result.SelectedShape.ToString();
        return $"Selected {shapeText} for {result.SelectedStrategy} using {sourceText}. {result.Reason}";
    }

    public TechnologyDecisionReport CreateReport(TechnologyDecisionContext context, TechnologyDecisionResult result)
    {
        var package = Package(context);
        return new TechnologyDecisionReport
        {
            Component = context.ComponentId == 0 ? "" : context.ComponentId.ToString(), Package = package.PackageName,
            Goal = context.TechnologyGoal.ToString(), Selected = result.SelectedShape.ToString(), Parameters = result.Parameters,
            Sources = result.Sources.Select(item => item.Name).ToArray(), Confidence = result.Confidence >= 0.8 ? "High" : result.Confidence >= 0.5 ? "Medium" : "Low",
            Explanation = ExplainDecision(result)
        };
    }

    ApertureStrategy ITechnologyDecisionProvider.SelectStrategy(ComponentDefinition component, PackageDefinition package, ProcessCondition processCondition)
    {
        var goal = processCondition.RequiresVoidReduction ? TechnologyDecisionGoal.VoidReduction
            : processCondition.IsFinePitch ? TechnologyDecisionGoal.FinePitch
            : processCondition.DefectRisks.Contains(StencilDefectType.SolderBall) ? TechnologyDecisionGoal.AntiSolderBall
            : TechnologyDecisionGoal.StandardAssembly;
        return SelectStrategy(new TechnologyDecisionContext
        {
            ComponentId = component.Id, PackageId = package.Id, PackageFamily = package.PackageFamily,
            TechnologyGoal = goal, StencilThickness = 0.12, HistoricalDefects = processCondition.DefectRisks
        });
    }

    private List<Candidate> BuildCandidates(TechnologyDecisionContext context, PackageDefinition package)
    {
        var expectedGoal = context.TechnologyGoal.ToString();
        var recommendations = _recommendations.GetByPackage(package.Id)
            .Where(item => item.TechnologyGoal.Equals(expectedGoal, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var candidates = recommendations.Select(item => CandidateFor(item, _rules.GetById(item.RuleId ?? 0), _sources.GetById(item.SourceId), context)).ToList();
        if (candidates.Count == 0)
        {
            var rule = _knowledge.GetBestRule(package, expectedGoal);
            if (rule is not null) candidates.Add(CandidateFor(null, rule, rule.TechnologySourceId is int sourceId ? _sources.GetById(sourceId) : null, context));
        }
        foreach (var rule in context.AvailableRules.Where(rule => GoalMatches(rule, expectedGoal)))
            candidates.Add(CandidateFor(null, rule, rule.TechnologySourceId is int sourceId ? _sources.GetById(sourceId) : null, context));
        return candidates;
    }

    private static Candidate CandidateFor(TechnologyRecommendation? recommendation, StencilTechnologyRule? rule, TechnologySource? source, TechnologyDecisionContext context)
    {
        var priority = recommendation?.Priority ?? rule?.Priority ?? 0;
        var confidence = rule?.ConfidenceLevel > 0 ? rule.ConfidenceLevel : SourceConfidence(source?.SourceType);
        var sourceRank = SourceRank(source?.SourceType);
        var shape = rule?.PreferredShape ?? recommendation?.ParameterJson ?? "default";
        var customerMatch = Matches(context.CustomerRequirement, shape) || Matches(context.CustomerRequirement, context.TechnologyGoal.ToString());
        var historicalMatch = (context.HistoricalDefects.Contains(StencilDefectType.SolderBall) && shape.Contains("Snubnose", StringComparison.OrdinalIgnoreCase))
            || (context.HistoricalDefects.Contains(StencilDefectType.Void) && shape.Contains("WindowPane", StringComparison.OrdinalIgnoreCase));
        var experience = context.HistoricalExperience.FirstOrDefault(item => item.RecommendedStrategy.Contains(shape, StringComparison.OrdinalIgnoreCase) || shape.Contains(item.RecommendedStrategy, StringComparison.OrdinalIgnoreCase));
        var experienceMatch = experience is not null;
        var score = sourceRank * 1000000d + priority * 100d + confidence * 100d + (customerMatch ? 1000000000d : 0d) + (experienceMatch ? 600000000d * experience!.Confidence : 0d) + (historicalMatch ? 500000d : 0d);
        var reason = customerMatch ? "Customer requirement matched the selected recommendation."
            : experienceMatch ? experience!.Reason
            : historicalMatch ? "Historical production defect prevention matched the selected recommendation."
            : $"{source?.Name ?? "Default rule"} has source precedence and priority {priority}.";
        return new Candidate(recommendation, rule, source, new DecisionRuleScore { RuleId = rule?.Id, Source = source?.Name ?? "Default Rule", Priority = priority, Confidence = confidence, Score = score, Reason = reason });
    }

    private static int SourceRank(TechnologySourceType? sourceType) => sourceType switch
    {
        TechnologySourceType.ProductionExperience => 700,
        TechnologySourceType.ComponentManufacturer => 600,
        TechnologySourceType.SolderPasteManufacturer => 500,
        TechnologySourceType.EquipmentManufacturer => 400,
        TechnologySourceType.IndustryStandard => 300,
        _ => 100
    };

    private static double SourceConfidence(TechnologySourceType? sourceType) => sourceType switch
    {
        TechnologySourceType.ProductionExperience => .90,
        TechnologySourceType.ComponentManufacturer => .85,
        TechnologySourceType.SolderPasteManufacturer => .80,
        TechnologySourceType.EquipmentManufacturer => .75,
        TechnologySourceType.IndustryStandard => .70,
        _ => .50
    };

    private static bool GoalMatches(StencilTechnologyRule rule, string goal) =>
        rule.TechnologyGoal.Equals(goal, StringComparison.OrdinalIgnoreCase)
        || TechnologyKnowledgeService.ToStrategy(rule.TechnologyGoal) == TechnologyKnowledgeService.ToStrategy(goal);

    private static bool Matches(string requirement, string value) => !string.IsNullOrWhiteSpace(requirement) && requirement.Contains(value, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, string> ReadParameters(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, string>();
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.EnumerateObject().ToDictionary(item => item.Name, item => item.Value.ToString());
        }
        catch (JsonException) { return new Dictionary<string, string> { ["recommendation"] = json }; }
    }

    private static ApertureShapeType SelectShape(StencilTechnologyRule? rule, IReadOnlyDictionary<string, string> parameters, IReadOnlyList<PastePrimitive> pastePattern)
    {
        var value = parameters.TryGetValue("shape", out var parameterShape) ? parameterShape : rule?.PreferredShape ?? "";
        if (value.Equals("WindowPane", StringComparison.OrdinalIgnoreCase)) return ApertureShapeType.Array;
        if (Enum.TryParse<ApertureShapeType>(value, true, out var shape)) return shape;
        return pastePattern.FirstOrDefault()?.ShapeType ?? ApertureShapeType.Rectangle;
    }

    private PackageDefinition Package(TechnologyDecisionContext context) => _packages.GetById(context.PackageId)
        ?? throw new InvalidOperationException($"Package {context.PackageId} was not found in MasterLibrary.");

    private sealed record Candidate(TechnologyRecommendation? Recommendation, StencilTechnologyRule? Rule, TechnologySource? Source, DecisionRuleScore Score);
}