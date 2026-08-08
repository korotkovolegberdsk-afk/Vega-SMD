using Vega.DefectDictionary.Data;
using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;
using Vega.QualityAnalytics.Data;
using Vega.QualityAnalytics.Models;
using Vega.StencilHistory.Models;

namespace Vega.QualityAnalytics;

public class QualityAnalyticsService
{
    private readonly QualityAnalyticsRepository _metrics;
    private readonly ProcessLearningRepository _processLearning;
    private readonly DefectDictionaryRepository? _dictionary;

    public QualityAnalyticsService(QualityAnalyticsRepository? metrics = null, ProcessLearningRepository? processLearning = null, DefectDictionaryRepository? dictionary = null)
    {
        _metrics = metrics ?? new QualityAnalyticsRepository();
        _processLearning = processLearning ?? new ProcessLearningRepository();
        _dictionary = dictionary;
    }

    public QualityAnalysisResult AnalyzeLot(int productionLotId, int totalBoards, int firstPassBoards, int totalComponents)
    {
        if (totalBoards <= 0) throw new ArgumentOutOfRangeException(nameof(totalBoards));
        if (firstPassBoards < 0 || firstPassBoards > totalBoards) throw new ArgumentOutOfRangeException(nameof(firstPassBoards));
        var defects = _processLearning.GetDefectsByProductionLot(productionLotId);
        var defectCount = defects.Sum(defect => defect.Quantity);
        var defectBoards = Math.Min(totalBoards, defects.Select(defect => defect.ComponentRef).Where(reference => !string.IsNullOrWhiteSpace(reference)).Distinct().Count());
        var result = new QualityAnalysisResult
        {
            ProductionLotId = productionLotId, TotalBoards = totalBoards, GoodBoards = Math.Max(0, totalBoards - defectBoards), DefectBoards = defectBoards,
            Yield = CalculateYield(totalBoards - defectBoards, totalBoards), FPY = CalculateFPY(firstPassBoards, totalBoards), DefectRate = CalculateDefectRate(defectCount, totalBoards), PPM = CalculatePpm(defectCount, totalComponents),
            TopDefects = BuildPareto(productionLotId, defects), Recommendations = BuildRecommendations(defects)
        };
        SaveMetrics(result);
        return result;
    }

    public double CalculateYield(int goodBoards, int totalBoards) => Percentage(goodBoards, totalBoards);
    public double CalculateFPY(int firstPassBoards, int totalBoards) => Percentage(firstPassBoards, totalBoards);
    public double CalculateDefectRate(int defects, int totalBoards) => Percentage(defects, totalBoards);
    public double CalculatePpm(int defects, int totalComponents) => totalComponents <= 0 ? 0 : defects / (double)totalComponents * 1_000_000;

    public IReadOnlyList<DefectStatistic> BuildPareto(int productionLotId, IEnumerable<ProcessDefectRecord> defects)
    {
        var defectList = defects.ToList(); var total = defectList.Sum(defect => defect.Quantity);
        return defectList.GroupBy(defect => new { defect.DefectDefinitionId, defect.DefectType, defect.Severity })
            .Select(group => new DefectStatistic { ProductionLotId = productionLotId, DefectDefinitionId = group.Key.DefectDefinitionId, Count = group.Sum(item => item.Quantity), Percentage = Percentage(group.Sum(item => item.Quantity), total), Severity = group.Key.Severity, Trend = QualityTrend.Stable, DefectName = GetDefectName(group.Key.DefectDefinitionId, group.Key.DefectType) })
            .OrderByDescending(item => item.Count).ThenBy(item => item.DefectName).ToList();
    }

    public StencilRevisionComparison CompareRevisions(string oldRevision, double defectBefore, string newRevision, double defectAfter)
    {
        if (defectBefore < 0 || defectAfter < 0) throw new ArgumentOutOfRangeException(nameof(defectBefore));
        var improvement = defectBefore == 0 ? 0 : (defectBefore - defectAfter) / defectBefore * 100;
        var conclusion = improvement > 0 ? "Quality improved after stencil revision." : improvement < 0 ? "Quality worsened after stencil revision." : "No measurable quality change.";
        return new StencilRevisionComparison { OldRevision = oldRevision, NewRevision = newRevision, DefectBefore = defectBefore, DefectAfter = defectAfter, ImprovementPercent = improvement, Conclusion = conclusion };
    }

