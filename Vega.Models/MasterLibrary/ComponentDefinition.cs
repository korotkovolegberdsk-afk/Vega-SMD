namespace Vega.Models.MasterLibrary;

public class ComponentDefinition
{
    public int Id { get; set; }


    // Идентификация компонента

    public string ManufacturerPartNumber { get; set; } = "";

    public string Manufacturer { get; set; } = "";

    public string Description { get; set; } = "";


    // Тип компонента

    public string ComponentType { get; set; } = "";

    public string Value { get; set; } = "";

    public string Tolerance { get; set; } = "";

    public string VoltageRating { get; set; } = "";

    public string PowerRating { get; set; } = "";


    // Связь с корпусом

    public int PackageId { get; set; }

    public PackageDefinition? Package { get; set; }


    // Состояние компонента

    public string LifecycleStatus { get; set; } = "";

    public string DatasheetUrl { get; set; } = "";

    public string InternalPartNumber { get; set; } = "";

    public string Notes { get; set; } = "";


    public bool IsActive { get; set; } = true;


    // Аудит

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = "";

    public int Version { get; set; }

    public string ChangeComment { get; set; } = "";
}