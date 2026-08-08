using System.Net;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vega.Report.Models;
using Vega.StencilHistory.Models;
using Vega.StencilWorkflow.Models;

namespace Vega.Report;

public class StencilReportGeneratorService
{
    static StencilReportGeneratorService() => QuestPDF.Settings.License = LicenseType.Community;

    public StencilTechnicalReport CreateReport(
        StencilManufacturingProject project,
        StencilProjectRecord? historyProject = null,
        StencilRevision? revision = null,
        IEnumerable<string>? previewImages = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        var changes = (project.CorrectedPaste?.Changes ?? Array.Empty<Vega.Gerber.Models.PasteCorrectionChange>())
            .Select(change => ToChangeReport(change.RefDes, change.OriginalGeometry, change.NewGeometry, change.Reason)).ToList();
        var outputFiles = project.OutputFiles ?? Array.Empty<string>();
        var warnings = Enumerable.Range(1, project.AnalysisResult?.WarningCount ?? 0).Select(index => $"Paste analysis warning {index}.").ToList();
        var errors = project.Status == StencilWorkflowStatus.Error ? new[] { "Workflow finished with an error." } : Array.Empty<string>();
        return new StencilTechnicalReport
        {
            ProjectName = project.ProjectName, CustomerName = historyProject?.CustomerName ?? "", BoardName = historyProject?.BoardName ?? project.ProjectName,
            Revision = revision?.Revision ?? "V001", CreatedDate = revision?.CreatedDate ?? DateTime.UtcNow,
            InputSource = project.InputProject?.SourceType.ToString() ?? historyProject?.InputSource ?? "",
            InputFiles = project.InputProject?.SourceFiles ?? historyProject?.SourceFiles ?? Array.Empty<string>(),
            FrameName = project.Frame?.Name ?? historyProject?.FrameName ?? revision?.FrameName ?? "",
            StencilSize = project.Frame is null ? "" : $"{project.Frame.StencilWidth:0.##} x {project.Frame.StencilHeight:0.##} mm",
            PasteSide = project.CorrectedPaste?.Side ?? project.OriginalPaste?.Side ?? historyProject?.PasteSide ?? "",
            StencilThickness = 0.12, ComponentCount = project.InputProject?.Components.Count ?? 0,
            ApertureCount = project.AnalysisResult?.ApertureCount ?? project.OriginalPaste?.Primitives.Count ?? 0,
            ModifiedApertures = changes.Count,
            WindowPaneCount = changes.Count(change => change.AfterShape.Contains("WindowPane", StringComparison.OrdinalIgnoreCase)),
            HomePlateCount = changes.Count(change => change.AfterShape.Contains("HomePlate", StringComparison.OrdinalIgnoreCase)),
            SnubnoseCount = changes.Count(change => change.AfterShape.Contains("Snubnose", StringComparison.OrdinalIgnoreCase)),
            Warnings = warnings, Errors = errors, ApertureChanges = changes,
            GerberFiles = outputFiles.Where(IsGerberFile).ToList(), ReportFiles = outputFiles.Where(IsReportFile).ToList(),
            PreviewImages = (IReadOnlyList<string>?)previewImages?.Where(File.Exists).ToList() ?? Array.Empty<string>(), Status = project.Status
        };
    }

    public string GenerateTXT(StencilTechnicalReport report, string outputFile)
    {
        ArgumentNullException.ThrowIfNull(report);
        PrepareOutput(outputFile);
        var text = new StringBuilder();
        text.AppendLine("STENCIL MANUFACTURING REPORT");
        text.AppendLine();
        text.AppendLine($"Project: {report.ProjectName}"); text.AppendLine($"Customer: {report.CustomerName}"); text.AppendLine($"Revision: {report.Revision}");
        text.AppendLine($"Input: {report.InputSource}"); text.AppendLine($"Frame: {report.FrameName}");
        text.AppendLine($"Stencil: {report.StencilThickness * 1000:0} um"); text.AppendLine($"Paste: {report.PasteSide}");
        text.AppendLine(); text.AppendLine("ANALYSIS:"); text.AppendLine($"Components: {report.ComponentCount}");
        text.AppendLine($"Apertures: {report.ApertureCount}"); text.AppendLine($"Modified: {report.ModifiedApertures}");
        text.AppendLine($"WindowPane: {report.WindowPaneCount}"); text.AppendLine($"HomePlate: {report.HomePlateCount}"); text.AppendLine($"Snubnose: {report.SnubnoseCount}");
        text.AppendLine(); text.AppendLine("CHANGES:");
        foreach (var group in report.ApertureChanges.GroupBy(change => change.AfterShape)) text.AppendLine($"{group.Key}: {group.Count()}");
        if (report.ApertureChanges.Count == 0) text.AppendLine("No aperture changes.");
        text.AppendLine(); text.AppendLine("OUTPUT FILES:");
        foreach (var file in report.GerberFiles.Concat(report.ReportFiles)) text.AppendLine(file);
        text.AppendLine(); text.AppendLine($"STATUS: {report.Status}");
        File.WriteAllText(outputFile, text.ToString());
        return outputFile;
    }

