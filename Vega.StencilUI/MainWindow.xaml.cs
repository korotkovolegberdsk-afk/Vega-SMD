using System.IO;
using System.Windows;
using Microsoft.Win32;
using Vega.Infrastructure.Tools.ProjectArchive;
using Vega.StencilUI.ViewModels;

namespace Vega.StencilUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        System.Diagnostics.Debug.WriteLine("[INFO] MainWindow loaded");
        InitializeComponent();
    }

    private void NewProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new NewStencilProjectWindow { Owner = this };
        if (dialog.ShowDialog() == true && dialog.Session?.WorkflowProject is not null && DataContext is EngineeringWorkspaceViewModel workspace)
            workspace.LoadStencilProject(dialog.Session);
    }

    private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();
    private void MasterLibrary_Click(object sender, RoutedEventArgs e) => new MasterLibraryWindow { Owner = this }.Show();
    private void CreateProgramArchive_Click(object sender, RoutedEventArgs e)
    {
        const string sourceRoot = @"D:\Projects\Vega-SMD\Source\Vega-SMD";
        if (!Directory.Exists(sourceRoot))
        {
            MessageBox.Show("Не найдена папка исходного кода Vega-SMD.", "Архив программы", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить архив программы Vega-SMD",
            Filter = "ZIP archive (*.zip)|*.zip",
            DefaultExt = ".zip",
            AddExtension = true,
            FileName = $"Vega-SMD_Source_{timestamp}.zip",
            OverwritePrompt = true
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var result = new ProjectArchiveService().CreateSourceArchive(sourceRoot, dialog.FileName);
            var sizeMb = result.SizeBytes / 1024d / 1024d;
            MessageBox.Show($"Архив программы создан.\n\nFile: {Path.GetFileName(result.ArchivePath)}\nFiles: {result.FileCount}\nSize: {sizeMb:0.##} MB", "Архив программы", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Не удалось создать архив программы:\n{exception.Message}", "Архив программы", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void CreateProjectArchive_Click(object sender, RoutedEventArgs e)
    {
        var sourceDirectory = (DataContext as EngineeringWorkspaceViewModel)?.ProjectArchiveSourceDirectory;
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            MessageBox.Show("Сначала загрузите проект с входными файлами.", "Архив проекта", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new OpenFolderDialog { Title = "Выберите папку для архива проекта" };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var archive = new ProjectArchiveService().CreateArchive(sourceDirectory, dialog.FolderName);
            MessageBox.Show($"Архив создан:\n{archive}", "Архив проекта", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Не удалось создать архив:\n{exception.Message}", "Архив проекта", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}