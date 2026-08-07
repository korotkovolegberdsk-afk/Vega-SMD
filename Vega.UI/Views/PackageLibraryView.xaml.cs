using System.Windows;
using System.Windows.Controls;
using Vega.Services.MasterLibrary;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class PackageLibraryView : UserControl
{
    private readonly PackageLibraryMasterViewModel _viewModel;
    private readonly PackageDefinitionService _packageService;

    public PackageLibraryView()
    {
        InitializeComponent();

        _packageService = new PackageDefinitionService();
        _viewModel = new PackageLibraryMasterViewModel(_packageService);

        DataContext = _viewModel;
    }

    public void Reload()
    {
        _viewModel.Reload();
    }

    private void Category_Selected(
        object sender,
        RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is MasterPackageCategoryNode category)
        {
            _viewModel.SelectedCategory = category;
        }
    }

    private void NewPackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        var editor = new PackageEditorWindow();

        if (editor.ShowDialog() == true)
        {
            Reload();
        }
    }

    private void EditPackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedPackage == null)
        {
            return;
        }

        var editor = new PackageEditorWindow(_viewModel.SelectedPackage.Id);

        if (editor.ShowDialog() == true)
        {
            Reload();
        }
    }

    private void DeletePackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedPackage == null)
        {
            return;
        }

        var answer = MessageBox.Show(
            $"Деактивировать корпус {_viewModel.SelectedPackage.PackageName}?",
            "Vega-SMD",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        _packageService.Deactivate(_viewModel.SelectedPackage.Id);
        Reload();
    }

    private void PackageList_MouseDoubleClick(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedPackage == null)
        {
            return;
        }

        var editor = new PackageEditorWindow(_viewModel.SelectedPackage.Id);

        if (editor.ShowDialog() == true)
        {
            Reload();
        }
    }
}
