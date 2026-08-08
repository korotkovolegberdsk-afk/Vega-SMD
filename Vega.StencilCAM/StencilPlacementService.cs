using Vega.Gerber.Models;
using Vega.StencilCAM.Models;

namespace Vega.StencilCAM;

public class StencilPlacementService
{
    private readonly StencilFrameService _frameService;
    private readonly StencilFrameLibraryService _frameLibraryService;

    public StencilPlacementService(StencilFrameService? frameService = null, StencilFrameLibraryService? frameLibraryService = null)
    {
        _frameService = frameService ?? new StencilFrameService();
        _frameLibraryService = frameLibraryService ?? new StencilFrameLibraryService();
    }

    public StencilPlacementResult PlacePasteLayer(PasteLayer layer, StencilTransformation transformation)
    {
        var frame = _frameLibraryService.GetDefaultFrame()
            ?? throw new InvalidOperationException("No default stencil frame is configured.");
        return PlacePasteLayer(layer, frame, transformation);
    }

    public StencilPlacementResult PlacePasteLayer(PasteLayer layer, StencilFrame frame, StencilTransformation transformation)
    {
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(transformation);

        var pasteBounds = _frameService.CalculateBounds(layer);
        var preliminary = TransformLayer(layer, transformation, 0, 0);
        var preliminaryBounds = _frameService.CalculateBounds(preliminary);
        var (centerX, centerY) = _frameService.GetCenter(frame);
        var centerOffsetX = transformation.AutoCenter ? centerX - preliminaryBounds.CenterX : 0;
        var centerOffsetY = transformation.AutoCenter ? centerY - preliminaryBounds.CenterY : 0;
        var offsetX = centerOffsetX + transformation.OffsetX;
        var offsetY = centerOffsetY + transformation.OffsetY;
        var placed = TransformLayer(layer, transformation, offsetX, offsetY);
        var finalBounds = _frameService.CalculateBounds(placed);

        return new StencilPlacementResult
        {
            FrameName = frame.Name, PasteBounds = pasteBounds, FinalBounds = finalBounds,
            OffsetX = offsetX, OffsetY = offsetY, Rotation = transformation.RotationAngle,
            IsFit = _frameService.ValidateFit(finalBounds, frame), PlacedLayer = placed
        };
    }

    public IReadOnlyList<StencilFiducial> PlaceFiducials(IEnumerable<StencilFiducial> fiducials, StencilTransformation transformation, double offsetX, double offsetY)
    {
        ArgumentNullException.ThrowIfNull(fiducials);
        ArgumentNullException.ThrowIfNull(transformation);
        return fiducials.Select(fiducial =>
        {
            var (x, y) = TransformPoint(fiducial.X, fiducial.Y, transformation, offsetX, offsetY);
            return new StencilFiducial
            {
                Type = fiducial.Type, Shape = fiducial.Shape, Diameter = fiducial.Diameter, Layer = fiducial.Layer,
                X = x, Y = y, Rotation = NormalizeRotation(fiducial.Rotation + transformation.RotationAngle)
            };
        }).ToList();
    }

    private static PasteLayer TransformLayer(PasteLayer layer, StencilTransformation transformation, double offsetX, double offsetY)
    {
        var result = new PasteLayer { FileName = layer.FileName, Side = layer.Side };
        result.Apertures.AddRange(layer.Apertures);
        foreach (var primitive in layer.Primitives)
        {
            var (x, y) = TransformPoint(primitive.X, primitive.Y, transformation, offsetX, offsetY);
            result.Primitives.Add(new PastePrimitive
            {
                ShapeType = primitive.ShapeType, ApertureId = primitive.ApertureId, X = x, Y = y,
                Rotation = NormalizeRotation(primitive.Rotation + transformation.RotationAngle),
                Width = primitive.Width, Height = primitive.Height, Area = primitive.Area, Perimeter = primitive.Perimeter
            });
        }
        return result;
    }

    private static (double X, double Y) TransformPoint(double x, double y, StencilTransformation transformation, double offsetX, double offsetY)
    {
        if (transformation.MirrorX) x = -x;
        if (transformation.MirrorY) y = -y;
        var radians = transformation.RotationAngle * Math.PI / 180d;
        var transformedX = x * Math.Cos(radians) - y * Math.Sin(radians) + offsetX;
        var transformedY = x * Math.Sin(radians) + y * Math.Cos(radians) + offsetY;
        return (transformedX, transformedY);
    }

    private static double NormalizeRotation(double rotation) => (rotation % 360 + 360) % 360;
}


