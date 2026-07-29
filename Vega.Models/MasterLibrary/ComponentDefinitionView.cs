namespace Vega.Models.MasterLibrary;

public class ComponentDefinitionView
{
    public int Id { get; set; }

    public string ManufacturerPartNumber { get; set; } = "";

    public string Manufacturer { get; set; } = "";

    public string ComponentType { get; set; } = "";

    public string Description { get; set; } = "";

    public string PackageName { get; set; } = "";

    public string PackageCategory { get; set; } = "";

    public string PackageFamily { get; set; } = "";
}