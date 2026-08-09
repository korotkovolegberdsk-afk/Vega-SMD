using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vega.Gerber.Models;
using Vega.StencilUI.Models;
using Vega.StencilViewer.Models;

namespace Vega.StencilUI.ViewModels;

public sealed class StencilViewerWindowViewModel : INotifyPropertyChanged
{
    private string _modeName = "Overlay";
    private StencilViewMode _viewMode = StencilViewMode.Overlay;
    private double _zoom = 1.0, _offsetX, _offsetY, _selectedWidth, _selectedHeight;
    private bool _showFrame = true, _showBoard = true, _showApertures = true, _editMode, _synchronizingSelection;
    private IReadOnlyList<PastePrimitive> _selectedPrimitives = Array.Empty<PastePrimitive>();
    private IReadOnlyDictionary<int, EditableAperture> _apertureOverrides = new Dictionary<int, EditableAperture>();
    private EditableAperture? _selectedAperture;

    public StencilViewerWindowViewModel(StencilViewDocument document)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        LoadModifiedPasteLayer();
        ShowOriginalCommand = new ViewerCommand(() => Select("Original", StencilViewMode.Original));
        ShowCorrectedCommand = new ViewerCommand(() => Select("Corrected", StencilViewMode.Corrected));
        ShowOverlayCommand = new ViewerCommand(() => Select("Overlay", StencilViewMode.Overlay));
        ShowProductionCommand = new ViewerCommand(() => Select("Production", StencilViewMode.Production));
        ZoomInCommand = new ViewerCommand(() => Zoom = Math.Min(Zoom * 1.25, 10.0)); ZoomOutCommand = new ViewerCommand(() => Zoom = Math.Max(Zoom / 1.25, 0.1));
        FitViewCommand = new ViewerCommand(ResetView); Zoom100Command = new ViewerCommand(() => Zoom = 1.0); Zoom500Command = new ViewerCommand(() => Zoom = 5.0); Zoom1000Command = new ViewerCommand(() => Zoom = 10.0);
        ApplyEditCommand = new ViewerCommand(ApplyEdit); ApplyApertureTableCommand = new ViewerCommand(ApplyApertureTable);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public StencilViewDocument Document { get; }
    public ModifiedPasteLayer ModifiedPasteLayer { get; } = new();
    public string ModeName { get => _modeName; private set => SetField(ref _modeName, value); }
    public StencilViewMode ViewMode { get => _viewMode; private set => SetField(ref _viewMode, value); }
    public double Zoom { get => _zoom; private set => SetField(ref _zoom, value); }
    public double OffsetX { get => _offsetX; private set => SetField(ref _offsetX, value); }
    public double OffsetY { get => _offsetY; private set => SetField(ref _offsetY, value); }
    public string ZoomText => $"{Zoom * 100:0}%";
    public bool ShowFrame { get => _showFrame; set => SetField(ref _showFrame, value); }
    public bool ShowBoard { get => _showBoard; set => SetField(ref _showBoard, value); }
    public bool ShowApertures { get => _showApertures; set => SetField(ref _showApertures, value); }
    public bool EditMode { get => _editMode; set { if (SetField(ref _editMode, value) && !value) SetSelection([]); } }
    public IReadOnlyList<PastePrimitive> SelectedPrimitives { get => _selectedPrimitives; private set => SetField(ref _selectedPrimitives, value); }
    public IReadOnlyDictionary<int, EditableAperture> ApertureOverrides { get => _apertureOverrides; private set => SetField(ref _apertureOverrides, value); }
    public EditableAperture? SelectedAperture { get => _selectedAperture; set { if (SetField(ref _selectedAperture, value) && !_synchronizingSelection) SelectPrimitivesForAperture(value); } }
    public int SelectedCount => SelectedPrimitives.Count;
    public double SelectedWidth { get => _selectedWidth; set => SetField(ref _selectedWidth, value); }
    public double SelectedHeight { get => _selectedHeight; set => SetField(ref _selectedHeight, value); }
    public string SelectionInfo => SelectedCount == 0 ? "No apertures selected" : $"Selected: {SelectedCount} · Width {SelectedWidth:F3} mm · Height {SelectedHeight:F3} mm";
    public ICommand ShowOriginalCommand { get; } public ICommand ShowCorrectedCommand { get; } public ICommand ShowOverlayCommand { get; } public ICommand ShowProductionCommand { get; }
    public ICommand ZoomInCommand { get; } public ICommand ZoomOutCommand { get; } public ICommand FitViewCommand { get; } public ICommand Zoom100Command { get; } public ICommand Zoom500Command { get; } public ICommand Zoom1000Command { get; }
    public ICommand ApplyEditCommand { get; } public ICommand ApplyApertureTableCommand { get; }

