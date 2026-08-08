using Vega.Data.MasterLibrary.Database;
using Vega.Data.MasterLibrary.Repository;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class StencilTechnologyRuleServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly StencilTechnologyRuleService _service = new();

    [Theory]
    [InlineData("R0603", ApertureStrategy.StandardPasteRelease, "Rectangle", ApertureShapeType.Rectangle)]
    [InlineData("R0603", ApertureStrategy.AntiSolderBall, "Snubnose", ApertureShapeType.Snubnose)]
    [InlineData("QFP-0.5", ApertureStrategy.FinePitch, "HomePlate", ApertureShapeType.HomePlate)]
    [InlineData("QFN-32", ApertureStrategy.VoidReduction, "WindowPane", ApertureShapeType.Array)]
    [InlineData("BGA-144", ApertureStrategy.BGARelease, "Round", ApertureShapeType.Round)]
    [InlineData("MELF-1206", ApertureStrategy.StandardPasteRelease, "MELF", ApertureShapeType.MELF)]
    public void GetRule_ReturnsExpectedRuleAndShape(
        string packageName,
        ApertureStrategy strategy,
        string preferredShape,
        ApertureShapeType expectedShape)
    {
        var package = new PackageDefinition { PackageName = packageName };

        var rule = _service.GetRule(package, strategy);
        var shape = _service.GetPreferredShape(package, strategy);

        Assert.NotNull(rule);
        Assert.Equal(preferredShape, rule!.PreferredShape);
        Assert.Equal(expectedShape, shape);
        Assert.False(string.IsNullOrWhiteSpace(rule.Source));
        Assert.Equal("Internal SMT Technology Rule", rule.DocumentReference);
    }

    [Fact]
    public void Repository_AddUpdateAndSetActive_ManagesTechnologyRule()
    {
        var repository = new StencilTechnologyRuleRepository();
        var rule = new StencilTechnologyRule
        {
            PackageFamily = "TEST", PackageName = "TASK027", ComponentType = "Test",
            TechnologyGoal = "StandardPasteRelease", PreferredShape = "Rectangle",
            AlternativeShape = "", RecommendedThickness = 0.12, MinAreaRatio = 0.66,
            MinAspectRatio = 1.5, Coverage = 100, Source = "Internal production experience",
            DocumentReference = "Internal SMT Technology Rule", TechnologyReason = "Repository test",
            Priority = 1, IsActive = true
        };

        var id = repository.Add(rule);
        rule.Id = id;
        rule.PreferredShape = "Snubnose";
        repository.Update(rule);
        repository.SetActive(id, false);

        Assert.DoesNotContain(repository.GetByPackage("TASK027"), item => item.Id == id);

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM StencilTechnologyRule WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public void Dispose() => _database.Dispose();
}

