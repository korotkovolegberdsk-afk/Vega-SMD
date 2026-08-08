using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.Models;
using Vega.Models.MasterLibrary;
using Vega.PnP;
using Vega.PnP.Models;
using Vega.Services.MasterLibrary;

namespace Vega.Services;

public class StencilProjectAnalyzerService
{
    private readonly PackageDefinitionService _packageService;
    private readonly StencilTechnologyRuleService _technologyRuleService;
    private readonly ApertureStrategySelectorService _strategySelector;
    private readonly StencilRecommendationAnalyzerService _recommendationAnalyzer;
    private readonly PnpParserService _pnpParser = new();
    private readonly ComponentMappingService _mappingService = new();
    private readonly ComponentPasteAnalyzerService _pasteAnalyzer = new();

    public StencilProjectAnalyzerService(
        PackageDefinitionService? packageService = null,
        StencilTechnologyRuleService? technologyRuleService = null,
        ApertureStrategySelectorService? strategySelector = null,
        StencilRecommendationAnalyzerService? recommendationAnalyzer = null)
    {
        _packageService = packageService ?? new PackageDefinitionService();
        _technologyRuleService = technologyRuleService ?? new StencilTechnologyRuleService();
        _strategySelector = strategySelector ?? new ApertureStrategySelectorService();
        _recommendationAnalyzer = recommendationAnalyzer ?? new StencilRecommendationAnalyzerService(_packageService);
    }

    public StencilProjectAnalysisResult Analyze(StencilProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        var components = project.Components.Count > 0
            ? project.Components.ToList()
            : string.IsNullOrWhiteSpace(project.PnpFile) ? new List<PnpComponent>() : _pnpParser.Parse(project.PnpFile);
        var packageByName = _packageService.GetAll()
            .ToDictionary(package => package.PackageName, StringComparer.OrdinalIgnoreCase);
        var results = new List<ComponentStencilAnalysisResult>();

        AnalyzeSide(project.TopPasteFile, "Top", components, packageByName, results);
        AnalyzeSide(project.BottomPasteFile, "Bottom", components, packageByName, results);

        var analyzed = results.Count(result => result.Status != StencilAnalysisStatus.Critical || result.PastePattern is not null);
        return new StencilProjectAnalysisResult
        {
            ProjectName = project.ProjectName,
            TotalComponents = components.Count,
            AnalyzedComponents = analyzed,
            OkCount = results.Count(result => result.Status == StencilAnalysisStatus.OK),
            WarningCount = results.Count(result => result.Status == StencilAnalysisStatus.Warning),
            CriticalCount = results.Count(result => result.Status == StencilAnalysisStatus.Critical),
            ComponentResults = results
        };
    }

    private void AnalyzeSide(
        string pasteFile,
        string side,
        IReadOnlyList<PnpComponent> components,
        IReadOnlyDictionary<string, PackageDefinition> packageByName,
        List<ComponentStencilAnalysisResult> results)
    {
        var sideComponents = components.Where(component => IsSide(component.Side, side)).ToArray();
        if (sideComponents.Length == 0)
            return;

        if (string.IsNullOrWhiteSpace(pasteFile) || !File.Exists(pasteFile))
        {
            results.AddRange(sideComponents.Select(component => MissingPaste(component, side)));
            return;
        }

        var parser = new GerberPasteParserService();
        parser.Load(pasteFile);
        var layer = parser.Parse();
        var mapped = _mappingService.Map(sideComponents, layer);
        foreach (var component in sideComponents)
        {
            var mapping = mapped.Single(item => item.RefDes.Equals(component.RefDes, StringComparison.OrdinalIgnoreCase));
            var pattern = _pasteAnalyzer.Analyze(mapping);
            results.Add(AnalyzeComponent(component, side, pattern, packageByName));
        }
    }

