using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class SpecialPassiveLibraryTests
{
 static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 [Theory][InlineData("ALC5-7.0H",5,5,7)][InlineData("ALC6.3-7.7H",6.3,6.3,7.7)][InlineData("ALC8-10.2H",8,8,10.2)][InlineData("ALC10-10.2H",10,10,10.2)] public void AluminumCapExists(string name,double l,double w,double h){var p=S().GetPackageByName(name);Assert.NotNull(p);Assert.Equal("ALUMINUM_CAP",p!.PackageFamily);Assert.Equal(l,p.BodyLength);Assert.Equal(w,p.BodyWidth);Assert.Equal(h,p.Height);Assert.Equal("Capacitor",p.ComponentType);}
 [Fact] public void AliasesAreUnambiguous(){var s=S();Assert.Single(s.Search("ALC5X7").Where(x=>x.PackageName=="ALC5-7.0H"));var all=s.GetAll();Assert.Equal(all.Count,all.Select(x=>x.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());}
}
