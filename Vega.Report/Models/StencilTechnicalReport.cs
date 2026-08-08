using Vega.StencilWorkflow.Models;

namespace Vega.Report.Models;

public class StencilTechnicalReport
{
    public string ProjectName { get; init; } = "";
    public string CustomerName { get; init; } = "";
    public string BoardName { get; init; } = "";
    public string Revision { get; init; } = "";
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public string InputSource { get; init; } = "";
    public IReadOnlyList<string> InputFiles { get; init; } = Array.Empty<string>();
    public string FrameName { get; init; } = "";
    public string StencilSize { get; init; } = "";
    public string PasteSide { get; init; } = "";
    public double StencilThickness { get; init; }
    public int ComponentCount { get; init; }
    public int ApertureCount { get; init; }
    public int ModifiedApertures { get; init; }
    public int WindowPaneCount { get; init; }
    public int HomePlateCount { get; init; }
    public int SnubnoseCount { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ApertureChangeReport> ApertureChanges { get; init; } = Array.Empty<ApertureChangeReport>();
    public IReadOnlyList<string> GerberFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ReportFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PreviewImages { get; init; } = Array.Empty<string>();
    public StencilWorkflowStatus Status { get; init; }
}