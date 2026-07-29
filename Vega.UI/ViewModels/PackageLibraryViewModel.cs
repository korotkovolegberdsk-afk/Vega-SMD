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


    public ObservableCollection<PackageCategoryNode> Categories { get; }
        = new();


    private PackageSearchResult? _selectedPackage;

    public PackageSearchResult? SelectedPackage
    {
        get => _selectedPackage;

        set
        {
            if (value != null)
            {
                var package =
                    _packageService.GetPackageById(value.Id);

                if (package != null)
                {
                    CopyPackage(package, value);
                }
            }

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


    private PackageCategoryNode? _selectedCategory;

    public PackageCategoryNode? SelectedCategory
    {
        get => _selectedCategory;

        set
        {
            _selectedCategory = value;

            OnPropertyChanged();

            FilterPackages();

            SelectPackageFromCategory();
        }
    }


    public PackageLibraryViewModel()
    {
        _packageService = new PackageService();

        Reload();
    }


    public void Reload()
    {
        _allPackages =
            _packageService.GetPackages();

        LoadCategories();

        FilterPackages();
    }


    private void LoadCategories()
    {
        Categories.Clear();

        foreach (var categoryGroup in _allPackages
                     .Where(x =>
                         !string.IsNullOrWhiteSpace(x.Category))
                     .GroupBy(x => x.Category.Trim())
                     .OrderBy(x => x.Key))
        {
            var categoryNode =
                new PackageCategoryNode(
                    categoryGroup.Key,
                    categoryGroup.Key);

            foreach (var familyGroup in categoryGroup
                         .Where(x =>
                             !string.IsNullOrWhiteSpace(x.Family))
                         .GroupBy(x => x.Family.Trim())
                         .OrderBy(x => x.Key))
            {
                var familyNode =
                    new PackageCategoryNode(
                        familyGroup.Key,
                        categoryGroup.Key,
                        familyGroup.Key);

                foreach (var package in familyGroup
                             .Where(x =>
                                 !string.IsNullOrWhiteSpace(
                                     x.PackageName))
                             .OrderBy(x => x.PackageName))
                {
                    familyNode.Children.Add(
                        new PackageCategoryNode(
                            package.PackageName,
                            categoryGroup.Key,
                            familyGroup.Key,
                            package.PackageName));
                }

                categoryNode.Children.Add(familyNode);
            }

            foreach (var package in categoryGroup
                         .Where(x =>
                             string.IsNullOrWhiteSpace(x.Family)
                             && !string.IsNullOrWhiteSpace(
                                 x.PackageName))
                         .OrderBy(x => x.PackageName))
            {
                categoryNode.Children.Add(
                    new PackageCategoryNode(
                        package.PackageName,
                        categoryGroup.Key,
                        packageName: package.PackageName));
            }

            Categories.Add(categoryNode);
        }
    }


    private void FilterPackages()
    {
        Packages.Clear();

        var text =
            SearchText.Trim();

        foreach (var package in _allPackages)
        {
            if (SelectedCategory != null
                && !MatchesSelectedCategory(package))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(text))
            {
                bool searchMatch =
                    Contains(package.PackageName, text)
                    || Contains(package.DisplayName, text)
                    || Contains(package.Category, text)
                    || Contains(package.Family, text)
                    || Contains(package.IPCName, text)
                    || Contains(package.YamahaName, text)
                    || Contains(package.MirtecName, text);

                if (!searchMatch)
                {
                    continue;
                }
            }

            Packages.Add(package);
        }

        if (SelectedPackage != null
            && !Packages.Contains(SelectedPackage))
        {
            SelectedPackage =
                Packages.FirstOrDefault();
        }
    }


    private bool MatchesSelectedCategory(
        PackageSearchResult package)
    {
        if (SelectedCategory == null)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(
                SelectedCategory.PackageName))
        {
            return EqualsText(
                package.PackageName,
                SelectedCategory.PackageName);
        }

        if (!string.IsNullOrWhiteSpace(
                SelectedCategory.Family))
        {
            return EqualsText(
                       package.Category,
                       SelectedCategory.Category)
                   && EqualsText(
                       package.Family,
                       SelectedCategory.Family);
        }

        return EqualsText(
            package.Category,
            SelectedCategory.Category);
    }


    private void SelectPackageFromCategory()
    {
        if (SelectedCategory == null
            || string.IsNullOrWhiteSpace(
                SelectedCategory.PackageName))
        {
            return;
        }

        SelectedPackage =
            Packages.FirstOrDefault(
                x => EqualsText(
                    x.PackageName,
                    SelectedCategory.PackageName));
    }


    private static void CopyPackage(
        PackageSearchResult source,
        PackageSearchResult target)
    {
        target.Id = source.Id;
        target.PackageName = source.PackageName;
        target.DisplayName = source.DisplayName;

        target.Category = source.Category;
        target.Family = source.Family;

        target.Length = source.Length;
        target.Width = source.Width;
        target.Height = source.Height;

        target.Pitch = source.Pitch;
        target.LeadCount = source.LeadCount;

        target.IPCName = source.IPCName;
        target.JEDECName = source.JEDECName;

        target.YamahaName = source.YamahaName;
        target.MirtecName = source.MirtecName;

        target.StencilThickness =
            source.StencilThickness;

        target.AreaRatio = source.AreaRatio;
        target.AspectRatio = source.AspectRatio;

        target.ApertureType = source.ApertureType;
        target.TypicalDefects = source.TypicalDefects;

        target.AOIRecommendations =
            source.AOIRecommendations;

        target.SPIRecommendations =
            source.SPIRecommendations;

        target.Notes = source.Notes;
    }


    private static bool Contains(
        string? value,
        string text)
    {
        return !string.IsNullOrEmpty(value)
               && value.Contains(
                   text,
                   StringComparison.OrdinalIgnoreCase);
    }


    private static bool EqualsText(
        string? first,
        string? second)
    {
        return string.Equals(
            first,
            second,
            StringComparison.OrdinalIgnoreCase);
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


public class PackageCategoryNode
{
    public PackageCategoryNode(
        string name,
        string category = "",
        string family = "",
        string packageName = "")
    {
        Name = name;
        Category = category;
        Family = family;
        PackageName = packageName;
    }


    public string Name { get; }

    public string Category { get; }

    public string Family { get; }

    public string PackageName { get; }


    public ObservableCollection<PackageCategoryNode> Children { get; }
        = new();
}