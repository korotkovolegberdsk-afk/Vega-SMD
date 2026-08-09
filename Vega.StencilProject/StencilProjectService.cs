using Vega.Report;
using Vega.Gerber.Models;
using Vega.TechnologyDecision;
using Vega.TechnologyDecision.Models;
using Vega.Models.MasterLibrary;
using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Vega.StencilInput;
using Vega.StencilInput.Models;
using Vega.StencilProjects.Models;
using Vega.StencilWorkflow;
using Vega.StencilWorkflow.Models;

namespace Vega.StencilProjects;

public class StencilProjectService
{
    private static int _nextId;
    private readonly StencilInputManagerService _inputManager;
    private readonly StencilManufacturingService _workflow;
    private readonly StencilFrameLibraryService _frames;
    private readonly StencilReportGeneratorService _reports;

    public StencilProjectService(StencilInputManagerService? inputManager = null, StencilManufacturingService? workflow = null, StencilFrameLibraryService? frames = null, StencilReportGeneratorService? reports = null)
    {
        _inputManager = inputManager ?? new StencilInputManagerService();
        _workflow = workflow ?? new StencilManufacturingService();
        _frames = frames ?? new StencilFrameLibraryService();
        _reports = reports ?? new StencilReportGeneratorService();
    }

    public IReadOnlyList<StencilFrame> GetFrames()
    {
        try { return _frames.GetFrames(); }
        catch { return []; }
    }

    public StencilProjectSession Create(string projectName, string customer, StencilProjectInputSource source, StencilProjectPasteSide side, StencilFrame? frame = null)
    {
        if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is required.", nameof(projectName));
        var project = new Models.StencilProject
        {
            Id = Interlocked.Increment(ref _nextId), ProjectName = projectName.Trim(), Customer = customer?.Trim() ?? "",
            InputSource = source, PasteSide = side, FrameId = frame?.Id
        };
        System.Diagnostics.Debug.WriteLine("[INFO] Session created");
        return new StencilProjectSession { Project = project, Frame = frame };
    }

    public StencilProjectSession LoadPasteGerber(StencilProjectSession session, string? topPasteFile, string? bottomPasteFile, string? assemblyDrawing = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (string.IsNullOrWhiteSpace(topPasteFile) && string.IsNullOrWhiteSpace(bottomPasteFile)) throw new ArgumentException("Select a TOP or BOTTOM paste Gerber file.");
        var input = _inputManager.LoadPasteOnlyProject(topPasteFile, bottomPasteFile, assemblyDrawing);
        input.ProjectName = session.Project.ProjectName;
        var workflowProject = _workflow.CreateProject(session.Project.ProjectName);
        _workflow.LoadInput(workflowProject, input, session.Project.PasteSide == StencilProjectPasteSide.Bottom ? "Bottom" : "Top");
        session.Input = input;
        session.WorkflowProject = workflowProject;
        session.AnalysisContext = new StencilAnalysisContext { Apertures = input.PasteLayers.SelectMany(layer => layer.Apertures).ToArray(), Pads = workflowProject.OriginalPaste?.Primitives.ToArray() ?? Array.Empty<PastePrimitive>(), BoardBounds = input.BoardOutline, Side = workflowProject.OriginalPaste?.Side ?? "" };
        System.Diagnostics.Debug.WriteLine($"=== STENCIL INPUT RESULT === Apertures: {session.AnalysisContext.Apertures.Count}; OriginalPasteLayer.Primitives.Count: {workflowProject.OriginalPaste?.Primitives.Count ?? 0}");
        session.Project.InputFiles.Clear();
        AddInputFile(session.Project, topPasteFile, "PasteGerberTop"); AddInputFile(session.Project, bottomPasteFile, "PasteGerberBottom"); AddInputFile(session.Project, assemblyDrawing, "AssemblyDrawing");
        session.Project.Status = StencilProjectStatus.InputLoaded;
        return session;
    }

