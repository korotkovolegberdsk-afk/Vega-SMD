using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageDefinitionRepositoryTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionRepository _repository = new();

    [Fact]
    public void GetAll_Should_Return_Added_Package()
    {
        var package = AddPackage("REPOSITORY-GET-ALL");

        var packages = _repository.GetAll();

        Assert.Contains(packages, x => x.Id == package.Id);
    }

    [Fact]
    public void GetById_Should_Return_Added_Package()
    {
        var package = AddPackage("REPOSITORY-GET-BY-ID");

        var result = _repository.GetById(package.Id);

        Assert.NotNull(result);
        Assert.Equal(package.PackageName, result!.PackageName);
        Assert.Equal(package.CategoryId, result.CategoryId);
        Assert.Equal(package.FamilyId, result.FamilyId);
    }

    [Fact]
    public void Add_Should_Persist_All_Package_Fields()
    {
        var package = AddPackage("REPOSITORY-ADD");

        var result = _repository.GetById(package.Id);

        Assert.NotNull(result);
        Assert.Equal(5.1, result!.Length);
        Assert.Equal(8, result.LeadCount);
        Assert.Equal(1, result.ThermalPadCount);
        Assert.Equal("LP-TEST", result.LandPatternName);
        Assert.Equal("Package test notes", result.Notes);
        Assert.True(result.IsActive);
        Assert.Equal(1, result.Version);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotEqual(default, result.UpdatedAt);
    }

    [Fact]
    public void Update_Should_Persist_Changed_Fields()
    {
        var package = AddPackage("REPOSITORY-UPDATE");
        package.DisplayName = "Updated package";
        package.Length = 10.5;
        package.PadCount = 12;
        package.Notes = "Updated notes";

        _repository.Update(package);

        var result = _repository.GetById(package.Id);

        Assert.NotNull(result);
        Assert.Equal("Updated package", result!.DisplayName);
        Assert.Equal(10.5, result.Length);
        Assert.Equal(12, result.PadCount);
        Assert.Equal("Updated notes", result.Notes);
        Assert.Equal(2, result.Version);
    }

    [Fact]
    public void SetActive_Should_Deactivate_Package()
    {
        var package = AddPackage("REPOSITORY-SET-ACTIVE");

        _repository.SetActive(package.Id, false);

        var result = _repository.GetById(package.Id);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
        Assert.Equal(2, result.Version);
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    private PackageDefinition AddPackage(string prefix)
    {
        var package = _database.CreatePackage(prefix);
        _repository.Add(package);

        return _repository.GetAll()
            .Single(x => x.PackageName == package.PackageName);
    }
}
