using Vega.QualityAnalytics.Models;
using Vega.Report.Models;

namespace Vega.QualityAnalytics;

public static class QualityAnalysisReportMapper
{
    public static QualityAnalysisReportItem ToReportItem(QualityAnalysisResult analysis, StencilRevisionComparison? comparison = null) => new()
    {
        ProductionLotId = analysis.ProductionLotId, Yield = analysis.Yield, FPY = analysis.FPY, DefectRate = analysis.DefectRate, PPM = analysis.PPM,
        Pareto = analysis.TopDefects.Select(item => $"{item.DefectName}: {item.Percentage:0.##}% ({item.Count})").ToList(),
        Trend = string.Join(", ", analysis.TopDefects.Select(item => $"{item.DefectName}: {item.Trend}")),
        RevisionComparison = comparison is null ? "" : $"{comparison.OldRevision} → {comparison.NewRevision}: {comparison.ImprovementPercent:0.##}%",
        Recommendations = analysis.Recommendations
    };
}