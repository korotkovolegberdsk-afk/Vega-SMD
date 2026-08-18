using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.StencilUI.PackageDrawing;

namespace Vega.StencilUI.ViewModels;

public sealed record DrawingParameterView(string Key, string DisplayName, string Value, bool IsCount);

public sealed class MasterLibraryViewModel : INotifyPropertyChanged
{
    private readonly PackageDefinitionService _service;
    private readonly ComponentDefinitionService _componentService = new();
    private readonly ComponentTapeReelGeometryService _tapeService = new();
    private readonly StencilTechnologyRuleService _stencilTechnologyRuleService = new();
    private string _searchText = string.Empty;
    private string? _selectedFamily;
    private PackageDefinition? _selectedPackage;
    private PackageGeometry? _selectedGeometry;
    private ComponentDefinition? _selectedComponent;
    private ComponentTapeReelGeometry? _selectedProfile;
    private StencilTechnologyRule? _selectedStencilRule;

    public MasterLibraryViewModel() : this(new PackageDefinitionService()) { }
    public MasterLibraryViewModel(PackageDefinitionService service) { _service = service; Load(); }

    public ObservableCollection<PackageDefinition> Packages { get; } = [];
    public ObservableCollection<string> Families { get; } = [];
    public ObservableCollection<PackageAlias> Aliases { get; } = [];
    public ObservableCollection<DrawingParameterView> DrawingParameters { get; } = [];
    public ObservableCollection<ComponentDefinition> RelatedComponents { get; } = [];

    public string SearchText { get => _searchText; set { if (Set(ref _searchText, value)) LoadPackages(); } }
    public string? SelectedFamily { get => _selectedFamily; set { if (Set(ref _selectedFamily, value)) LoadPackages(); } }
    public PackageDefinition? SelectedPackage
    {
        get => _selectedPackage;
        set
        {
            if (!Set(ref _selectedPackage, value)) return;
            LoadAliases();
            LoadDrawingParameters();
            LoadComponentCard();
            WriteSelectionDebug();
            Raise(nameof(HasSelectedPackage));
            Raise(nameof(ResolvedTemplateName));
            Raise(nameof(OutlineReference));
            Raise(nameof(AlignmentType));
            Raise(nameof(StatusText));
        }
    }

    public bool HasSelectedPackage => SelectedPackage is not null;
    public PackageGeometry? SelectedGeometry { get => _selectedGeometry; private set => Set(ref _selectedGeometry, value); }
    public ComponentDefinition? SelectedComponent { get => _selectedComponent; private set => Set(ref _selectedComponent, value); }
    public ComponentTapeReelGeometry? SelectedProfile { get => _selectedProfile; private set => Set(ref _selectedProfile, value); }
    public StencilTechnologyRule? SelectedStencilRule { get => _selectedStencilRule; private set => Set(ref _selectedStencilRule, value); }
    public string ResolvedTemplateName => SelectedPackage is null ? string.Empty : PackageDrawingTemplateResolver.Resolve(SelectedPackage).Name;
    public string AlignmentType => SelectedPackage is null ? string.Empty : PackageDrawingTemplateResolver.Resolve(SelectedPackage).AlignmentType;
    public string OutlineReference => SelectedPackage is null ? string.Empty : PackageDrawingTemplateResolver.GetReference(SelectedPackage) is { } reference ? $"Reference: {reference.StandardReference}" : "Reference drawing not assigned";
    public string StatusText => $"Family: {SelectedFamily ?? "All"}   Packages: {Packages.Count}   Selected: {SelectedPackage?.PackageName ?? "None"}";

    public void ShowAllPackages() { SelectedFamily = null; SearchText = string.Empty; }
    public void RefreshAndSelect(string? packageName)
    {
        LoadPackages();
        SelectedPackage = Packages.FirstOrDefault(p => p.PackageName.Equals(packageName, StringComparison.OrdinalIgnoreCase)) ?? Packages.FirstOrDefault();
    }
    public void SelectFamily(string? family) => SelectedFamily = family;

