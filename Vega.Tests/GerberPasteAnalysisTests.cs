using Vega.Gerber;
using Xunit;

namespace Vega.Tests;

public class GerberPasteAnalysisTests
{
    [Fact]
    public void Analyze_Should_Return_Top_Paste_Statistics()
    {
        var layer = Parse("paste-top.gtp");
        var result = new PasteAnalyzerService().Analyze(layer);

        Assert.Equal(2, result.PrimitiveCount);
        Assert.Equal(2, result.ApertureCount);
        Assert.Equal(1, result.ShapeStatistics["Circle"]);
        Assert.Equal(1, result.ShapeStatistics["Rectangle"]);
        Assert.Equal(0.6, result.MinApertureSize, 6);
        Assert.Equal(1.2, result.MaxApertureSize, 6);
        Assert.Equal(0, result.WarningCount);
    }

    [Fact]
    public void MirrorBottom_Should_Mirror_X_And_Preserve_Paste_Data()
    {
        var layer = Parse("paste-bottom.gbs");
        var transformed = new GerberTransformService().Transform(
            layer,
            mirrorBottom: true,
            rotationDegrees: 90,
            offsetX: 10,
            offsetY: 5);

        var primitive = Assert.Single(transformed.Primitives);
        Assert.Equal(8, primitive.X, 6);
        Assert.Equal(4, primitive.Y, 6);
        Assert.Equal(90, primitive.Rotation, 6);
        Assert.Single(transformed.Apertures);
        Assert.Equal(0, new PasteAnalyzerService().Analyze(transformed).WarningCount);
    }

    private static Vega.Gerber.Models.PasteLayer Parse(string fileName)
    {
        var parser = new GerberPasteParserService();
        parser.Load(Path.Combine(AppContext.BaseDirectory, "TestData", fileName));
        return parser.Parse();
    }
}
