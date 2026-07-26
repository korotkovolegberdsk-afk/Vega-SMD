using System.Windows;
using Vega.Models.Packages;
using Vega.Services.Library;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class PackageEditorWindow : Window
{
    private readonly PackageService _packageService;

    private readonly PackageSearchResult? _editPackage;



    public PackageEditorWindow()
    {
        InitializeComponent();

        DataContext = new PackageEditorViewModel();

        _packageService = new PackageService();
    }



    public PackageEditorWindow(PackageSearchResult package)
        : this()
    {
        _editPackage = package;


        var model = DataContext as PackageEditorViewModel;


        if (model == null)
            return;


        model.PackageName = package.PackageName;

        model.DisplayName = package.DisplayName;

        model.Length = package.Length;

        model.Width = package.Width;

        model.Height = package.Height;

        model.IPCName = package.IPCName;

        model.AOIRecommendations = package.AOIRecommendations;
    }



    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var model = DataContext as PackageEditorViewModel;


        if (model == null)
            return;



        var package = new PackageSearchResult
        {
            Id = _editPackage?.Id ?? 0,

            PackageName = model.PackageName,

            DisplayName = model.DisplayName,

            Length = model.Length,

            Width = model.Width,

            Height = model.Height,

            IPCName = model.IPCName,

            AOIRecommendations = model.AOIRecommendations
        };



        if (_editPackage == null)
        {
            _packageService.AddPackage(package);
        }
        else
        {
            _packageService.UpdatePackage(package);
        }



        DialogResult = true;

        Close();
    }



    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;

        Close();
    }
}