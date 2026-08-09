using Vega.Models;
namespace Vega.Services.MasterLibrary.Import;
public sealed class ProjectComponentNormalizer
{
 public List<ProjectComponent> Normalize(IEnumerable<ProjectComponentInput> inputs)
 {
  return inputs.Where(i=>!string.IsNullOrWhiteSpace(i.RefDes)).GroupBy(i=>i.RefDes.Trim(),StringComparer.OrdinalIgnoreCase).Select(g=>Merge(g.Key,g)).ToList();
 }
 private static ProjectComponent Merge(string refDes,IEnumerable<ProjectComponentInput> inputs)
 {
  var all=inputs.ToList(); string Get(Func<ProjectComponentInput,string> f)=>all.Select(f).FirstOrDefault(x=>!string.IsNullOrWhiteSpace(x))?.Trim()??""; var pnp=all.FirstOrDefault(i=>i.X.HasValue||i.Y.HasValue);
  return new ProjectComponent{RefDes=refDes,PartNumber=Get(i=>i.PartNumber),Value=Get(i=>i.Value),Comment=Get(i=>i.Comment),SourceFootprint=Get(i=>i.Footprint),NormalizedFootprint=Get(i=>i.Footprint).Trim().ToUpperInvariant(),X=pnp?.X??0,Y=pnp?.Y??0,Rotation=pnp?.Rotation??0,Side=pnp?.Side??"Top",BOMSource=Get(i=>i.SourceKind.Equals("BOM",StringComparison.OrdinalIgnoreCase)?i.Source:""),PnPSource=Get(i=>i.SourceKind.Equals("PNP",StringComparison.OrdinalIgnoreCase)?i.Source:""),YGXSource=Get(i=>i.SourceKind.Equals("YGX",StringComparison.OrdinalIgnoreCase)?i.Source:"")};
 }
}