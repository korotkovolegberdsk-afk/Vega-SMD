namespace Vega.Report.Models;

public class QualityAnalysisReportItem
{
    public int ProductionLotId { get; init; }
    public double Yield { get; init; }
    public double FPY { get; init; }
    public double DefectRate { get; init; }
    public double PPM { get; init; }
    public IReadOnlyList<string> Pareto { get; init; } = Array.Empty<string>();
    public string Trend { get; init; } = "";
    public string RevisionComparison { get; init; } = "";
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}