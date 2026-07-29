using System.Windows;
using System.Windows.Controls;
using Vega.Data.MasterLibrary.Repository;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class ComponentLibraryView : UserControl
{
    private readonly ComponentLibraryViewModel _viewModel;

    private readonly ComponentDefinitionRepository _repository;



    public ComponentLibraryView()
    {
        InitializeComponent();


        _viewModel =
            new ComponentLibraryViewModel();


        _repository =
            new ComponentDefinitionRepository();


        DataContext =
            _viewModel;
    }



    private void NewComponent_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            new ComponentEditorWindow();


        var result =
            window.ShowDialog();


        if (result == true)
        {
            _viewModel.Reload();
        }
    }



    private void ComponentsGrid_MouseDoubleClick(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedComponent == null)
        {
            return;
        }


        var component =
            _repository.GetById(
                _viewModel.SelectedComponent.Id);



        if (component == null)
        {
            MessageBox.Show(
                "Component not found.",
                "Vega-SMD",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }



        var window =
            new ComponentEditorWindow(component);



        var result =
            window.ShowDialog();



        if (result == true)
        {
            _viewModel.Reload();
        }
    }
}