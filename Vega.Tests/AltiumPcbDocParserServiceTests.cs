using Vega.Altium;
using Xunit;

namespace Vega.Tests;

public class AltiumPcbDocParserServiceTests
{
    [Fact]
    public void ParseComponents_OpensPcbDocAndExportsBomAndPickAndPlace()
    {
        var parser = new AltiumPcbDocParserService();
        parser.Load(Path.Combine(AppContext.BaseDirectory, "TestData", "bin_s_0_1a.PcbDoc"));

        var result = parser.Parse();

        Assert.Equal("bin_s_0_1a", result.ProjectName);
        Assert.Equal(2, result.Components.Count);
        var resistor = Assert.Single(result.Components, component => component.RefDes == "R1");
        Assert.Equal(10.5, resistor.X, 6);
        Assert.Equal(20.25, resistor.Y, 6);
        Assert.Equal("R0603", resistor.Footprint);
        Assert.Equal("TopLayer", resistor.Layer);
        Assert.Equal(2, result.Bom.Count);
        Assert.Equal(2, result.PickAndPlace.Count);
        Assert.Contains("Designator,Comment,Value,Footprint,Quantity", parser.ExportBomCsv());
        Assert.Contains("RefDes,X,Y,Rotation,Layer,Footprint", parser.ExportPickAndPlaceCsv());
    }
}
