using Vega.StencilHistory;
using Vega.StencilHistory.Data;
using Vega.StencilHistory.Models;
using Vega.StencilInput.Models;
using Vega.StencilWorkflow;
using Vega.StencilWorkflow.Models;
using Xunit;

namespace Vega.Tests;

public class StencilHistoryServiceTests
{
    [Fact]
    public void CreateProject_PersistsProjectRecord()
    {
        using var database = new TemporaryHistoryDatabase();
        var service = database.Service;

        var id = service.CreateProject(Project("Controller Board", "LPKF_DEFAULT"));
        var project = service.GetProject(id);

        Assert.NotNull(project);
        Assert.Equal("Controller Board", project!.ProjectName);
        Assert.Equal("LPKF_DEFAULT", project.FrameName);
    }

    [Fact]
    public void CreateRevision_AssignsV001ThenV002()
    {
        using var database = new TemporaryHistoryDatabase();
        var projectId = database.Service.CreateProject(Project("Revision Board", "Frame A"));

        var first = database.Service.CreateRevision(projectId, new StencilRevision { Description = "Original paste imported", FrameName = "Frame A" });
        var second = database.Service.CreateRevision(projectId, new StencilRevision { Description = "QFN thermal corrected", FrameName = "Frame B" });

        Assert.Equal("V001", first.Revision);
        Assert.Equal("V002", second.Revision);
        Assert.Equal(2, database.Service.GetRevisions(projectId).Count);
    }

    [Fact]
    public void AddChange_PersistsApertureHistory()
    {
        using var database = new TemporaryHistoryDatabase();
        var projectId = database.Service.CreateProject(Project("Change Board", "Frame A"));
        var revision = database.Service.CreateRevision(projectId, new StencilRevision { ChangesCount = 1, FrameName = "Frame A" },
        [new StencilChangeRecord { RefDes = "U5", ChangeType = StencilChangeType.WindowPane, Before = "6x6 Solid", After = "WindowPane 4x4", Reason = "QFN thermal pad optimization" }]);

        var report = database.Service.CreateProjectReport(projectId, revision.Id);

        Assert.Contains(report.Changes, change => change.Contains("U5 WindowPane x1", StringComparison.Ordinal));
    }

    [Fact]
    public void GetProjectHistory_ReturnsProjectsAndRevisions()
    {
        using var database = new TemporaryHistoryDatabase();
        var projectId = database.Service.CreateProject(Project("History Board", "Frame A"));
        database.Service.CreateRevision(projectId, new StencilRevision { FrameName = "Frame A" });

        Assert.Contains(database.Service.GetProjects(), project => project.Id == projectId);
        Assert.Single(database.Service.GetRevisions(projectId));
    }

    [Fact]
    public void ExistingRevision_KeepsFrameSnapshotAfterDefaultFrameChanges()
    {
        using var database = new TemporaryHistoryDatabase();
        var projectId = database.Service.CreateProject(Project("Frame Snapshot", "LPKF_DEFAULT"));
        var revision = database.Service.CreateRevision(projectId, new StencilRevision { FrameName = "LPKF_DEFAULT" });
        var newDefaultFrame = "LPKF_FRAME_2";

        var stored = database.Service.GetRevisions(projectId).Single();

        Assert.Equal("LPKF_DEFAULT", stored.FrameName);
        Assert.NotEqual(newDefaultFrame, stored.FrameName);
        Assert.Equal(revision.Id, stored.Id);
    }

    [Fact]
    public void ExportGerber_RecordsWorkflowRevisionAndProjectReport()
    {
        using var database = new TemporaryHistoryDatabase();
        var workflow = new StencilManufacturingService(historySink: database.Service);
        var project = workflow.CreateProject("Workflow History Board");
        var outputDirectory = Path.Combine(Path.GetTempPath(), "VegaStencilHistory", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        try
        {
            workflow.LoadInput(project, StencilInputSourceType.PasteOnly, [TestData("paste-top.gtp")]);
            workflow.AnalyzePaste(project);
            workflow.ApplyCorrections(project);
            workflow.PlaceOnFrame(project);
            workflow.GenerateMarking(project);
            workflow.CreatePreview(project);
            workflow.ExportGerber(project, outputDirectory);

            var record = database.Service.GetProjects().Single(item => item.ProjectName == project.ProjectName);
            var revision = Assert.Single(database.Service.GetRevisions(record.Id));
            var reportFile = database.Service.ExportProjectReport(record.Id, Path.Combine(outputDirectory, "history.txt"));

            Assert.Equal("V001", revision.Revision);
            Assert.Equal(project.CorrectedPaste!.Changes.Count, revision.ChangesCount);
            Assert.True(File.Exists(reportFile));
        }
        finally { Directory.Delete(outputDirectory, true); }
    }

    private static StencilProjectRecord Project(string name, string frame) => new()
    {
        ProjectName = name, BoardName = name, CustomerName = "ABC", Status = StencilWorkflowStatus.Created,
        InputSource = "PasteOnly", SourceFiles = ["TOP.GTP"], FrameName = frame, PasteSide = "Top", Operator = "Engineer"
    };
    private static string TestData(string name) => Path.Combine(AppContext.BaseDirectory, "TestData", name);

    private sealed class TemporaryHistoryDatabase : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "VegaStencilHistoryTests", Guid.NewGuid().ToString("N"));
        public StencilHistoryService Service { get; }
        public TemporaryHistoryDatabase()
        {
            Directory.CreateDirectory(_directory);
            Service = new StencilHistoryService(new StencilHistoryRepository(Path.Combine(_directory, "StencilHistory.db")));
        }
        public void Dispose() => Directory.Delete(_directory, true);
    }
}