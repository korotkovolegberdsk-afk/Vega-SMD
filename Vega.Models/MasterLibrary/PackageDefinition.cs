namespace Vega.Models.MasterLibrary;

public class PackageDefinition
{
    public int Id { get; set; }


    // Основная информация

    public string PackageName { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public string Description { get; set; } = "";


    // Классификация

    public int CategoryId { get; set; }

    public int FamilyId { get; set; }


    // Геометрия корпуса

    public double Length { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }


    // Выводы и контактные площадки

    public double Pitch { get; set; }

    public int LeadCount { get; set; }

    public int PadCount { get; set; }

    public int ThermalPadCount { get; set; }


    // Стандарты

    public string IPCName { get; set; } = "";

    public string JEDECName { get; set; } = "";

    public string LandPatternName { get; set; } = "";


    // Полярность

    public string PolarityMark { get; set; } = "";


    // Документация

    public string DatasheetUrl { get; set; } = "";

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