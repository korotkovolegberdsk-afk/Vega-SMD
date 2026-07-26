using System.Windows;
using Vega.Infrastructure.Tools.ProjectArchive;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();

        DataContext = new PackageLibraryViewModel();
    }


    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }


    private void CreateArchive_Click(object sender, RoutedEventArgs e)
    {
        var service = new ProjectArchiveService();


        var archive = service.CreateArchive(
            @"D:\Projects\Vega-SMD\Source\Vega-SMD",
            @"D:\Projects\Vega-SMD\Archives");


        MessageBox.Show(
            $"Архив создан:\n{archive}",
            "Vega-SMD");
    }
}