    public void Load()
    {
        Families.Clear();
        foreach (var family in _service.GetAll().Where(p => !string.IsNullOrWhiteSpace(p.PackageFamily)).Select(p => p.PackageFamily).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
            Families.Add(family);
        LoadPackages();
    }

    private void LoadPackages()
    {
        var source = string.IsNullOrWhiteSpace(SearchText) ? _service.GetAll() : _service.Search(SearchText);
        if (!string.IsNullOrWhiteSpace(SelectedFamily))
            source = source.Where(p => p.PackageFamily.Equals(SelectedFamily, StringComparison.OrdinalIgnoreCase)).ToList();
        Packages.Clear();
        foreach (var package in source.OrderBy(p => p.PackageName)) Packages.Add(package);
        SelectedPackage = Packages.FirstOrDefault();
        Raise(nameof(StatusText));
    }

    private void LoadAliases()
    {
        Aliases.Clear();
        if (SelectedPackage is not null)
            foreach (var alias in _service.GetAliases(SelectedPackage.Id)) Aliases.Add(alias);
    }

    private void LoadDrawingParameters()
    {
        DrawingParameters.Clear();
        if (SelectedPackage is null) return;

        var template = PackageDrawingTemplateResolver.Resolve(SelectedPackage);
        foreach (var parameter in template.Parameters)
            DrawingParameters.Add(new DrawingParameterView(
                parameter.Key,
                parameter.Name,
                ValueFor(SelectedPackage, parameter.SourceProperty, parameter.IsCount),
                parameter.IsCount));
    }

    private void LoadComponentCard()
    {
        var package = SelectedPackage;
        SelectedGeometry = package is null ? null : _service.GetGeometry(package.Id);
        RelatedComponents.Clear();
        if (package is not null)
        {
            foreach (var component in _componentService.GetAll()
                         .Where(component => component.PackageId == package.Id)
                         .OrderBy(component => component.Manufacturer)
                         .ThenBy(component => component.ManufacturerPartNumber))
                RelatedComponents.Add(component);
        }
        SelectedComponent = RelatedComponents.FirstOrDefault();

        if (SelectedComponent is null)
        {
            SelectedProfile = null;
        }
        else
        {
            var profiles = _tapeService.GetProfiles(SelectedComponent.Id);
            SelectedProfile = profiles.FirstOrDefault(profile => profile.IsDefault) ?? profiles.FirstOrDefault();
        }

        SelectedStencilRule = package is null
            ? null
            : _stencilTechnologyRuleService.GetRule(package, ApertureStrategy.StandardPasteRelease);
    }

    private static string ValueFor(PackageDefinition p, string source, bool isCount)
    {
        var value = source switch
        {
            "BodyLength" => p.BodyLength > 0 ? p.BodyLength : p.Length,
            "BodyWidth" => p.BodyWidth > 0 ? p.BodyWidth : p.Width,
            "Height" => p.Height,
            "LeadCount" => p.LeadCount > 0 ? p.LeadCount : p.PadCount,
            "PadCount" => p.PadCount,
            "Pitch" => p.Pitch,
            "LeadLength" => p.LeadLength,
            "LeadWidth" => p.LeadWidth,
            "ThermalPadLength" => p.ThermalPadLength,
            "ThermalPadWidth" => p.ThermalPadWidth,
            "BallPitch" => p.BallPitch,
            "BallDiameter" => p.BallDiameter,
            _ => 0d
        };
        if (isCount) return value > 0 ? $"{value:0}" : "";
        return value > 0 ? $"{value:0.###} mm" : "";
    }

    private void WriteSelectionDebug()
    {
        var p = SelectedPackage;
        if (p is null) return;
        Debug.WriteLine("=== MASTER LIBRARY PACKAGE SELECTED ===");
        Debug.WriteLine($"PackageId: {p.Id}\nPackageName: {p.PackageName}\nFamily: {p.PackageFamily}\nComponentType: {p.ComponentType}");
        Debug.WriteLine($"Length: {p.Length}\nWidth: {p.Width}\nHeight: {p.Height}\nBodyLength: {p.BodyLength}\nBodyWidth: {p.BodyWidth}");
        Debug.WriteLine($"LeadCount: {p.LeadCount}\nPadCount: {p.PadCount}\nPitch: {p.Pitch}\nLeadLength: {p.LeadLength}\nLeadWidth: {p.LeadWidth}");
        Debug.WriteLine($"ThermalPadLength: {p.ThermalPadLength}\nThermalPadWidth: {p.ThermalPadWidth}\nBallPitch: {p.BallPitch}\nBallDiameter: {p.BallDiameter}");
        Debug.WriteLine($"Aliases: {(Aliases.Count == 0 ? "(none)" : string.Join(", ", Aliases.Select(a => a.Alias)))}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value; PropertyChanged?.Invoke(this, new(name)); return true;
    }
    private void Raise(string name) => PropertyChanged?.Invoke(this, new(name));
}
