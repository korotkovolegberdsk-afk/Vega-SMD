using System.Windows;
using Vega.Infrastructure.Tools.ProjectArchive;

namespace Vega.UI.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }


    private void Exit_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }


    private void CreateArchive_Click(
        object sender,
        RoutedEventArgs e)
    {
        var service =
            new ProjectArchiveService();


        var archive =
            service.CreateArchive(
                @"D:\Projects\Vega-SMD\Source\Vega-SMD",
                @"D:\Projects\Vega-SMD\Archives");


        MessageBox.Show(
            $"Архив создан:\n{archive}",
            "Vega-SMD");
    }


    private void AddPackage_Click(
        object sender,
        RoutedEventArgs e)
    {
        var editor =
            new PackageEditorWindow();


        var result =
            editor.ShowDialog();


        if (result == true)
        {
            PackageLibrary.Reload();
        }
    }


    private void OpenComponentLibrary_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            new Window
            {
                Title = "Master Library - Components",
                Width = 1200,
                Height = 700,
                Content =
                    new ComponentLibraryView()
            };


        window.Show();
    }
}