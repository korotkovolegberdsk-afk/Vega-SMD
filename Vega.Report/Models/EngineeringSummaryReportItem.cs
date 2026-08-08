namespace Vega.Report.Models;

public class EngineeringSummaryReportItem
{
    public string ProjectOverview { get; init; } = "";
    public string StencilSummary { get; init; } = "";
    public IReadOnlyList<string> TechnologyDecisions { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, double> QualityKpis { get; init; } = new Dictionary<string, double>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}