using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class PackageManufacturerDrawingAssetServiceTests
{
    [Fact]
    public void ValidateForRenderingAcceptsVerifiedAssetAndNormalizedCrop()
    {
        var geometry = new PackageManufacturerDrawingGeometry { Id = 12, VerificationStatus = "Verified", IsCurrent = true };
        var asset = ValidAsset();
        var result = new PackageManufacturerDrawingAssetService().ValidateForRendering(geometry, asset);
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Theory]
    [InlineData("{\"page\":1,\"x\":0.8,\"y\":0,\"width\":0.4,\"height\":1,\"coordinateSystem\":\"normalized-page\"}")]
    [InlineData("{\"page\":1,\"x\":0,\"y\":0,\"width\":1,\"height\":0,\"coordinateSystem\":\"normalized-page\"}")]
    [InlineData("{\"page\":1,\"x\":0,\"y\":0,\"width\":1,\"height\":1,\"coordinateSystem\":\"mm\"}")]
    public void ValidateForRenderingRejectsInvalidCrop(string crop)
    {
        var geometry = new PackageManufacturerDrawingGeometry { Id = 12, VerificationStatus = "Verified", IsCurrent = true };
        var asset = ValidAsset(); asset.CropReference = crop;
        Assert.False(new PackageManufacturerDrawingAssetService().ValidateForRendering(geometry, asset).IsValid);
    }

    private static PackageManufacturerDrawingAsset ValidAsset() => new()
    {
        PackageManufacturerDrawingGeometryId = 12, ProjectionType = "Top", AssetType = "RasterImage",
        SourceDocument = "Infineon Package Outline", SourceRevision = "05_00", SourcePage = "1",
        DocumentReference = "https://example.invalid/outline.pdf", VerificationStatus = "Verified",
        IsCurrent = true, IsActive = true, Sha256 = new string('a', 64),
        CropReference = "{\"page\":1,\"x\":0,\"y\":0,\"width\":1,\"height\":1,\"coordinateSystem\":\"normalized-page\"}"
    };
}
