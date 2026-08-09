using Vega.Gerber;
using Xunit;

namespace Vega.Tests;

public class GerberPasteParserDrawTests
{
    [Fact]
    public void Parse_Should_Create_PastePrimitive_For_Coordinate_Draw_Command()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"vega-paste-{Guid.NewGuid():N}.gtp");
        File.WriteAllLines(fileName,
        [
            "%FSLAX24Y24*%",
            "%MOMM*%",
            "%ADD10R,1.200X0.600*%",
            "D10*",
            "X010000Y020000D03*",
            "X020000Y020000D01*",
            "M02*"
        ]);

        try
        {
            var parser = new GerberPasteParserService();
            parser.Load(fileName);
            var layer = parser.Parse();

            Assert.Single(layer.Apertures);
            Assert.Equal(2, layer.Primitives.Count);
            Assert.Equal(2, layer.Primitives[1].X, 6);
            Assert.Equal(2, layer.Primitives[1].Y, 6);
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}