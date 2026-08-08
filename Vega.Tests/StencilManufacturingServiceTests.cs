using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Vega.StencilInput.Models;
using Vega.StencilWorkflow;
using Vega.StencilWorkflow.Models;
using Xunit;

namespace Vega.Tests;

public class StencilManufacturingServiceTests
{
    [Fact]
    public void PasteOnly_Workflow_ExportsGeneratedGerberSet()
    {
        var service = new StencilManufacturingService();
        var project = service.CreateProject("Paste Only Board");
        var output = TemporaryDirectory();
        try
        {
            service.LoadInput(project, StencilInputSourceType.PasteOnly, [TestData("paste-top.gtp")]);
            service.AnalyzePaste(project);
            service.ApplyCorrections(project);
            service.PlaceOnFrame(project);
            service.GenerateFiducials(project);
            service.GenerateMarking(project);
            service.CreatePreview(project);
            var files = service.ExportGerber(project, output);

            Assert.Equal(StencilWorkflowStatus.Generated, project.Status);
            Assert.Equal(3, files.Count);
            Assert.All(files, file => Assert.True(File.Exists(file)));
            Assert.Contains(files, file => file.EndsWith("_PASTE_TOP_V001.GTP", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(files, file => File.Exists(file));
        }
        finally { Directory.Delete(output, true); }
    }

    [Fact]
    public void AltiumProject_WithPaste_CompletesWorkflow()
    {
        var service = new StencilManufacturingService();
        var project = service.CreateProject("Altium Board");
        var output = TemporaryDirectory();
        try
        {
            service.LoadInput(project, StencilInputSourceType.AltiumProject, [TestData("bin_s_0_1a.PcbDoc"), TestData("paste-top.gtp")]);
            service.AnalyzePaste(project);
            service.ApplyCorrections(project);
            service.PlaceOnFrame(project);
            service.CreatePreview(project);
            service.ExportGerber(project, output);

            Assert.Equal(StencilInputSourceType.AltiumProject, project.InputProject!.SourceType);
            Assert.Equal(2, project.InputProject.Components.Count);
            Assert.Equal(StencilWorkflowStatus.Generated, project.Status);
        }
        finally { Directory.Delete(output, true); }
    }

    [Fact]
    public void PlaceOnFrame_UsesDefaultFrameWhenNoneIsSelected()
    {
        var service = new StencilManufacturingService();
        var project = service.CreateProject("Default Frame");
        service.LoadInput(project, StencilInputSourceType.PasteOnly, [TestData("paste-top.gtp")]);
        service.ApplyCorrections(project);

        var placement = service.PlaceOnFrame(project);

        Assert.Equal(new StencilFrameLibraryService().GetDefaultFrame()!.Name, placement.FrameName);
        Assert.Equal(StencilWorkflowStatus.PlacedOnFrame, project.Status);
    }

    [Fact]
    public void BottomPaste_AppliesMirrorAndRotationToPasteAndFiducials()
    {
        var service = new StencilManufacturingService();
        var project = service.CreateProject("Bottom Board");
        service.LoadInput(project, StencilInputSourceType.PasteOnly, [TestData("paste-bottom.gbs")]);
        service.ApplyCorrections(project);
        service.PlaceOnFrame(project);
        var fiducials = service.GenerateFiducials(project, [new StencilFiducial { X = 1, Y = 2, Diameter = 1, Shape = "Round" }]);

        Assert.True(project.Transformations!.MirrorX);
        Assert.Equal(180, project.Transformations.RotationAngle, 6);
        Assert.Equal(180, Assert.Single(fiducials).Rotation, 6);
    }

    [Fact]
    public void GenerateMarking_AlwaysUsesMirrorAndZeroRotation()
    {
        var service = new StencilManufacturingService();
        var project = service.CreateProject("Marking Board");
        service.LoadInput(project, StencilInputSourceType.PasteOnly, [TestData("paste-top.gtp")]);
        service.ApplyCorrections(project);
        service.PlaceOnFrame(project);

        var marking = Assert.Single(service.GenerateMarking(project, "MARKING", 10, 20));

        Assert.True(marking.Mirror);
        Assert.Equal(0, marking.Rotation, 6);
        Assert.Equal(10, marking.PositionX, 6);
        Assert.Equal(20, marking.PositionY, 6);
    }

    [Fact]
    public void GenerateReport_SummarizesWorkflowData()
    {
        var service = new StencilManufacturingService();
        var project = service.CreateProject("Report Board");
        service.LoadInput(project, StencilInputSourceType.PasteOnly, [TestData("paste-top.gtp")]);
        service.AnalyzePaste(project);
        service.ApplyCorrections(project);
        service.PlaceOnFrame(project);

        var report = service.GenerateReport(project);

        Assert.Equal("Report Board", report.ProjectName);
        Assert.Equal("PasteOnly", report.InputType);
        Assert.Equal(project.Frame!.Name, report.FrameName);
        Assert.Equal(project.CorrectedPaste!.Changes.Count, report.ModifiedApertures);
    }

    private static string TestData(string name) => Path.Combine(AppContext.BaseDirectory, "TestData", name);
    private static string TemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "VegaStencilWorkflow", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}