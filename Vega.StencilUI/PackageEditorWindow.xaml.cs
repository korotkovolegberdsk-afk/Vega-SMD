using System.ComponentModel;
using System.Windows;
using Vega.Models.MasterLibrary;
using Vega.StencilUI.ViewModels;
namespace Vega.StencilUI;
public partial class PackageEditorWindow:Window
{
 public PackageEditorWindow(PackageEditorMode mode,PackageDefinition? package=null){InitializeComponent();DataContext=new PackageEditorViewModel(mode,package);Closing+=OnClosing;}
 public PackageEditorViewModel ViewModel=>(PackageEditorViewModel)DataContext;
 private void Save_Click(object s,RoutedEventArgs e){if(ViewModel.Save()){DialogResult=true;Close();}}
 private void Cancel_Click(object s,RoutedEventArgs e){DialogResult=false;Close();}
 private void OnClosing(object? sender,CancelEventArgs e){if(ViewModel.IsSaved||!ViewModel.IsDirty)return;var answer=MessageBox.Show("Save changes?","Package Editor",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);if(answer==MessageBoxResult.Cancel){e.Cancel=true;return;}if(answer==MessageBoxResult.Yes&&!ViewModel.Save())e.Cancel=true;}
}