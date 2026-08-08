using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class StencilRecommendationServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _packageService = new();
    private readonly StencilRecommendationService _service = new();
    private int _qfnFamilyId;

    [Theory]
    [InlineData("CHIP", "Resistor", ApertureShape.Rectangle, 0.12)]
    [InlineData("SOIC", "IC", ApertureShape.Rectangle, 0.12)]
    [InlineData("QFN", "IC", ApertureShape.RoundedRectangle, 0.10)]
    public void GetRulesByPackageFamily_Should_Return_Base_Rules(
        string packageFamily,
        string componentType,
        ApertureShape shape,
        double thickness)
    {
        var rule = Assert.Single(
            _service.GetRulesByPackageFamily(packageFamily));

        Assert.Equal(componentType, rule.ComponentType);
        Assert.Equal(shape, rule.ApertureShape);
        Assert.Equal(thickness, rule.RecommendedStencilThickness);
        Assert.Equal(0.66, rule.AreaRatioMinimum);
        Assert.Equal(1.5, rule.AspectRatioMinimum);
    }

    [Fact]
    public void GetRuleForPackage_Should_Use_Package_Family()
    {
        _qfnFamilyId = CreateQfnFamily();
        var package = _database.CreatePackage("QFN-RULE");
        package.FamilyId = _qfnFamilyId;
        _packageService.Add(package);

        var savedPackage = _packageService.GetAll()
            .Single(x => x.PackageName == package.PackageName);
        var rule = _service.GetRuleForPackage(savedPackage.Id, "IC");

        Assert.NotNull(rule);
        Assert.Equal("QFN", rule!.PackageFamily);
        Assert.Equal(ApertureShape.RoundedRectangle, rule.ApertureShape);
    }

    public void Dispose()
    {
        if (_qfnFamilyId > 0)
        {
            using var connection = MasterLibraryConnection.Create();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM PackageDefinition WHERE FamilyId = $id;";
                command.Parameters.AddWithValue("$id", _qfnFamilyId);
                command.ExecuteNonQuery();
            }

            using var familyCommand = connection.CreateCommand();
            familyCommand.CommandText = "DELETE FROM PackageFamily WHERE Id = $id;";
            familyCommand.Parameters.AddWithValue("$id", _qfnFamilyId);
            familyCommand.ExecuteNonQuery();
        }

        _database.Dispose();
    }

    private int CreateQfnFamily()
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO PackageFamily (CategoryId, Code, Name)
        VALUES ($categoryId, $code, 'QFN');
        SELECT last_insert_rowid();
        """;
        command.Parameters.AddWithValue("$categoryId", _database.CategoryId);
        command.Parameters.AddWithValue("$code", $"QFN_TEST_{Guid.NewGuid():N}");
        return Convert.ToInt32(command.ExecuteScalar());
    }
}