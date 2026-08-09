using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class ChipLibraryTests
{
 static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 [Theory][InlineData("R0603","Resistor")][InlineData("C0603","Capacitor")][InlineData("L0603","Inductor")] public void Chip0603Exists(string name,string type){var p=S().GetPackageByName(name);Assert.NotNull(p);Assert.Equal(type,p!.ComponentType);Assert.Equal("CHIP",p.PackageFamily);}
 [Fact] public void AllChipPackagesHavePositiveDimensionsAndType(){var p=S().GetAll().Where(x=>x.PackageFamily=="CHIP").ToList();Assert.True(p.Count>=25);Assert.All(p,x=>{Assert.True(x.Length>0&&x.Width>0&&x.Height>0);Assert.True(x.PadCount==2);Assert.False(string.IsNullOrWhiteSpace(x.ComponentType));});}
 [Fact] public void NoDuplicateNamesOrAmbiguousGeneric0603(){var s=S();var p=s.GetAll().Where(x=>x.PackageFamily=="CHIP").ToList();Assert.Equal(p.Count,p.Select(x=>x.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());var owners=p.Where(x=>s.GetAliases(x.Id).Any(a=>a.Alias.Equals("0603",StringComparison.OrdinalIgnoreCase))).ToList();Assert.True(owners.Count<=1);Assert.Equal("R0603",owners.SingleOrDefault()?.PackageName);}
 [Fact] public void ImperialMetricAliasesValid(){var s=S();var r=s.GetPackageByName("R0603")!;Assert.Contains(s.GetAliases(r.Id),a=>a.Alias=="1608");Assert.Contains(s.GetAliases(r.Id),a=>a.Alias=="RESC1608");}
}
