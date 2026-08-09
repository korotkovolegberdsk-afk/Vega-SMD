using System.Windows;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.StencilUI.ViewModels;
namespace Vega.StencilUI;
public partial class MasterLibraryWindow : Window
{
 public MasterLibraryWindow(){InitializeComponent();}
 private MasterLibraryViewModel Vm=>(MasterLibraryViewModel)DataContext;
 private readonly PackageDefinitionService _service=new();
 private void AllPackages_Click(object sender,RoutedEventArgs e)=>Vm.ShowAllPackages();
 private void AddPackage_Click(object sender,RoutedEventArgs e)=>OpenEditor(PackageEditorMode.Create,null);
 private void EditPackage_Click(object sender,RoutedEventArgs e){if(Vm.SelectedPackage is not null)OpenEditor(PackageEditorMode.Edit,Vm.SelectedPackage);}
 private void Duplicate_Click(object sender,RoutedEventArgs e){if(Vm.SelectedPackage is not null)OpenEditor(PackageEditorMode.Duplicate,_service.ClonePackage(Vm.SelectedPackage.Id));}
 private void Delete_Click(object sender,RoutedEventArgs e){var p=Vm.SelectedPackage;if(p is null)return;var check=_service.CheckDeletePackage(p.Id);if(!check.CanDelete){MessageBox.Show($"Package cannot be deleted:\n{check.Reason}","Master Library");return;}if(MessageBox.Show($"Delete package:\n{p.PackageName}?","Master Library",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes)return;var result=_service.DeletePackage(p.Id);if(!result.CanDelete){MessageBox.Show($"Package cannot be deleted:\n{result.Reason}","Master Library");return;}Vm.RefreshAndSelect(null);}
 private void OpenEditor(PackageEditorMode mode,PackageDefinition? package){var w=new PackageEditorWindow(mode,package){Owner=this};if(w.ShowDialog()==true)Vm.RefreshAndSelect(w.ViewModel.WorkingCopy.PackageName);}
}