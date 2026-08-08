namespace Vega.Models.MasterLibrary;

public enum PackageDocumentType
{
    Drawing,
    Model3D,
    Datasheet,
    ApplicationNote
}

public class PackageDocument
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public PackageDocumentType DocumentType { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string Description { get; set; } = "";
}