using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.Services.Gerber;
using Xunit;

namespace Vega.Tests;

public class GerberPasteWriterServiceTests
{
    [Fact]
    public void Write_RectangleReduction_CreatesParseableFileWithCorrectGeometry()
    {
        var source = CreateLayer(10, 20, 0.9, 0.45);
        var corrected = new CorrectedPasteLayerBuilderService().Build(source,
        [
            new OptimizedAperturePattern
            {
                RefDes = "R1",
                PatternType = AperturePatternType.RectangleReduction,
                RecommendedWidth = 0.81,
                RecommendedHeight = 0.405,
                Reason = "R0603 reduction"
            }
        ]);

        var outputFile = CreateOutputFile();
        try
        {
            new GerberPasteWriterService().Write(corrected, outputFile);
            var written = Parse(outputFile);

            Assert.True(File.Exists(outputFile));
            Assert.Single(written.Primitives);
            Assert.Equal(0.81, written.Primitives[0].Width, 6);
            Assert.Equal(0.405, written.Primitives[0].Height, 6);
            Assert.Equal(10, written.Primitives[0].X, 6);
            Assert.Equal(20, written.Primitives[0].Y, 6);
            Assert.Equal(0.9, source.Primitives[0].Width, 6);
        }
        finally
        {
            File.Delete(outputFile);
        }
    }

    [Fact]
    public void Write_WindowPane_CreatesOneFlashForEveryCorrectedPrimitive()
    {
        var source = CreateLayer(10, 20, 6, 6);
        var corrected = new CorrectedPasteLayerBuilderService().Build(source,
        [
            new OptimizedAperturePattern
            {
                RefDes = "U5",
                PatternType = AperturePatternType.WindowPane,
                RecommendedWidth = 1.275,
                RecommendedHeight = 1.275,
                Rows = 4,
                Columns = 4,
                WebWidth = 0.3,
                Reason = "QFN thermal pad segmentation"
            }
        ]);

        var outputFile = CreateOutputFile();
        try
        {
            new GerberPasteWriterService().Write(corrected, outputFile);
            var written = Parse(outputFile);
            var report = new GerberPasteWriterService().CreateCompareReport(corrected, outputFile);

            Assert.Equal(corrected.CorrectedPrimitiveCount, written.Primitives.Count);
            Assert.Equal(16, written.Primitives.Count);
            Assert.Equal(
                corrected.CorrectedPrimitives.Average(primitive => primitive.X),
                written.Primitives.Average(primitive => primitive.X), 6);
            Assert.Equal(
                corrected.CorrectedPrimitives.Average(primitive => primitive.Y),
                written.Primitives.Average(primitive => primitive.Y), 6);
            Assert.Equal(15, report.AddedCount);
            Assert.Equal(0, report.RemovedCount);
            Assert.Equal("source.gtp", report.OriginalFile);
        }
        finally
        {
            File.Delete(outputFile);
        }
    }

    private static PasteLayer CreateLayer(double x, double y, double width, double height)
    {
        var layer = new PasteLayer { FileName = "source.gtp", Side = "Top" };
        layer.Primitives.Add(new PastePrimitive
        {
            ApertureId = 10,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Area = width * height,
            Perimeter = 2 * (width + height)
        });
        return layer;
    }

    private static PasteLayer Parse(string fileName)
    {
        var parser = new GerberPasteParserService();
        parser.Load(fileName);
        return parser.Parse();
    }

    private static string CreateOutputFile() =>
        Path.Combine(Path.GetTempPath(), $"vega-paste-{Guid.NewGuid():N}.gtp");
}
