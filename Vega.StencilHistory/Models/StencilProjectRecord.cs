using Vega.StencilWorkflow.Models;

namespace Vega.StencilHistory.Models;

public class StencilProjectRecord
{
    public int Id { get; init; }
    public string ProjectName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string BoardName { get; set; } = "";
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public StencilWorkflowStatus Status { get; set; }
    public string InputSource { get; set; } = "";
    public IReadOnlyList<string> SourceFiles { get; set; } = Array.Empty<string>();
    public string FrameName { get; set; } = "";
    public int? ReflowProfileId { get; set; }
    public string PasteSide { get; set; } = "";
    public string Operator { get; set; } = "";
    public string Notes { get; set; } = "";
}