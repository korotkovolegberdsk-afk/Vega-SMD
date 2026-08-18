namespace Vega.Models.MasterLibrary;

/// <summary>
/// An official manufacturer drawing asset associated with a verified package
/// geometry. Assets are display evidence; they are never converted into
/// inferred mechanical coordinates.
/// </summary>
public sealed class PackageManufacturerDrawingAsset
{
    public const string RasterImage = "RasterImage";
    public const string SvgImage = "SvgImage";
    public const string PdfPage = "PdfPage";
    public const string PdfRegion = "PdfRegion";

    public int Id { get; set; }
    public int PackageManufacturerDrawingGeometryId { get; set; }
    public int? ProjectionGeometryId { get; set; }

    public string ProjectionType { get; set; } = "";
    public string SourceDocument { get; set; } = "";
    public string SourceRevision { get; set; } = "";
    public string SourcePage { get; set; } = "";
    public string AssetType { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string DocumentReference { get; set; } = "";
    public string CropReference { get; set; } = "";
    public string VerificationStatus { get; set; } = "Unverified";

    public bool IsCurrent { get; set; }
    public bool IsActive { get; set; } = true;

    public string Sha256 { get; set; } = "";
    public string MimeType { get; set; } = "";
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
