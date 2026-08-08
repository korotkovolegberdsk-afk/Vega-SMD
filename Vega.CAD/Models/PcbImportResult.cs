namespace Vega.CAD.Models;

public class PcbImportResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
    public PcbProject? Project { get; set; }
}
