using Vega.Data.MasterLibrary.Database;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class ComponentTapeReelGeometryTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _packageService = new();
    private readonly ComponentDefinitionRepository _componentRepository = new();
    private readonly ComponentTapeReelGeometryService _service = new();
    private readonly List<int> _componentIds = [];

    [Fact]
    public void CreateTapeReelGeometry()
    {
        var component = CreateComponent();

        var created = _service.CreateProfile(NewProfile(component.Id));

        Assert.True(created.Id > 0);
        Assert.Equal(component.Id, created.ComponentDefinitionId);
    }

    [Fact]
    public void ReadByComponentId()
    {
        var component = CreateComponent();
        _service.CreateProfile(NewProfile(component.Id, "REEL-A"));
        _service.CreateProfile(NewProfile(component.Id, "REEL-B"));

        var profiles = _service.GetProfiles(component.Id);

        Assert.Equal(2, profiles.Count);
    }

    [Fact]
    public void UpdateTapeReelGeometry()
    {
        var component = CreateComponent();
        var profile = _service.CreateProfile(NewProfile(component.Id));
        profile.CarrierTapeWidth = 12;
        profile.PocketPitch = 8;
        profile.QuantityPerReel = 5_000;

        _service.UpdateProfile(profile);

        var updated = _service.GetProfiles(component.Id).Single(x => x.Id == profile.Id);
        Assert.Equal(12, updated.CarrierTapeWidth);
        Assert.Equal(8, updated.PocketPitch);
        Assert.Equal(5_000, updated.QuantityPerReel);
    }

    [Fact]
    public void DeleteTapeReelGeometry()
    {
        var component = CreateComponent();
        var profile = _service.CreateProfile(NewProfile(component.Id));

        _service.DeleteProfile(profile.Id);

        Assert.Empty(_service.GetProfiles(component.Id));
    }

    [Fact]
    public void SetDefaultProfile()
    {
        var component = CreateComponent();
        var first = _service.CreateProfile(NewProfile(component.Id, "REEL-A", isDefault: true));
        var second = _service.CreateProfile(NewProfile(component.Id, "REEL-B"));

        _service.SetDefaultProfile(component.Id, second.Id);

        var profiles = _service.GetProfiles(component.Id);
        Assert.False(profiles.Single(x => x.Id == first.Id).IsDefault);
        Assert.True(profiles.Single(x => x.Id == second.Id).IsDefault);
    }

    [Fact]
    public void OnlyOneDefaultProfile()
    {
        var component = CreateComponent();
        _service.CreateProfile(NewProfile(component.Id, "REEL-A", isDefault: true));
        _service.CreateProfile(NewProfile(component.Id, "REEL-B", isDefault: true));

        Assert.Single(_service.GetProfiles(component.Id).Where(x => x.IsDefault));
    }

    [Fact]
    public void OneComponentCanHaveMultipleTapeProfiles()
    {
        var component = CreateComponent();        var tape8 = NewProfile(component.Id, "8MM");
        tape8.CarrierTapeWidth = 8;
        tape8.PocketPitch = 4;
        var tape12 = NewProfile(component.Id, "12MM");
        tape12.CarrierTapeWidth = 12;
        tape12.PocketPitch = 8;
        _service.CreateProfile(tape8);
        _service.CreateProfile(tape12);

        var profiles = _service.GetProfiles(component.Id);

        Assert.Contains(profiles, x => x.CarrierTapeWidth == 8 && x.PocketPitch == 4);
        Assert.Contains(profiles, x => x.CarrierTapeWidth == 12 && x.PocketPitch == 8);
    }

    [Fact]
    public void RejectMissingComponent()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateProfile(NewProfile(999_999_999)));
    }

    [Fact]
    public void RejectNegativeDimensions()
    {
        var profile = NewProfile(CreateComponent().Id);
        profile.PocketDepth = -0.1;

        Assert.Throws<ArgumentException>(() => _service.CreateProfile(profile));
    }

    [Fact]
    public void RejectNegativeQuantity()
    {
        var profile = NewProfile(CreateComponent().Id);
        profile.QuantityPerReel = -1;

        Assert.Throws<ArgumentException>(() => _service.CreateProfile(profile));
    }

    [Theory]
    [InlineData(360, 0)]
    [InlineData(370, 10)]
    [InlineData(-90, 270)]
    public void NormalizePickupRotation(double source, double expected)
    {
        var profile = NewProfile(CreateComponent().Id);
        profile.PickupRotation = source;

        var created = _service.CreateProfile(profile);

        Assert.Equal(expected, created.PickupRotation);
    }

    [Fact]
    public void RejectInvalidPocketOrientation()
    {
        var profile = NewProfile(CreateComponent().Id);
        profile.PocketOrientation = (TapePocketOrientation)999;

        Assert.Throws<ArgumentException>(() => _service.CreateProfile(profile));
    }

    [Fact]
    public void RejectInvalidFeedDirection()
    {
        var profile = NewProfile(CreateComponent().Id);
        profile.FeedDirection = (TapeFeedDirection)999;

        Assert.Throws<ArgumentException>(() => _service.CreateProfile(profile));
    }

    [Fact]
    public void RejectInvalidVerificationStatus()
    {
        var profile = NewProfile(CreateComponent().Id);
        profile.VerificationStatus = (TapeVerificationStatus)999;

        Assert.Throws<ArgumentException>(() => _service.CreateProfile(profile));
    }

    [Fact]
    public void TapeGeometryDoesNotModifyPackageDefinition()
    {
        var package = CreatePackage();
        var snapshot = (package.Length, package.Width, package.Height, package.Pitch, package.LeadCount);
        var component = CreateComponent(package.Id);

        _service.CreateProfile(NewProfile(component.Id));

        var reloaded = _packageService.GetById(package.Id)!;
        Assert.Equal(snapshot, (reloaded.Length, reloaded.Width, reloaded.Height, reloaded.Pitch, reloaded.LeadCount));
    }

    [Fact]
    public void TapeGeometryDoesNotModifyPackageGeometry()
    {
        var package = CreatePackage();
        var geometry = new PackageGeometry { PackageId = package.Id, BodyLength = 5.2, BodyWidth = 4.4, BodyHeight = 1.1, LeadPitch = 0.65, LeadCount = 8 };
        _packageService.AddGeometry(geometry);
        var component = CreateComponent(package.Id);

        _service.CreateProfile(NewProfile(component.Id));

        var reloaded = _packageService.GetGeometry(package.Id)!;
        Assert.Equal(5.2, reloaded.BodyLength);
        Assert.Equal(4.4, reloaded.BodyWidth);
        Assert.Equal(0.65, reloaded.LeadPitch);
        Assert.Equal(8, reloaded.LeadCount);
    }

    [Fact(Skip = "ComponentDefinitionRepository.Delete has no dependency-protection mechanism yet; this test is prepared for the future deletion policy.")]
    public void ComponentDeleteBlockedWhenTapeGeometryExists()
    {
        var component = CreateComponent();
        _service.CreateProfile(NewProfile(component.Id));

        Assert.Throws<InvalidOperationException>(() => _componentRepository.Delete(component.Id));
    }

    public void Dispose()
    {
        using var connection = MasterLibraryConnection.Create();
        foreach (var componentId in _componentIds)
        {
            using var deleteTape = connection.CreateCommand();
            deleteTape.CommandText = "DELETE FROM ComponentTapeReelGeometry WHERE ComponentDefinitionId = $componentDefinitionId;";
            deleteTape.Parameters.AddWithValue("$componentDefinitionId", componentId);
            deleteTape.ExecuteNonQuery();

            using var deleteComponent = connection.CreateCommand();
            deleteComponent.CommandText = "DELETE FROM ComponentDefinition WHERE Id = $componentDefinitionId;";
            deleteComponent.Parameters.AddWithValue("$componentDefinitionId", componentId);
            deleteComponent.ExecuteNonQuery();
        }
        _database.Dispose();
    }

    private PackageDefinition CreatePackage()
    {
        var package = _database.CreatePackage("TAPE_REEL_PACKAGE");
        _packageService.Add(package);
        return _packageService.GetAll().Single(x => x.PackageName == package.PackageName);
    }

    private ComponentDefinition CreateComponent(int? packageId = null)
    {
        var package = packageId is null ? CreatePackage() : _packageService.GetById(packageId.Value)!;
        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = $"TAPE-REEL-{Guid.NewGuid():N}",
            Manufacturer = "Vega Test",
            Description = "Tape-and-reel test component",
            ComponentType = "IC",
            PackageId = package.Id,
            Version = 1
        };
        _componentRepository.Add(component);
        var saved = _componentRepository.GetAll().Single(x => x.ManufacturerPartNumber == component.ManufacturerPartNumber);
        _componentIds.Add(saved.Id);
        return saved;
    }

    private static ComponentTapeReelGeometry NewProfile(int componentId, string packagingCode = "REEL", bool isDefault = false) => new()
    {
        ComponentDefinitionId = componentId,
        PackagingCode = packagingCode,
        TapeStandard = "EIA-481",
        CarrierTapeWidth = 8,
        PocketPitch = 4,
        PocketLength = 2,
        PocketWidth = 1.5,
        PocketDepth = 0.8,
        SprocketHolePitch = 4,
        SprocketHoleDiameter = 1.5,
        ReelDiameter = 180,
        HubDiameter = 60,
        QuantityPerReel = 10_000,
        FeedDirection = TapeFeedDirection.LeftToRight,
        PocketOrientation = TapePocketOrientation.Deg0,
        VerificationStatus = TapeVerificationStatus.InternalVerified,
        IsDefault = isDefault
    };
}

