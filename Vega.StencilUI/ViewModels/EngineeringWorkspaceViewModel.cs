using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vega.EngineeringDashboard;
using Vega.EngineeringDashboard.Models;
using Vega.Report;
using Vega.Report.Models;
using Vega.StencilHistory.Data;
using Vega.StencilProjects;
using Vega.StencilProjects.Models;
using Vega.StencilWorkflow;
using Vega.StencilWorkflow.Models;
using Vega.TechnologyDecision;
using Vega.TechnologyDecision.Models;
using Microsoft.Win32;
using System.Windows.Media;
using Vega.StencilUI.Services;
using Vega.StencilViewer.Models;

namespace Vega.StencilUI.ViewModels;

public class EngineeringWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly EngineeringDashboardService _dashboardService;
    private readonly StencilReportGeneratorService _reportService;
    private readonly StencilManufacturingService _workflow = new();
    private readonly TechnologyDecisionEngine _technologyDecision = new();
    private readonly StencilHistoryRepository _history = new();
    private readonly StencilProjectService _stencilProjectService = new();
    private StencilProjectSession? _stencilProjectSession;
    private StencilManufacturingProject? _workflowProject;
    private TechnologyDecisionContext? _technologyContext;
    private string _projectName = "Проект не загружен";
    private string _boardRevision = "";
    private string _customer = "";
    private string _stencilRevision = "Not assigned";
    private string _technologyStatus = "Нет технологических решений";
    private string _frame = "";
    private double _stencilThickness;
    private int _apertureCount;
    private int _changesCount;
    private double _yield;
    private double _fpy;
    private double _ppm;
    private string _qualitySummary = "No quality data";
    private string _reportOutputPath = Path.Combine(Environment.CurrentDirectory, "EngineeringSummary.txt");
    private string _statusMessage = "Проект не загружен";
    private int _selectedPreviewIndex = 2;
    private string _analysisResultView = "";
    private string _technologyDecisionView = "";
    private string _reportStatus = "";
    private ImageSource? _originalPreviewImage;
    private ImageSource? _correctedPreviewImage;
    private ImageSource? _overlayPreviewImage;
    private ImageSource? _productionPreviewImage;
    private string _previewSummary = "";
    private double _previewZoom = 1.0;
    private EngineeringDashboardData? _dashboard;

    public EngineeringWorkspaceViewModel()
        : this(null, null)
    {
    }

    public EngineeringWorkspaceViewModel(EngineeringDashboardService? dashboardService = null, StencilReportGeneratorService? reportService = null)
    {
        _dashboardService = dashboardService ?? new EngineeringDashboardService();
        _reportService = reportService ?? new StencilReportGeneratorService();
        OpenProjectCommand = new EngineeringWorkspaceCommand(OpenProject);
        AnalyzeStencilCommand = new EngineeringWorkspaceCommand(AnalyzeStencil);
        TechnologyDecisionCommand = new EngineeringWorkspaceCommand(RunTechnologyDecision);
        OpenPreviewCommand = new EngineeringWorkspaceCommand(OpenPreview);
        PreviewZoomInCommand = new EngineeringWorkspaceCommand(() => { PreviewZoom = Math.Min(PreviewZoom + 0.25, 3.0); Log($"[INFO] Preview zoom: {PreviewZoom:0.##}"); });
        PreviewZoomOutCommand = new EngineeringWorkspaceCommand(() => { PreviewZoom = Math.Max(PreviewZoom - 0.25, 0.5); Log($"[INFO] Preview zoom: {PreviewZoom:0.##}"); });
        ResetPreviewViewCommand = new EngineeringWorkspaceCommand(() => { PreviewZoom = 1.0; Log("[INFO] Preview zoom reset: 1.0"); });
        ExportReportCommand = new EngineeringWorkspaceCommand(ExportReport);
        ExportPdfCommand = new EngineeringWorkspaceCommand(ExportPdf, () => true);
        ExportHtmlCommand = new EngineeringWorkspaceCommand(ExportHtml, () => true);
        ExportCanvaCommand = new EngineeringWorkspaceCommand(ExportCanva, () => true);
        OpenHistoryCommand = new EngineeringWorkspaceCommand(OpenHistory);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? ActionRequested;
    public string ProjectName { get => _projectName; private set => SetField(ref _projectName, value); }
    public string BoardRevision { get => _boardRevision; private set => SetField(ref _boardRevision, value); }
    public string Customer { get => _customer; private set => SetField(ref _customer, value); }
    public string StencilRevision { get => _stencilRevision; private set => SetField(ref _stencilRevision, value); }
    public string TechnologyStatus { get => _technologyStatus; private set => SetField(ref _technologyStatus, value); }
    public string Frame { get => _frame; private set => SetField(ref _frame, value); }
    public double StencilThickness { get => _stencilThickness; private set => SetField(ref _stencilThickness, value); }
    public int ApertureCount { get => _apertureCount; private set => SetField(ref _apertureCount, value); }
    public int ChangesCount { get => _changesCount; private set => SetField(ref _changesCount, value); }
    public double Yield { get => _yield; private set => SetField(ref _yield, value); }
    public double FPY { get => _fpy; private set => SetField(ref _fpy, value); }
    public double PPM { get => _ppm; private set => SetField(ref _ppm, value); }
    public string QualitySummary { get => _qualitySummary; private set => SetField(ref _qualitySummary, value); }
    public string ReportOutputPath { get => _reportOutputPath; set => SetField(ref _reportOutputPath, value); }
    public string StatusMessage { get => _statusMessage; private set => SetField(ref _statusMessage, value); }
    public int SelectedPreviewIndex { get => _selectedPreviewIndex; private set => SetField(ref _selectedPreviewIndex, value); }
    public string AnalysisResultView { get => _analysisResultView; private set => SetField(ref _analysisResultView, value); }
    public string TechnologyDecisionView { get => _technologyDecisionView; private set => SetField(ref _technologyDecisionView, value); }
    public string ReportStatus { get => _reportStatus; private set => SetField(ref _reportStatus, value); }
    public ImageSource? OriginalPreviewImage { get => _originalPreviewImage; private set => SetField(ref _originalPreviewImage, value); }
    public ImageSource? CorrectedPreviewImage { get => _correctedPreviewImage; private set => SetField(ref _correctedPreviewImage, value); }
    public ImageSource? OverlayPreviewImage { get => _overlayPreviewImage; private set => SetField(ref _overlayPreviewImage, value); }
    public ImageSource? ProductionPreviewImage { get => _productionPreviewImage; private set => SetField(ref _productionPreviewImage, value); }
    public string PreviewSummary { get => _previewSummary; private set => SetField(ref _previewSummary, value); }
    public StencilViewDocument? PreviewDocument => _stencilProjectSession?.PreviewData;
    public string? ProjectArchiveSourceDirectory => _stencilProjectSession?.Project.InputFiles
        .Select(input => Path.GetDirectoryName(input.Path))
        .FirstOrDefault(directory => !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory));
    public double PreviewZoom { get => _previewZoom; private set => SetField(ref _previewZoom, value); }
    public IReadOnlyList<string> PreviewItems => _stencilProjectSession?.PreviewData is null ? Array.Empty<string>() : ["Original", "Corrected", "Overlay", "Production"];
    public ObservableCollection<DashboardWarning> WarningList { get; } = new();
    public ObservableCollection<EngineeringRecommendation> RecommendationList { get; } = new();
    public ObservableCollection<string> MainDefects { get; } = new();
    public ObservableCollection<string> TechnologyDecisions { get; } = new();
    public ObservableCollection<string> DiagnosticLog { get; } = new();
    public IReadOnlyList<string> PreviewTabs { get; } = ["Original", "Corrected", "Overlay", "Production"];
    public ICommand OpenProjectCommand { get; }
    public ICommand AnalyzeStencilCommand { get; }
    public ICommand TechnologyDecisionCommand { get; }
    public ICommand RunTechnologyDecisionCommand => TechnologyDecisionCommand;
    public ICommand OpenPreviewCommand { get; }
    public ICommand PreviewCommand => OpenPreviewCommand;
    public ICommand PreviewZoomInCommand { get; }
    public ICommand PreviewZoomOutCommand { get; }
    public ICommand ResetPreviewViewCommand { get; }
    public ICommand ExportReportCommand { get; }
    public ICommand ExportPdfCommand { get; }
    public ICommand ExportHtmlCommand { get; }
    public ICommand ExportCanvaCommand { get; }
    public ICommand OpenHistoryCommand { get; }

    public void LoadDashboard(EngineeringDashboardData dashboard, double ppm = 0)
    {
        ArgumentNullException.ThrowIfNull(dashboard); _dashboard = dashboard;
        ProjectName = dashboard.ProjectName; BoardRevision = dashboard.BoardRevision; Customer = dashboard.Customer; StencilRevision = dashboard.StencilRevision; TechnologyStatus = dashboard.TechnologyStatus;
        Frame = dashboard.Frame; StencilThickness = dashboard.StencilThickness; ApertureCount = dashboard.ApertureCount; ChangesCount = dashboard.ChangeCount; Yield = dashboard.Yield; FPY = dashboard.FPY; PPM = ppm;
        QualitySummary = $"Yield {Yield:0.##}% | FPY {FPY:0.##}% | PPM {PPM:0.##}";
        Replace(WarningList, dashboard.Warnings); Replace(RecommendationList, dashboard.Recommendations); Replace(MainDefects, dashboard.MainDefects); Replace(TechnologyDecisions, dashboard.TechnologyDecisions);
        ((EngineeringWorkspaceCommand)ExportPdfCommand).RaiseCanExecuteChanged();
        ((EngineeringWorkspaceCommand)ExportHtmlCommand).RaiseCanExecuteChanged();
        ((EngineeringWorkspaceCommand)ExportCanvaCommand).RaiseCanExecuteChanged();
    }

    public void OpenProject()
    {
        var dialog = new OpenFileDialog { Title = "Open stencil project input", Filter = "Paste Gerber (*.gtp;*.gbp)|*.gtp;*.gbp|All files (*.*)|*.*" };
        if (dialog.ShowDialog() != true) { StatusMessage = "Выбор проекта отменён"; return; }
        try
        {
            var bottom = Path.GetExtension(dialog.FileName).Equals(".gbp", StringComparison.OrdinalIgnoreCase);
            _stencilProjectSession = _stencilProjectService.Create(Path.GetFileNameWithoutExtension(dialog.FileName), "", Vega.StencilProjects.Models.StencilProjectInputSource.PasteGerber, bottom ? Vega.StencilProjects.Models.StencilProjectPasteSide.Bottom : Vega.StencilProjects.Models.StencilProjectPasteSide.Top);
            _stencilProjectService.LoadPasteGerber(_stencilProjectSession, bottom ? null : dialog.FileName, bottom ? dialog.FileName : null);
            LoadStencilProject(_stencilProjectSession);
            LogSessionState("AFTER OPEN PROJECT");
            StatusMessage = "Проект загружен";
        }
        catch (Exception exception) { _stencilProjectSession = null; StatusMessage = exception.Message; }
    }
    public void AnalyzeStencil()
    {
        if (_stencilProjectSession is null) { StatusMessage = "Проект не загружен"; return; }
        try { Log($"[INFO] Project loaded: {(_stencilProjectSession is not null ? "yes" : "no")}"); var result = _stencilProjectService.Analyze(_stencilProjectSession); ApertureCount = result.ApertureCount; ChangesCount = _workflowProject.CorrectedPaste?.Changes.Count ?? 0; AnalysisResultView = $"Apertures: {ApertureCount}; Warnings: {result.WarningCount}; Changes: {ChangesCount}"; Log($"[INFO] AnalysisContext: Apertures count {ApertureCount}"); Log($"[INFO] Changes count {ChangesCount}"); StatusMessage = "Анализ трафарета выполнен"; LogSessionState("AFTER ANALYZE"); ActionRequested?.Invoke("AnalyzeStencil"); }
        catch (Exception exception) { StatusMessage = exception.Message; }
    }

    public void RunTechnologyDecision()
    {
        if (_workflowProject is null) { StatusMessage = "Нет данных для анализа"; return; }
        if (_stencilProjectSession is not null && _stencilProjectSession.AnalysisContext is null) { StatusMessage = "Сначала выполните анализ трафарета"; return; }
        if (_stencilProjectSession is not null) { var decision = _stencilProjectService.EvaluateTechnology(_stencilProjectSession); TechnologyDecisions.Add($"{decision.SelectedShape}: {decision.Reason}"); TechnologyStatus = "Ready"; TechnologyDecisionView = $"{decision.SelectedShape}: {decision.Reason}"; Log("[INFO] TechnologyDecision: created"); StatusMessage = "Технологическое решение выполнено"; return; }
        if (_technologyContext is null) { StatusMessage = "Нет данных для анализа"; return; }
        try { var result = _technologyDecision.Evaluate(_technologyContext); TechnologyDecisions.Add($"{result.SelectedStrategy}: {result.SelectedShape}; {result.Reason}"); TechnologyStatus = "Ready"; StatusMessage = "Технологическое решение выполнено"; ActionRequested?.Invoke("TechnologyDecision"); }
        catch (Exception exception) { StatusMessage = exception.Message; }
    }

    public void OpenPreview()
    {
        if (_workflowProject is null) { StatusMessage = "Проект не загружен"; return; }
        if (_stencilProjectSession is not null)
        {
            _stencilProjectService.CreatePreview(_stencilProjectSession);
            var preview = _stencilProjectSession.PreviewData;
            if (preview is null) { StatusMessage = "Не удалось создать предпросмотр"; return; }

            Log($"[INFO] Preview source: original primitives={preview.OriginalPasteLayer?.Primitives.Count ?? 0}; corrected primitives={preview.CorrectedPasteLayer?.CorrectedPrimitives.Count ?? 0}; original apertures={preview.OriginalPasteLayer?.Apertures.Count ?? 0}");
            var renderer = new StencilPreviewRenderer();
            OriginalPreviewImage = renderer.Render(preview, StencilViewMode.Original);
            CorrectedPreviewImage = renderer.Render(preview, StencilViewMode.Corrected);
            OverlayPreviewImage = renderer.Render(preview, StencilViewMode.Overlay);
            ProductionPreviewImage = renderer.Render(preview, StencilViewMode.Production);
            var originalCount = preview.OriginalPasteLayer?.Primitives.Count ?? 0;
            var correctedCount = preview.CorrectedPasteLayer?.CorrectedPrimitives.Count ?? 0;
            PreviewSummary = $"Layers: Original, Corrected, Overlay, Production | Elements: {originalCount} / {correctedCount} | Side: {preview.OriginalPasteLayer?.Side ?? "n/a"}";
            Log("[INFO] PreviewData: Created");
            Log($"[INFO] Preview items count: {PreviewItems.Count}");
            Log($"[INFO] Image source: {(OverlayPreviewImage is null ? "null" : "created")}");
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewDocument)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewItems)));
        SelectedPreviewIndex = 2;
        StatusMessage = "Вкладка предпросмотра открыта";
        ActionRequested?.Invoke("OpenPreview");
    }

    public void OpenHistory()
    {
        var count = _history.GetProjects().Count; StatusMessage = count == 0 ? "История ревизий пуста" : $"Ревизий в истории: {count}"; ActionRequested?.Invoke("OpenHistory");
    }

    public void LoadStencilProject(StencilProjectSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        AttachWorkflowProject(session.WorkflowProject ?? throw new InvalidOperationException("Project input has not been loaded."));
        _stencilProjectSession = session;
        Customer = session.Project.Customer;
        BoardRevision = session.Project.Revision;
        Frame = session.Frame?.Name ?? "Default Frame";
        ApertureCount = session.ApertureCount;
        StencilRevision = session.Project.Revision;
        TechnologyStatus = "Paste Gerber loaded";
    }
    public void AttachWorkflowProject(StencilManufacturingProject project) { _workflowProject = project ?? throw new ArgumentNullException(nameof(project)); ProjectName = project.ProjectName; StatusMessage = "Проект загружен"; }
    public void SetTechnologyDecisionContext(TechnologyDecisionContext context) => _technologyContext = context ?? throw new ArgumentNullException(nameof(context));
    public void ExportReport() { if (_stencilProjectSession is not null) { var output = OutputDirectory(); var files = _stencilProjectService.ExportReports(_stencilProjectSession, output); ReportOutputPath = files.First(file => file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)); Log($"[INFO] Report path: {ReportOutputPath}"); Log("[INFO] TXT created"); Log("[INFO] HTML created"); Log("[INFO] PDF created"); ReportStatus = ReportOutputPath; StatusMessage = "Отчёты созданы"; return; } Export("txt"); }
    public void ExportPdf() { System.Windows.MessageBox.Show("Export PDF CLICKED"); Log("[INFO] Export PDF clicked"); LogSessionState("BEFORE EXPORT PDF"); ExportSessionOrDashboard("pdf"); }
    public void ExportHtml() { System.Windows.MessageBox.Show("Export HTML CLICKED"); Log("[INFO] Export HTML clicked"); LogSessionState("BEFORE EXPORT HTML"); ExportSessionOrDashboard("html"); }
    public void ExportCanva()
    {
        if (_dashboard is null) { StatusMessage = "Для создания отчёта загрузите проект"; return; }
        _reportService.ExportCanva(CreateReport(), OutputDirectory());
        ActionRequested?.Invoke("ExportCanva");
    }

    private void ExportSessionOrDashboard(string format) { if (_stencilProjectSession is not null) { var files = _stencilProjectService.ExportReports(_stencilProjectSession, OutputDirectory()); ReportOutputPath = files.First(file => file.EndsWith("." + format, StringComparison.OrdinalIgnoreCase)); Log($"[INFO] {format.ToUpperInvariant()} created: {ReportOutputPath}"); ReportStatus = ReportOutputPath; StatusMessage = "Отчёты созданы"; return; } Export(format); }

    private void LogSessionState(string stage)
    {
        var session = _stencilProjectSession;
        Log($"=== SESSION STATE {stage} ===");
        Log($"CurrentProjectSession: {(session is null ? "null" : "created")}");
        Log($"Project: {session?.Project.ProjectName ?? ""}");
        Log($"AnalysisContext: {(session?.AnalysisContext is null ? "null" : "created")}");
        Log($"AnalysisResult: {(session?.AnalysisResult is null ? "null" : "created")}");
        Log($"CorrectedLayer: {(session?.WorkflowProject?.CorrectedPaste is null ? "null" : "created")}");
        Log($"Changes: {session?.WorkflowProject?.CorrectedPaste?.Changes.Count ?? 0}");
        Log($"TechnologyDecision: {(session?.TechnologyDecision is null ? "null" : "created")}");
        Log($"PreviewData: {(session?.PreviewData is null ? "null" : "created")}");
        Log($"ReportContext: {(session?.ReportContext is null ? "null" : "created")}");
    }
    private void Log(string message) { DiagnosticLog.Add(message); System.Diagnostics.Debug.WriteLine(message); }

    private void Export(string format)
    {
        if (_dashboard is null) { StatusMessage = "Для создания отчёта загрузите проект"; return; }
        var output = Path.ChangeExtension(ReportOutputPath, format);
        var report = CreateReport();
        switch (format) { case "pdf": _reportService.GeneratePDF(report, output); break; case "html": _reportService.GenerateHTML(report, output); break; default: _reportService.GenerateTXT(report, output); break; }
        ReportOutputPath = output; ActionRequested?.Invoke("ExportReport");
    }

    private StencilTechnicalReport CreateReport() => new() { ProjectName = ProjectName, CustomerName = Customer, Revision = StencilRevision, EngineeringSummary = EngineeringDashboardReportMapper.ToReportItem(_dashboard!) };
    private string OutputDirectory() => Path.GetDirectoryName(Path.GetFullPath(ReportOutputPath))!;
    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source) { target.Clear(); foreach (var item in source) target.Add(item); }
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); return true; }
}

public sealed class EngineeringWorkspaceCommand : ICommand
{
    private readonly Action _execute; private readonly Func<bool>? _canExecute;
    public EngineeringWorkspaceCommand(Action execute, Func<bool>? canExecute = null) { _execute = execute; _canExecute = canExecute; }
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}