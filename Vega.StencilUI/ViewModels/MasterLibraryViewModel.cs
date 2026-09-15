using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vega.Data.MasterLibrary.Repository;
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
    private readonly ComponentFootprintRepository _componentFootprintRepository = new();
    private readonly ComponentCadModelRepository _componentCadModelRepository = new();
    private string _searchText = string.Empty;
    private string? _selectedFamily;
    private PackageDefinition? _selectedPackage;
    private PackageGeometry? _selectedGeometry;
    private ComponentDefinition? _selectedComponent;
    private ComponentTapeReelGeometry? _selectedProfile;
    private StencilTechnologyRule? _selectedStencilRule;
    private ComponentFootprint? _selectedComponentFootprint;
    private ComponentCadModel? _selectedComponentCadModel;

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
    public ComponentFootprint? SelectedComponentFootprint { get => _selectedComponentFootprint; private set => Set(ref _selectedComponentFootprint, value); }
    public ComponentCadModel? SelectedComponentCadModel { get => _selectedComponentCadModel; private set => Set(ref _selectedComponentCadModel, value); }
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
        SelectedComponent = RelatedComponents.FirstOrDefault(component =>
            _componentFootprintRepository.GetByComponentId(component.Id) is not null &&
            _componentCadModelRepository.GetByComponentId(component.Id) is not null) ?? RelatedComponents.FirstOrDefault();

        SelectedComponentFootprint = SelectedComponent is null ? null : _componentFootprintRepository.GetByComponentId(SelectedComponent.Id);
        SelectedComponentCadModel = SelectedComponent is null ? null : _componentCadModelRepository.GetByComponentId(SelectedComponent.Id);

        if (SelectedComponent is null && package is not null)
            SelectedComponent = CreatePackageReference(package);

        if (SelectedComponent is null)
        {
            SelectedProfile = null;
        }
        else
        {
            var profiles = _tapeService.GetProfiles(SelectedComponent.Id);
            SelectedProfile = profiles.FirstOrDefault(profile => profile.IsDefault) ?? profiles.FirstOrDefault();
            if (SelectedProfile is null && package is not null && IsSot23(package))
                SelectedProfile = CreateSot23TapeProfile(SelectedComponent.Id);
            if (SelectedProfile is null && package is not null && IsC0402(package))
                SelectedProfile = CreateC0402ReferenceTapeProfile(SelectedComponent.Id);
            if (SelectedProfile is null && package is not null)
                SelectedProfile = CreateProportionalTapeProfile(SelectedComponent.Id, package);
        }

        SelectedStencilRule = package is null
            ? null
            : _stencilTechnologyRuleService.GetRule(package, ApertureStrategy.StandardPasteRelease);
        if (SelectedStencilRule is null && package is not null && IsSot23(package))
            SelectedStencilRule = CreateSot23StencilRule();
        if (SelectedStencilRule is null && package is not null)
            SelectedStencilRule = CreateReferenceStencilRule(package);
    }

    private static bool IsSot23(PackageDefinition? package) =>
        package is not null && string.Equals(package.PackageName?.Trim(), "SOT23", StringComparison.OrdinalIgnoreCase);

    private static bool IsC0402(PackageDefinition? package) =>
        package is not null && string.Equals(package.PackageName?.Trim(), "C0402", StringComparison.OrdinalIgnoreCase);

    private static ComponentDefinition CreatePackageReference(PackageDefinition package) => new()
    {
        PackageId = package.Id,
        Package = package,
        Manufacturer = "Справочный корпус",
        ManufacturerPartNumber = package.PackageName,
        Description = $"{package.PackageFamily}: {package.BodyLength:0.###} × {package.BodyWidth:0.###} × {package.Height:0.###} мм"
    };

    private static ComponentTapeReelGeometry CreateSot23TapeProfile(int componentId) => new()
    {
        ComponentDefinitionId = componentId,
        PackagingCode = "TE85L / L",
        SourceReference = "Toshiba SOT23 package page",
        TapeStandard = "Embossed Tape",
        CarrierTapeWidth = 8.0,
        PocketPitch = 4.0,
        PocketLength = 3.3,
        PocketWidth = 2.0,
        PocketDepth = 1.1,
        SprocketHolePitch = 4.0,
        SprocketHoleDiameter = 1.5,
        FeedDirection = TapeFeedDirection.LeftToRight,
        PocketOrientation = TapePocketOrientation.Deg0,
        Pin1Orientation = "Вывод 1 слева по направлению подачи",
        VerificationStatus = TapeVerificationStatus.ManufacturerVerified,
        Notes = "Подтверждённая карточная спецификация SOT23."
    };

    private static ComponentTapeReelGeometry CreateC0402ReferenceTapeProfile(int componentId) => new()
    {
        ComponentDefinitionId = componentId,
        PackagingCode = "0402 / 8 mm tape",
        SourceReference = "Справочный образец Vega-SMD; размеры кармана TDK C-150C-h",
        TapeStandard = "Embossed Tape",
        CarrierTapeWidth = 8.0,
        PocketPitch = 2.0,
        PocketLength = 1.15,
        PocketWidth = 0.65,
        PocketDepth = 0.55,
        SprocketHolePitch = 4.0,
        SprocketHoleDiameter = 1.5, // Approved C0402_Tape_1 reference: D/W = 90/480 = 1.5/8.
        FeedDirection = TapeFeedDirection.LeftToRight,
        PocketOrientation = TapePocketOrientation.Deg90,
        PickupRotation = 90,
        Pin1Orientation = "Компонент расположен вертикально",
        VerificationStatus = TapeVerificationStatus.Estimated,
        Notes = "Справочная визуализация утверждённого образца; ширина 8 мм и шаг 2 мм требуют подтверждения для конкретного MPN."
    };

    private static ComponentTapeReelGeometry CreateProportionalTapeProfile(int componentId, PackageDefinition package)
    {
        static double Pocket(double size) => size + Math.Min(size * .10, 1.00);
        var length=Math.Max(package.Length,package.BodyLength);
        var width=Math.Max(package.Width,package.BodyWidth);
        return new ComponentTapeReelGeometry
        {
            ComponentDefinitionId=componentId,PackagingCode="Vega proportional pocket",SourceReference="Vega-SMD proportional fallback",
            TapeStandard="Estimated embossed carrier tape",CarrierTapeWidth=Math.Max(8,Math.Ceiling((width+3)/4)*4),
            PocketPitch=Math.Max(2,Math.Ceiling((length+.8)/2)*2),PocketLength=Pocket(length),PocketWidth=Pocket(width),PocketDepth=Pocket(package.Height),
            SprocketHolePitch=4,SprocketHoleDiameter=1.5,FeedDirection=TapeFeedDirection.LeftToRight,
            PocketOrientation=TapePocketOrientation.Deg90,PickupRotation=90,Pin1Orientation="Компонент расположен вертикально",
            VerificationStatus=TapeVerificationStatus.Estimated,
            Notes="Карман увеличен на 10% по каждой оси, но не более чем на 1,00 мм суммарно (0,50 мм с каждой стороны)."
        };
    }

    private static StencilTechnologyRule CreateSot23StencilRule() => new()
    {
        PackageFamily = "SOT",
        PackageName = "SOT23",
        ComponentType = "Transistor",
        TechnologyGoal = "StandardPasteRelease",
        PreferredShape = "Rectangle",
        RecommendedThickness = 0.12,
        StencilThicknessMin = 0.10,
        StencilThicknessMax = 0.15,
        MinAreaRatio = 0.66,
        MinAspectRatio = 1.5,
        Coverage = 100,
        Source = "Vega-SMD recommendation",
        SourceReference = "Aspect Ratio calculation",
        RecommendedBy = "Vega-SMD"
    };

    private static StencilTechnologyRule CreateReferenceStencilRule(PackageDefinition package) => new()
    {
        PackageFamily = package.PackageFamily,
        PackageName = package.PackageName,
        ComponentType = package.ComponentType,
        TechnologyGoal = "ReferencePreview",
        PreferredShape = "Rectangle",
        RecommendedThickness = 0.12,
        StencilThicknessMin = 0.10,
        StencilThicknessMax = 0.15,
        PreferredReductionX = 0,
        PreferredReductionY = 0,
        Coverage = 100,
        Source = "Vega-SMD reference preview",
        SourceReference = "Package contacts from the selected package definition",
        RecommendedBy = "Requires manufacturer verification"
    };
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
