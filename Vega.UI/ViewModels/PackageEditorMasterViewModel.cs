using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.UI.ViewModels;

public class PackageEditorMasterViewModel : INotifyPropertyChanged
{
    private readonly PackageDefinitionService _packageService;
    private string _packageName = string.Empty;
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private int _selectedCategoryId;
    private int _selectedFamilyId;
    private double _length;
    private double _width;
    private double _height;
    private double _pitch;
    private int _leadCount;
    private int _padCount;
    private int _thermalPadCount;
    private string _ipcName = string.Empty;
    private string _jedecName = string.Empty;
    private string _landPatternName = string.Empty;
    private string _polarityMark = string.Empty;
    private string _notes = string.Empty;
    private string _datasheetUrl = string.Empty;
    private string _createdBy = string.Empty;
    private string _updatedBy = string.Empty;
    private string _changeComment = string.Empty;
    private bool _isActive = true;
    private int _version;
    private PackageProcessProfile _processProfile = new();
    private PackageGeometry _geometry = new();
    private PackageFootprint _footprint = new();
    private double _stencilThickness;
    private string _apertureType = string.Empty;
    private string _apertureReduction = string.Empty;
    private double _areaRatio;
    private double _aspectRatio;
    private string _spiRecommendations = string.Empty;
    private string _aoiRecommendations = string.Empty;
    private string _typicalDefects = string.Empty;
    private string _recommendedProfile = string.Empty;

    public PackageEditorMasterViewModel(
        PackageDefinitionService? packageService = null)
    {
        _packageService = packageService
            ?? new PackageDefinitionService();

        LoadCategories();
        CreateNew();
    }

    public ObservableCollection<PackageCategory> Categories { get; }
        = new();

    public ObservableCollection<PackageFamily> Families { get; }
        = new();

    public ObservableCollection<EquipmentAlias> Aliases { get; }
        = new();

    public int Id { get; private set; }

    public bool IsNew => Id == 0;

