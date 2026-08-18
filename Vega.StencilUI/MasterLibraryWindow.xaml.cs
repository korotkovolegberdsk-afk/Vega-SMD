using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Vega.Services.MasterLibrary.Import;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.StencilUI.ViewModels;

namespace Vega.StencilUI;

public partial class MasterLibraryWindow : Window
{
    public MasterLibraryWindow() => InitializeComponent();

    private MasterLibraryViewModel Vm => (MasterLibraryViewModel)DataContext;
    private readonly PackageDefinitionService _service = new();

    private void AllPackages_Click(object sender, RoutedEventArgs e) => Vm.ShowAllPackages();
    private void AddPackage_Click(object sender, RoutedEventArgs e) => OpenEditor(PackageEditorMode.Create, null);
    private void ImportPackages_Click(object sender, RoutedEventArgs e) => new PackageImportWindow { Owner = this }.ShowDialog();

    private void ExportImportTemplate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Excel workbook|*.xlsx", FileName = "MasterLibrary_Package_Template.xlsx" };
        if (dialog.ShowDialog() == true)
            new PackageImportService().ExportTemplate(dialog.FileName);
    }

    private void EditPackage_Click(object sender, RoutedEventArgs e) => EditSelectedPackage();

    private void PackagesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FindParent<DataGridRow>(e.OriginalSource as DependencyObject)?.DataContext is not PackageDefinition package)
            return;

        Vm.SelectedPackage = package;
        OpenEditor(PackageEditorMode.Edit, package);
    }

    private void PackagesGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Vm.SelectedPackage is null)
            return;

        e.Handled = true;
        EditSelectedPackage();
    }

    private void EditSelectedPackage()
    {
        if (Vm.SelectedPackage is not null)
            OpenEditor(PackageEditorMode.Edit, Vm.SelectedPackage);
    }

    private void Duplicate_Click(object sender, RoutedEventArgs e)
    {
        if (Vm.SelectedPackage is not null)
            OpenEditor(PackageEditorMode.Duplicate, _service.ClonePackage(Vm.SelectedPackage.Id));
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        var package = Vm.SelectedPackage;
        if (package is null)
            return;

        var check = _service.CheckDeletePackage(package.Id);
        if (!check.CanDelete)
        {
            MessageBox.Show($"Package cannot be deleted:\n{check.Reason}", "Master Library");
            return;
        }

        if (MessageBox.Show($"Delete package:\n{package.PackageName}?", "Master Library", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        var result = _service.DeletePackage(package.Id);
        if (!result.CanDelete)
        {
            MessageBox.Show($"Package cannot be deleted:\n{result.Reason}", "Master Library");
            return;
        }

        Vm.RefreshAndSelect(null);
    }

    private void OpenEditor(PackageEditorMode mode, PackageDefinition? package)
    {
        var window = new PackageEditorWindow(mode, package) { Owner = this };
        if (window.ShowDialog() == true)
            Vm.RefreshAndSelect(window.ViewModel.WorkingCopy.PackageName);
    }

    private static T? FindParent<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source is not null)
        {
            if (source is T match)
                return match;
            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }
}
