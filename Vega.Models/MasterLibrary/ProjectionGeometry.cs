namespace Vega.Models.MasterLibrary;

public sealed class ProjectionGeometry
{
    public int Id { get; set; }
    public int ManufacturerDrawingGeometryId { get; set; }

    public string ProjectionType { get; set; } = "";
    public string CoordinateSystem { get; set; } = "mm";
    public double OriginX { get; set; }
    public double OriginY { get; set; }
    public string BodyContour { get; set; } = "";
    public string LeadContour { get; set; } = "";
    public string AuxiliaryGeometry { get; set; } = "";
    public bool IsAvailable { get; set; } = true;
}
