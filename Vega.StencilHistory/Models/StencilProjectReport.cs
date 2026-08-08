namespace Vega.StencilHistory.Models;

public class StencilProjectReport
{
    public string ProjectName { get; init; } = "";
    public string CustomerName { get; init; } = "";
    public string Revision { get; init; } = "";
    public string Input { get; init; } = "";
    public string Frame { get; init; } = "";
    public IReadOnlyList<string> Changes { get; init; } = Array.Empty<string>();
    public string Status { get; init; } = "";
    public string ToText() => string.Join(Environment.NewLine,
        $"PROJECT: {ProjectName}", $"CUSTOMER: {CustomerName}", $"REVISION: {Revision}",
        $"INPUT: {Input}", $"FRAME: {Frame}", "CHANGES:",
        string.Join(Environment.NewLine, Changes), $"STATUS: {Status}");
}