using Vega.StencilInput.Models;
using Vega.StencilUI.ViewModels;
using Vega.StencilWorkflow.Models;
using Xunit;

namespace Vega.Tests;

public class StencilWorkspaceViewModelTests
{
    [Fact]
    public void CreateViewModel_ProvidesWorkflowCommands()
    {
        var viewModel = new StencilWorkspaceViewModel();

        Assert.NotNull(viewModel.CreateProjectCommand);
        Assert.NotNull(viewModel.LoadInputCommand);
        Assert.NotNull(viewModel.AnalyzeCommand);
        Assert.NotNull(viewModel.CorrectCommand);
        Assert.NotNull(viewModel.PreviewCommand);
        Assert.NotNull(viewModel.ExportCommand);
        Assert.True(viewModel.CreateProjectCommand.CanExecute(null));
    }

    [Fact]
    public void LoadProject_UpdatesWorkspaceStatus()
    {
        var viewModel = new StencilWorkspaceViewModel { ProjectName = "UI Board", InputSource = StencilInputSourceType.PasteOnly };
        viewModel.CreateProject();

        viewModel.LoadInput([TestData("paste-top.gtp")]);

        Assert.Equal(StencilWorkflowStatus.InputLoaded, viewModel.WorkflowStatus);
        Assert.Equal("UI Board", viewModel.ProjectName);
        Assert.Empty(viewModel.ErrorMessage);
    }

    [Fact]
    public void Analyze_ChangesStatusAndExposesAnalysis()
    {
        var viewModel = LoadedViewModel();

        viewModel.AnalyzeCommand.Execute(null);

        Assert.Equal(StencilWorkflowStatus.Analyzed, viewModel.WorkflowStatus);
        Assert.NotNull(viewModel.AnalysisResult);
        Assert.True(viewModel.AnalysisResult!.ApertureCount > 0);
    }

    [Fact]
    public void ExportCommand_InvokesWorkflowAndPublishesOutputFiles()
    {
        var output = Path.Combine(Path.GetTempPath(), "VegaStencilUi", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        try
        {
            var viewModel = LoadedViewModel();
            viewModel.OutputDirectory = output;
            viewModel.CorrectCommand.Execute(null);
            viewModel.PreviewCommand.Execute(null);
            viewModel.ExportCommand.Execute(null);

            Assert.Equal(StencilWorkflowStatus.Generated, viewModel.WorkflowStatus);
            Assert.Equal(3, viewModel.OutputFiles.Count);
            Assert.All(viewModel.OutputFiles, file => Assert.True(File.Exists(file)));
        }
        finally { Directory.Delete(output, true); }
    }

    private static StencilWorkspaceViewModel LoadedViewModel()
    {
        var viewModel = new StencilWorkspaceViewModel { ProjectName = "Workspace Board", InputSource = StencilInputSourceType.PasteOnly };
        viewModel.CreateProject();
        viewModel.LoadInput([TestData("paste-top.gtp")]);
        return viewModel;
    }

    private static string TestData(string name) => Path.Combine(AppContext.BaseDirectory, "TestData", name);
}