    public PasteAnalysisResult Analyze(StencilProjectSession session)
    {
        var workflowProject = RequireWorkflow(session);
        var result = _workflow.AnalyzePaste(workflowProject);
        _workflow.ApplyCorrections(workflowProject);
        session.AnalysisResult = result;
        session.CorrectedLayer = workflowProject.CorrectedPaste;
        session.Changes = workflowProject.CorrectedPaste?.Changes ?? Array.Empty<PasteCorrectionChange>();
        System.Diagnostics.Debug.WriteLine("=== ANALYZE RESULT ===");
        System.Diagnostics.Debug.WriteLine($"Context: {(session.AnalysisContext is null ? "null" : "created")}");
        System.Diagnostics.Debug.WriteLine($"Apertures: {session.AnalysisContext?.Apertures.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"Pads: {session.AnalysisContext?.Pads.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"Geometry: {session.AnalysisContext?.Pads.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"Changes: {workflowProject.CorrectedPaste?.Changes.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"Warnings: {result.WarningCount}");
        System.Diagnostics.Debug.WriteLine($"CorrectedLayer: {(workflowProject.CorrectedPaste is null ? "null" : "created")}");
        System.Diagnostics.Debug.WriteLine($"PreviewData: {(session.PreviewData is null ? "null" : "created")}");
        System.Diagnostics.Debug.WriteLine($"TechnologyDecision: {(session.TechnologyDecision is null ? "null" : "created")}");
        System.Diagnostics.Debug.WriteLine($"ReportContext: {(session.ReportContext is null ? "null" : "created")}");
        session.Project.Status = StencilProjectStatus.Analyzed;
        return result;
    }

    public TechnologyDecisionResult EvaluateTechnology(StencilProjectSession session)
    {
        var context = session.AnalysisContext ?? throw new InvalidOperationException("Load paste Gerber before evaluating technology.");
        try
        {
            var decisionContext = new TechnologyDecisionContext { PackageId = 0, PackageFamily = "Gerber", PastePattern = context.Pads, StencilThickness = .12, TechnologyGoal = TechnologyDecisionGoal.StandardAssembly };
            session.TechnologyDecision = new TechnologyDecisionEngine().Evaluate(decisionContext);
        }
        catch
        {
            session.TechnologyDecision = new TechnologyDecisionResult { SelectedShape = ApertureShapeType.Rectangle, Confidence = .25, Reason = "Gerber-only input: generic rectangle recommendation.", Warnings = ["No MasterLibrary package was identified."] };
        }
        return session.TechnologyDecision;
    }
    public StencilProjectSession CreatePreview(StencilProjectSession session)
    {
        var workflowProject = RequireWorkflow(session);
        var frame = session.Frame ?? _frames.GetDefaultFrame() ?? CreateFallbackFrame();
        _workflow.PlaceOnFrame(workflowProject, frame);
        _workflow.GenerateFiducials(workflowProject);
        _workflow.GenerateMarking(workflowProject);
        session.Frame = frame;
        session.Project.FrameId = frame.Id;
        session.Preview = _workflow.CreatePreview(workflowProject);
        session.PreviewData = session.Preview;
        session.Project.Status = StencilProjectStatus.PreviewReady;
        return session;
    }

    public IReadOnlyList<string> ExportReports(StencilProjectSession session, string outputDirectory)
    {
        var workflowProject = RequireWorkflow(session);
        if (workflowProject.AnalysisResult is null) Analyze(session);
        if (session.Preview is null) CreatePreview(session);
        Directory.CreateDirectory(outputDirectory);
        var report = _reports.CreateReport(workflowProject);
        var baseName = string.Concat(session.Project.ProjectName.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        var files = new[]
        {
            _reports.GenerateTXT(report, Path.Combine(outputDirectory, baseName + "_REPORT.txt")),
            _reports.GenerateHTML(report, Path.Combine(outputDirectory, baseName + "_REPORT.html")),
            _reports.GeneratePDF(report, Path.Combine(outputDirectory, baseName + "_REPORT.pdf"))
        };
        session.ReportContext = new StencilProjectReportContext { Files = files };
        session.Project.Status = StencilProjectStatus.ReportGenerated;
        return files;
    }

    private static StencilManufacturingProject RequireWorkflow(StencilProjectSession session) => session.WorkflowProject ?? throw new InvalidOperationException("Load paste Gerber before this operation.");
    private static void AddInputFile(Models.StencilProject project, string? file, string type)
    {
        if (string.IsNullOrWhiteSpace(file)) return;
        project.InputFiles.Add(new StencilInputFile { Id = project.InputFiles.Count + 1, ProjectId = project.Id, FileName = System.IO.Path.GetFileName(file), FileType = type, Path = file });
    }
    private static StencilFrame CreateFallbackFrame() => new() { Id = 0, Name = "Default Frame", FrameWidth = 400, FrameHeight = 500, StencilWidth = 400, StencilHeight = 500, IsActive = true, IsDefault = true };
}