using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;
using Vega.ProductionTracking.Data;
using Vega.ProductionTracking.Models;
using Vega.QualityAnalytics;
using Vega.QualityAnalytics.Data;
using Vega.Report;
using Vega.Report.Models;
using Xunit;

namespace Vega.Tests;

public class QualityAnalyticsTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "VegaQuality", Guid.NewGuid().ToString());
    private readonly ProcessLearningRepository _process;
    private readonly QualityAnalyticsService _service;
    private readonly int _lotId;

    public QualityAnalyticsTests()
    {
        _process = new ProcessLearningRepository(Path.Combine(_directory, "ProcessLearning.db"));
        var production = new ProductionTrackingRepository(Path.Combine(_directory, "ProductionTracking.db"), _process);
        _lotId = production.CreateLot(new ProductionLot { OrderNumber = "Q-001", BoardName = "Controller", StencilRevisionId = 2 });
        _service = new QualityAnalyticsService(new QualityAnalyticsRepository(Path.Combine(_directory, "QualityAnalytics.db")), _process);
        AddDefect("R1", ProcessDefectType.SolderBridge, ProcessDefectSeverity.High, 9);
        AddDefect("R2", ProcessDefectType.SolderBall, ProcessDefectSeverity.Medium, 5);
        AddDefect("U1", ProcessDefectType.Void, ProcessDefectSeverity.High, 3);
    }

    [Fact]
    public void AnalyzeLot_CalculatesYield()
    {
        var result = _service.AnalyzeLot(_lotId, totalBoards: 100, firstPassBoards: 94, totalComponents: 10_000);

        Assert.Equal(97, result.Yield);
        Assert.Equal(94, result.FPY);
        Assert.Equal(3, result.DefectBoards);
    }

    [Fact]
    public void CalculatePpm_UsesTotalComponentCount()
    {
        Assert.Equal(1_700, _service.CalculatePpm(17, 10_000));
    }

    [Fact]
    public void BuildPareto_OrdersDefectsByCount()
    {
        var pareto = _service.BuildPareto(_lotId, _process.GetDefectsByProductionLot(_lotId));

        Assert.Equal(3, pareto.Count);
        Assert.Equal("SolderBridge", pareto[0].DefectName);
        Assert.InRange(pareto[0].Percentage, 52, 54);
    }

    [Fact]
    public void CompareRevisions_ReturnsImprovement()
    {
        var comparison = _service.CompareRevisions("V001", 120, "V002", 20);

        Assert.Equal(83.333, comparison.ImprovementPercent, 2);
        Assert.Contains("improved", comparison.Conclusion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateImprovementExperience_SavesValidatedExperience()
    {
        _service.CreateImprovementExperience(42, ProcessDefectType.SolderBall, "Rectangle", "Snubnose", "90%", "85%");

        var experience = Assert.Single(_process.GetExperience(42));
        Assert.Equal(ProcessExperienceResult.Improved, experience.Result);
        Assert.Equal("Snubnose", experience.NewStrategy);
    }

    [Fact]
    public void QualityAnalysis_WritesReportSection()
    {
        var result = _service.AnalyzeLot(_lotId, 100, 94, 10_000);
        var comparison = _service.CompareRevisions("V001", 120, "V002", 20);
        var report = new StencilTechnicalReport { QualityAnalysis = QualityAnalysisReportMapper.ToReportItem(result, comparison) };
        var output = Path.Combine(_directory, "quality.txt");

        new StencilReportGeneratorService().GenerateTXT(report, output);
        var text = File.ReadAllText(output);

        Assert.Contains("QUALITY ANALYSIS", text);
        Assert.Contains("SolderBridge", text);
        Assert.Contains("V001", text);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private void AddDefect(string reference, ProcessDefectType type, ProcessDefectSeverity severity, int quantity) => _process.AddDefect(new ProcessDefectRecord { PackageId = 42, ComponentRef = reference, ProductionLotId = _lotId, DefectType = type, Severity = severity, Quantity = quantity });
}