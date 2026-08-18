namespace Vega.Models.MasterLibrary;

/// <summary>
/// A source drawing primitive belonging to one verified package projection.
/// Coordinates are stored as supplied by the manufacturer; no geometry is
/// inferred from package family, lead count or dimensions.
/// </summary>
public sealed class PackageDrawingPrimitive
{
    public int Id { get; set; }
    public int ProjectionGeometryId { get; set; }

    /// <summary>Line, Arc, Circle or Polygon.</summary>
    public string PrimitiveType { get; set; } = "";

    public double? StartX { get; set; }
    public double? StartY { get; set; }
    public double? EndX { get; set; }
    public double? EndY { get; set; }

    public double? CenterX { get; set; }
    public double? CenterY { get; set; }
    public double? Radius { get; set; }
    public double? StartAngle { get; set; }
    public double? EndAngle { get; set; }

    /// <summary>JSON geometry payload for Polygon primitives.</summary>
    public string GeometryData { get; set; } = "";

    public double? StrokeWidth { get; set; }

    /// <summary>Body, Lead, Pin1, Dimension or Auxiliary.</summary>
    public string Layer { get; set; } = "";

    public string CoordinateSystem { get; set; } = "mm";
}
