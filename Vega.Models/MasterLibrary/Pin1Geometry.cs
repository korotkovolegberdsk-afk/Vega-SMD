namespace Vega.Models.MasterLibrary;

public sealed class Pin1Geometry
{
    public int Id { get; set; }
    public int ManufacturerDrawingGeometryId { get; set; }

    public string ProjectionType { get; set; } = "";
    public bool IsPresent { get; set; }
    public string MarkerType { get; set; } = "";
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
