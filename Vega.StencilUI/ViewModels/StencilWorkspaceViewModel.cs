using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vega.Gerber.Models;
using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Vega.StencilInput;
using Vega.StencilInput.Models;
using Vega.StencilViewer;
using Vega.StencilViewer.Models;
using Vega.StencilWorkflow;
using Vega.StencilWorkflow.Models;

namespace Vega.StencilUI.ViewModels;

public class StencilWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly StencilManufacturingService _workflow;
    private readonly StencilInputManagerService _inputManager;
    private readonly StencilFrameLibraryService _frameLibrary;
    private readonly StencilOverlayService _overlayService = new();
    private StencilManufacturingProject? _project;
    private string _projectName = "Stencil Project";
    private StencilInputSourceType _inputSource = StencilInputSourceType.PasteOnly;
    private StencilFrame? _selectedFrame;
    private string _pasteSide = "Top";
    private StencilWorkflowStatus _workflowStatus = StencilWorkflowStatus.Created;
    private PasteAnalysisResult? _analysisResult;
    private StencilViewDocument? _previewDocument;
    private StencilViewMode _previewMode = StencilViewMode.Overlay;
    private string _inputFilePath = "";
    private string _outputDirectory = Path.Combine(Environment.CurrentDirectory, "StencilOutput");
    private string _errorMessage = "";
    private IReadOnlyList<string> _outputFiles = Array.Empty<string>();
    private IReadOnlyList<string> _visiblePreviewLayers = Array.Empty<string>();

    public StencilWorkspaceViewModel(
        StencilManufacturingService? workflow = null,
        StencilInputManagerService? inputManager = null,
        StencilFrameLibraryService? frameLibrary = null)
    {
        _workflow = workflow ?? new StencilManufacturingService();
        _inputManager = inputManager ?? new StencilInputManagerService();
        _frameLibrary = frameLibrary ?? new StencilFrameLibraryService();
        Frames = _frameLibrary.GetFrames();
        SelectedFrame = _frameLibrary.GetDefaultFrame() ?? Frames.FirstOrDefault();

        CreateProjectCommand = new WorkspaceCommand(CreateProject);
        LoadInputCommand = new WorkspaceCommand(() => LoadInput(ParseInputFiles()));
        AnalyzeCommand = new WorkspaceCommand(Analyze);
        CorrectCommand = new WorkspaceCommand(Correct);
        PreviewCommand = new WorkspaceCommand(CreatePreview);
        ExportCommand = new WorkspaceCommand(Export);
        OpenOutputFolderCommand = new WorkspaceCommand(OpenOutputFolder, () => OutputFiles.Count > 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ProjectName { get => _projectName; set => SetField(ref _projectName, value); }
    public StencilInputSourceType InputSource { get => _inputSource; set => SetField(ref _inputSource, value); }
    public IReadOnlyList<StencilFrame> Frames { get; }
    public StencilFrame? SelectedFrame { get => _selectedFrame; set => SetField(ref _selectedFrame, value); }
    public string PasteSide { get => _pasteSide; set => SetField(ref _pasteSide, value); }
    public StencilWorkflowStatus WorkflowStatus { get => _workflowStatus; private set { if (SetField(ref _workflowStatus, value)) OnPropertyChanged(nameof(StatusColor)); } }
    public PasteAnalysisResult? AnalysisResult { get => _analysisResult; private set => SetField(ref _analysisResult, value); }
    public StencilViewDocument? PreviewDocument { get => _previewDocument; private set => SetField(ref _previewDocument, value); }
    public StencilViewMode PreviewMode
    {
        get => _previewMode;
        set
        {
            if (!SetField(ref _previewMode, value) || PreviewDocument is null) return;
            VisiblePreviewLayers = _overlayService.CreateOverlay(value).Select(layer => layer.Name).ToList();
        }
    }
    public string InputFilePath { get => _inputFilePath; set => SetField(ref _inputFilePath, value); }
    public string OutputDirectory { get => _outputDirectory; set => SetField(ref _outputDirectory, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetField(ref _errorMessage, value); }
    public IReadOnlyList<string> OutputFiles { get => _outputFiles; private set { if (SetField(ref _outputFiles, value)) ((WorkspaceCommand)OpenOutputFolderCommand).RaiseCanExecuteChanged(); } }
    public IReadOnlyList<string> VisiblePreviewLayers { get => _visiblePreviewLayers; private set => SetField(ref _visiblePreviewLayers, value); }
    public IReadOnlyList<StencilInputSourceType> InputSources { get; } = Enum.GetValues<StencilInputSourceType>();
    public IReadOnlyList<string> PasteSides { get; } = ["Top", "Bottom"];
    public IReadOnlyList<StencilViewMode> PreviewModes { get; } = Enum.GetValues<StencilViewMode>();
    public int ModifiedApertures => _project?.CorrectedPaste?.Changes.Count ?? 0;
    public int WindowPaneCount => _project?.CorrectedPaste?.Changes.Count(change => change.ChangeType.Contains("WindowPane", StringComparison.OrdinalIgnoreCase) || change.ChangeType.Contains("Segmentation", StringComparison.OrdinalIgnoreCase)) ?? 0;
    public int HomePlateCount => _project?.CorrectedPaste?.Changes.Count(change => change.NewGeometry.Contains("HomePlate", StringComparison.OrdinalIgnoreCase)) ?? 0;
    public int SnubnoseCount => _project?.CorrectedPaste?.Changes.Count(change => change.NewGeometry.Contains("Snubnose", StringComparison.OrdinalIgnoreCase)) ?? 0;
    public int WarningCount => AnalysisResult?.WarningCount ?? 0;
    public string StatusColor => WorkflowStatus switch
    {
        StencilWorkflowStatus.Created => "#6B7280",
        StencilWorkflowStatus.InputLoaded => "#2563EB",
        StencilWorkflowStatus.Analyzed => "#7C3AED",
        StencilWorkflowStatus.Corrected => "#D97706",
        StencilWorkflowStatus.PlacedOnFrame or StencilWorkflowStatus.PreviewReady => "#0891B2",
        StencilWorkflowStatus.Generated => "#16A34A",
        StencilWorkflowStatus.Error => "#DC2626",
        _ => "#6B7280"
    };

    public ICommand CreateProjectCommand { get; }
    public ICommand LoadInputCommand { get; }
    public ICommand AnalyzeCommand { get; }
    public ICommand CorrectCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand OpenOutputFolderCommand { get; }

    public void CreateProject()
    {
        Execute(() =>
        {
            _project = _workflow.CreateProject(string.IsNullOrWhiteSpace(ProjectName) ? "Stencil Project" : ProjectName);
            AnalysisResult = null;
            PreviewDocument = null;
            OutputFiles = Array.Empty<string>();
            VisiblePreviewLayers = Array.Empty<string>();
            RefreshState();
        });
    }

    public void LoadInput(IEnumerable<string> files)
    {
        Execute(() =>
        {
            var sourceFiles = files.Where(file => !string.IsNullOrWhiteSpace(file)).ToList();
            if (sourceFiles.Count == 0) throw new ArgumentException("Select at least one input file.");
            if (_project is null) _project = _workflow.CreateProject(ProjectName);
            var source = InputSource == StencilInputSourceType.Manual ? _inputManager.DetectInputType(sourceFiles) : InputSource;
            var input = LoadInputProject(source, sourceFiles);
            var side = input.PasteLayers.Any(layer => layer.Side.Equals(PasteSide, StringComparison.OrdinalIgnoreCase)) ? PasteSide : null;
            _workflow.LoadInput(_project, input, side);
            ProjectName = _project.ProjectName;
            RefreshState();
        });
    }

    public void Analyze() => Execute(() =>
    {
        var project = RequireProject();
        AnalysisResult = _workflow.AnalyzePaste(project);
        RefreshState();
    });

    public void Correct() => Execute(() =>
    {
        _workflow.ApplyCorrections(RequireProject());
        RefreshState();
    });

    public void CreatePreview() => Execute(() =>
    {
        var project = RequireProject();
        if (project.CorrectedPaste is null) _workflow.ApplyCorrections(project);
        if (project.Placement is null) _workflow.PlaceOnFrame(project, SelectedFrame);
        if (project.Fiducials.Count == 0) _workflow.GenerateFiducials(project);
        if (project.Marking.Count == 0) _workflow.GenerateMarking(project);
        PreviewDocument = _workflow.CreatePreview(project);
        _overlayService.LoadProject(PreviewDocument);
        VisiblePreviewLayers = _overlayService.CreateOverlay(PreviewMode).Select(layer => layer.Name).ToList();
        RefreshState();
    });

    public void Export() => Execute(() =>
    {
        var project = RequireProject();
        if (project.Preview is null) CreatePreview();
        if (WorkflowStatus == StencilWorkflowStatus.Error) return;
        OutputFiles = _workflow.ExportGerber(project, OutputDirectory);
        RefreshState();
    });

    public void OpenOutputFolder()
    {
        if (OutputFiles.Count == 0) return;
        Process.Start(new ProcessStartInfo { FileName = OutputDirectory, UseShellExecute = true });
    }

    private StencilInputProject LoadInputProject(StencilInputSourceType source, IReadOnlyList<string> files) => source switch
    {
        StencilInputSourceType.PasteOnly => _inputManager.LoadPasteOnlyProject(
            files.FirstOrDefault(file => Path.GetExtension(file).Equals(".gtp", StringComparison.OrdinalIgnoreCase)),
            files.FirstOrDefault(IsBottomPasteFile)),
        StencilInputSourceType.AltiumProject => _inputManager.LoadAltiumProject(
            files.First(file => Path.GetExtension(file).Equals(".PcbDoc", StringComparison.OrdinalIgnoreCase)),
            files.Where(IsPasteFile)),
        StencilInputSourceType.PanelGerber => _inputManager.LoadPanelProject(files),
        _ => throw new ArgumentException("Select PasteOnly, AltiumProject, or PanelGerber input.", nameof(source))
    };

    private StencilManufacturingProject RequireProject() => _project ?? throw new InvalidOperationException("Create a project before running workflow commands.");
    private IEnumerable<string> ParseInputFiles() => InputFilePath.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    private void RefreshState()
    {
        WorkflowStatus = _project?.Status ?? StencilWorkflowStatus.Created;
        ErrorMessage = "";
    }
    private void Execute(Action action)
    {
        try { action(); }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            if (_project is not null) _project.Status = StencilWorkflowStatus.Error;
            WorkflowStatus = StencilWorkflowStatus.Error;
        }
    }
    private static bool IsBottomPasteFile(string file) => Path.GetExtension(file).Equals(".gbp", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(file).Equals(".gbs", StringComparison.OrdinalIgnoreCase);
    private static bool IsPasteFile(string file) => Path.GetExtension(file).Equals(".gtp", StringComparison.OrdinalIgnoreCase) || IsBottomPasteFile(file);
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}