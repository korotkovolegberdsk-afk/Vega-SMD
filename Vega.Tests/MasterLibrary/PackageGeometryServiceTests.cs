using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageGeometryServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _service = new();

    [Fact]
    public void Geometry_Should_Add_And_Update_For_Package()
    {
        var package = _database.CreatePackage("GEOMETRY");
        _service.Add(package);
        var savedPackage = _service.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        var geometry = new PackageGeometry
        {
            PackageId = savedPackage.Id,
            BodyLength = 5.2,
            BodyWidth = 4.4,
            BodyHeight = 1.1,
            LeadLength = 0.8,
            LeadWidth = 0.3,
            LeadPitch = 0.65,
            LeadCount = 8,
            PadLength = 1.4,
            PadWidth = 0.5,
            PadPitch = 0.65,
            CenterX = 0.2,
            CenterY = -0.1
        };

        _service.AddGeometry(geometry);

        var savedGeometry = _service.GetGeometry(savedPackage.Id);
        Assert.NotNull(savedGeometry);
        Assert.Equal(5.2, savedGeometry!.BodyLength);
        Assert.Equal(8, savedGeometry.LeadCount);
        Assert.Equal(-0.1, savedGeometry.CenterY);

        savedGeometry.BodyLength = 5.4;
        savedGeometry.PadPitch = 0.7;
        _service.UpdateGeometry(savedGeometry);

        var updatedGeometry = _service.GetGeometry(savedPackage.Id);
        Assert.NotNull(updatedGeometry);
        Assert.Equal(5.4, updatedGeometry!.BodyLength);
        Assert.Equal(0.7, updatedGeometry.PadPitch);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
