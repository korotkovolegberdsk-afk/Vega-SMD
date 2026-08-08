using Vega.Gerber.Models;
using Vega.StencilCAM.Models;
using Vega.StencilViewer;
using Vega.StencilViewer.Models;
using Xunit;

namespace Vega.Tests;

public class StencilOverlayServiceTests
{
    [Fact]
    public void LoadProject_CreatesOriginalPasteOverlay()
    {
        var service = new StencilOverlayService();
        service.LoadProject(Document());

        var overlay = service.CreateOverlay(StencilViewMode.Original);

        var layer = Assert.Single(overlay);
        Assert.Equal(StencilOverlayLayerType.OriginalPaste, layer.LayerType);
        Assert.Single(layer.Geometry);
    }

    [Fact]
    public void CreateOverlay_CombinesOriginalAndCorrectedAndShowsChanges()
    {
        var service = new StencilOverlayService();
        service.LoadProject(Document());

        var overlay = service.CreateOverlay(StencilViewMode.Overlay);
        var comparison = service.CreateCompareView();

        Assert.Equal(2, overlay.Count);
        Assert.Equal(1, comparison.ModifiedApertures);
        Assert.True(comparison.ChangedShape);
        Assert.Single(comparison.Changes);
    }

    [Fact]
    public void Validate_ConfirmsPasteAndFiducialsInsideFrame()
    {
        var service = new StencilOverlayService();
        service.LoadProject(Document());

        var validation = service.Validate();

        Assert.True(validation.IsValid);
        Assert.True(validation.PasteInsideFrame);
        Assert.True(validation.FiducialsInsideFrame);
        Assert.True(validation.MarkingOutsideWorkingArea);
    }

    [Fact]
    public void BottomDocument_ShowsTransformedPasteAndValidatesMirror()
    {
        var layer = Layer("Bottom", 10, -20, 180, 0.9, 0.45);
        var service = new StencilOverlayService();
        service.LoadProject(new StencilViewDocument
        {
            ProjectName = "Bottom", Frame = Frame(), OriginalPasteLayer = layer,
            Transformations = new StencilTransformation { MirrorX = true, RotationAngle = 180 }
        });

        var geometry = Assert.Single(service.CreateOverlay(StencilViewMode.Original)).Geometry.Single();

        Assert.Equal(10, geometry.X, 6);
        Assert.Equal(-20, geometry.Y, 6);
        Assert.Equal(180, geometry.Rotation, 6);
        Assert.True(service.Validate().BottomTransformationApplied);
    }

    [Fact]
    public void MarkingLayer_IsMirroredAtFixedPositionWithoutRotation()
    {
        var service = new StencilOverlayService();
        service.LoadProject(Document());

        var markingLayer = Assert.Single(service.CreateOverlay(StencilViewMode.Production), layer => layer.LayerType == StencilOverlayLayerType.Marking);
        var marking = Assert.Single(markingLayer.Geometry);

        Assert.Equal("MirroredText", marking.Shape);
        Assert.Equal(110, marking.X, 6);
        Assert.Equal(110, marking.Y, 6);
        Assert.Equal(0, marking.Rotation, 6);
    }

    private static StencilViewDocument Document()
    {
        var original = Layer("Top", 50, 50, 0, 0.9, 0.45);
        return new StencilViewDocument
        {
            ProjectName = "Controller", Frame = Frame(), OriginalPasteLayer = original,
            CorrectedPasteLayer = new CorrectedPasteLayer
            {
                OriginalLayer = original, OriginalPrimitiveCount = 1, CorrectedPrimitiveCount = 1,
                CorrectedPrimitives = [new PastePrimitive { X = 50, Y = 50, Width = 0.81, Height = 0.405, Area = 0.32805, Perimeter = 2.43 }],
                Changes = [new PasteCorrectionChange { RefDes = "R1", ChangeType = "ApertureResize", OriginalGeometry = "Rectangle 0.90x0.45", NewGeometry = "Snubnose 0.81x0.405", Reason = "Solder ball prevention" }]
            },
            Fiducials = [new StencilFiducial { X = 10, Y = 10, Diameter = 1, Shape = "Round" }],
            MarkingLayer = [new StencilMarking { Text = "V001", PositionX = 110, PositionY = 110, Height = 2, Mirror = true, Rotation = 0 }]
        };
    }

    private static StencilFrame Frame() => new() { Name = "Frame", StencilWidth = 100, StencilHeight = 100, IsActive = true };
    private static PasteLayer Layer(string side, double x, double y, double rotation, double width, double height)
    {
        var layer = new PasteLayer { Side = side };
        layer.Primitives.Add(new PastePrimitive { X = x, Y = y, Rotation = rotation, Width = width, Height = height, Area = width * height, Perimeter = 2 * (width + height) });
        return layer;
    }
}
