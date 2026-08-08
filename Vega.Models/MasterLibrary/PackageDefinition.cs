namespace Vega.Models.MasterLibrary;

public class PackageDefinition
{
    public int Id { get; set; }
    public string PackageName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string StandardName { get; set; } = "";
    public string PackageFamily { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string ManufacturerPartNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public int CategoryId { get; set; }
    public int FamilyId { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Pitch { get; set; }
    public int LeadCount { get; set; }
    public int PadCount { get; set; }
    public int ThermalPadCount { get; set; }
    public string IPCName { get; set; } = "";
    public string JEDECName { get; set; } = "";
    public string LandPatternName { get; set; } = "";
    public string PolarityMark { get; set; } = "";
    public string DatasheetUrl { get; set; } = "";
    public string DrawingFile { get; set; } = "";
    public string Model3DFile { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public int Version { get; set; }
    public string ChangeComment { get; set; } = "";
}