    public void SelectSingle(PastePrimitive? primitive) => SetSelection(primitive is null ? [] : [primitive]);
    public void TogglePrimitive(PastePrimitive? primitive) { if (primitive is null) return; var selection = SelectedPrimitives.ToList(); if (!selection.Remove(primitive)) selection.Add(primitive); SetSelection(selection); }
    public void SelectRange(IEnumerable<PastePrimitive> primitives) => SetSelection(primitives);
    public void Pan(double dx, double dy) { OffsetX += dx; OffsetY += dy; }
    public void ZoomByMouseWheel(int delta) => Zoom = delta > 0 ? Math.Min(Zoom * 1.25, 10.0) : Math.Max(Zoom / 1.25, 0.1);
    public void SelectApertureFromViewer() { if (SelectedPrimitives.FirstOrDefault() is { } primitive) SelectedAperture = ModifiedPasteLayer.Apertures.FirstOrDefault(item => item.ApertureId == primitive.ApertureId); }

    private void LoadModifiedPasteLayer()
    {
        var definitions = (Document.OriginalPasteLayer?.Apertures ?? []).ToDictionary(item => item.ApertureId);
        foreach (var group in AllPrimitives().GroupBy(item => item.ApertureId).OrderBy(group => group.Key))
        {
            var first = group.First();
            definitions.TryGetValue(group.Key, out var definition);
            ModifiedPasteLayer.Apertures.Add(new EditableAperture
            {
                ApertureId = group.Key, Shape = definition?.Shape ?? first.ShapeType?.ToString() ?? "Rectangle",
                Width = definition?.Width ?? first.Width, Height = definition?.Height ?? first.Height,
                Radius = definition?.Diameter / 2 ?? 0, X = first.X, Y = first.Y, Rotation = first.Rotation, UsageCount = group.Count()
            });
        }
        foreach (var definition in definitions.Values.Where(item => ModifiedPasteLayer.Apertures.All(current => current.ApertureId != item.ApertureId)))
            ModifiedPasteLayer.Apertures.Add(new EditableAperture { ApertureId = definition.ApertureId, Shape = definition.Shape, Width = definition.Width, Height = definition.Height, Radius = definition.Diameter / 2, UsageCount = 0 });
        ApplyApertureTable();
    }

    private IEnumerable<PastePrimitive> AllPrimitives() =>
        (Document.OriginalPasteLayer?.Primitives ?? []).Concat(Document.CorrectedPasteLayer?.CorrectedPrimitives ?? []);
    private void SelectPrimitivesForAperture(EditableAperture? aperture) { if (aperture is null) return; SetSelection(AllPrimitives().Where(item => item.ApertureId == aperture.ApertureId)); SelectedWidth = aperture.Width; SelectedHeight = aperture.Height; }
    private void SetSelection(IEnumerable<PastePrimitive> primitives)
    {
        SelectedPrimitives = primitives.Distinct().ToList(); var first = SelectedPrimitives.FirstOrDefault(); SelectedWidth = first?.Width ?? 0; SelectedHeight = first?.Height ?? 0;
        _synchronizingSelection = true; SelectedAperture = first is null ? null : ModifiedPasteLayer.Apertures.FirstOrDefault(item => item.ApertureId == first.ApertureId); _synchronizingSelection = false;
        Raise(nameof(SelectedCount)); Raise(nameof(SelectionInfo));
    }
    private void ApplyEdit()
    {
        if (SelectedCount == 0) return; foreach (var id in SelectedPrimitives.Select(item => item.ApertureId).Distinct()) { var aperture = ModifiedPasteLayer.Apertures.FirstOrDefault(item => item.ApertureId == id); if (aperture is not null) { aperture.Width = Math.Max(.001, SelectedWidth); aperture.Height = Math.Max(.001, SelectedHeight); } } ApplyApertureTable(); Raise(nameof(SelectionInfo));
    }
    private void ApplyApertureTable() { foreach (var item in ModifiedPasteLayer.Apertures.Where(item => item.Shape.Contains("Circle", StringComparison.OrdinalIgnoreCase) || item.Shape.Contains("Round", StringComparison.OrdinalIgnoreCase))) { if (item.Radius > 0) { item.Width = item.Radius * 2; item.Height = item.Radius * 2; } } ApertureOverrides = ModifiedPasteLayer.Apertures.ToDictionary(item => item.ApertureId); }
    private void Select(string name, StencilViewMode mode) { ModeName = name; ViewMode = mode; }
    private void ResetView() { Zoom = 1; OffsetX = 0; OffsetY = 0; }
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; Raise(name ?? string.Empty); if (name == nameof(Zoom)) Raise(nameof(ZoomText)); return true; }
    private void Raise(string name) => PropertyChanged?.Invoke(this, new(name));
    private sealed class ViewerCommand(Action execute) : ICommand { public event EventHandler? CanExecuteChanged { add { } remove { } } public bool CanExecute(object? parameter) => true; public void Execute(object? parameter) => execute(); }
}