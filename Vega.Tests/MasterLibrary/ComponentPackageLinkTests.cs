using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class ComponentPackageLinkTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _packageService = new();
    private readonly ComponentDefinitionRepository _componentRepository = new();

    [Fact]
    public void Component_Should_Load_With_Assigned_Package()
    {
        var package = _database.CreatePackage("COMPONENT-LINK");
        _packageService.Add(package);
        var savedPackage = _packageService.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = $"LINK-{Guid.NewGuid():N}",
            Manufacturer = "Vega Test",
            ComponentType = "IC",
            Description = "Component package link test",
            PackageId = savedPackage.Id,
            Version = 1
        };

        _componentRepository.Add(component);

        var savedComponent = _componentRepository.GetAll()
            .Single(x => x.ManufacturerPartNumber == component.ManufacturerPartNumber);
        var loadedComponent = _componentRepository.GetById(savedComponent.Id);

        Assert.NotNull(loadedComponent);
        Assert.Equal(savedPackage.Id, loadedComponent!.PackageId);
        Assert.NotNull(loadedComponent.Package);
        Assert.Equal(savedPackage.Id, loadedComponent.Package!.Id);
        Assert.Equal(savedPackage.PackageName, loadedComponent.Package.PackageName);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
