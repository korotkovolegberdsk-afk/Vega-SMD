using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Models.Packages;
using Vega.Services.Library;

namespace Vega.UI.ViewModels;

public class PackageLibraryViewModel : INotifyPropertyChanged
{
    private readonly PackageService _packageService;


    private List<PackageSearchResult> _allPackages = new();



    public ObservableCollection<PackageSearchResult> Packages { get; }
        = new();



    private PackageSearchResult? _selectedPackage;

    public PackageSearchResult? SelectedPackage
    {
        get => _selectedPackage;

        set
        {
            _selectedPackage = value;
            OnPropertyChanged();
        }
    }



    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;

        set
        {
            _searchText = value;

            OnPropertyChanged();

            FilterPackages();
        }
    }



    public PackageLibraryViewModel()
    {
        _packageService = new PackageService();

        _allPackages = _packageService.GetPackages();

        FilterPackages();
    }



    private void FilterPackages()
    {
        Packages.Clear();


        foreach (var package in _allPackages)
        {
            var text = SearchText.Trim();


            if (string.IsNullOrEmpty(text))
            {
                Packages.Add(package);
                continue;
            }



            if (package.PackageName.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase)
                ||
                package.Category.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase)
                ||
                package.Family.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase)
                ||
                package.IPCName.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase)
                ||
                package.YamahaName.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase)
                ||
                package.MirtecName.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase))
            {
                Packages.Add(package);
            }
        }
    }



    public event PropertyChangedEventHandler? PropertyChanged;


    private void OnPropertyChanged(
        [CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(name));
    }
}