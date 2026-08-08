using Vega.Gerber.Models;
using Vega.Report;
using Vega.StencilCAM.Models;
using Vega.StencilInput.Models;
using Vega.StencilWorkflow.Models;
using Xunit;

namespace Vega.Tests;

public class StencilReportGeneratorServiceTests
{
    [Fact]
    public void CreateReport_CollectsTechnicalDataAndApertureChanges()
    {
        using var output = new ReportOutput();
        var report = new StencilReportGeneratorService().CreateReport(Project(), previewImages: [output.ImageFile]);

        Assert.Equal("Controller Board", report.ProjectName);
        Assert.Equal(2, report.ApertureCount);
        Assert.Equal(1, report.ModifiedApertures);
        var change = Assert.Single(report.ApertureChanges);
        Assert.Equal("R0603", change.RefDes);
        Assert.Equal("Rectangle", change.BeforeShape);
        Assert.Equal("Snubnose", change.AfterShape);
        Assert.Single(report.PreviewImages);
    }

    [Fact]
    public void GenerateTXT_CreatesReadableManufacturingReport()
    {
        using var output = new ReportOutput();
        var generator = new StencilReportGeneratorService();
        var file = generator.GenerateTXT(generator.CreateReport(Project()), Path.Combine(output.Directory, "report.txt"));
        var text = File.ReadAllText(file);

        Assert.True(File.Exists(file));
        Assert.Contains("STENCIL MANUFACTURING REPORT", text, StringComparison.Ordinal);
        Assert.Contains("Project: Controller Board", text, StringComparison.Ordinal);
        Assert.Contains("Modified: 1", text, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateHTML_CreatesTablesAndPreviewSection()
    {
        using var output = new ReportOutput();
        var generator = new StencilReportGeneratorService();
        var file = generator.GenerateHTML(generator.CreateReport(Project(), previewImages: [output.ImageFile]), Path.Combine(output.Directory, "report.html"));
        var html = File.ReadAllText(file);

        Assert.True(File.Exists(file));
        Assert.Contains("Changed apertures", html, StringComparison.Ordinal);
        Assert.Contains("R0603", html, StringComparison.Ordinal);
        Assert.Contains("Preview", html, StringComparison.Ordinal);
    }

    [Fact]
    public void GeneratePDF_CreatesPdfDocument()
    {
        using var output = new ReportOutput();
        var generator = new StencilReportGeneratorService();
        var file = generator.GeneratePDF(generator.CreateReport(Project()), Path.Combine(output.Directory, "report.pdf"));

        Assert.True(File.Exists(file));
        Assert.True(new FileInfo(file).Length > 0);
        Assert.StartsWith("%PDF", File.ReadAllText(file)[..4], StringComparison.Ordinal);
    }

    [Fact]
    public void ExportCanva_CreatesJsonCsvAndCopiesImages()
    {
        using var output = new ReportOutput();
        var generator = new StencilReportGeneratorService();
        var result = generator.ExportCanva(generator.CreateReport(Project(), previewImages: [output.ImageFile]), Path.Combine(output.Directory, "canva"));

        Assert.True(File.Exists(result.JsonFile));
        Assert.True(File.Exists(result.CsvFile));
        Assert.Single(result.ImageFiles);
        Assert.Contains("R0603", File.ReadAllText(result.CsvFile), StringComparison.Ordinal);
    }

    private static StencilManufacturingProject Project()
    {
        var input = new StencilInputProject { ProjectName = "Controller Board", SourceType = StencilInputSourceType.AltiumProject };
        input.SourceFiles.Add("Controller.PcbDoc");
        input.Components.Add(new Vega.CAD.Models.Component { RefDes = "R1" });
        return new StencilManufacturingProject
        {
            ProjectName = "Controller Board", InputProject = input,
            Frame = new StencilFrame { Name = "LPKF_DEFAULT_FRAME", StencilWidth = 400, StencilHeight = 500 },
            OriginalPaste = new PasteLayer { Side = "Top" },
            AnalysisResult = new PasteAnalysisResult { ApertureCount = 2, PrimitiveCount = 2, WarningCount = 1 },
            CorrectedPaste = new CorrectedPasteLayer
            {
                Side = "Top", OriginalPrimitiveCount = 2, CorrectedPrimitiveCount = 2,
                Changes = [new PasteCorrectionChange { RefDes = "R0603", ChangeType = "ApertureResize", OriginalGeometry = "Rectangle 0.90 x 0.45", NewGeometry = "Snubnose 0.81 x 0.405", Reason = "Solder ball prevention" }]
            },
            OutputFiles = ["Controller_PASTE_TOP_V001.GTP", "Controller_MARKING_V001.GBR", "Controller_REPORT.txt"],
            Status = StencilWorkflowStatus.Generated
        };
    }

    private sealed class ReportOutput : IDisposable
    {
        public string Directory { get; } = Path.Combine(Path.GetTempPath(), "VegaReportTests", Guid.NewGuid().ToString("N"));
        public string ImageFile => Path.Combine(Directory, "preview.png");
        public ReportOutput()
        {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllBytes(ImageFile, Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8JwAAAABJRU5ErkJggg=="));
        }
        public void Dispose() => System.IO.Directory.Delete(Directory, true);
    }
}