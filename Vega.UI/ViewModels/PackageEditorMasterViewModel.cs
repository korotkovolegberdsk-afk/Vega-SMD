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
    private int _categoryId;
    private int _familyId;
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

    public PackageEditorMasterViewModel(
        PackageDefinitionService? packageService = null)
    {
        _packageService = packageService
            ?? new PackageDefinitionService();

        CreateNew();
    }

    public int Id { get; private set; }

    public bool IsNew => Id == 0;

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

    public int CategoryId
    {
        get => _categoryId;
        set => SetField(ref _categoryId, value);
    }

    public int FamilyId
    {
        get => _familyId;
        set => SetField(ref _familyId, value);
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

    public void CreateNew()
    {
        Apply(new PackageDefinition());
        _isActive = true;
        OnPropertyChanged(nameof(IsNew));
    }

    public void Load(int id)
    {
        var package = _packageService.GetById(id)
            ?? throw new InvalidOperationException(
                $"Корпус с Id {id} не найден.");

        Apply(package);
        OnPropertyChanged(nameof(IsNew));
    }

    public void Update()
    {
        if (IsNew)
        {
            throw new InvalidOperationException(
                "Новый корпус необходимо сохранить через Save.");
        }

        _packageService.Update(CreatePackageDefinition());
        _version++;
    }

    public void Save()
    {
        if (IsNew)
        {
            _packageService.Add(CreatePackageDefinition());

            var package = _packageService.GetAll()
                .Single(x => x.PackageName == PackageName);

            Apply(package);
            OnPropertyChanged(nameof(IsNew));
            return;
        }

        Update();
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

    private PackageDefinition CreatePackageDefinition()
    {
        return new PackageDefinition
        {
            Id = Id,
            PackageName = PackageName.Trim(),
            DisplayName = DisplayName.Trim(),
            Description = Description.Trim(),
            CategoryId = CategoryId,
            FamilyId = FamilyId,
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

    private void Apply(PackageDefinition package)
    {
        Id = package.Id;
        PackageName = package.PackageName;
        DisplayName = package.DisplayName;
        Description = package.Description;
        CategoryId = package.CategoryId;
        FamilyId = package.FamilyId;
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
