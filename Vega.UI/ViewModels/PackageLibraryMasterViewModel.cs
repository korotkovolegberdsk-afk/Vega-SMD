using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.UI.ViewModels;

public class PackageLibraryMasterViewModel : INotifyPropertyChanged
{
    private readonly PackageDefinitionService _packageService;
    private List<PackageDefinition> _allPackages = new();
    private PackageDefinition? _selectedPackage;
    private string _searchText = string.Empty;
    private MasterPackageCategoryNode? _selectedCategory;

    public PackageLibraryMasterViewModel(
        PackageDefinitionService packageService)
    {
        _packageService = packageService
            ?? throw new ArgumentNullException(nameof(packageService));

        Reload();
    }

    public ObservableCollection<PackageDefinition> Packages { get; }
        = new();

    public ObservableCollection<MasterPackageCategoryNode> Categories { get; }
        = new();

    public PackageDefinition? SelectedPackage
    {
        get => _selectedPackage;
        set
        {
            if (_selectedPackage == value)
            {
                return;
            }

            _selectedPackage = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
            {
                return;
            }

            _searchText = value;
            OnPropertyChanged();
            FilterPackages();
        }
    }

    public MasterPackageCategoryNode? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory == value)
            {
                return;
            }

            _selectedCategory = value;
            OnPropertyChanged();
            FilterPackages();
            SelectPackageFromCategory();
        }
    }

    public void Reload()
    {
        _allPackages = _packageService.GetAll()
            .Where(x => x.IsActive)
            .ToList();

        LoadCategories();
        FilterPackages();
    }

    private void LoadCategories()
    {
        Categories.Clear();

        foreach (var categoryGroup in _allPackages
                     .GroupBy(x => x.CategoryId)
                     .OrderBy(x => x.Key))
        {
            var categoryNode = new MasterPackageCategoryNode(
                $"Category {categoryGroup.Key}",
                categoryGroup.Key);

            foreach (var familyGroup in categoryGroup
                         .GroupBy(x => x.FamilyId)
                         .OrderBy(x => x.Key))
            {
                var familyNode = new MasterPackageCategoryNode(
                    $"Family {familyGroup.Key}",
                    categoryGroup.Key,
                    familyGroup.Key);

                foreach (var package in familyGroup
                             .OrderBy(x => x.PackageName))
                {
                    familyNode.Children.Add(
                        new MasterPackageCategoryNode(
                            package.PackageName,
                            categoryGroup.Key,
                            familyGroup.Key,
                            package.Id));
                }

                categoryNode.Children.Add(familyNode);
            }

            Categories.Add(categoryNode);
        }
    }

    private void FilterPackages()
    {
        Packages.Clear();

        var searchText = SearchText.Trim();

        foreach (var package in _allPackages)
        {
            if (SelectedCategory != null
                && !MatchesSelectedCategory(package))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(searchText)
                && !MatchesSearch(package, searchText))
            {
                continue;
            }

            Packages.Add(package);
        }

        if (SelectedPackage != null
            && !Packages.Contains(SelectedPackage))
        {
            SelectedPackage = Packages.FirstOrDefault();
        }
    }

    private bool MatchesSelectedCategory(PackageDefinition package)
    {
        if (SelectedCategory == null)
        {
            return true;
        }

        if (SelectedCategory.PackageId.HasValue)
        {
            return package.Id == SelectedCategory.PackageId.Value;
        }

        if (SelectedCategory.FamilyId.HasValue)
        {
            return package.CategoryId == SelectedCategory.CategoryId
                   && package.FamilyId == SelectedCategory.FamilyId.Value;
        }

        return package.CategoryId == SelectedCategory.CategoryId;
    }

    private void SelectPackageFromCategory()
    {
        if (SelectedCategory?.PackageId is not int packageId)
        {
            return;
        }

        SelectedPackage = Packages.FirstOrDefault(x => x.Id == packageId);
    }

    private static bool MatchesSearch(
        PackageDefinition package,
        string searchText)
    {
        return Contains(package.PackageName, searchText)
               || Contains(package.DisplayName, searchText)
               || Contains(package.Description, searchText)
               || Contains(package.IPCName, searchText)
               || Contains(package.JEDECName, searchText)
               || Contains(package.LandPatternName, searchText)
               || Contains(package.Notes, searchText);
    }

    private static bool Contains(string value, string searchText)
    {
        return value.Contains(
            searchText,
            StringComparison.OrdinalIgnoreCase);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}

public class MasterPackageCategoryNode
{
    public MasterPackageCategoryNode(
        string name,
        int categoryId,
        int? familyId = null,
        int? packageId = null)
    {
        Name = name;
        CategoryId = categoryId;
        FamilyId = familyId;
        PackageId = packageId;
    }

    public string Name { get; }

    public int CategoryId { get; }

    public int? FamilyId { get; }

    public int? PackageId { get; }

    public ObservableCollection<MasterPackageCategoryNode> Children { get; }
        = new();
}
