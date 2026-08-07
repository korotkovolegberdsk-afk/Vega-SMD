using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageClassificationServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _service = new();

    [Fact]
    public void Add_Should_Persist_Selected_Category_And_Family()
    {
        var category = _service.GetCategories()
            .Single(x => x.Id == _database.CategoryId);

        var family = _service.GetFamilies(category.Id)
            .Single(x => x.Id == _database.FamilyId);

        var package = _database.CreatePackage("CLASSIFICATION-SAVE");
        package.CategoryId = category.Id;
        package.FamilyId = family.Id;

        _service.Add(package);

        var saved = _service.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        Assert.Equal(category.Id, saved.CategoryId);
        Assert.Equal(family.Id, saved.FamilyId);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
