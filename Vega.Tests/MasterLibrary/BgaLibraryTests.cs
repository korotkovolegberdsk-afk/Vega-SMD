using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class BgaLibraryTests
{
 static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 [Fact] public void ConfirmedBgaPackageExists(){var p=S().GetPackageByName("BGA064P080W800");Assert.NotNull(p);Assert.Equal("BGA",p!.PackageFamily);Assert.Equal(64,p.PadCount);Assert.Equal(0,p.LeadCount);Assert.Equal(0.80,p.BallPitch);Assert.Equal("",p.YamahaName);}
 [Fact] public void BgaGeometrySearchUsesPitchBodyAndPadCount(){var p=S().FindByGeometry("BGA",ballPitch:0.80,bodyLength:8.00,bodyWidth:8.00,padCount:64);Assert.Contains(p,x=>x.PackageName=="BGA064P080W800");}
 [Fact] public void BgaAliasesAndNamesAreUnambiguous(){var s=S();Assert.Single(s.Search("BGA-64-0.80").Where(x=>x.PackageName=="BGA064P080W800"));var all=s.GetAll();Assert.Equal(all.Count,all.Select(x=>x.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());}
}
