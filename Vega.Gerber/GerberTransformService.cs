using Vega.Gerber.Models;

namespace Vega.Gerber;

public class GerberTransformService
{
    public PasteLayer MirrorBottom(PasteLayer layer)
    {
        return Transform(layer, mirrorBottom: true, rotationDegrees: 0, offsetX: 0, offsetY: 0);
    }

    public PasteLayer Rotate(PasteLayer layer, double rotationDegrees)
    {
        return Transform(layer, mirrorBottom: false, rotationDegrees, 0, 0);
    }

    public PasteLayer Offset(PasteLayer layer, double offsetX, double offsetY)
    {
        return Transform(layer, mirrorBottom: false, rotationDegrees: 0, offsetX, offsetY);
    }

    public PasteLayer Transform(
        PasteLayer layer,
        bool mirrorBottom,
        double rotationDegrees,
        double offsetX,
        double offsetY)
    {
        var result = CopyLayer(layer);
        var radians = rotationDegrees * Math.PI / 180;
        var cosine = Math.Cos(radians);
        var sine = Math.Sin(radians);

        foreach (var primitive in layer.Primitives)
        {
            var x = mirrorBottom ? -primitive.X : primitive.X;
            var y = primitive.Y;
            var transformedX = x * cosine - y * sine + offsetX;
            var transformedY = x * sine + y * cosine + offsetY;
            var rotation = primitive.Rotation + rotationDegrees;

            if (mirrorBottom)
            {
                rotation = 180 - rotation;
            }

            result.Primitives.Add(new PastePrimitive
            {
                X = transformedX,
                Y = transformedY,
                Rotation = NormalizeRotation(rotation),
                ApertureId = primitive.ApertureId,
                Width = primitive.Width,
                Height = primitive.Height,
                Area = primitive.Area,
                Perimeter = primitive.Perimeter
            });
        }

        return result;
    }

    private static PasteLayer CopyLayer(PasteLayer layer)
    {
        var copy = new PasteLayer
        {
            FileName = layer.FileName,
            Side = layer.Side
        };

        copy.Apertures.AddRange(layer.Apertures);
        return copy;
    }

    private static double NormalizeRotation(double rotation)
    {
        var normalized = rotation % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }
}
