using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.TechnologyKnowledge;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class TechnologyKnowledgeTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionRepository _packages = new();
    private readonly TechnologyKnowledgeService _service = new();

    [Fact]
    public void R0603_StandardAssembly_ReturnsRectangleWithTenPercentReduction()
    {
        var package = Package("R0603");
        var recommendation = _service.GetRecommendation(package, "StandardAssembly");
        var rule = _service.GetBestRule(package, "StandardAssembly");

        Assert.NotNull(recommendation);
        Assert.Contains("Rectangle", recommendation!.ParameterJson);
        Assert.Contains("10", recommendation.ParameterJson);
        Assert.NotNull(rule);
        Assert.Equal("Rectangle", rule!.PreferredShape);
        Assert.Equal(10, rule.ReductionX);
    }

    [Fact]
    public void Qfn_VoidReduction_ReturnsWindowPaneRecommendation()
    {
        var recommendation = _service.GetRecommendation(Package("QFN"), "VoidReduction");

        Assert.NotNull(recommendation);
        Assert.Contains("WindowPane", recommendation!.ParameterJson);
        Assert.Contains("coverageMin", recommendation.ParameterJson);
    }

    [Fact]
    public void Qfp_FinePitch_ReturnsHomePlateRecommendation()
    {
        var rule = _service.GetBestRule(Package("QFP"), "FinePitch");

        Assert.NotNull(rule);
        Assert.Equal("HomePlate", rule!.PreferredShape);
    }

    [Fact]
    public void Recommendation_ReturnsItsTechnologySource()
    {
        var recommendation = _service.GetRecommendation(Package("QFN"), "VoidReduction");
        var source = _service.GetSourceInformation(recommendation!.SourceId);

        Assert.NotNull(source);
        Assert.Equal("Indium", source!.Name);
        Assert.Equal(TechnologySourceType.SolderPasteManufacturer, source.SourceType);
    }

    [Fact]
    public void StrategySelector_UsesKnowledgeRecommendationGoal()
    {
        var package = Package("QFP");
        var strategy = new ApertureStrategySelectorService().SelectStrategy(
            new ComponentDefinition { ManufacturerPartNumber = "TEST-QFP", PackageId = package.Id },
            package,
            new ProcessCondition { IsFinePitch = true },
            _service);

        Assert.Equal(ApertureStrategy.FinePitch, strategy);
    }

    public void Dispose() => _database.Dispose();

    private PackageDefinition Package(string name) => _packages.GetAll().Single(item => item.PackageName == name);
}