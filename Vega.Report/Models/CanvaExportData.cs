namespace Vega.Report.Models;

public class CanvaExportData
{
    public string JsonFile { get; init; } = "";
    public string CsvFile { get; init; } = "";
    public IReadOnlyList<string> ImageFiles { get; init; } = Array.Empty<string>();
}