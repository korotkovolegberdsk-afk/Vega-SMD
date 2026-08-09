using System.Windows;
namespace Vega.StencilUI;
public partial class MasterLibraryWindow : Window
{
 public MasterLibraryWindow(){InitializeComponent();}
 private void AllPackages_Click(object sender, RoutedEventArgs e) => ((ViewModels.MasterLibraryViewModel)DataContext).ShowAllPackages();
}