using Vega.Gerber;
using Xunit;

namespace Vega.Tests;

public class GerberPasteParserServiceTests
{
    [Fact]
    public void Parse_Should_Load_Apertures_And_Flash_Primitives()
    {
        var fileName = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "paste-top.gtp");
        var service = new GerberPasteParserService();

        service.Load(fileName);
        var layer = service.Parse();

        Assert.Equal("paste-top.gtp", layer.FileName);
        Assert.Equal("Top", layer.Side);
        Assert.Equal(2, layer.Apertures.Count);
        Assert.Equal(2, layer.Primitives.Count);
        Assert.Equal(0.6, layer.Apertures[0].Diameter, 6);
        Assert.Equal(1.2, layer.Apertures[1].Width, 6);
        Assert.Equal(0.6, layer.Apertures[1].Height, 6);
        Assert.Equal(1, layer.Primitives[0].X, 6);
        Assert.Equal(2, layer.Primitives[0].Y, 6);
        Assert.Equal(1.2, layer.Primitives[1].Width, 6);
    }
}
