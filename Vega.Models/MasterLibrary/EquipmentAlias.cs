namespace Vega.Models.MasterLibrary;

public class EquipmentAlias
{
    public int Id { get; set; }


    // Связь с корпусом

    public int PackageId { get; set; }


    // Оборудование

    public string EquipmentType { get; set; } = "";

    public string Vendor { get; set; } = "";

    public string Model { get; set; } = "";


    // Имя корпуса в программе оборудования

    public string Alias { get; set; } = "";


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