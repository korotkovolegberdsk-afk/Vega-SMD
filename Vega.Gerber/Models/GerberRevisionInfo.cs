namespace Vega.Gerber.Models;

public class GerberRevisionInfo
{
    public string ProjectName { get; init; } = "";
    public string OriginalFile { get; init; } = "";
    public string GeneratedFile { get; init; } = "";
    public string Revision { get; init; } = "";
    public DateTime CreatedDate { get; init; }
    public string SoftwareVersion { get; init; } = "";
    public int ChangesCount { get; init; }
}
