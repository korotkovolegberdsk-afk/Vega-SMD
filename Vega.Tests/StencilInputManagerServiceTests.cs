using Vega.CAD.Models;
using Vega.StencilCAM.Models;
using Vega.StencilInput;
using Vega.StencilInput.Models;
using Xunit;

namespace Vega.Tests;

public class StencilInputManagerServiceTests
{
    private readonly StencilInputManagerService _service = new();

    [Fact]
    public void LoadPasteOnlyProject_CreatesUnifiedInputProject()
    {
        var project = _service.LoadPasteOnlyProject(TestData("paste-top.gtp"), null);

        Assert.Equal(StencilInputSourceType.PasteOnly, project.SourceType);
        Assert.Single(project.PasteLayers);
        Assert.NotNull(project.BoardOutline);
        Assert.True(project.BoardOutline!.Width > 0);
        Assert.Equal(StencilInputSourceType.PasteOnly, _service.DetectInputType(project.SourceFiles));
    }

    [Fact]
    public void LoadAltiumProject_ProvidesPcbProjectAndComponents()
    {
        var project = _service.LoadAltiumProject(TestData("bin_s_0_1a.PcbDoc"));

        Assert.Equal(StencilInputSourceType.AltiumProject, project.SourceType);
        Assert.NotNull(project.PcbProject);
        Assert.Equal(2, project.Components.Count);
        Assert.Equal(PcbSourceType.AltiumPcbDoc, project.PcbProject!.SourceType);
        Assert.NotNull(project.BoardOutline);
    }

    [Fact]
    public void LoadPanelProject_LoadsPasteLayersAndPanelBounds()
    {
        var project = _service.LoadPanelProject([TestData("paste-top.gtp"), TestData("paste-bottom.gbs")]);

        Assert.Equal(StencilInputSourceType.PanelGerber, project.SourceType);
        Assert.Equal(2, project.PasteLayers.Count);
        Assert.NotNull(project.BoardOutline);
    }

    [Fact]
    public void Validate_ReportsInsufficientInputData()
    {
        var project = new StencilInputProject { ProjectName = "Empty", SourceType = StencilInputSourceType.Manual };

        var validation = _service.Validate(project, null, null);

        Assert.False(validation.IsValid);
        Assert.False(validation.HasPasteLayer);
        Assert.False(validation.HasBoardBounds);
        Assert.Contains(validation.Errors, error => error.Contains("Paste layer", StringComparison.Ordinal));
    }

    private static string TestData(string fileName) => Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
}
