using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.Services.Gerber;
using Vega.Services.MasterLibrary;
using Vega.Models.MasterLibrary;
using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Vega.StencilInput;
using Vega.StencilInput.Models;
using Vega.StencilViewer;
using Vega.StencilViewer.Models;
using Vega.StencilWorkflow.Models;

namespace Vega.StencilWorkflow;

public class StencilManufacturingService
{
    private static int _nextProjectId;
    private readonly StencilInputManagerService _inputManager;
    private readonly StencilFrameLibraryService _frameLibrary;
    private readonly PasteAnalyzerService _pasteAnalyzer;
    private readonly ApertureOptimizationService _optimizationService;
    private readonly CorrectedPasteLayerBuilderService _correctionBuilder;
    private readonly StencilPlacementService _placementService;
    private readonly StencilFiducialGeneratorService _fiducialGenerator;
    private readonly StencilMarkingGeneratorService _markingGenerator;
    private readonly StencilOverlayService _overlayService;
    private readonly GerberPasteWriterService _pasteWriter;
    private readonly IStencilHistorySink? _historySink;

    public StencilManufacturingService(
        StencilInputManagerService? inputManager = null,
        StencilFrameLibraryService? frameLibrary = null,
        PasteAnalyzerService? pasteAnalyzer = null,
        ApertureOptimizationService? optimizationService = null,
        CorrectedPasteLayerBuilderService? correctionBuilder = null,
        StencilPlacementService? placementService = null,
        StencilFiducialGeneratorService? fiducialGenerator = null,
        StencilMarkingGeneratorService? markingGenerator = null,
        StencilOverlayService? overlayService = null,
        GerberPasteWriterService? pasteWriter = null,
        IStencilHistorySink? historySink = null)
    {
        _inputManager = inputManager ?? new StencilInputManagerService();
        _frameLibrary = frameLibrary ?? new StencilFrameLibraryService();
        _pasteAnalyzer = pasteAnalyzer ?? new PasteAnalyzerService();
        _optimizationService = optimizationService ?? new ApertureOptimizationService();
        _correctionBuilder = correctionBuilder ?? new CorrectedPasteLayerBuilderService();
        _placementService = placementService ?? new StencilPlacementService(frameLibraryService: _frameLibrary);
        _fiducialGenerator = fiducialGenerator ?? new StencilFiducialGeneratorService();
        _markingGenerator = markingGenerator ?? new StencilMarkingGeneratorService();
        _overlayService = overlayService ?? new StencilOverlayService();
        _pasteWriter = pasteWriter ?? new GerberPasteWriterService();
        _historySink = historySink;
    }

    public StencilManufacturingProject CreateProject(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is required.", nameof(projectName));
        return new StencilManufacturingProject { Id = Interlocked.Increment(ref _nextProjectId), ProjectName = projectName };
    }

    public StencilManufacturingProject LoadInput(StencilManufacturingProject project, StencilInputProject inputProject, string? pasteSide = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(inputProject);
        var paste = SelectPaste(inputProject, pasteSide);
        project.InputProject = inputProject;
        project.PasteSource = paste.FileName;
        project.OriginalPaste = paste;
        project.Status = StencilWorkflowStatus.InputLoaded;
        return project;
    }

    public StencilManufacturingProject LoadInput(StencilManufacturingProject project, StencilInputSourceType sourceType, IEnumerable<string> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        var sourceFiles = files.Where(file => !string.IsNullOrWhiteSpace(file)).ToList();
        StencilInputProject input = sourceType switch
        {
            StencilInputSourceType.PasteOnly => _inputManager.LoadPasteOnlyProject(
                sourceFiles.FirstOrDefault(file => Path.GetExtension(file).Equals(".gtp", StringComparison.OrdinalIgnoreCase)),
                sourceFiles.FirstOrDefault(file => IsBottomPasteFile(file))),
            StencilInputSourceType.AltiumProject => _inputManager.LoadAltiumProject(
                sourceFiles.First(file => Path.GetExtension(file).Equals(".PcbDoc", StringComparison.OrdinalIgnoreCase)),
                sourceFiles.Where(IsPasteFile)),
            StencilInputSourceType.PanelGerber => _inputManager.LoadPanelProject(sourceFiles),
            _ => throw new ArgumentException("A supported input source type is required.", nameof(sourceType))
        };
        return LoadInput(project, input);
    }

    public PasteAnalysisResult AnalyzePaste(StencilManufacturingProject project)
    {
        var paste = RequirePaste(project);
        var result = _pasteAnalyzer.Analyze(paste);
        project.AnalysisResult = result;
        project.Status = StencilWorkflowStatus.Analyzed;
        return result;
    }

    public CorrectedPasteLayer ApplyCorrections(StencilManufacturingProject project, StencilRecommendationRule? rule = null)
    {
        var paste = RequirePaste(project);
        rule ??= DefaultRule();
        var patterns = paste.Primitives.Select(primitive => _optimizationService.Optimize(primitive, rule)).ToList();
        var corrected = _correctionBuilder.Build(paste, patterns);
        project.CorrectedPaste = corrected;
        project.Status = StencilWorkflowStatus.Corrected;
        return corrected;
    }

