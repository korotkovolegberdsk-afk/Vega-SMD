using Vega.EngineeringDashboard.Models;
using Vega.StencilUI.ViewModels;
using Xunit;

namespace Vega.Tests;

public class EngineeringWorkspaceTests
{
    [Fact]
    public void LoadDashboard_UpdatesProjectInformation()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.LoadDashboard(CreateDashboard(), 220);

        Assert.Equal("Controller", viewModel.ProjectName);
        Assert.Equal("V002", viewModel.StencilRevision);
        Assert.Equal("ABC", viewModel.Customer);
    }

    [Fact]
    public void LoadDashboard_ShowsQualityKpis()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.LoadDashboard(CreateDashboard(), 220);

        Assert.Equal(98.5, viewModel.Yield);
        Assert.Equal(96, viewModel.FPY);
        Assert.Equal(220, viewModel.PPM);
        Assert.Contains("Yield", viewModel.QualitySummary);
    }

    [Fact]
    public void LoadDashboard_ShowsWarnings()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.LoadDashboard(CreateDashboard());

        var warning = Assert.Single(viewModel.WarningList);
        Assert.Equal(DashboardWarningCategory.Stencil, warning.Category);
    }

    [Fact]
    public void LoadDashboard_ShowsRecommendations()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.LoadDashboard(CreateDashboard());

        var recommendation = Assert.Single(viewModel.RecommendationList);
        Assert.Equal("Snubnose", recommendation.Recommendation);
    }

    [Fact]
    public void ExportReportCommand_WritesEngineeringReport()
    {
        var directory = Path.Combine(Path.GetTempPath(), "VegaEngineeringUi", Guid.NewGuid().ToString("N"));
        try
        {
            var viewModel = new EngineeringWorkspaceViewModel { ReportOutputPath = Path.Combine(directory, "summary.txt") };
            viewModel.LoadDashboard(CreateDashboard());
            Assert.True(viewModel.ExportReportCommand.CanExecute(null));

            viewModel.ExportReportCommand.Execute(null);

            Assert.True(File.Exists(viewModel.ReportOutputPath));
            Assert.Contains("ENGINEERING SUMMARY", File.ReadAllText(viewModel.ReportOutputPath));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void AnalyzeStencilCommand_WithoutProject_ShowsRequiredMessage()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.AnalyzeStencilCommand.Execute(null);
        Assert.Equal("Для анализа трафарета загрузите проект", viewModel.StatusMessage);
    }

    [Fact]
    public void TechnologyDecisionCommand_WithoutProject_ShowsRequiredMessage()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.TechnologyDecisionCommand.Execute(null);
        Assert.Equal("Нет данных для анализа", viewModel.StatusMessage);
    }

    [Fact]
    public void OpenPreviewCommand_WithoutProject_ShowsRequiredMessage()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        viewModel.OpenPreviewCommand.Execute(null);
        Assert.Equal("Проект не загружен", viewModel.StatusMessage);
    }
    [Fact]
    public void ExportReportCommand_WithoutDashboard_ShowsRequiredMessage()
    {
        var viewModel = new EngineeringWorkspaceViewModel();
        Assert.True(viewModel.ExportReportCommand.CanExecute(null));
        viewModel.ExportReportCommand.Execute(null);
        Assert.Equal("Для создания отчёта загрузите проект", viewModel.StatusMessage);
    }
    private static EngineeringDashboardData CreateDashboard() => new()
    {
        ProjectName = "Controller", BoardRevision = "Rev A", Customer = "ABC", StencilRevision = "V002", TechnologyStatus = "Ready", Frame = "LPKF_DEFAULT", StencilThickness = .12,
        ApertureCount = 850, ChangeCount = 12, Yield = 98.5, FPY = 96, MainDefects = ["SolderBridge: 45%"],
        Warnings = [new DashboardWarning { Severity = DashboardWarningSeverity.Warning, Category = DashboardWarningCategory.Stencil, Message = "QFN thermal without WindowPane", Source = "Stencil" }],
        Recommendations = [new EngineeringRecommendation { Type = "Stencil", Component = "R0603", CurrentState = "Rectangle", Recommendation = "Snubnose", Reason = "Solder ball reduction", Confidence = "High" }],
        TechnologyDecisions = ["R0603: Rectangle → Snubnose"]
    };
}