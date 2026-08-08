using Vega.Gerber.Models;
using Vega.StencilHistory.Data;
using Vega.StencilHistory.Models;
using Vega.StencilWorkflow;
using Vega.StencilWorkflow.Models;

namespace Vega.StencilHistory;

public class StencilHistoryService : IStencilHistorySink
{
    private readonly StencilHistoryRepository _repository;

    public StencilHistoryService(StencilHistoryRepository? repository = null)
    {
        _repository = repository ?? new StencilHistoryRepository();
    }

    public int CreateProject(StencilProjectRecord project) => _repository.CreateProject(project);
    public List<StencilProjectRecord> GetProjects() => _repository.GetProjects();
    public StencilProjectRecord? GetProject(int projectId) => _repository.GetProject(projectId);
    public List<StencilRevision> GetRevisions(int projectId) => _repository.GetRevisions(projectId);
    public int AddChange(StencilChangeRecord change) => _repository.AddChange(change);

    public StencilRevision CreateRevision(int projectId, StencilRevision revision, IEnumerable<StencilChangeRecord>? changes = null)
    {
        var revisions = _repository.GetRevisions(projectId);
        revision.Revision = string.IsNullOrWhiteSpace(revision.Revision) ? $"V{revisions.Count + 1:000}" : revision.Revision;
        var revisionId = _repository.CreateRevision(new StencilRevision
        {
            ProjectId = projectId, Revision = revision.Revision, CreatedDate = revision.CreatedDate, Description = revision.Description,
            OriginalPasteFile = revision.OriginalPasteFile, CorrectedPasteFile = revision.CorrectedPasteFile,
            MarkingFile = revision.MarkingFile, ReportFile = revision.ReportFile, ChangesCount = revision.ChangesCount,
            WarningsCount = revision.WarningsCount, FrameName = revision.FrameName
        });
        foreach (var change in changes ?? Array.Empty<StencilChangeRecord>())
        {
            _repository.AddChange(new StencilChangeRecord
            {
                RevisionId = revisionId, RefDes = change.RefDes, ChangeType = change.ChangeType,
                Before = change.Before, After = change.After, Reason = change.Reason
            });
        }
        return _repository.GetRevisions(projectId).Single(revisionItem => revisionItem.Id == revisionId);
    }

    public void RecordGenerated(StencilManufacturingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        var record = _repository.GetProjects().FirstOrDefault(item => item.ProjectName.Equals(project.ProjectName, StringComparison.OrdinalIgnoreCase));
        var projectId = record?.Id ?? _repository.CreateProject(new StencilProjectRecord
        {
            ProjectName = project.ProjectName, BoardName = project.ProjectName, Status = project.Status,
            InputSource = project.InputProject?.SourceType.ToString() ?? "", SourceFiles = project.InputProject?.SourceFiles ?? [],
            FrameName = project.Frame?.Name ?? "", PasteSide = project.CorrectedPaste?.Side ?? project.OriginalPaste?.Side ?? ""
        });
        var changes = project.CorrectedPaste?.Changes ?? Array.Empty<PasteCorrectionChange>();
        var revision = CreateRevision(projectId, new StencilRevision
        {
            Description = changes.Count == 0 ? "Paste imported" : "Gerber correction generated",
            OriginalPasteFile = project.PasteSource,
            CorrectedPasteFile = OutputFile(project.OutputFiles, ".gtp", ".gbp"),
            MarkingFile = OutputFile(project.OutputFiles, ".gbr"), ReportFile = OutputFile(project.OutputFiles, ".txt"),
            ChangesCount = changes.Count, WarningsCount = project.AnalysisResult?.WarningCount ?? 0, FrameName = project.Frame?.Name ?? ""
        }, changes.Select(change => new StencilChangeRecord
        {
            RefDes = change.RefDes, ChangeType = MapChangeType(change.ChangeType, change.NewGeometry), Before = change.OriginalGeometry,
            After = change.NewGeometry, Reason = change.Reason
        }));
    }

    public StencilProjectReport CreateProjectReport(int projectId, int? revisionId = null)
    {
        var project = _repository.GetProject(projectId) ?? throw new ArgumentException("Stencil project was not found.", nameof(projectId));
        var revisions = _repository.GetRevisions(projectId);
        var revision = revisionId.HasValue ? revisions.Single(item => item.Id == revisionId.Value) : revisions.LastOrDefault();
        IReadOnlyList<StencilChangeRecord> changes = revision is null ? Array.Empty<StencilChangeRecord>() : _repository.GetChanges(revision.Id);
        return new StencilProjectReport
        {
            ProjectName = project.ProjectName, CustomerName = project.CustomerName, Revision = revision?.Revision ?? "",
            Input = project.InputSource, Frame = revision?.FrameName ?? project.FrameName,
            Changes = changes.GroupBy(change => $"{change.RefDes} {change.ChangeType}").Select(group => $"{group.Key} x{group.Count()}").ToList(),
            Status = project.Status.ToString()
        };
    }

    public string ExportProjectReport(int projectId, string outputFile, int? revisionId = null)
    {
        if (string.IsNullOrWhiteSpace(outputFile)) throw new ArgumentException("Output file is required.", nameof(outputFile));
        var report = CreateProjectReport(projectId, revisionId);
        var directory = Path.GetDirectoryName(Path.GetFullPath(outputFile));
        Directory.CreateDirectory(directory!);
        File.WriteAllText(outputFile, report.ToText());
        return outputFile;
    }

    private static string OutputFile(IEnumerable<string> files, params string[] extensions) => files.FirstOrDefault(file => extensions.Any(extension => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase))) ?? "";
    private static StencilChangeType MapChangeType(string changeType, string newGeometry)
    {
        if (changeType.Contains("WindowPane", StringComparison.OrdinalIgnoreCase) || changeType.Contains("Segmentation", StringComparison.OrdinalIgnoreCase)) return StencilChangeType.WindowPane;
        if (newGeometry.Contains("HomePlate", StringComparison.OrdinalIgnoreCase)) return StencilChangeType.HomePlate;
        if (newGeometry.Contains("Snubnose", StringComparison.OrdinalIgnoreCase)) return StencilChangeType.Snubnose;
        if (changeType.Contains("Shape", StringComparison.OrdinalIgnoreCase)) return StencilChangeType.ShapeChange;
        return StencilChangeType.ApertureReduction;
    }
}