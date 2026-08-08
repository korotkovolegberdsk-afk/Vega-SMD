namespace Vega.DefectDictionary.Models;

public enum DefectCategory
{
    SolderPrinting,
    ComponentPlacement,
    Reflow,
    AOI,
    Inspection,
    Mechanical
}

public enum DefectSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public class DefectDefinition
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string EnglishName { get; set; } = "";
    public string RussianName { get; set; } = "";
    public DefectCategory Category { get; set; }
    public string DescriptionEN { get; set; } = "";
    public string DescriptionRU { get; set; } = "";
    public DefectSeverity Severity { get; set; }
    public string TypicalCause { get; set; } = "";
    public string TypicalSolution { get; set; } = "";
}