namespace Vega.Models.MasterLibrary;

public class AuditInfo
{
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = "";


    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = "";


    public int Version { get; set; }

    public string ChangeComment { get; set; } = "";
}