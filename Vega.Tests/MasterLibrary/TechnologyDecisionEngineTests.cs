using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.TechnologyDecision;
using Vega.TechnologyDecision.Models;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class TechnologyDecisionEngineTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionRepository _packages = new();
    private readonly TechnologyDecisionEngine _engine = new();
    private int _temporaryRecommendationId;
    private int _temporaryRuleId;

    [Fact]
    public void R0603_StandardAssembly_SelectsRectangleWithTenPercentReduction()
    {
        var result = Evaluate("R0603", TechnologyDecisionGoal.StandardAssembly);

        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.Rectangle, result.SelectedShape);
        Assert.Equal("10", result.Parameters["reductionX"]);
        Assert.NotEmpty(result.Sources);
    }

    [Fact]
    public void R0603_AntiSolderBall_SelectsSnubnose()
    {
        var result = Evaluate("R0603", TechnologyDecisionGoal.AntiSolderBall);

        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.Snubnose, result.SelectedShape);
    }

    [Fact]
    public void Qfn_VoidReduction_SelectsWindowPane()
    {
        var result = Evaluate("QFN", TechnologyDecisionGoal.VoidReduction);

        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.Array, result.SelectedShape);
        Assert.Equal("50", result.Parameters["coverageMin"]);
        Assert.Equal("0.20", result.Parameters["webMin"]);
    }

    [Fact]
    public void Qfp_FinePitch_SelectsHomePlate()
    {
        var result = Evaluate("QFP", TechnologyDecisionGoal.FinePitch);

        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.HomePlate, result.SelectedShape);
    }

    [Fact]
    public void ConflictingRules_ProductionExperienceWinsOverIpcPriority()
    {
        var package = Package("R0603");
        var ipcSource = new TechnologySourceRepository().GetAll().Single(item => item.Name == "IPC-7525");
        _temporaryRuleId = new StencilTechnologyRuleRepository().Add(new StencilTechnologyRule
        {
            PackageFamily = "CHIP", PackageName = "R0603", TechnologyGoal = "StandardAssembly", PreferredShape = "Round",
            Priority = 999999, IsActive = true, TechnologySourceId = ipcSource.Id, ConfidenceLevel = 1
        });
        _temporaryRecommendationId = new TechnologyRecommendationRepository().Add(new TechnologyRecommendation
        {
            PackageId = package.Id, RuleId = _temporaryRuleId, SourceId = ipcSource.Id, TechnologyGoal = "StandardAssembly",
            RecommendationText = "Conflicting IPC rule", ParameterJson = "{\"shape\":\"Round\"}", Priority = 999999
        });

        var result = Evaluate("R0603", TechnologyDecisionGoal.StandardAssembly);

        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.Rectangle, result.SelectedShape);
        Assert.Equal("Internal SMT Experience", Assert.Single(result.Sources).Name);
    }

    [Fact]
    public void ExplainDecision_ReturnsReason()
    {
        var result = Evaluate("QFN", TechnologyDecisionGoal.VoidReduction);
        var explanation = _engine.ExplainDecision(result);

        Assert.Contains("WindowPane", explanation);
        Assert.False(string.IsNullOrWhiteSpace(result.Reason));
    }

    public void Dispose()
    {
        if (_temporaryRecommendationId != 0)
        {
            using var connection = MasterLibraryConnection.Create();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM MasterLibrary_TechnologyRecommendations WHERE Id = $recommendationId; DELETE FROM StencilTechnologyRule WHERE Id = $ruleId;";
            command.Parameters.AddWithValue("$recommendationId", _temporaryRecommendationId);
            command.Parameters.AddWithValue("$ruleId", _temporaryRuleId);
            command.ExecuteNonQuery();
        }
        _database.Dispose();
    }

    private TechnologyDecisionResult Evaluate(string packageName, TechnologyDecisionGoal goal)
    {
        var package = Package(packageName);
        return _engine.Evaluate(new TechnologyDecisionContext
        {
            PackageId = package.Id, PackageFamily = package.PackageFamily, StencilThickness = .12, TechnologyGoal = goal
        });
    }

    private PackageDefinition Package(string name) => _packages.GetAll().Single(item => item.PackageName == name);
}