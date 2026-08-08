namespace Vega.Models.MasterLibrary;

public enum PackageRecognitionMatchType
{
    Exact,
    Contains,
    Regex,
    Geometry
}

public class PackageRecognitionRule
{
    public int Id { get; set; }
    public string Pattern { get; set; } = "";
    public int PackageId { get; set; }
    public int Priority { get; set; }
    public PackageRecognitionMatchType MatchType { get; set; }
}