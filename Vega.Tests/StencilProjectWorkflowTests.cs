using Vega.StencilCAM.Models;
using Vega.StencilProjects;
using Vega.StencilProjects.Models;
using Xunit;

namespace Vega.Tests;

public class StencilProjectWorkflowTests : IDisposable
{
    private readonly string _output = Path.Combine(Path.GetTempPath(), "VegaStencilProject", Guid.NewGuid().ToString("N"));

    [Fact]
    public void PasteGerber_ProjectCanBeLoadedAnalyzedPreviewedAndReported()
    {
        var service = new StencilProjectService();
        var frame = new StencilFrame { Id = 1, Name = "TEST_FRAME", FrameWidth = 400, FrameHeight = 500, StencilWidth = 400, StencilHeight = 500, IsActive = true };
        var session = service.Create("Demo_R0603_QFN_QFP", "Demo Customer", StencilProjectInputSource.PasteGerber, StencilProjectPasteSide.Top, frame);
        var source = Path.Combine(AppContext.BaseDirectory, "TestData", "test-paste.gtp");
        service.LoadPasteGerber(session, source, null);

        var analysis = service.Analyze(session);
        service.CreatePreview(session);
        var reports = service.ExportReports(session, _output);

        Assert.True(analysis.ApertureCount > 0);
        Assert.NotNull(session.Preview);
        Assert.NotNull(session.Preview!.OriginalPasteLayer);
        Assert.NotNull(session.Preview.CorrectedPasteLayer);
        Assert.Equal(3, reports.Count);
        Assert.All(reports, path => Assert.True(File.Exists(path)));
    }

    public void Dispose() { if (Directory.Exists(_output)) Directory.Delete(_output, true); }
}