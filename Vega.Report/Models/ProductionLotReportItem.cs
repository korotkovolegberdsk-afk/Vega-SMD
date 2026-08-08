namespace Vega.Report.Models;

public class ProductionLotReportItem
{
    public string OrderNumber { get; init; } = "";
    public string Customer { get; init; } = "";
    public string BoardName { get; init; } = "";
    public string BoardRevision { get; init; } = "";
    public string StencilRevision { get; init; } = "";
    public string Paste { get; init; } = "";
    public string ReflowProfile { get; init; } = "";
    public IReadOnlyList<string> Equipment { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Defects { get; init; } = Array.Empty<string>();
    public double Yield { get; init; }
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}