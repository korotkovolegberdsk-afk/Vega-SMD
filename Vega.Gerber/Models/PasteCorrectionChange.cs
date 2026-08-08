namespace Vega.Gerber.Models;

public class PasteCorrectionChange
{
    public string RefDes { get; init; } = "";
    public string ChangeType { get; init; } = "";
    public string OriginalGeometry { get; init; } = "";
    public string NewGeometry { get; init; } = "";
    public string Reason { get; init; } = "";
}