    public StencilPlacementResult PlaceOnFrame(StencilManufacturingProject project, StencilFrame? frame = null, StencilTransformation? transformations = null)
    {
        var source = RequirePaste(project);
        frame ??= project.Frame ?? _frameLibrary.GetDefaultFrame()
            ?? throw new InvalidOperationException("No default stencil frame is configured.");
        var transformation = transformations ?? CreateDefaultTransformation(source.Side);
        var placement = _placementService.PlacePasteLayer(source, frame, transformation);
        project.Frame = frame;
        project.Transformations = transformation;
        project.Placement = placement;
        project.OriginalPaste = placement.PlacedLayer;

        if (project.CorrectedPaste is not null)
        {
            var correctedSource = new PasteLayer { FileName = project.CorrectedPaste.OriginalFileName, Side = project.CorrectedPaste.Side };
            correctedSource.Primitives.AddRange(project.CorrectedPaste.CorrectedPrimitives);
            var placedCorrected = _placementService.PlacePasteLayer(correctedSource, frame, CopyWithPlacementOffset(transformation, placement));
            project.CorrectedPaste = new CorrectedPasteLayer
            {
                OriginalFileName = project.CorrectedPaste.OriginalFileName,
                Side = project.CorrectedPaste.Side,
                OriginalPrimitiveCount = project.CorrectedPaste.OriginalPrimitiveCount,
                CorrectedPrimitiveCount = placedCorrected.PlacedLayer.Primitives.Count,
                OriginalLayer = placement.PlacedLayer,
                CorrectedPrimitives = placedCorrected.PlacedLayer.Primitives,
                Changes = project.CorrectedPaste.Changes
            };
        }
        project.Status = StencilWorkflowStatus.PlacedOnFrame;
        return placement;
    }

    public IReadOnlyList<StencilFiducial> GenerateFiducials(StencilManufacturingProject project, IEnumerable<StencilFiducial>? fiducials = null)
    {
        if (project.Frame is null) throw new InvalidOperationException("Place paste on a frame before generating fiducials.");
        var source = fiducials?.ToList() ?? [];
        if (source.Count == 0)
        {
            source.Add(_fiducialGenerator.GeneratePcbFiducial("Round", 1, project.Frame.OriginX + 5, project.Frame.OriginY + 5));
        }
        var transformation = project.Transformations ?? CreateDefaultTransformation(project.OriginalPaste?.Side ?? "Top");
        var placement = project.Placement;
        project.Fiducials = _placementService.PlaceFiducials(source, transformation, placement?.OffsetX ?? 0, placement?.OffsetY ?? 0);
        return project.Fiducials;
    }

    public IReadOnlyList<StencilMarking> GenerateMarking(StencilManufacturingProject project, string? text = null, double? positionX = null, double? positionY = null)
    {
        if (project.Frame is null) throw new InvalidOperationException("Select a frame before generating marking.");
        var marking = _markingGenerator.Generate(
            text ?? $"{project.ProjectName} V001",
            positionX ?? project.Frame.OriginX - 5,
            positionY ?? project.Frame.OriginY - 5,
            2,
            "Default");
        project.Marking = [marking];
        return project.Marking;
    }

    public StencilViewDocument CreatePreview(StencilManufacturingProject project)
    {
        if (project.Frame is null) throw new InvalidOperationException("Select a frame before creating preview.");
        var document = new StencilViewDocument
        {
            ProjectName = project.ProjectName, Frame = project.Frame, OriginalPasteLayer = project.OriginalPaste,
            CorrectedPasteLayer = project.CorrectedPaste, Fiducials = project.Fiducials,
            MarkingLayer = project.Marking, Transformations = project.Transformations
        };
        _overlayService.LoadProject(document);
        project.Preview = document;
        project.Status = StencilWorkflowStatus.PreviewReady;
        return document;
    }

    public IReadOnlyList<string> ExportGerber(StencilManufacturingProject project, string outputDirectory)
    {
        if (project.CorrectedPaste is null) throw new InvalidOperationException("Apply corrections before exporting Gerber.");
        if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentException("Output directory is required.", nameof(outputDirectory));
        Directory.CreateDirectory(outputDirectory);
        var name = SafeFileName(project.ProjectName);
        var isBottom = project.CorrectedPaste.Side.Equals("Bottom", StringComparison.OrdinalIgnoreCase);
        var pasteFile = Path.Combine(outputDirectory, $"{name}_PASTE_{(isBottom ? "BOTTOM_V001.GBP" : "TOP_V001.GTP")}");
        var markingFile = Path.Combine(outputDirectory, $"{name}_MARKING_V001.GBR");
        var reportFile = Path.Combine(outputDirectory, $"{name}_REPORT.txt");
        _pasteWriter.Write(project.CorrectedPaste, pasteFile);
        File.WriteAllText(markingFile, CreateMarkingGerber(project.Marking));
        File.WriteAllText(reportFile, FormatReport(GenerateReport(project)));
        project.OutputFiles = [pasteFile, markingFile, reportFile];
        project.Status = StencilWorkflowStatus.Generated;
        _historySink?.RecordGenerated(project);
        return project.OutputFiles;
    }

