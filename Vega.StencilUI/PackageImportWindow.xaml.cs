using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using Vega.StencilUI.ViewModels;
namespace Vega.StencilUI;
public partial class PackageImportWindow:Window
{
 public PackageImportWindow()=>InitializeComponent(); PackageImportViewModel Vm=>(PackageImportViewModel)DataContext;
 void SelectFile_Click(object s,RoutedEventArgs e){var d=new OpenFileDialog{Filter="Package files|*.csv;*.xlsx|CSV|*.csv|Excel|*.xlsx"};if(d.ShowDialog()==true)Vm.Load(d.FileName);}
 void Sheet_SelectionChanged(object s,SelectionChangedEventArgs e){if(IsLoaded)Vm.ReloadSheet();}
 void Validate_Click(object s,RoutedEventArgs e)=>Vm.Validate();
 void Import_Click(object s,RoutedEventArgs e){if(Vm.Preview.Count==0){MessageBox.Show("Run validation first.");return;}var r=Vm.Import();MessageBox.Show($"Imported: {r.Imported}\nUpdated: {r.Updated}\nSkipped: {r.Skipped}\nErrors: {r.Errors.Count}","Package Import");}
 void ShowErrors_Click(object s,RoutedEventArgs e){var errors=Vm.Preview.Where(x=>x.Status==Vega.Services.MasterLibrary.Import.PackageImportStatus.Error).Select(x=>$"Row {x.RowNumber} | {x.PackageName} | {string.Join("; ",x.Messages)}");MessageBox.Show(errors.Any()?string.Join(Environment.NewLine,errors):"No errors.","Import Errors");}
 void Close_Click(object s,RoutedEventArgs e)=>Close();
}
