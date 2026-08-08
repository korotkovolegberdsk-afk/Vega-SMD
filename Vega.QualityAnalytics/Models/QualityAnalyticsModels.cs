using Vega.ProcessLearning.Models;

namespace Vega.QualityAnalytics.Models;

public enum QualityMetricType { Yield, FPY, DefectRate, PPM, DefectCount, ReworkRate }
public enum QualityTrend { Improving, Stable, Worsening }

public class QualityMetric
{
    public int Id { get; set; }
    public int ProductionLotId { get; set; }
    public QualityMetricType MetricType { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class DefectStatistic
{
    public int? DefectDefinitionId { get; set; }
    public int ProductionLotId { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
    public ProcessDefectSeverity Severity { get; set; }
    public QualityTrend Trend { get; set; } = QualityTrend.Stable;
    public string DefectName { get; set; } = "";
}

public class QualityAnalysisResult
{
    public int ProductionLotId { get; init; }
    public int TotalBoards { get; init; }
    public int GoodBoards { get; init; }
    public int DefectBoards { get; init; }
    public double Yield { get; init; }
    public double FPY { get; init; }
    public double DefectRate { get; init; }
    public double PPM { get; init; }
    public IReadOnlyList<DefectStatistic> TopDefects { get; init; } = Array.Empty<DefectStatistic>();
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}

public class StencilRevisionComparison
{
    public string OldRevision { get; init; } = "";
    public string NewRevision { get; init; } = "";
    public double DefectBefore { get; init; }
    public double DefectAfter { get; init; }
    public double ImprovementPercent { get; init; }
    public string Conclusion { get; init; } = "";
}

public class QualityDashboardData
{
    public IReadOnlyDictionary<string, double> KPIs { get; init; } = new Dictionary<string, double>();
    public IReadOnlyList<DefectStatistic> ParetoChart { get; init; } = Array.Empty<DefectStatistic>();
    public IReadOnlyList<QualityMetric> YieldTrend { get; init; } = Array.Empty<QualityMetric>();
    public IReadOnlyList<StencilRevisionComparison> RevisionComparisons { get; init; } = Array.Empty<StencilRevisionComparison>();
    public IReadOnlyDictionary<string, IReadOnlyList<object>> Tables { get; init; } = new Dictionary<string, IReadOnlyList<object>>();
}