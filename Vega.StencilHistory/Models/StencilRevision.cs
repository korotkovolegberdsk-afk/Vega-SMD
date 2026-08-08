namespace Vega.StencilHistory.Models;

public class StencilRevision
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public string Revision { get; set; } = "";
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public string Description { get; set; } = "";
    public string OriginalPasteFile { get; set; } = "";
    public string CorrectedPasteFile { get; set; } = "";
    public string MarkingFile { get; set; } = "";
    public string ReportFile { get; set; } = "";
    public int ChangesCount { get; set; }
    public int WarningsCount { get; set; }
    public string FrameName { get; set; } = "";
}