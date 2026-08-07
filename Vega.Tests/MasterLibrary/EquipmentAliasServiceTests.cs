using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class EquipmentAliasServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _service = new();

    [Fact]
    public void EquipmentAlias_Should_Add_Update_And_Delete()
    {
        var package = _database.CreatePackage("EQUIPMENT-ALIAS");
        _service.Add(package);
        var savedPackage = _service.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        var alias = new EquipmentAlias
        {
            PackageId = savedPackage.Id,
            Vendor = "Yamaha",
            Alias = "SO08P127W078",
            Notes = "SO-8 package for placement equipment",
            IsActive = true
        };

        _service.AddEquipmentAlias(alias);

        var savedAlias = Assert.Single(
            _service.GetEquipmentAliases(savedPackage.Id));
        Assert.Equal("Yamaha", savedAlias.Vendor);
        Assert.Equal("SO08P127W078", savedAlias.Alias);

        savedAlias.Vendor = "Mirtec";
        savedAlias.Alias = "SOP08";
        savedAlias.Notes = "AOI alias";
        _service.UpdateEquipmentAlias(savedAlias);

        var updatedAlias = Assert.Single(
            _service.GetEquipmentAliases(savedPackage.Id));
        Assert.Equal("Mirtec", updatedAlias.Vendor);
        Assert.Equal("SOP08", updatedAlias.Alias);
        Assert.Equal("AOI alias", updatedAlias.Notes);

        _service.DeleteEquipmentAlias(updatedAlias.Id);

        Assert.Empty(_service.GetEquipmentAliases(savedPackage.Id));
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