    public int SelectedCategoryId
    {
        get => _selectedCategoryId;
        set
        {
            if (_selectedCategoryId == value)
            {
                return;
            }

            _selectedCategoryId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CategoryId));
            LoadFamilies();
        }
    }

    public int SelectedFamilyId
    {
        get => _selectedFamilyId;
        set
        {
            if (_selectedFamilyId == value)
            {
                return;
            }

            _selectedFamilyId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FamilyId));
        }
    }

    public int CategoryId
    {
        get => SelectedCategoryId;
        set => SelectedCategoryId = value;
    }

    public int FamilyId
    {
        get => SelectedFamilyId;
        set => SelectedFamilyId = value;
    }

    public string PackageName
    {
        get => _packageName;
        set => SetField(ref _packageName, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetField(ref _displayName, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public double Length
    {
        get => _length;
        set => SetField(ref _length, value);
    }

    public double Width
    {
        get => _width;
        set => SetField(ref _width, value);
    }

    public double Height
    {
        get => _height;
        set => SetField(ref _height, value);
    }

    public double Pitch
    {
        get => _pitch;
        set => SetField(ref _pitch, value);
    }

    public int LeadCount
    {
        get => _leadCount;
        set => SetField(ref _leadCount, value);
    }

    public int PadCount
    {
        get => _padCount;
        set => SetField(ref _padCount, value);
    }

    public int ThermalPadCount
    {
        get => _thermalPadCount;
        set => SetField(ref _thermalPadCount, value);
    }

    public string IPCName
    {
        get => _ipcName;
        set => SetField(ref _ipcName, value);
    }

    public string JEDECName
    {
        get => _jedecName;
        set => SetField(ref _jedecName, value);
    }

    public string LandPatternName
    {
        get => _landPatternName;
        set => SetField(ref _landPatternName, value);
    }

    public string PolarityMark
    {
        get => _polarityMark;
        set => SetField(ref _polarityMark, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    public PackageProcessProfile ProcessProfile => _processProfile;

    public PackageGeometry Geometry => _geometry;

    public PackageFootprint Footprint => _footprint;

    public double StencilThickness
    {
        get => _stencilThickness;
        set => SetField(ref _stencilThickness, value);
    }

    public string ApertureType
    {
        get => _apertureType;
        set => SetField(ref _apertureType, value);
    }

    public string ApertureReduction
    {
        get => _apertureReduction;
        set => SetField(ref _apertureReduction, value);
    }

    public double AreaRatio
    {
        get => _areaRatio;
        set => SetField(ref _areaRatio, value);
    }

    public double AspectRatio
    {
        get => _aspectRatio;
        set => SetField(ref _aspectRatio, value);
    }

    public string SPIRecommendations
    {
        get => _spiRecommendations;
        set => SetField(ref _spiRecommendations, value);
    }

    public string AOIRecommendations
    {
        get => _aoiRecommendations;
        set => SetField(ref _aoiRecommendations, value);
    }

    public string TypicalDefects
    {
        get => _typicalDefects;
        set => SetField(ref _typicalDefects, value);
    }

    public string RecommendedProfile
    {
        get => _recommendedProfile;
        set => SetField(ref _recommendedProfile, value);
    }
    public void CreateNew()
    {
        Apply(new PackageDefinition());
        ApplyProcessProfile(new PackageProcessProfile());
        Aliases.Clear();
        _isActive = true;
    }

    public void Load(int id)
    {
        var package = _packageService.GetById(id)
            ?? throw new InvalidOperationException(
                $"Корпус с Id {id} не найден.");

        Apply(package);
        ApplyProcessProfile(
            _packageService.GetProcessProfile(package.Id)
            ?? new PackageProcessProfile { PackageId = package.Id });
        LoadAliases(package.Id);
        ApplyGeometry(_packageService.GetGeometry(package.Id) ?? new PackageGeometry { PackageId = package.Id });
        ApplyFootprint(_packageService.GetFootprint(package.Id) ?? new PackageFootprint { PackageId = package.Id });
    }

    public void Update()
    {
        if (IsNew)
        {
            throw new InvalidOperationException(
                "Новый корпус необходимо сохранить через Save.");
        }

        Save();
    }

    public void Save()
    {
        var package = CreatePackageDefinition();
        var processProfile = CreateProcessProfile();
        var geometry = Geometry;
        var footprint = Footprint;

        _packageService.Save(package, processProfile, geometry, footprint);

        var savedPackage = _packageService.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        SaveAliases(savedPackage.Id);
        Apply(savedPackage);
        ApplyProcessProfile(
            _packageService.GetProcessProfile(savedPackage.Id)
            ?? new PackageProcessProfile { PackageId = savedPackage.Id });
        LoadAliases(savedPackage.Id);
        ApplyGeometry(_packageService.GetGeometry(savedPackage.Id) ?? new PackageGeometry { PackageId = savedPackage.Id });
        ApplyFootprint(_packageService.GetFootprint(savedPackage.Id) ?? new PackageFootprint { PackageId = savedPackage.Id });
    }
    public void Deactivate()
    {
        if (IsNew)
        {
            throw new InvalidOperationException(
                "Новый корпус нельзя деактивировать.");
        }

        _packageService.Deactivate(Id);
        _isActive = false;
        _version++;
    }
    public void AddAlias()
    {
        Aliases.Add(new EquipmentAlias { IsActive = true });
    }

    public void RemoveAlias(EquipmentAlias alias)
    {
        if (alias.Id > 0)
        {
            _packageService.DeleteEquipmentAlias(alias.Id);
        }

        Aliases.Remove(alias);
    }

    private void LoadAliases(int packageId)
    {
        Aliases.Clear();

        foreach (var alias in _packageService.GetEquipmentAliases(packageId))
        {
            Aliases.Add(alias);
        }
    }

    private void SaveAliases(int packageId)
    {
        foreach (var alias in Aliases)
        {
            alias.PackageId = packageId;
            alias.Vendor = alias.Vendor.Trim();
            alias.Alias = alias.Alias.Trim();
            alias.Notes = alias.Notes.Trim();

            if (alias.Id == 0)
            {
                _packageService.AddEquipmentAlias(alias);
            }
            else
            {
                _packageService.UpdateEquipmentAlias(alias);
            }
        }
    }
    private void LoadCategories()
    {
        Categories.Clear();

        foreach (var category in _packageService.GetCategories())
        {
            Categories.Add(category);
        }
    }

    private void LoadFamilies()
    {
        var selectedFamilyId = _selectedFamilyId;

        Families.Clear();

        foreach (var family in _packageService.GetFamilies(SelectedCategoryId))
        {
            Families.Add(family);
        }

        SelectedFamilyId = Families.Any(x => x.Id == selectedFamilyId)
            ? selectedFamilyId
            : 0;
    }

    private PackageDefinition CreatePackageDefinition()
    {
        return new PackageDefinition
        {
            Id = Id,
            PackageName = PackageName.Trim(),
            DisplayName = DisplayName.Trim(),
            Description = Description.Trim(),
            CategoryId = SelectedCategoryId,
            FamilyId = SelectedFamilyId,
            Length = Length,
            Width = Width,
            Height = Height,
            Pitch = Pitch,
            LeadCount = LeadCount,
            PadCount = PadCount,
            ThermalPadCount = ThermalPadCount,
            IPCName = IPCName.Trim(),
            JEDECName = JEDECName.Trim(),
            LandPatternName = LandPatternName.Trim(),
            PolarityMark = PolarityMark.Trim(),
            DatasheetUrl = _datasheetUrl,
            Notes = Notes.Trim(),
            IsActive = _isActive,
            CreatedBy = _createdBy,
            UpdatedBy = _updatedBy,
            Version = _version,
            ChangeComment = _changeComment
        };
    }

    private PackageProcessProfile CreateProcessProfile()
    {
        _processProfile.PackageId = Id;
        _processProfile.StencilThickness = StencilThickness;
        _processProfile.ApertureType = ApertureType.Trim();
        _processProfile.AreaRatio = AreaRatio;
        _processProfile.AspectRatio = AspectRatio;
        _processProfile.SPIRecommendations = SPIRecommendations.Trim();
        _processProfile.AOIRecommendations = AOIRecommendations.Trim();
        _processProfile.TypicalDefects = TypicalDefects.Trim();
        _processProfile.ReflowRecommendations = RecommendedProfile.Trim();
        _processProfile.Notes = ApertureReduction.Trim();
        _processProfile.IsActive = _isActive;

        return _processProfile;
    }

    private void ApplyProcessProfile(PackageProcessProfile profile)
    {
        _processProfile = profile;
        StencilThickness = profile.StencilThickness;
        ApertureType = profile.ApertureType;
        ApertureReduction = profile.Notes;
        AreaRatio = profile.AreaRatio;
        AspectRatio = profile.AspectRatio;
        SPIRecommendations = profile.SPIRecommendations;
        AOIRecommendations = profile.AOIRecommendations ?? string.Empty;
        TypicalDefects = profile.TypicalDefects;
        RecommendedProfile = profile.ReflowRecommendations;
        OnPropertyChanged(nameof(ProcessProfile));
    }

    private void ApplyFootprint(PackageFootprint footprint)
    {
        _footprint = footprint;
        OnPropertyChanged(nameof(Footprint));
    }

    private void ApplyGeometry(PackageGeometry geometry)
    {
        _geometry = geometry;
        OnPropertyChanged(nameof(Geometry));
    }
    private void Apply(PackageDefinition package)
    {
        Id = package.Id;
        PackageName = package.PackageName;
        DisplayName = package.DisplayName;
        Description = package.Description;
        SelectedCategoryId = package.CategoryId;
        SelectedFamilyId = package.FamilyId;
        Length = package.Length;
        Width = package.Width;
        Height = package.Height;
        Pitch = package.Pitch;
        LeadCount = package.LeadCount;
        PadCount = package.PadCount;
        ThermalPadCount = package.ThermalPadCount;
        IPCName = package.IPCName;
        JEDECName = package.JEDECName;
        LandPatternName = package.LandPatternName;
        PolarityMark = package.PolarityMark;
        Notes = package.Notes;
        _datasheetUrl = package.DatasheetUrl;
        _createdBy = package.CreatedBy;
        _updatedBy = package.UpdatedBy;
        _changeComment = package.ChangeComment;
        _isActive = package.IsActive;
        _version = package.Version;

        OnPropertyChanged(nameof(Id));
        OnPropertyChanged(nameof(IsNew));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
