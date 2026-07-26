using System.Windows;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class ShellWindow : Window
{
    private readonly ShellViewModel _viewModel;

    public ShellWindow()
    {
        InitializeComponent();

        _viewModel = new ShellViewModel();
        DataContext = _viewModel;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}