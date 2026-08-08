using Vega.PackageRecognition;
using Vega.PackageRecognition.Models;
using Xunit;

namespace Vega.Tests;

public class PackageRecognitionServiceTests : IDisposable
{
    private readonly MasterLibrary.PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageRecognitionService _service = new();

    [Fact]
    public void Footprint_R0603_RecognizesR0603Package()
    {
        var result = _service.Recognize(new PackageRecognitionInput { RefDes = "R1", FootprintName = "R0603" });

        Assert.NotNull(result.DetectedPackage);
        Assert.Equal("R0603", result.DetectedPackage!.PackageName);
        Assert.Equal(PackageRecognitionSource.FootprintName, result.RecognitionSource);
        Assert.NotNull(result.MatchedRule);
    }

    [Fact]
    public void YamahaComment_SO08_RecognizesSoicPackage()
    {
        var result = _service.Recognize(new PackageRecognitionInput { RefDes = "U1", Comment = "SO08P127W078" });

        Assert.NotNull(result.DetectedPackage);
        Assert.Equal("SO08", result.DetectedPackage!.PackageName);
        Assert.Equal("SOIC", result.PackageFamily);
        Assert.Equal(PackageRecognitionSource.PnPComment, result.RecognitionSource);
    }

    [Fact]
    public void YamahaComment_Qfp_RecognizesQfpPackage()
    {
        var result = _service.Recognize(new PackageRecognitionInput { RefDes = "U2", Comment = "QFP032P065W092" });

        Assert.NotNull(result.DetectedPackage);
        Assert.Equal("QFP", result.DetectedPackage!.PackageName);
        Assert.Equal(PackageRecognitionSource.PnPComment, result.RecognitionSource);
    }

    [Fact]
    public void Geometry_Chip0603_RecognizesR0603()
    {
        var result = _service.Recognize(new PackageRecognitionInput { RefDes = "R2", PadCount = 2, PadLength = 0.8, PadWidth = 0.6, PadPitch = 0.9 });

        Assert.NotNull(result.DetectedPackage);
        Assert.Equal("R0603", result.DetectedPackage!.PackageName);
        Assert.Equal(PackageRecognitionSource.Geometry, result.RecognitionSource);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void UnknownPackage_ReturnsManualWarning()
    {
        var result = _service.Recognize(new PackageRecognitionInput { RefDes = "X1", FootprintName = "CUSTOM_UNKNOWN", PadCount = 0 });

        Assert.Null(result.DetectedPackage);
        Assert.Equal(PackageRecognitionSource.Manual, result.RecognitionSource);
        Assert.NotEmpty(result.Warnings);
    }

    public void Dispose() => _database.Dispose();
}