    public string GenerateHTML(StencilTechnicalReport report, string outputFile)
    {
        ArgumentNullException.ThrowIfNull(report);
        PrepareOutput(outputFile);
        var html = new StringBuilder();
        html.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>Stencil Manufacturing Report</title>");
        html.Append("<style>body{font-family:Segoe UI,Arial;margin:36px;color:#1f2937}h1{color:#13233d}table{border-collapse:collapse;width:100%;margin:12px 0}th,td{border:1px solid #cbd5e1;padding:7px;text-align:left}th{background:#e8eef7}.status{font-weight:bold}</style></head><body>");
        html.Append($"<h1>STENCIL MANUFACTURING REPORT</h1><p><b>Project:</b> {Html(report.ProjectName)}<br><b>Customer:</b> {Html(report.CustomerName)}<br><b>Revision:</b> {Html(report.Revision)}<br><b>Status:</b> <span class=\"status\">{report.Status}</span></p>");
        html.Append("<h2>General information</h2><table><tr><th>Input</th><td>").Append(Html(report.InputSource)).Append("</td></tr><tr><th>Frame</th><td>").Append(Html(report.FrameName)).Append("</td></tr><tr><th>Stencil</th><td>").Append(Html(report.StencilSize)).Append(" / ").Append(report.StencilThickness * 1000).Append(" um</td></tr><tr><th>Paste</th><td>").Append(Html(report.PasteSide)).Append("</td></tr></table>");
        html.Append("<h2>Analysis</h2><table><tr><th>Components</th><th>Apertures</th><th>Modified</th><th>WindowPane</th><th>HomePlate</th><th>Snubnose</th></tr>");
        html.Append($"<tr><td>{report.ComponentCount}</td><td>{report.ApertureCount}</td><td>{report.ModifiedApertures}</td><td>{report.WindowPaneCount}</td><td>{report.HomePlateCount}</td><td>{report.SnubnoseCount}</td></tr></table>");
        html.Append("<h2>Changed apertures</h2><table><tr><th>RefDes</th><th>Package</th><th>Before</th><th>After</th><th>Reason</th></tr>");
        foreach (var change in report.ApertureChanges) html.Append($"<tr><td>{Html(change.RefDes)}</td><td>{Html(change.Package)}</td><td>{Html(change.BeforeShape)} {Html(change.BeforeSize)}</td><td>{Html(change.AfterShape)} {Html(change.AfterSize)}</td><td>{Html(change.Reason)}</td></tr>");
        html.Append("</table><h2>Preview</h2>");
        foreach (var image in report.PreviewImages) html.Append($"<img src=\"{new Uri(Path.GetFullPath(image)).AbsoluteUri}\" style=\"max-width:100%;margin:8px 0\" alt=\"Stencil preview\">");
        if (report.PreviewImages.Count == 0) html.Append("<p>Preview image is not available.</p>");
        html.Append("<h2>Output files</h2><ul>");
        foreach (var file in report.GerberFiles.Concat(report.ReportFiles)) html.Append($"<li>{Html(file)}</li>");
        html.Append("</ul></body></html>");
        File.WriteAllText(outputFile, html.ToString());
        return outputFile;
    }

