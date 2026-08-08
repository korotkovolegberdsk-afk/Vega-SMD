using Vega.EngineeringDashboard;
using Vega.EngineeringDashboard.Models;
using Vega.ProcessLearning.Models;
using Vega.QualityAnalytics.Models;
using Vega.Report;
using Vega.Report.Models;
using Vega.StencilHistory.Models;
using Vega.StencilWorkflow.Models;
using Xunit;

namespace Vega.Tests;

public class EngineeringDashboardTests
{
    private readonly EngineeringDashboardService _service = new();

    [Fact]
    public void LoadProjectDashboard_ReturnsProjectInformation()
    {
        var dashboard = LoadDashboard();

        Assert.Equal("Controller", dashboard.ProjectName);
        Assert.Equal("ABC", dashboard.Customer);
        Assert.Equal("V002", dashboard.StencilRevision);
    }

    [Fact]
    public void LoadProjectDashboard_ReturnsStencilInformation()
    {
        var dashboard = LoadDashboard();

        Assert.Equal("LPKF_DEFAULT", dashboard.Frame);
        Assert.Equal(4, dashboard.ChangeCount);
        Assert.Equal("original.gtp", dashboard.OriginalPaste);
    }

    [Fact]
    public void GetQualitySummary_ReturnsKpis()
    {
        var summary = _service.GetQualitySummary(CreateQuality());

        Assert.Equal(98, summary.Yield);
        Assert.Equal(96, summary.FPY);
        Assert.Equal(250, summary.PPM);
    }

    [Fact]
    public void LoadProjectDashboard_ShowsMainDefects()
    {
        var dashboard = LoadDashboard();

        Assert.Contains(dashboard.MainDefects, item => item.Contains("SolderBridge"));
        Assert.Contains(dashboard.Warnings, item => item.Category == Vega.EngineeringDashboard.Models.DashboardWarningCategory.Quality);
    }

    [Fact]
    public void GetRecommendations_IncludesConfirmedImprovement()
    {
        var recommendations = _service.GetRecommendations(null, new ProcessLearningReport { Package = "R0603", ImprovedDecisions = new[] { new ProcessExperienceRecord { PreviousStrategy = "Rectangle", NewStrategy = "Snubnose", Result = ProcessExperienceResult.Improved, Confidence = .9 } } }, CreateQuality());

        Assert.Contains(recommendations, item => item.Recommendation == "Snubnose" && item.Confidence == "High");
    }

    [Fact]
    public void EngineeringSummary_WritesReportSection()
    {
        var dashboard = LoadDashboard();
        var report = new StencilTechnicalReport { EngineeringSummary = EngineeringDashboardReportMapper.ToReportItem(dashboard) };
        var directory = Path.Combine(Path.GetTempPath(), "VegaEngineering", Guid.NewGuid().ToString());
        var output = Path.Combine(directory, "engineering.txt");
        try
        {
            new StencilReportGeneratorService().GenerateTXT(report, output);
            var text = File.ReadAllText(output);
            Assert.Contains("ENGINEERING SUMMARY", text);
            Assert.Contains("Controller", text);
            Assert.Contains("Yield", text);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private EngineeringDashboardData LoadDashboard() => _service.LoadProjectDashboard(
        new StencilProjectRecord { ProjectName = "Controller project", BoardName = "Controller", CustomerName = "ABC", FrameName = "LPKF_DEFAULT", Status = StencilWorkflowStatus.Generated },
        new StencilRevision { Revision = "V002", FrameName = "LPKF_DEFAULT", OriginalPasteFile = "original.gtp", CorrectedPasteFile = "corrected.gtp", ChangesCount = 4 },
        CreateQuality(), learning: new ProcessLearningReport { Package = "R0603", ImprovedDecisions = new[] { new ProcessExperienceRecord { PreviousStrategy = "Rectangle", NewStrategy = "Snubnose", Result = ProcessExperienceResult.Improved, Confidence = .9 } } }, componentCount: 150, apertureCount: 800);

    private static QualityAnalysisResult CreateQuality() => new() { ProductionLotId = 7, Yield = 98, FPY = 96, PPM = 250, TopDefects = new[] { new DefectStatistic { DefectName = "SolderBridge", Percentage = 45, Count = 9, Severity = ProcessDefectSeverity.High } }, Recommendations = new[] { "Review SolderBridge occurrences." } };
}