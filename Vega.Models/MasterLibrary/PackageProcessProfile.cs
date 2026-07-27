namespace Vega.Models.MasterLibrary;

public class PackageProcessProfile
{
    public int Id { get; set; }


    // Связь с корпусом

    public int PackageId { get; set; }


    // Трафарет

    public double StencilThickness { get; set; }

    public string ApertureType { get; set; } = "";

    public double AreaRatio { get; set; }

    public double AspectRatio { get; set; }


    // SPI

    public string SPIRecommendations { get; set; } = "";


    // AOI

    public string AOIRecommendations { get; set; }


    // Типичные дефекты

    public string TypicalDefects { get; set; } = "";


    // Установка компонента

    public string PlacementRecommendations { get; set; } = "";


    // Оплавление

    public string ReflowRecommendations { get; set; } = "";


    // Приоритет контроля

    public string InspectionPriority { get; set; } = "";


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