namespace Vega.StencilWorkflow.Models;

public class StencilManufacturingReport
{
    public string ProjectName { get; init; } = "";
    public string InputType { get; init; } = "";
    public string FrameName { get; init; } = "";
    public string PasteSide { get; init; } = "";
    public double StencilThickness { get; init; }
    public int ComponentsCount { get; init; }
    public int ModifiedApertures { get; init; }
    public int WindowPaneCount { get; init; }
    public int HomePlateCount { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public StencilWorkflowStatus Status { get; init; }
}