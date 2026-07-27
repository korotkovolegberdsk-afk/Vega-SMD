namespace Vega.Models.MasterLibrary;

public class PackageFamily
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;


    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = "";

    public int Version { get; set; }

    public string ChangeComment { get; set; } = "";
}