using Vega.Models;
using Vega.Models.MasterLibrary;
namespace Vega.Services.MasterLibrary.Import;
public sealed class ProjectComponentRecognitionService
{
 private readonly PackageDefinitionService _packages; public ProjectComponentRecognitionService(PackageDefinitionService? packages=null)=>_packages=packages??new();
 public ProjectComponent Recognize(ProjectComponent component,PackageDefinition? manualOverride=null)
 {
  if(manualOverride is not null){component.PackageDefinitionId=manualOverride.Id;component.RecognitionMethod=ProjectComponentRecognitionMethod.Manual;component.RecognitionStatus=ProjectComponentRecognitionStatus.Manual;component.RecognitionConfidence=1;component.ManualOverride=true;return component;}
  var key=(string.IsNullOrWhiteSpace(component.NormalizedFootprint)?component.SourceFootprint:component.NormalizedFootprint).Trim(); var matches=_packages.GetAll().Where(p=>p.PackageName.Equals(key,StringComparison.OrdinalIgnoreCase)||_packages.GetAliases(p.Id).Any(a=>a.Alias.Equals(key,StringComparison.OrdinalIgnoreCase))).ToList();
  if(matches.Count==1){component.PackageDefinitionId=matches[0].Id;component.RecognitionMethod=matches[0].PackageName.Equals(key,StringComparison.OrdinalIgnoreCase)?ProjectComponentRecognitionMethod.Footprint:ProjectComponentRecognitionMethod.Alias;component.RecognitionStatus=ProjectComponentRecognitionStatus.Matched;component.RecognitionConfidence=component.RecognitionMethod==ProjectComponentRecognitionMethod.Footprint ? .98 : .9;}
  else if(matches.Count>1){component.RecognitionStatus=ProjectComponentRecognitionStatus.Ambiguous;component.RecognitionConfidence=.35;}
  else{component.RecognitionStatus=ProjectComponentRecognitionStatus.NotFound;component.RecognitionConfidence=0;}return component;
 }
}