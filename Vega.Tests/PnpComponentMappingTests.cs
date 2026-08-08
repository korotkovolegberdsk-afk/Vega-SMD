using Vega.Gerber;
using Vega.PnP;
using Xunit;

namespace Vega.Tests;

public class PnpComponentMappingTests
{
    [Fact]
    public void Map_Should_Associate_Components_With_Nearest_Paste_Primitives()
    {
        var pnpFile = Path.Combine(AppContext.BaseDirectory, "TestData", "test-pnp.csv");
        var gerberFile = Path.Combine(AppContext.BaseDirectory, "TestData", "test-paste.gtp");
        var components = new PnpParserService().Parse(pnpFile).ToArray();
        var parser = new GerberPasteParserService();
        parser.Load(gerberFile);
        var layer = parser.Parse();

        var mapped = new ComponentMappingService().Map(components, layer);

        Assert.Equal(2, components.Length);
        Assert.Equal("C1", components[0].RefDes);
        Assert.Equal(2, mapped.Count);
        Assert.All(mapped, result =>
        {
            Assert.Equal("Matched", result.Status);
            Assert.Equal(1, result.PastePrimitiveCount);
            Assert.Equal(0, result.MatchDistance, 6);
        });
        Assert.Equal("0603", mapped[0].PackageName);
        Assert.Equal("SOIC-8", mapped[1].PackageName);
    }
}
