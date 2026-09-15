namespace Vega.Models.MasterLibrary;

/// <summary>Verified three-dimensional model belonging to an exact MPN.</summary>
public sealed class ComponentCadModel
{
    public int Id { get; set; }
    public int ComponentDefinitionId { get; set; }
    public string ModelPath { get; set; } = "";
    public string FileSha256 { get; set; } = "";
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string SourceSystem { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string VerificationStatus { get; set; } = "";
    public string Notes { get; set; } = "";
}
