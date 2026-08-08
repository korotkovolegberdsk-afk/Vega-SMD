using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.ProcessLearning;
using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;
using Vega.TechnologyDecision;
using Vega.TechnologyDecision.Models;
using Xunit;

namespace Vega.Tests;

public class ProcessLearningTests : IDisposable
{
    private readonly MasterLibrary.PackageDefinitionMasterLibraryTestDatabase _masterLibrary = new();
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), "VegaProcessLearning", Guid.NewGuid() + ".db");
    private readonly ProcessLearningRepository _repository;
    private readonly ProcessLearningService _service;
    private readonly PackageDefinitionRepository _packages = new();

    public ProcessLearningTests()
    {
        _repository = new ProcessLearningRepository(_databasePath);
        _service = new ProcessLearningService(_repository, _packages);
    }

    [Fact]
    public void RegisterDefect_SavesPackageDefect()
    {
        var package = Package("R0603");
        var id = _service.RegisterDefect(new ProcessDefectRecord
        {
            ProjectId = 1, RevisionId = 1, ComponentRef = "R1", PackageId = package.Id,
            DefectType = ProcessDefectType.SolderBall, Severity = ProcessDefectSeverity.Medium, Quantity = 3, Description = "Paste balls"
        });

        var defect = Assert.Single(_repository.GetDefectsByPackage(package.Id));
        Assert.True(id > 0);
        Assert.Equal(ProcessDefectType.SolderBall, defect.DefectType);
    }

    [Fact]
    public void CreateExperienceRule_SavesExperience()
    {
        var package = Package("R0603");
        var recommendation = _service.CreateExperienceRule(Experience(package.Id));

        var experience = Assert.Single(_repository.GetExperience(package.Id));
        Assert.Equal("Snubnose", recommendation.RecommendedStrategy);
        Assert.Equal(ProcessExperienceResult.Improved, experience.Result);
    }

    [Fact]
    public void SuggestImprovement_ReturnsBestImprovedExperience()
    {
        var package = Package("R0603");
        _service.CreateExperienceRule(Experience(package.Id, .70));
        _service.CreateExperienceRule(Experience(package.Id, .95));

        var recommendation = _service.SuggestImprovement(package.Id, ProcessDefectType.SolderBall);

        Assert.NotNull(recommendation);
        Assert.Equal("Snubnose", recommendation!.RecommendedStrategy);
        Assert.Equal(.95, recommendation.Confidence);
    }

    [Fact]
    public void TechnologyDecision_UsesHistoricalExperience()
    {
        var package = Package("R0603");
        _service.CreateExperienceRule(Experience(package.Id));
        var insights = _service.GetInsights(package.Id, [StencilDefectType.SolderBall]);

        var result = new TechnologyDecisionEngine().Evaluate(new TechnologyDecisionContext
        {
            PackageId = package.Id, TechnologyGoal = TechnologyDecisionGoal.AntiSolderBall,
            HistoricalDefects = [StencilDefectType.SolderBall], HistoricalExperience = insights, StencilThickness = .12
        });

        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.Snubnose, result.SelectedShape);
        Assert.Contains("Validated production history", result.Reason);
    }

    [Fact]
    public void AnalyzeHistory_ReturnsImprovedDecisionReport()
    {
        var package = Package("QFN");
        _service.RegisterDefect(new ProcessDefectRecord { PackageId = package.Id, DefectType = ProcessDefectType.Void, Severity = ProcessDefectSeverity.High, Quantity = 2 });
        _service.CreateExperienceRule(new ProcessExperienceRecord
        {
            PackageId = package.Id, DefectType = ProcessDefectType.Void, PreviousStrategy = "WindowPane 3x3", NewStrategy = "WindowPane 4x4",
            BeforeParameters = "3x3", AfterParameters = "4x4; web=0.25", Result = ProcessExperienceResult.Improved, Confidence = .90
        });

        var report = _service.AnalyzeHistory(package.Id);

        Assert.Single(report.Defects);
        Assert.Single(report.ImprovedDecisions);
        Assert.Equal(.90, report.Confidence);
    }

    public void Dispose()
    {
        _masterLibrary.Dispose();
        if (File.Exists(_databasePath)) File.Delete(_databasePath);
    }

    private PackageDefinition Package(string name) => _packages.GetAll().Single(item => item.PackageName == name);

    private static ProcessExperienceRecord Experience(int packageId, double confidence = .90) => new()
    {
        PackageId = packageId, DefectType = ProcessDefectType.SolderBall, PreviousStrategy = "Rectangle 90%", NewStrategy = "Snubnose",
        BeforeParameters = "reduction=10", AfterParameters = "reduction=15", Result = ProcessExperienceResult.Improved, Confidence = confidence
    };
}