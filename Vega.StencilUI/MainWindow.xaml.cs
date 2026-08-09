using System.Windows;
using Vega.StencilProjects.Models;
using Vega.StencilUI.ViewModels;

namespace Vega.StencilUI;

public partial class MainWindow : Window
{
    public MainWindow() { System.Diagnostics.Debug.WriteLine("[INFO] MainWindow loaded"); InitializeComponent(); }
    private void NewProject_Click(object sender, RoutedEventArgs e) { var dialog = new NewStencilProjectWindow { Owner = this }; if (dialog.ShowDialog() == true && dialog.Session?.WorkflowProject is not null && DataContext is EngineeringWorkspaceViewModel workspace) workspace.LoadStencilProject(dialog.Session); }
    private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();
}