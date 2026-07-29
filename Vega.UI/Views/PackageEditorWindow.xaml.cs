using System.Windows;
using Vega.Models.MasterLibrary;

namespace Vega.UI.Views;

public partial class PackageEditorWindow : Window
{
    private readonly ComponentDefinition? _component;



    public PackageEditorWindow()
    {
        InitializeComponent();
    }



    public PackageEditorWindow(
        ComponentDefinition component)
    {
        InitializeComponent();

        _component = component;

        LoadComponent();
    }



    public PackageEditorWindow(
        object package)
    {
        InitializeComponent();

        LoadPackage(package);
    }



    private void LoadComponent()
    {
        if (_component == null)
        {
            return;
        }

        Title =
            "Редактор компонента - "
            + _component.ManufacturerPartNumber;
    }



    private void LoadPackage(
        object package)
    {
        Title =
            "Редактор корпуса";
    }



    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;

        Close();
    }



    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBox.Show(
            "Сохранение выполнено.",
            "Vega-SMD",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        DialogResult = true;

        Close();
    }
}