    public string GeneratePDF(StencilTechnicalReport report, string outputFile)
    {
        ArgumentNullException.ThrowIfNull(report);
        PrepareOutput(outputFile);
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4); page.Margin(32); page.DefaultTextStyle(style => style.FontSize(10));
                page.Header().Text("STENCIL MANUFACTURING REPORT").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                page.Content().Column(column =>
                {
                    column.Spacing(6);
                    column.Item().Text($"Project: {report.ProjectName}"); column.Item().Text($"Customer: {report.CustomerName}");
                    column.Item().Text($"Revision: {report.Revision}    Status: {report.Status}");
                    column.Item().Text($"Input: {report.InputSource}"); column.Item().Text($"Frame: {report.FrameName}; Stencil: {report.StencilSize}; Paste: {report.PasteSide}");
                    column.Item().PaddingTop(8).Text("ANALYSIS").Bold();
                    column.Item().Text($"Components: {report.ComponentCount}; Apertures: {report.ApertureCount}; Modified: {report.ModifiedApertures}; WindowPane: {report.WindowPaneCount}; HomePlate: {report.HomePlateCount}; Snubnose: {report.SnubnoseCount}");
                    column.Item().PaddingTop(8).Text("CHANGED APERTURES").Bold();
                    foreach (var change in report.ApertureChanges) column.Item().Text($"{change.RefDes}: {change.BeforeShape} {change.BeforeSize} -> {change.AfterShape} {change.AfterSize}; {change.Reason}");
                    column.Item().PaddingTop(8).Text("OUTPUT FILES").Bold();
                    foreach (var file in report.GerberFiles.Concat(report.ReportFiles)) column.Item().Text(file);
                    foreach (var image in report.PreviewImages.Where(File.Exists)) column.Item().PaddingTop(8).Image(image);
                });
                page.Footer().AlignCenter().Text(text => { text.Span("Vega-SMD • "); text.CurrentPageNumber(); });
            });
        }).GeneratePdf(outputFile);
        return outputFile;
    }

    public CanvaExportData ExportCanva(StencilTechnicalReport report, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(report);
        Directory.CreateDirectory(outputDirectory);
        var baseName = SafeFileName(report.ProjectName);
        var jsonFile = Path.Combine(outputDirectory, $"{baseName}_canva.json");
        var csvFile = Path.Combine(outputDirectory, $"{baseName}_aperture_changes.csv");
        File.WriteAllText(jsonFile, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        var csv = new StringBuilder("RefDes,Package,BeforeShape,AfterShape,BeforeSize,AfterSize,Reason");
        foreach (var change in report.ApertureChanges) csv.AppendLine().AppendJoin(',', Csv(change.RefDes), Csv(change.Package), Csv(change.BeforeShape), Csv(change.AfterShape), Csv(change.BeforeSize), Csv(change.AfterSize), Csv(change.Reason));
        File.WriteAllText(csvFile, csv.ToString());
        var imageDirectory = Path.Combine(outputDirectory, "Images");
        Directory.CreateDirectory(imageDirectory);
        var images = new List<string>();
        foreach (var image in report.PreviewImages.Where(File.Exists))
        {
            var destination = Path.Combine(imageDirectory, Path.GetFileName(image));
            File.Copy(image, destination, true);
            images.Add(destination);
        }
        return new CanvaExportData { JsonFile = jsonFile, CsvFile = csvFile, ImageFiles = images };
    }

    private static ApertureChangeReport ToChangeReport(string refDes, string before, string after, string reason)
    {
        var (beforeShape, beforeSize) = SplitGeometry(before);
        var (afterShape, afterSize) = SplitGeometry(after);
        return new ApertureChangeReport { RefDes = refDes, BeforeShape = beforeShape, AfterShape = afterShape, BeforeSize = beforeSize, AfterSize = afterSize, Reason = reason };
    }
    private static (string Shape, string Size) SplitGeometry(string geometry)
    {
        var parts = geometry.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? ("", "") : (parts[0], parts.Length == 1 ? "" : parts[1]);
    }
    private static bool IsGerberFile(string file) => file.EndsWith(".gtp", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".gbp", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".gbr", StringComparison.OrdinalIgnoreCase);
    private static bool IsReportFile(string file) => file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    private static string Html(string value) => WebUtility.HtmlEncode(value);
    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    private static string SafeFileName(string name) => string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
    private static void PrepareOutput(string outputFile)
    {
        if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException("Output file is required.", nameof(outputFile));
        var directory = Path.GetDirectoryName(Path.GetFullPath(outputFile));
        Directory.CreateDirectory(directory!);
    }
}