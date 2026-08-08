namespace Vega.CAD.Models;

public enum PcbSourceType
{
    AltiumPcbDoc,
    KiCadPCB,
    OrCAD,
    CAM350,
    Gerber,
    Manual
}

public enum BoardSide
{
    Top,
    Bottom
}

public enum FiducialType
{
    PCB_FIDUCIAL,
    LOCAL_FIDUCIAL,
    PANEL_FIDUCIAL
}

public class PcbProject
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = "";
    public PcbSourceType SourceType { get; set; }
    public string SourceFile { get; set; } = "";
    public Board Board { get; set; } = new();
    public List<Component> Components { get; } = [];
    public List<BomItem> BomItems { get; } = [];
    public List<Placement> Placements { get; } = [];
    public List<PasteLayerInfo> PasteLayers { get; } = [];
    public List<Fiducial> Fiducials { get; } = [];
}

public class Board
{
    public double Width { get; set; }
    public double Height { get; set; }
    public string OutlineFile { get; set; } = "";
    public List<string> Layers { get; } = [];
}

public class Component
{
    public int Id { get; set; }
    public string RefDes { get; set; } = "";
    public string Value { get; set; } = "";
    public string Description { get; set; } = "";
    public string Footprint { get; set; } = "";
    public string PackageName { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string ManufacturerPartNumber { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Rotation { get; set; }
    public BoardSide Side { get; set; }
}

public class Placement
{
    public string RefDes { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Rotation { get; set; }
    public BoardSide Side { get; set; }
    public string Nozzle { get; set; } = "";
    public string Comment { get; set; } = "";
}

public class BomItem
{
    public string PartNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public string Value { get; set; } = "";
    public string Footprint { get; set; } = "";
    public int Quantity { get; set; }
    public string Manufacturer { get; set; } = "";
}

public class PasteLayerInfo
{
    public string Name { get; set; } = "";
    public BoardSide Side { get; set; }
    public PcbSourceType SourceType { get; set; }
    public string FileName { get; set; } = "";
}

public class Fiducial
{
    public string Name { get; set; } = "";
    public FiducialType Type { get; set; }
    public string SourceLayer { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Diameter { get; set; }
    public string Shape { get; set; } = "";
}
