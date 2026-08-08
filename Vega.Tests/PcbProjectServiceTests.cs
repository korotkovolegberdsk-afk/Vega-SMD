using Vega.Altium;
using Vega.CAD;
using Vega.CAD.Models;
using Xunit;

namespace Vega.Tests;

public class PcbProjectServiceTests
{
    [Fact]
    public void ProjectService_CreatesProjectWithComponentsPasteLayerAndFiducial()
    {
        var service = new PcbProjectService();
        var project = service.CreateProject("Manual board");

        service.AddComponent(project, new Component { RefDes = "R1", Footprint = "R0603", PackageName = "R0603" });
        service.AddComponent(project, new Component { RefDes = "U1", Footprint = "QFP-64", PackageName = "QFP" });
        service.AddComponent(project, new Component { RefDes = "U2", Footprint = "QFN-32", PackageName = "QFN" });
        service.AddPasteLayer(project, new PasteLayerInfo { Name = "Top Paste", Side = BoardSide.Top, SourceType = PcbSourceType.Gerber, FileName = "board.gtp" });
        service.AddFiducial(project, new Fiducial { Name = "F1", Type = FiducialType.PCB_FIDUCIAL, X = 10, Y = 20, Diameter = 1, Shape = "Round" });

        Assert.Equal("Manual board", project.ProjectName);
        Assert.Equal(3, project.Components.Count);
        Assert.Single(project.PasteLayers);
        Assert.Single(project.Fiducials);
    }

    [Fact]
    public void AltiumAdapter_ConvertsPcbDocIntoPcbProject()
    {
        var fileName = Path.Combine(AppContext.BaseDirectory, "TestData", "bin_s_0_1a.PcbDoc");
        IPcbImporter importer = new AltiumPcbImporter();

        var project = importer.Import(fileName);

        Assert.True(importer.CanImport(fileName));
        Assert.Equal(PcbSourceType.AltiumPcbDoc, project.SourceType);
        Assert.Equal(2, project.Components.Count);
        Assert.Equal(2, project.Placements.Count);
        var resistor = Assert.Single(project.Components, component => component.RefDes == "R1");
        Assert.Equal("R0603", resistor.PackageName);
        Assert.Equal(BoardSide.Top, resistor.Side);
    }
}
