namespace Vega.Models.MasterLibrary;

public class PackageGeometry
{
    public int Id { get; set; }

    public int PackageId { get; set; }

    public double BodyLength { get; set; }
    public double BodyWidth { get; set; }
    public double BodyHeight { get; set; }

    public double LeadLength { get; set; }
    public double LeadWidth { get; set; }
    public double LeadPitch { get; set; }
    public int LeadCount { get; set; }

    public double PadLength { get; set; }
    public double PadWidth { get; set; }
    public double PadPitch { get; set; }

    public double CenterX { get; set; }
    public double CenterY { get; set; }
}