    private ComponentStencilAnalysisResult AnalyzeComponent(
        PnpComponent component,
        string side,
        ComponentPastePattern pattern,
        IReadOnlyDictionary<string, PackageDefinition> packageByName)
    {
        if (!packageByName.TryGetValue(component.PackageName, out var package))
        {
            return Critical(component, side, pattern, "PackageDefinition is not available in MasterLibrary.");
        }

        var condition = new ProcessCondition
        {
            HasThermalPad = package.ThermalPadCount > 0,
            IsFinePitch = package.Pitch is > 0 and <= 0.5,
            IsBga = package.PackageName.Contains("BGA", StringComparison.OrdinalIgnoreCase)
        };
        var strategy = _strategySelector.SelectStrategy(new ComponentDefinition { ComponentType = "" }, package, condition);
        var technologyRule = _technologyRuleService.GetRule(package, strategy);
        if (technologyRule is null)
        {
            return Critical(component, side, pattern, "Stencil technology rule is not available.");
        }

        var recommendedShape = _technologyRuleService.GetPreferredShape(package, strategy);
        var currentShape = pattern.PastePrimitives.FirstOrDefault()?.ShapeType ?? recommendedShape;
        var legacyRule = ToRecommendationRule(technologyRule, package);
        var engineering = _recommendationAnalyzer.Analyze(pattern, package, legacyRule);
        var areaRatio = MinimumAreaRatio(pattern, legacyRule.RecommendedStencilThickness);
        var aspectRatio = MinimumAspectRatio(pattern, legacyRule.RecommendedStencilThickness);
        var warnings = engineering.Warnings.ToList();
        if (recommendedShape.HasValue && currentShape.HasValue && recommendedShape != currentShape)
            warnings.Add("Current aperture shape differs from the technology recommendation.");

        var status = GetStatus(engineering.Status, technologyRule, areaRatio, aspectRatio, warnings);
        var recommendations = engineering.Recommendations.ToList();
        recommendations.Add($"Recommended aperture shape: {recommendedShape?.ToString() ?? technologyRule.PreferredShape}.");
        if (!string.IsNullOrWhiteSpace(technologyRule.TechnologyReason))
            recommendations.Add(technologyRule.TechnologyReason);

        return new ComponentStencilAnalysisResult
        {
            RefDes = component.RefDes, PackageName = component.PackageName, Side = side, PastePattern = pattern,
            TechnologyRule = technologyRule, CurrentShape = currentShape, RecommendedShape = recommendedShape,
            CurrentArea = engineering.CurrentPasteArea, RecommendedArea = engineering.ExpectedPasteArea,
            AreaRatio = areaRatio, AspectRatio = aspectRatio, Status = status,
            Warnings = warnings, Recommendations = recommendations
        };
    }

    private static ComponentStencilAnalysisResult MissingPaste(PnpComponent component, string side) =>
        Critical(component, side, null, "Paste layer is not available for this component side.");

    private static ComponentStencilAnalysisResult Critical(PnpComponent component, string side, ComponentPastePattern? pattern, string warning) => new()
    {
        RefDes = component.RefDes, PackageName = component.PackageName, Side = side, PastePattern = pattern,
        Status = StencilAnalysisStatus.Critical, Warnings = [warning], Recommendations = ["Provide MasterLibrary and Gerber Paste data."]
    };

    private static StencilRecommendationRule ToRecommendationRule(StencilTechnologyRule rule, PackageDefinition package) => new()
    {
        PackageFamily = rule.PackageFamily, ComponentType = rule.ComponentType,
        RecommendedStencilThickness = rule.RecommendedThickness, ReductionX = rule.ReductionX, ReductionY = rule.ReductionY,
        AreaRatioMinimum = rule.MinAreaRatio, AspectRatioMinimum = rule.MinAspectRatio,
        ThermalPadRule = package.ThermalPadCount > 0 ? rule.TechnologyReason : "",
        ApertureShape = ApertureShape.Rectangle
    };

    private static StencilAnalysisStatus GetStatus(string engineeringStatus, StencilTechnologyRule rule, double areaRatio, double aspectRatio, IReadOnlyList<string> warnings)
    {
        if (engineeringStatus == "FAIL" || areaRatio < rule.MinAreaRatio || aspectRatio < rule.MinAspectRatio)
            return StencilAnalysisStatus.Critical;
        return warnings.Count == 0 ? StencilAnalysisStatus.OK : StencilAnalysisStatus.Warning;
    }

    private static double MinimumAreaRatio(ComponentPastePattern pattern, double thickness) =>
        thickness <= 0 || pattern.PastePrimitives.Count == 0 ? 0 : pattern.PastePrimitives.Min(primitive => primitive.Perimeter <= 0 ? 0 : primitive.Area / (primitive.Perimeter * thickness));

    private static double MinimumAspectRatio(ComponentPastePattern pattern, double thickness) =>
        thickness <= 0 || pattern.PastePrimitives.Count == 0 ? 0 : pattern.PastePrimitives.Min(primitive => Math.Min(primitive.Width, primitive.Height) / thickness);

    private static bool IsSide(string componentSide, string expectedSide) =>
        expectedSide.Equals("Top", StringComparison.OrdinalIgnoreCase)
            ? !componentSide.Equals("Bottom", StringComparison.OrdinalIgnoreCase)
            : componentSide.Equals("Bottom", StringComparison.OrdinalIgnoreCase);
}
