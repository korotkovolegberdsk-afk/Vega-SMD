using Vega.EngineeringDashboard.Models;
using Vega.ProcessLearning.Models;
using Vega.ProductionTracking.Models;
using Vega.QualityAnalytics.Models;
using Vega.StencilHistory.Models;
using Vega.TechnologyDecision.Models;

namespace Vega.EngineeringDashboard;

public class EngineeringDashboardService
{
    public EngineeringDashboardData LoadProjectDashboard(StencilProjectRecord project, StencilRevision? revision = null, QualityAnalysisResult? quality = null, IReadOnlyList<TechnologyDecisionResult>? technologyDecisions = null, ProcessLearningReport? learning = null, ProductionLotReport? production = null, int componentCount = 0, int apertureCount = 0, double stencilThickness = 0.12)
    {
        ArgumentNullException.ThrowIfNull(project);
        var technology = GetTechnologySummary(technologyDecisions);
        var qualitySummary = GetQualitySummary(quality);
        var productionSummary = GetProductionSummary(production);
        var warnings = new List<DashboardWarning>(); warnings.AddRange(technology.Warnings);
        if (revision is null) warnings.Add(new DashboardWarning { Severity = DashboardWarningSeverity.Critical, Category = DashboardWarningCategory.Stencil, Message = "Missing Stencil Revision.", Source = "StencilHistory" });
        else if (revision.WarningsCount > 0) warnings.Add(new DashboardWarning { Severity = DashboardWarningSeverity.Warning, Category = DashboardWarningCategory.Stencil, Message = $"Stencil revision has {revision.WarningsCount} warnings.", Source = revision.Revision });
        if (quality is not null && quality.TopDefects.FirstOrDefault() is { Percentage: >= 30 } defect) warnings.Add(new DashboardWarning { Severity = DashboardWarningSeverity.Warning, Category = DashboardWarningCategory.Quality, Message = $"High {defect.DefectName} rate ({defect.Percentage:0.##}%).", Source = "QualityAnalytics" });
        if (project.ReflowProfileId is null) warnings.Add(new DashboardWarning { Severity = DashboardWarningSeverity.Warning, Category = DashboardWarningCategory.Reflow, Message = "Reflow profile is not assigned.", Source = "StencilHistory" });
        var recommendations = GetRecommendations(technologyDecisions, learning, quality);
        return new EngineeringDashboardData
        {
            ProjectName = string.IsNullOrWhiteSpace(project.BoardName) ? project.ProjectName : project.BoardName, BoardRevision = production?.Lot.BoardRevision ?? "", Customer = project.CustomerName,
            StencilRevision = revision?.Revision ?? "Not assigned", StencilStatus = revision is null ? "Missing" : project.Status.ToString(), TechnologyStatus = technology.Status,
            WarningCount = warnings.Count(item => item.Severity == DashboardWarningSeverity.Warning), ErrorCount = warnings.Count(item => item.Severity == DashboardWarningSeverity.Critical), ComponentCount = componentCount, ApertureCount = apertureCount, ChangeCount = revision?.ChangesCount ?? 0,
            Yield = qualitySummary.Yield, FPY = qualitySummary.FPY, MainDefects = qualitySummary.Pareto.Select(item => $"{item.DefectName}: {item.Percentage:0.##}%").ToList(), Recommendations = recommendations,
            ProjectDate = project.CreatedDate, Frame = revision?.FrameName ?? project.FrameName, StencilThickness = stencilThickness, OriginalPaste = revision?.OriginalPasteFile ?? "", CorrectedPaste = revision?.CorrectedPasteFile ?? "",
            TechnologyDecisions = technology.Decisions, ConfirmedImprovements = learning?.ImprovedDecisions.Select(item => $"{item.PreviousStrategy} → {item.NewStrategy}").ToList() ?? (IReadOnlyList<string>)Array.Empty<string>(),
            FailedDecisions = learning?.PreviousDecisions.Where(item => item.Result == ProcessExperienceResult.Worse).Select(item => $"{item.PreviousStrategy} → {item.NewStrategy}").ToList() ?? (IReadOnlyList<string>)Array.Empty<string>(), Warnings = warnings
        };
    }

