using Vega.Gerber.Models;

namespace Vega.PackageRecognition.Models;

public class PackageRecognitionInput
{
    public string RefDes { get; set; } = "";
    public string ComponentName { get; set; } = "";
    public string FootprintName { get; set; } = "";
    public string Comment { get; set; } = "";
    public string ManufacturerPartNumber { get; set; } = "";
    public int PadCount { get; set; }
    public double PadLength { get; set; }
    public double PadWidth { get; set; }
    public double PadPitch { get; set; }
    public double ComponentHeight { get; set; }
    public IReadOnlyList<PastePrimitive> PastePattern { get; set; } = Array.Empty<PastePrimitive>();
}