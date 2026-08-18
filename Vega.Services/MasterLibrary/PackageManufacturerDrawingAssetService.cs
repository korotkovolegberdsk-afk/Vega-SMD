using System.Security.Cryptography;
using System.Text.Json;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public sealed class PackageManufacturerDrawingAssetValidationResult
{
    public List<string> Errors { get; } = [];
    public bool IsValid => Errors.Count == 0;
}

public sealed class PackageManufacturerDrawingAssetService
{
    private static readonly HashSet<string> ProjectionTypes = new(StringComparer.OrdinalIgnoreCase) { "Top", "Side", "End", "ThreeD" };
    private static readonly HashSet<string> AssetTypes = new(StringComparer.OrdinalIgnoreCase) { "RasterImage", "SvgImage", "PdfPage", "PdfRegion" };
    private readonly PackageManufacturerDrawingAssetRepository _repository = new();

    public PackageManufacturerDrawingAsset? GetVerifiedAsset(PackageManufacturerDrawingGeometry geometry, string projectionType)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        var asset = _repository.GetCurrentVerified(geometry.Id, projectionType);
        return asset is not null && ValidateForRendering(geometry, asset).IsValid ? asset : null;
    }

    public PackageManufacturerDrawingAsset? GetVerifiedAsset(PackageManufacturerDrawingGeometry geometry, string projectionType, string assetType)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        var asset = _repository.GetCurrentVerified(geometry.Id, projectionType, assetType);
        return asset is not null && ValidateForRendering(geometry, asset).IsValid ? asset : null;
    }

    public List<PackageManufacturerDrawingAsset> GetVerifiedAssets(PackageManufacturerDrawingGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        if (!IsVerifiedCurrent(geometry)) return [];
        return _repository.GetByGeometryId(geometry.Id).Where(x => ValidateForRendering(geometry, x).IsValid).ToList();
    }

    public PackageManufacturerDrawingAssetValidationResult ValidateForRendering(PackageManufacturerDrawingGeometry geometry, PackageManufacturerDrawingAsset asset)
    {
        ArgumentNullException.ThrowIfNull(geometry); ArgumentNullException.ThrowIfNull(asset);
        var result = new PackageManufacturerDrawingAssetValidationResult();
        if (asset.PackageManufacturerDrawingGeometryId != geometry.Id) result.Errors.Add("Asset geometry Id does not match.");
        if (!IsVerifiedCurrent(geometry)) result.Errors.Add("Geometry is not verified and current.");
        if (!string.Equals(asset.VerificationStatus, "Verified", StringComparison.OrdinalIgnoreCase)) result.Errors.Add("Asset is not verified.");
        if (!asset.IsCurrent) result.Errors.Add("Asset is not current.");
        if (!asset.IsActive) result.Errors.Add("Asset is inactive.");
        if (!ProjectionTypes.Contains(asset.ProjectionType)) result.Errors.Add("ProjectionType is not supported.");
        if (!AssetTypes.Contains(asset.AssetType)) result.Errors.Add("AssetType is not supported.");
        if (string.IsNullOrWhiteSpace(asset.SourceDocument)) result.Errors.Add("SourceDocument is required.");
        if (string.IsNullOrWhiteSpace(asset.SourceRevision)) result.Errors.Add("SourceRevision is required.");
        if (string.IsNullOrWhiteSpace(asset.SourcePage)) result.Errors.Add("SourcePage is required.");
        if (string.IsNullOrWhiteSpace(asset.FilePath) && string.IsNullOrWhiteSpace(asset.DocumentReference)) result.Errors.Add("FilePath or DocumentReference is required.");
        if (!string.IsNullOrWhiteSpace(asset.Sha256) && !IsSha256(asset.Sha256)) result.Errors.Add("Sha256 must be 64 hexadecimal characters.");
        ValidateCrop(asset.CropReference, result);
        return result;
    }

    private static bool IsVerifiedCurrent(PackageManufacturerDrawingGeometry g) => g.Id > 0 && g.IsCurrent && string.Equals(g.VerificationStatus, "Verified", StringComparison.OrdinalIgnoreCase);

    private static bool IsSha256(string value) => value.Length == 64 && value.All(Uri.IsHexDigit);

    private static void ValidateCrop(string raw, PackageManufacturerDrawingAssetValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        try
        {
            using var doc = JsonDocument.Parse(raw); var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) { result.Errors.Add("CropReference must be a JSON object."); return; }
            var required = new[] { "page", "x", "y", "width", "height", "coordinateSystem" };
            if (required.Any(x => !root.TryGetProperty(x, out _))) { result.Errors.Add("CropReference must contain normalized-page fields."); return; }
            if (!root.GetProperty("coordinateSystem").GetString()!.Equals("normalized-page", StringComparison.Ordinal)) result.Errors.Add("CropReference coordinateSystem must be normalized-page.");
            var page = root.GetProperty("page").GetInt32(); var x = root.GetProperty("x").GetDouble(); var y = root.GetProperty("y").GetDouble(); var w = root.GetProperty("width").GetDouble(); var h = root.GetProperty("height").GetDouble();
            if (page < 1 || x < 0 || x > 1 || y < 0 || y > 1 || w <= 0 || w > 1 || h <= 0 || h > 1 || x + w > 1 || y + h > 1) result.Errors.Add("CropReference normalized-page values are out of range.");
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or OverflowException) { result.Errors.Add("CropReference is not valid normalized-page JSON."); }
    }
}
