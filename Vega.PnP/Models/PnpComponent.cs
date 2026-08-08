namespace Vega.PnP.Models;

public class PnpComponent
{
    public string RefDes { get; init; } = "";
    public string PartNumber { get; init; } = "";
    public string PackageName { get; init; } = "";
    public double X { get; init; }
    public double Y { get; init; }
    public double Rotation { get; init; }
    public string Side { get; init; } = "Top";
}
