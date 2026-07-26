namespace Vega.Models.Projects;

public class ProjectInfo
{
    public string ProjectName { get; set; } = string.Empty;

    public string Customer { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public string BoardName { get; set; } = string.Empty;

    public string Revision { get; set; } = string.Empty;

    public string ProjectFolder { get; set; } = string.Empty;

    public string Units { get; set; } = "mm";

    public DateTime Created { get; set; } = DateTime.Now;
}