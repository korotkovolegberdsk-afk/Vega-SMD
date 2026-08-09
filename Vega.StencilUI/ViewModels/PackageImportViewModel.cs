using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Services.MasterLibrary.Import;
namespace Vega.StencilUI.ViewModels;
public sealed class PackageImportViewModel:INotifyPropertyChanged
{
 readonly PackageImportService _service; PackageImportDocument? _document; string _fileName="",_selectedSheet="",_unit="mm",_message=""; PackageImportMode _mode;
 public PackageImportViewModel():this(new()){} public PackageImportViewModel(PackageImportService service)=>_service=service;
 public ObservableCollection<string> Sheets {get;}=[]; public ObservableCollection<PackageImportMapping> Mappings {get;}=[]; public ObservableCollection<PackageImportValidationResult> Preview {get;}=[];
 public string FileName{get=>_fileName;private set=>Set(ref _fileName,value);} public string SelectedSheet{get=>_selectedSheet;set=>Set(ref _selectedSheet,value);} public string Unit{get=>_unit;set=>Set(ref _unit,value);} public PackageImportMode Mode{get=>_mode;set=>Set(ref _mode,value);} public string Message{get=>_message;private set=>Set(ref _message,value);} public int RowCount=>_document?.Rows.Count??0;
 public void Load(string file){_document=_service.Load(file);FileName=file;Sheets.Clear();foreach(var s in _document.Sheets)Sheets.Add(s);SelectedSheet=Sheets.FirstOrDefault()??"";Mappings.Clear();foreach(var m in _service.AutoMap(_document.Headers))Mappings.Add(m);Preview.Clear();Raise(nameof(RowCount));Message=$"Rows: {RowCount}";}
 public void ReloadSheet(){if(_document is null||Path.GetExtension(FileName).Equals(".csv",StringComparison.OrdinalIgnoreCase))return;_document=_service.Load(FileName,SelectedSheet);Mappings.Clear();foreach(var m in _service.AutoMap(_document.Headers))Mappings.Add(m);Preview.Clear();Raise(nameof(RowCount));}
 public void Validate(){if(_document is null)return;Preview.Clear();foreach(var r in _service.Validate(_document.Rows,Mappings,Mode,Unit))Preview.Add(r);Message=$"OK: {Preview.Count(x=>x.Status==PackageImportStatus.OK)}; Errors: {Preview.Count(x=>x.Status==PackageImportStatus.Error)}";}
 public PackageImportResult Import(){var result=_service.Import(Preview);Message=$"Imported: {result.Imported}; Updated: {result.Updated}; Skipped: {result.Skipped}; Errors: {result.Errors.Count}";return result;}
 public event PropertyChangedEventHandler? PropertyChanged; void Raise(string n)=>PropertyChanged?.Invoke(this,new(n)); bool Set<T>(ref T f,T v,[CallerMemberName]string? n=null){if(EqualityComparer<T>.Default.Equals(f,v))return false;f=v;Raise(n!);return true;}
}