    public StencilManufacturingReport GenerateReport(StencilManufacturingProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        var changes = project.CorrectedPaste?.Changes ?? Array.Empty<PasteCorrectionChange>();
        var warningCount = project.AnalysisResult?.WarningCount ?? 0;
        return new StencilManufacturingReport
        {
            ProjectName = project.ProjectName,
            InputType = project.InputProject?.SourceType.ToString() ?? "Not loaded",
            FrameName = project.Frame?.Name ?? "Not selected",
            PasteSide = project.OriginalPaste?.Side ?? project.CorrectedPaste?.Side ?? "Not selected",
            StencilThickness = 0.12,
            ComponentsCount = project.InputProject?.Components.Count ?? 0,
            ModifiedApertures = changes.Count,
            WindowPaneCount = changes.Count(change => change.ChangeType.Contains("WindowPane", StringComparison.OrdinalIgnoreCase) || change.ChangeType.Contains("Segmentation", StringComparison.OrdinalIgnoreCase)),
            HomePlateCount = changes.Count(change => change.NewGeometry.Contains("HomePlate", StringComparison.OrdinalIgnoreCase)),
            Warnings = Enumerable.Range(1, warningCount).Select(index => $"Paste analysis warning {index}.").ToList(),
            Status = project.Status
        };
    }

    private static PasteLayer SelectPaste(StencilInputProject inputProject, string? pasteSide) =>
        string.IsNullOrWhiteSpace(pasteSide)
            ? inputProject.PasteLayers.FirstOrDefault() ?? throw new InvalidOperationException("Input project contains no paste layer.")
            : inputProject.PasteLayers.FirstOrDefault(layer => layer.Side.Equals(pasteSide, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Paste layer '{pasteSide}' was not found.");

    private static PasteLayer RequirePaste(StencilManufacturingProject project) => project.OriginalPaste
        ?? throw new InvalidOperationException("Load input before executing this workflow step.");

    private static StencilTransformation CreateDefaultTransformation(string side) => side.Equals("Bottom", StringComparison.OrdinalIgnoreCase)
        ? new StencilTransformation { MirrorX = true, RotationAngle = 180, AutoCenter = true }
        : new StencilTransformation { AutoCenter = true };

    private static StencilTransformation CopyWithPlacementOffset(StencilTransformation source, StencilPlacementResult placement) => new()
    {
        MirrorX = source.MirrorX, MirrorY = source.MirrorY, RotationAngle = source.RotationAngle,
        OffsetX = placement.OffsetX, OffsetY = placement.OffsetY, AutoCenter = false
    };

    private static StencilRecommendationRule DefaultRule() => new()
    {
        PackageFamily = "Generic", ComponentType = "Generic", RecommendedStencilThickness = 0.12,
        ReductionX = 10, ReductionY = 10, AreaRatioMinimum = 0.66, AspectRatioMinimum = 1.5
    };

    private static bool IsBottomPasteFile(string file) => Path.GetExtension(file).Equals(".gbp", StringComparison.OrdinalIgnoreCase)
        || Path.GetExtension(file).Equals(".gbs", StringComparison.OrdinalIgnoreCase);
    private static bool IsPasteFile(string file) => Path.GetExtension(file).Equals(".gtp", StringComparison.OrdinalIgnoreCase) || IsBottomPasteFile(file);
    private static string SafeFileName(string name) => string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
    private static string CreateMarkingGerber(IEnumerable<StencilMarking> markings) => string.Join(Environment.NewLine,
        new[] { "G04 Vega-SMD stencil marking (mirrored, rotation 0)*", "%FSLAX24Y24*%", "%MOMM*%" }
            .Concat(markings.Select(marking => $"G04 {marking.Text}; X={marking.PositionX:0.####}; Y={marking.PositionY:0.####}; Mirror={marking.Mirror}; Rotation={marking.Rotation:0.####}*"))
            .Append("M02*"));
    private static string FormatReport(StencilManufacturingReport report) => string.Join(Environment.NewLine,
        $"PROJECT: {report.ProjectName}", $"INPUT: {report.InputType}", $"FRAME: {report.FrameName}",
        $"PASTE: {report.PasteSide}", $"THICKNESS: {report.StencilThickness:0.###} mm",
        $"MODIFIED APERTURES: {report.ModifiedApertures}", $"WINDOW PANE: {report.WindowPaneCount}",
        $"HOME PLATE: {report.HomePlateCount}", $"STATUS: {report.Status}");
}