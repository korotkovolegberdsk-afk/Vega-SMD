namespace Vega.StencilHistory.Models;

public enum StencilChangeType
{
    ApertureReduction,
    ShapeChange,
    WindowPane,
    HomePlate,
    Snubnose,
    ManualEdit
}

public class StencilChangeRecord
{
    public int Id { get; init; }
    public int RevisionId { get; init; }
    public string RefDes { get; set; } = "";
    public StencilChangeType ChangeType { get; set; }
    public string Before { get; set; } = "";
    public string After { get; set; } = "";
    public string Reason { get; set; } = "";
}