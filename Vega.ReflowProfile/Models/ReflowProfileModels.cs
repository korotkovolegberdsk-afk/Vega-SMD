namespace Vega.ReflowProfile.Models;

public enum ReflowProfileType
{
    LeadFree,
    LeadBased,
    LowTemperature,
    Custom
}

public enum ReflowProfileStatus
{
    OK,
    Warning,
    Fail
}

public class ReflowProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string EquipmentName { get; set; } = "";
    public string OvenModel { get; set; } = "";
    public string SolderPaste { get; set; } = "";
    public string PasteAlloy { get; set; } = "";
    public ReflowProfileType ProfileType { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string Operator { get; set; } = "";
    public string Notes { get; set; } = "";
}

public class ReflowProfilePoint
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public double TimeSeconds { get; set; }
    public double TemperatureC { get; set; }
    public string SensorChannel { get; set; } = "";
}

public class ReflowProfileAnalysis
{
    public int ProfileId { get; set; }
    public double RampRate { get; set; }
    public double SoakStart { get; set; }
    public double SoakEnd { get; set; }
    public double SoakTime { get; set; }
    public double LiquidusTemperature { get; set; }
    public double TimeAboveLiquidus { get; set; }
    public double PeakTemperature { get; set; }
    public double CoolingRate { get; set; }
    public ReflowProfileStatus Status { get; set; }
}

public class ReflowProfileRecommendation
{
    public int ProfileId { get; set; }
    public string Parameter { get; set; } = "";
    public double CurrentValue { get; set; }
    public string RecommendedRange { get; set; } = "";
    public ReflowProfileStatus Status { get; set; }
    public string Message { get; set; } = "";
}