    public StencilRevisionComparison CompareRevisions(StencilRevision oldRevision, double defectBefore, StencilRevision newRevision, double defectAfter)
    {
        ArgumentNullException.ThrowIfNull(oldRevision);
        ArgumentNullException.ThrowIfNull(newRevision);
        return CompareRevisions(oldRevision.Revision, defectBefore, newRevision.Revision, defectAfter);
    }
    public ProcessExperienceRecord CreateImprovementExperience(int packageId, ProcessDefectType defectType, string previousStrategy, string newStrategy, string beforeParameters, string afterParameters, double confidence = 0.8)
    {
        var experience = new ProcessExperienceRecord { PackageId = packageId, DefectType = defectType, PreviousStrategy = previousStrategy, NewStrategy = newStrategy, BeforeParameters = beforeParameters, AfterParameters = afterParameters, Result = ProcessExperienceResult.Improved, Confidence = confidence };
        _processLearning.AddExperience(experience);
        return experience;
    }

    public QualityDashboardData CreateDashboardData(QualityAnalysisResult analysis, IEnumerable<StencilRevisionComparison>? comparisons = null)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return new QualityDashboardData
        {
            KPIs = new Dictionary<string, double> { ["Yield"] = analysis.Yield, ["FPY"] = analysis.FPY, ["DefectRate"] = analysis.DefectRate, ["PPM"] = analysis.PPM },
            ParetoChart = analysis.TopDefects, YieldTrend = _metrics.GetMetrics(analysis.ProductionLotId).Where(metric => metric.MetricType == QualityMetricType.Yield).ToList(),
            RevisionComparisons = comparisons?.ToList() ?? (IReadOnlyList<StencilRevisionComparison>)Array.Empty<StencilRevisionComparison>(),
            Tables = new Dictionary<string, IReadOnlyList<object>> { ["DefectPareto"] = analysis.TopDefects.Cast<object>().ToList() }
        };
    }

    private void SaveMetrics(QualityAnalysisResult result)
    {
        var date = DateTime.UtcNow;
        foreach (var metric in new[] { new QualityMetric { ProductionLotId = result.ProductionLotId, MetricType = QualityMetricType.Yield, Value = result.Yield, Unit = "%", Date = date }, new QualityMetric { ProductionLotId = result.ProductionLotId, MetricType = QualityMetricType.FPY, Value = result.FPY, Unit = "%", Date = date }, new QualityMetric { ProductionLotId = result.ProductionLotId, MetricType = QualityMetricType.DefectRate, Value = result.DefectRate, Unit = "%", Date = date }, new QualityMetric { ProductionLotId = result.ProductionLotId, MetricType = QualityMetricType.PPM, Value = result.PPM, Unit = "ppm", Date = date }, new QualityMetric { ProductionLotId = result.ProductionLotId, MetricType = QualityMetricType.DefectCount, Value = result.TopDefects.Sum(item => item.Count), Unit = "count", Date = date } }) _metrics.AddMetric(metric);
    }
    private string GetDefectName(int? id, ProcessDefectType type) => id is not null && _dictionary?.GetAll().FirstOrDefault(item => item.Id == id.Value) is { } definition ? definition.EnglishName : type.ToString();
    private static IReadOnlyList<string> BuildRecommendations(IEnumerable<ProcessDefectRecord> defects) => defects.GroupBy(defect => defect.DefectType).OrderByDescending(group => group.Sum(item => item.Quantity)).Select(group => $"Investigate {group.Key} occurrences and validate the related stencil and process settings.").ToList();
    private static double Percentage(int value, int total) => total <= 0 ? 0 : value / (double)total * 100;
}