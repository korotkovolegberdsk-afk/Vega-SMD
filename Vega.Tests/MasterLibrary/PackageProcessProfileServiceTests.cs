using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageProcessProfileServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _service = new();

    [Fact]
    public void Save_Should_Persist_Package_And_Process_Profile()
    {
        var package = _database.CreatePackage("PROCESS-PROFILE");
        var profile = new PackageProcessProfile
        {
            StencilThickness = 0.12,
            ApertureType = "Rounded rectangle",
            AreaRatio = 0.72,
            AspectRatio = 1.5,
            SPIRecommendations = "Inspect volume and offset.",
            AOIRecommendations = "Inspect polarity and solder joints.",
            TypicalDefects = "Tombstone",
            ReflowRecommendations = "Lead-free standard profile",
            Notes = "10% aperture reduction",
            IsActive = true
        };

        _service.Save(package, profile);

        var savedPackage = _service.GetAll()
            .Single(x => x.PackageName == package.PackageName);
        var savedProfile = _service.GetProcessProfile(savedPackage.Id);

        Assert.NotNull(savedProfile);
        Assert.Equal(0.12, savedProfile!.StencilThickness);
        Assert.Equal("Rounded rectangle", savedProfile.ApertureType);
        Assert.Equal(0.72, savedProfile.AreaRatio);
        Assert.Equal(1.5, savedProfile.AspectRatio);
        Assert.Equal("Inspect volume and offset.", savedProfile.SPIRecommendations);
        Assert.Equal("Inspect polarity and solder joints.", savedProfile.AOIRecommendations);
        Assert.Equal("Tombstone", savedProfile.TypicalDefects);
        Assert.Equal("Lead-free standard profile", savedProfile.ReflowRecommendations);
        Assert.Equal("10% aperture reduction", savedProfile.Notes);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
