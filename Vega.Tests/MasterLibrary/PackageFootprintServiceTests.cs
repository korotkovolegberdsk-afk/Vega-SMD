using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageFootprintServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _service = new();

    [Fact]
    public void Footprint_Should_Add_And_Update_For_Package()
    {
        var package = _database.CreatePackage("FOOTPRINT");
        _service.Add(package);
        var savedPackage = _service.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        var footprint = new PackageFootprint
        {
            PackageId = savedPackage.Id,
            PatternName = "SOIC-8-1.27",
            StandardName = "IPC-7351",
            Description = "SOIC-8 land pattern",
            PadCount = 8,
            PadLength = 1.5,
            PadWidth = 0.6,
            PadPitch = 1.27,
            Pin1Offset = 0.4,
            RowCount = 2,
            ColumnCount = 4,
            PasteReduction = 0.1,
            ApertureType = "Rounded rectangle",
            SourceSystem = "Ultra Librarian",
            SourceUrl = "https://app.ultralibrarian.com/details/example",
            SourceMpn = "MLG1005S56NJTD25",
            SourceVariant = "IND_1005_TDK",
            VerificationStatus = "Verified"
        };

        _service.AddFootprint(footprint);

        var savedFootprint = _service.GetFootprint(savedPackage.Id);
        Assert.NotNull(savedFootprint);
        Assert.Equal("SOIC-8-1.27", savedFootprint!.PatternName);
        Assert.Equal(8, savedFootprint.PadCount);
        Assert.Equal("Rounded rectangle", savedFootprint.ApertureType);
        Assert.Equal("Ultra Librarian", savedFootprint.SourceSystem);
        Assert.Equal("MLG1005S56NJTD25", savedFootprint.SourceMpn);
        Assert.Equal("IND_1005_TDK", savedFootprint.SourceVariant);
        Assert.Equal("Verified", savedFootprint.VerificationStatus);

        savedFootprint.PadWidth = 0.65;
        savedFootprint.PasteReduction = 0.12;
        _service.UpdateFootprint(savedFootprint);

        var updatedFootprint = _service.GetFootprint(savedPackage.Id);
        Assert.NotNull(updatedFootprint);
        Assert.Equal(0.65, updatedFootprint!.PadWidth);
        Assert.Equal(0.12, updatedFootprint.PasteReduction);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
