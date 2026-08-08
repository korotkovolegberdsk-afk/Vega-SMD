using Vega.Gerber.Models;

namespace Vega.Gerber;

public class ApertureShapeGeneratorService
{
    public IReadOnlyList<PastePrimitive> Generate(ApertureGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var width = ResolveWidth(geometry);
        var height = ResolveHeight(geometry, width);
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(geometry), "Aperture dimensions must be positive.");
        }

        return geometry.ShapeType == ApertureShapeType.Array
            ? GenerateArray(geometry, width, height)
            : [CreatePrimitive(geometry.ShapeType, 0, 0, width, height, geometry.Rotation)];
    }

    private static IReadOnlyList<PastePrimitive> GenerateArray(ApertureGeometry geometry, double width, double height)
    {
        var rows = Math.Max(1, geometry.Rows);
        var columns = Math.Max(1, geometry.Columns);
        var cellWidth = (width - (columns - 1) * geometry.WebWidth) / columns;
        var cellHeight = (height - (rows - 1) * geometry.WebWidth) / rows;
        if (cellWidth <= 0 || cellHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(geometry), "Web width leaves no area for aperture windows.");
        }

        var baseCoverage = rows * columns * cellWidth * cellHeight / (width * height) * 100d;
        var targetCoverage = Math.Min(Math.Clamp(geometry.Coverage, 0, 100), baseCoverage);
        var scale = Math.Sqrt(targetCoverage / baseCoverage);
        cellWidth *= scale;
        cellHeight *= scale;
        var result = new List<PastePrimitive>(rows * columns);
        var radians = geometry.Rotation * Math.PI / 180d;
        var cosine = Math.Cos(radians);
        var sine = Math.Sin(radians);

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var localX = -width / 2 + (column + 0.5) * (width / columns);
                var localY = -height / 2 + (row + 0.5) * (height / rows);
                result.Add(CreatePrimitive(
                    ApertureShapeType.Array,
                    localX * cosine - localY * sine,
                    localX * sine + localY * cosine,
                    cellWidth,
                    cellHeight,
                    geometry.Rotation));
            }
        }

        return result;
    }

    private static PastePrimitive CreatePrimitive(ApertureShapeType shapeType, double x, double y, double width, double height, double rotation)
    {
        var (area, perimeter) = shapeType switch
        {
            ApertureShapeType.Round or ApertureShapeType.Ellipse =>
                (Math.PI * width * height / 4d,
                 Math.PI * (3 * (width + height) - Math.Sqrt((3 * width + height) * (width + 3 * height)))),
            ApertureShapeType.Oblong or ApertureShapeType.MELF => ObroundMetrics(width, height),
            ApertureShapeType.Triangle or ApertureShapeType.Diamond =>
                (width * height / 2d, 2 * Math.Sqrt(width * width + height * height)),
            _ => (width * height, 2 * (width + height))
        };

        return new PastePrimitive
        {
            ShapeType = shapeType,
            X = x,
            Y = y,
            Rotation = rotation,
            Width = width,
            Height = height,
            Area = area,
            Perimeter = perimeter
        };
    }

    private static (double Area, double Perimeter) ObroundMetrics(double width, double height)
    {
        var minor = Math.Min(width, height);
        var major = Math.Max(width, height);
        return (Math.PI * minor * minor / 4d + minor * (major - minor), Math.PI * minor + 2 * (major - minor));
    }

    private static double ResolveWidth(ApertureGeometry geometry) => geometry.Width > 0 ? geometry.Width : geometry.Radius * 2;
    private static double ResolveHeight(ApertureGeometry geometry, double width) => geometry.Height > 0 ? geometry.Height : geometry.Length > 0 ? geometry.Length : width;
}

