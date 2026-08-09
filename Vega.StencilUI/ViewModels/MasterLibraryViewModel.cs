using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.StencilUI.ViewModels;

public sealed class MasterLibraryViewModel : INotifyPropertyChanged
{
    private readonly PackageDefinitionService _service;
    private string _searchText = string.Empty;
    private string? _selectedFamily;
    private PackageDefinition? _selectedPackage;

    public MasterLibraryViewModel() : this(new PackageDefinitionService()) { }
    public MasterLibraryViewModel(PackageDefinitionService service)
    {
        _service = service;
        Load();
    }

    public ObservableCollection<PackageDefinition> Packages { get; } = [];
    public ObservableCollection<string> Families { get; } = [];
    public ObservableCollection<PackageAlias> Aliases { get; } = [];
    public string SearchText { get => _searchText; set { if (Set(ref _searchText, value)) LoadPackages(); } }
    public string? SelectedFamily { get => _selectedFamily; set { if (Set(ref _selectedFamily, value)) LoadPackages(); } }
    public PackageDefinition? SelectedPackage
    {
        get => _selectedPackage;
        set { if (Set(ref _selectedPackage, value)) { LoadAliases(); Raise(nameof(HasSelectedPackage)); } }
    }
    public bool HasSelectedPackage => SelectedPackage is not null;
    public string StatusText => $"Family: {SelectedFamily ?? "All"}   Packages: {Packages.Count}   Selected: {SelectedPackage?.PackageName ?? "None"}";

    public void ShowAllPackages() { SelectedFamily = null; SearchText = string.Empty; }
    public void RefreshAndSelect(string? packageName) { LoadPackages(); SelectedPackage = Packages.FirstOrDefault(p => p.PackageName.Equals(packageName, StringComparison.OrdinalIgnoreCase)) ?? Packages.FirstOrDefault(); }
    public void SelectFamily(string? family) => SelectedFamily = family;
    public void Load()
    {
        Families.Clear();
        foreach (var family in _service.GetAll().Where(p => !string.IsNullOrWhiteSpace(p.PackageFamily)).Select(p => p.PackageFamily).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x)) Families.Add(family);
        LoadPackages();
    }
    private void LoadPackages()
    {
        var source = string.IsNullOrWhiteSpace(SearchText) ? _service.GetAll() : _service.Search(SearchText);
        if (!string.IsNullOrWhiteSpace(SelectedFamily)) source = source.Where(p => p.PackageFamily.Equals(SelectedFamily, StringComparison.OrdinalIgnoreCase)).ToList();
        Packages.Clear(); foreach (var package in source.OrderBy(p => p.PackageName)) Packages.Add(package);
        SelectedPackage = Packages.FirstOrDefault(); Raise(nameof(StatusText));
    }
    private void LoadAliases()
    {
        Aliases.Clear(); if (SelectedPackage is not null) foreach (var alias in _service.GetAliases(SelectedPackage.Id)) Aliases.Add(alias); Raise(nameof(StatusText));
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field,value)) return false; field=value; PropertyChanged?.Invoke(this,new(name)); return true; }
    private void Raise(string name) => PropertyChanged?.Invoke(this,new(name));
}