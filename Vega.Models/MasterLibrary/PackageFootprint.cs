namespace Vega.Models.MasterLibrary;

public class PackageFootprint
{
    public int Id { get; set; }
    public int PackageId { get; set; }

    public string PatternName { get; set; } = "";
    public string StandardName { get; set; } = "";
    public string Description { get; set; } = "";

    public int PadCount { get; set; }
    public double PadLength { get; set; }
    public double PadWidth { get; set; }
    public double PadPitch { get; set; }

    public double Pin1Offset { get; set; }
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }

    public double PasteReduction { get; set; }
    public string ApertureType { get; set; } = "";
}
