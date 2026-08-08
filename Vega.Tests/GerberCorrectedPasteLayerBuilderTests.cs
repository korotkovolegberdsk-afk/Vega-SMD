using Vega.Gerber.Models;
using Vega.Services.Gerber;
using Xunit;

namespace Vega.Tests;

public class GerberCorrectedPasteLayerBuilderTests
{
    [Fact]
    public void Build_RectangleReduction_CreatesNewPrimitiveWithoutChangingSource()
    {
        var source = CreateLayer("BOARD_TOP_PASTE.GTP", "Top", 10, 20, 0, 0.9, 0.45);
        var pattern = new OptimizedAperturePattern
        {
            RefDes = "R1",
            PatternType = AperturePatternType.RectangleReduction,
            OriginalWidth = 0.9,
            OriginalHeight = 0.45,
            RecommendedWidth = 0.81,
            RecommendedHeight = 0.405,
            Reason = "R0603 aperture reduction"
        };

        var result = new CorrectedPasteLayerBuilderService().Build(source, [pattern]);

        Assert.Same(source, result.OriginalLayer);
        Assert.Equal(0.9, source.Primitives[0].Width, 6);
        Assert.Equal(0.45, source.Primitives[0].Height, 6);
        Assert.Equal(1, result.OriginalPrimitiveCount);
        Assert.Equal(1, result.CorrectedPrimitiveCount);
        Assert.Equal(0.81, result.CorrectedPrimitives[0].Width, 6);
        Assert.Equal(0.405, result.CorrectedPrimitives[0].Height, 6);
        Assert.Equal(10, result.CorrectedPrimitives[0].X, 6);
        Assert.Equal(20, result.CorrectedPrimitives[0].Y, 6);
        Assert.Single(result.Changes);
        Assert.Equal("R1", result.Changes[0].RefDes);
        Assert.Equal("ApertureResize", result.Changes[0].ChangeType);
    }

    [Fact]
    public void Build_WindowPane_SegmentsThermalPadAndPreservesItsCenter()
    {
        var source = CreateLayer("BOARD_TOP_PASTE.GTP", "Top", 10, 20, 30, 6, 6);
        var pattern = new OptimizedAperturePattern
        {
            RefDes = "U5",
            PatternType = AperturePatternType.WindowPane,
            OriginalWidth = 6,
            OriginalHeight = 6,
            RecommendedWidth = 1.275,
            RecommendedHeight = 1.275,
            Rows = 4,
            Columns = 4,
            WebWidth = 0.3,
            CoveragePercent = 72.25,
            Reason = "QFN thermal pad optimization"
        };

        var result = new CorrectedPasteLayerBuilderService().Build(source, [pattern]);

        Assert.Single(source.Primitives);
        Assert.Equal(6, source.Primitives[0].Width, 6);
        Assert.Equal(16, result.CorrectedPrimitiveCount);
        Assert.All(result.CorrectedPrimitives, primitive => Assert.Equal(30, primitive.Rotation, 6));
        Assert.Equal(10, result.CorrectedPrimitives.Average(primitive => primitive.X), 6);
        Assert.Equal(20, result.CorrectedPrimitives.Average(primitive => primitive.Y), 6);
        var coverage = result.CorrectedPrimitives.Sum(primitive => primitive.Area)
            / source.Primitives[0].Area * 100;
        Assert.Equal(pattern.CoveragePercent, coverage, 6);
        Assert.Single(result.Changes);
        Assert.Equal("U5", result.Changes[0].RefDes);
        Assert.Equal("ThermalPadSegmentation", result.Changes[0].ChangeType);
    }

    [Fact]
    public void Create_RevisionOne_UsesStandardFileNames()
    {
        var layer = CreateLayer("input.gtp", "Top", 0, 0, 0, 1, 1);

        var revision = new GerberRevisionService().Create("BOARD", layer, 1, 2);

        Assert.Equal("V001", revision.Revision);
        Assert.Equal("BOARD_TOP_PASTE_ORIGINAL.GTP", revision.OriginalFile);
        Assert.Equal("BOARD_TOP_PASTE_CORRECTED_V001.GTP", revision.GeneratedFile);
        Assert.Equal(2, revision.ChangesCount);
    }

    private static PasteLayer CreateLayer(
        string fileName,
        string side,
        double x,
        double y,
        double rotation,
        double width,
        double height)
    {
        var layer = new PasteLayer { FileName = fileName, Side = side };
        layer.Primitives.Add(new PastePrimitive
        {
            ApertureId = 10,
            X = x,
            Y = y,
            Rotation = rotation,
            Width = width,
            Height = height,
            Area = width * height,
            Perimeter = 2 * (width + height)
        });
        return layer;
    }
}



