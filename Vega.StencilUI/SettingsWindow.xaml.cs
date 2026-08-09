using System.Windows;
using Vega.StencilUI.ViewModels;

namespace Vega.StencilUI;

public partial class SettingsWindow : Window
{
    public SettingsWindow() => InitializeComponent();
    private void Save_Click(object sender, RoutedEventArgs e) { ((SettingsViewModel)DataContext).Save(); DialogResult = true; }
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}