using Vega.Gerber.Models;
using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Xunit;

namespace Vega.Tests;

public class StencilFrameCamTests
{
    private readonly StencilPlacementService _placementService = new();

    [Fact]
    public void PlacePasteLayer_CentersHundredByEightyBoardInFrame()
    {
        var layer = Layer("Top", (0d, 0d), (100d, 80d));
        var frame = Frame(400, 500);

        var result = _placementService.PlacePasteLayer(layer, frame, new StencilTransformation { AutoCenter = true });

        Assert.True(result.IsFit);
        Assert.Equal(200, result.FinalBounds.CenterX, 6);
        Assert.Equal(250, result.FinalBounds.CenterY, 6);
        Assert.Equal(100, result.FinalBounds.Width, 6);
        Assert.Equal(80, result.FinalBounds.Height, 6);
    }

    [Fact]
    public void PlacePasteLayer_BottomMirrorAndRotationAlsoTransformsFiducials()
    {
        var transformation = new StencilTransformation { MirrorX = true, RotationAngle = 180 };
        var result = _placementService.PlacePasteLayer(Layer("Bottom", (10d, 20d)), Frame(400, 500), transformation);
        var fiducial = new StencilFiducialGeneratorService().GeneratePcbFiducial("Round", 1, 10, 20);
        var placedFiducial = Assert.Single(_placementService.PlaceFiducials([fiducial], transformation, result.OffsetX, result.OffsetY));

        var paste = Assert.Single(result.PlacedLayer.Primitives);
        Assert.Equal(10, paste.X, 6);
        Assert.Equal(-20, paste.Y, 6);
        Assert.Equal(180, paste.Rotation, 6);
        Assert.Equal(paste.X, placedFiducial.X, 6);
        Assert.Equal(paste.Y, placedFiducial.Y, 6);
        Assert.Equal(180, placedFiducial.Rotation, 6);
    }

    [Fact]
    public void GenerateMarking_AlwaysMirrorsWithoutTransformingPosition()
    {
        var marking = new StencilMarkingGeneratorService().Generate("BOARD V001", 25, 35, 2, "Stroke");

        Assert.True(marking.Mirror);
        Assert.Equal(0, marking.Rotation, 6);
        Assert.Equal(25, marking.PositionX, 6);
        Assert.Equal(35, marking.PositionY, 6);
    }

    [Fact]
    public void PlacePasteLayer_ReportsOutOfFrameBoard()
    {
        var result = _placementService.PlacePasteLayer(
            Layer("Top", (0d, 0d), (500d, 100d)), Frame(400, 500), new StencilTransformation());

        Assert.False(result.IsFit);
    }

    [Fact]
    public void Write_CreatesPasteAndMarkingFilesWithStandardNames()
    {
        var layer = new PasteLayer { FileName = "board.gtp", Side = "Top" };
        layer.Primitives.Add(new PastePrimitive { X = 1, Y = 1, Width = 1, Height = 1, Area = 1, Perimeter = 4 });
        var placement = _placementService.PlacePasteLayer(layer, Frame(400, 500), new StencilTransformation());
        var marking = new StencilMarkingGeneratorService().Generate("BOARD", 10, 20, 2, "Stroke");
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"vega-cam-{Guid.NewGuid():N}");
        try
        {
            var files = new StencilCamGerberOutputService().Write("PROJECT", placement, marking, outputDirectory);

            Assert.Equal("PROJECT_PASTE_TOP_V001.GTP", Path.GetFileName(files.PasteFile));
            Assert.Equal("PROJECT_MARKING_V001.GBR", Path.GetFileName(files.MarkingFile));
            Assert.True(File.Exists(files.PasteFile));
            Assert.True(File.Exists(files.MarkingFile));
        }
        finally
        {
            if (Directory.Exists(outputDirectory)) Directory.Delete(outputDirectory, recursive: true);
        }
    }
    private static StencilFrame Frame(double width, double height) => new()
    {
        Name = "Universal", FrameWidth = width, FrameHeight = height,
        StencilWidth = width, StencilHeight = height
    };

    private static PasteLayer Layer(string side, params (double X, double Y)[] points)
    {
        var layer = new PasteLayer { FileName = "board.gtp", Side = side };
        foreach (var (x, y) in points)
            layer.Primitives.Add(new PastePrimitive { X = x, Y = y, Area = 1, Perimeter = 4 });
        return layer;
    }
}
