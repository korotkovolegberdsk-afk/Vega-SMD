using Microsoft.Win32;
using System.Windows;
using Vega.StencilProjects.Models;
using Vega.StencilUI.ViewModels;

namespace Vega.StencilUI;

public partial class NewStencilProjectWindow : Window
{
    public NewStencilProjectWindow() => InitializeComponent();
    public StencilProjectSession? Session => (DataContext as NewStencilProjectViewModel)?.Session;
    private NewStencilProjectViewModel ViewModel => (NewStencilProjectViewModel)DataContext;
    private void BrowseTop_Click(object sender, RoutedEventArgs e) => ViewModel.TopPasteFile = SelectFile("TOP Paste Gerber (*.gtp)|*.gtp|Gerber files (*.gtp;*.gbp)|*.gtp;*.gbp");
    private void BrowseBottom_Click(object sender, RoutedEventArgs e) => ViewModel.BottomPasteFile = SelectFile("BOTTOM Paste Gerber (*.gbp)|*.gbp|Gerber files (*.gtp;*.gbp)|*.gtp;*.gbp");
    private void BrowseAssembly_Click(object sender, RoutedEventArgs e) => ViewModel.AssemblyDrawingFile = SelectFile("Assembly drawing (*.pdf)|*.pdf|All files (*.*)|*.*");
    private static string SelectFile(string filter) { var dialog = new OpenFileDialog { Filter = filter }; return dialog.ShowDialog() == true ? dialog.FileName : ""; }
    private void Create_Click(object sender, RoutedEventArgs e)
    {
        try { ViewModel.Create(); DialogResult = true; }
        catch (Exception exception) { MessageBox.Show(this, exception.Message, "Новый проект трафарета", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}