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



    public ObservableCollection<string> Categories { get; }
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





    private string _selectedCategory = "";

    public string SelectedCategory
    {
        get => _selectedCategory;

        set
        {
            _selectedCategory = value;

            OnPropertyChanged();

            FilterPackages();
        }
    }





    public PackageLibraryViewModel()
    {
        _packageService = new PackageService();

        Reload();
    }





    public void Reload()
    {
        _allPackages = _packageService.GetPackages();


        LoadCategories();


        FilterPackages();
    }





    private void LoadCategories()
    {
        Categories.Clear();


        foreach (var category in _allPackages
                     .Select(x => x.Category)
                     .Where(x => !string.IsNullOrEmpty(x))
                     .Distinct()
                     .OrderBy(x => x))
        {
            Categories.Add(category);
        }
    }





    private void FilterPackages()
    {
        Packages.Clear();


        var text = SearchText.Trim();



        foreach (var package in _allPackages)
        {

            if (!string.IsNullOrEmpty(SelectedCategory))
            {
                bool categoryMatch =
                    package.Category.Contains(
                        SelectedCategory,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    package.Family.Contains(
                        SelectedCategory,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    package.PackageName.Contains(
                        SelectedCategory,
                        StringComparison.OrdinalIgnoreCase);



                if (!categoryMatch)
                {
                    continue;
                }
            }




            if (!string.IsNullOrEmpty(text))
            {
                bool searchMatch =
                    package.PackageName.Contains(
                        text,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    package.DisplayName.Contains(
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
                        StringComparison.OrdinalIgnoreCase);



                if (!searchMatch)
                {
                    continue;
                }
            }



            Packages.Add(package);
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