    public TechnologyDashboardSummary GetTechnologySummary(IReadOnlyList<TechnologyDecisionResult>? decisions)
    {
        var list = decisions ?? Array.Empty<TechnologyDecisionResult>();
        var warnings = list.SelectMany(item => item.Warnings.Select(message => new DashboardWarning { Severity = DashboardWarningSeverity.Warning, Category = DashboardWarningCategory.Technology, Message = message, Source = "TechnologyDecision" })).ToList();
        var formatted = list.Select(item => $"{item.SelectedShape}; goal: {item.SelectedStrategy}; confidence: {item.Confidence:0.##}; {item.Reason}").ToList();
        return new TechnologyDashboardSummary { Status = list.Count == 0 ? "No technology decisions" : warnings.Count == 0 ? "Ready" : "Warnings", Decisions = formatted, Warnings = warnings };
    }

    public QualityDashboardSummary GetQualitySummary(QualityAnalysisResult? quality) => quality is null ? new QualityDashboardSummary() : new QualityDashboardSummary { Yield = quality.Yield, FPY = quality.FPY, PPM = quality.PPM, Pareto = quality.TopDefects };

    public ProductionDashboardSummary GetProductionSummary(ProductionLotReport? production) => production is null ? new ProductionDashboardSummary { Summary = "Production lot is not assigned." } : new ProductionDashboardSummary { Summary = $"{production.Lot.OrderNumber}; {production.Paste}; {production.Reflow}", Equipment = production.Equipment.Select(item => $"{item.EquipmentType}: {item.Manufacturer} {item.Model}").ToList(), Defects = production.Defects };

    public IReadOnlyList<EngineeringRecommendation> GetRecommendations(IReadOnlyList<TechnologyDecisionResult>? technologyDecisions, ProcessLearningReport? learning, QualityAnalysisResult? quality)
    {
        var result = new List<EngineeringRecommendation>();
        foreach (var experience in learning?.ImprovedDecisions ?? Array.Empty<ProcessExperienceRecord>()) result.Add(new EngineeringRecommendation { Type = "Process Learning", Component = learning?.Package ?? "", CurrentState = experience.PreviousStrategy, Recommendation = experience.NewStrategy, Reason = "Confirmed production improvement.", Confidence = experience.Confidence >= .8 ? "High" : "Medium" });
        foreach (var decision in technologyDecisions ?? Array.Empty<TechnologyDecisionResult>()) result.Add(new EngineeringRecommendation { Type = "Technology", Component = "", CurrentState = decision.SelectedStrategy.ToString(), Recommendation = decision.SelectedShape.ToString(), Reason = decision.Reason, Confidence = decision.Confidence >= .8 ? "High" : "Medium" });
        foreach (var recommendation in quality?.Recommendations ?? (IReadOnlyList<string>)Array.Empty<string>()) result.Add(new EngineeringRecommendation { Type = "Quality", Recommendation = recommendation, Reason = "Quality analytics recommendation.", Confidence = "Medium" });
        return result;
    }

    public EngineeringDashboardExport CreateExport(EngineeringDashboardData dashboard, QualityDashboardData? quality = null, IEnumerable<string>? images = null)
    {
        ArgumentNullException.ThrowIfNull(dashboard);
        return new EngineeringDashboardExport
        {
            KPIs = new Dictionary<string, double> { ["Yield"] = dashboard.Yield, ["FPY"] = dashboard.FPY, ["Warnings"] = dashboard.WarningCount, ["Errors"] = dashboard.ErrorCount },
            Charts = new Dictionary<string, IReadOnlyList<object>> { ["DefectPareto"] = quality?.ParetoChart.Cast<object>().ToList() ?? (IReadOnlyList<object>)Array.Empty<object>(), ["YieldTrend"] = quality?.YieldTrend.Cast<object>().ToList() ?? (IReadOnlyList<object>)Array.Empty<object>() },
            Tables = new Dictionary<string, IReadOnlyList<object>> { ["TechnologyDecisions"] = dashboard.TechnologyDecisions.Cast<object>().ToList(), ["Recommendations"] = dashboard.Recommendations.Cast<object>().ToList() },
            Images = images?.ToList() ?? (IReadOnlyList<string>)Array.Empty<string>(), Warnings = dashboard.Warnings
        };
    }
}