using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageDefinitionServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _service = new();

    [Fact]
    public void GetAll_Should_Return_Added_Package()
    {
        var package = AddPackage("SERVICE-GET-ALL");

        var packages = _service.GetAll();

        Assert.Contains(packages, x => x.Id == package.Id);
    }

    [Fact]
    public void GetById_Should_Return_Added_Package()
    {
        var package = AddPackage("SERVICE-GET-BY-ID");

        var result = _service.GetById(package.Id);

        Assert.NotNull(result);
        Assert.Equal(package.PackageName, result!.PackageName);
    }

    [Fact]
    public void Add_Should_Save_Valid_Package()
    {
        var package = _database.CreatePackage("SERVICE-ADD");

        _service.Add(package);

        Assert.Contains(
            _service.GetAll(),
            x => x.PackageName == package.PackageName);
    }

    [Fact]
    public void Update_Should_Save_Valid_Package()
    {
        var package = AddPackage("SERVICE-UPDATE");
        package.DisplayName = "Updated by service";
        package.JEDECName = "JEDEC-UPDATED";

        _service.Update(package);

        var result = _service.GetById(package.Id);

        Assert.NotNull(result);
        Assert.Equal("Updated by service", result!.DisplayName);
        Assert.Equal("JEDEC-UPDATED", result.JEDECName);
    }

    [Fact]
    public void Deactivate_Should_Set_Package_Inactive()
    {
        var package = AddPackage("SERVICE-DEACTIVATE");

        _service.Deactivate(package.Id);

        var result = _service.GetById(package.Id);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    private PackageDefinition AddPackage(string prefix)
    {
        var package = _database.CreatePackage(prefix);
        _service.Add(package);

        return _service.GetAll()
            .Single(x => x.PackageName == package.PackageName);
    }
}
