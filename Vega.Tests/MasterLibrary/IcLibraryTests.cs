using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class IcLibraryTests
{
 static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 [Theory][InlineData("SO08P127W078",8)][InlineData("SSOP80P065W140",8)][InlineData("QFP032P065W092",32)][InlineData("QFN032P050W500",32)] public void ConfirmedIcExists(string name,int pins){var p=S().GetPackageByName(name);Assert.NotNull(p);Assert.Equal("IC",p!.ComponentType);Assert.Equal(pins,p.LeadCount);Assert.Equal(pins,p.PadCount);}
 [Fact] public void ManufacturerDependentGeometryIsUnspecified(){var p=S().GetPackageByName("QFN032P050W500")!;Assert.Equal(0,p.Height);Assert.Equal(0,p.LeadLength);Assert.Equal(0,p.ThermalPadLength);Assert.Equal("",p.YamahaName);}
 [Fact] public void AliasesAndNamesAreUnambiguous(){var s=S();Assert.Single(s.Search("SO-8").Where(x=>x.PackageName=="SO08P127W078"));Assert.Single(s.Search("QFN-32").Where(x=>x.PackageName=="QFN032P050W500"));var all=s.GetAll();Assert.Equal(all.Count,all.Select(x=>x.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());}
}
