using System.Windows;
using System.Windows.Controls;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class PackageLibraryView : UserControl
{
    private readonly PackageLibraryViewModel _viewModel;


    public PackageLibraryView()
    {
        InitializeComponent();


        _viewModel = new PackageLibraryViewModel();

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
        if (e.NewValue is PackageCategoryNode category)
        {
            _viewModel.SelectedCategory = category;
        }
    }





    private void NewPackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        var editor = new PackageEditorWindow();


        var result = editor.ShowDialog();


        if (result == true)
        {
            Reload();
        }
    }





    private void EditPackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedPackage == null)
            return;


        var editor = new PackageEditorWindow(
            _viewModel.SelectedPackage);


        var result = editor.ShowDialog();


        if (result == true)
        {
            Reload();
        }
    }





    private void DeletePackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedPackage == null)
            return;



        var answer = MessageBox.Show(
            $"Удалить компонент {_viewModel.SelectedPackage.PackageName}?",
            "Vega-SMD",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);



        if (answer != MessageBoxResult.Yes)
            return;



        var service = new Vega.Services.Library.PackageService();


        service.DeletePackage(
            _viewModel.SelectedPackage.Id);



        Reload();
    }





    private void PackageList_MouseDoubleClick(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedPackage == null)
            return;


        var editor = new PackageEditorWindow(
            _viewModel.SelectedPackage);


        var result = editor.ShowDialog();


        if (result == true)
        {
            Reload();
        }
    }
}
