using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class PackageLibrarySearchTests
{
    private static PackageDefinitionService CreateService()
    {
        MasterLibraryMigrationRunner.Apply();
        return new PackageDefinitionService();
    }

    [Fact]
    public void Search_Finds_R0603_By_0603_Alias()
        => Assert.Contains(CreateService().Search("0603"), item => item.PackageName == "R0603");

    [Fact]
    public void Search_Finds_Sot23_By_Alias()
        => Assert.Contains(CreateService().Search("SOT-23"), item => item.PackageName == "SOT23");

    [Fact]
    public void Search_Finds_Yamaha_So08_Name()
        => Assert.Contains(CreateService().Search("SO08P127W078"), item => item.PackageName == "SO08P127W078");

    [Fact]
    public void Geometry_Finds_Qfn_By_Pitch_And_Body()
        => Assert.Contains(CreateService().FindByGeometry("QFN", 0.50, 5.00, 5.00), item => item.PackageName == "QFN032P050W500");

    [Fact]
    public void Geometry_Finds_Bga_By_BallPitch()
        => Assert.Contains(CreateService().FindByGeometry("BGA", ballPitch: 0.80), item => item.PackageName == "BGA064P080W800");

    [Fact]
    public void Seed_Does_Not_Contain_Duplicate_PackageNames()
        => Assert.Equal(CreateService().GetAll().Select(item => item.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count(), CreateService().GetAll().Count);
}