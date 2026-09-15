using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class IcLibraryTests
{
 static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 [Theory][InlineData("SO08P127W078",8)][InlineData("SSOP08P065W43",8)][InlineData("SSOP16P065W78",16)][InlineData("SSOP28P065W78",28)][InlineData("TSSOP08P065W64",8)][InlineData("TSSOP14P065W64",14)][InlineData("TSSOP16P065W64",16)][InlineData("TSSOP20P065W64",20)][InlineData("TSSOP24P065W64",24)][InlineData("TSSOP28P065W64",28)][InlineData("LQFP032P080W091",32)][InlineData("QFP032P065W092",32)][InlineData("QFN032P050W500",32)] public void ConfirmedIcExists(string name,int pins){var p=S().GetPackageByName(name);Assert.NotNull(p);Assert.Equal("IC",p!.ComponentType);Assert.Equal(pins,p.LeadCount);Assert.Equal(pins,p.PadCount);}
 [Fact] public void Qfn32_UsesDocumentedStGeometry(){var p=S().GetPackageByName("QFN032P050W500")!;Assert.Equal(.90,p.Height);Assert.Equal(.40,p.LeadLength);Assert.Equal(3.60,p.ThermalPadLength);Assert.Equal(.50,p.Pitch);}
 [Fact] public void AliasesAndNamesAreUnambiguous(){var s=S();Assert.Single(s.Search("SO-8").Where(x=>x.PackageName=="SO08P127W078"));Assert.Single(s.Search("QFN-32").Where(x=>x.PackageName=="QFN032P050W500"));var all=s.GetAll();Assert.Equal(all.Count,all.Select(x=>x.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());}
}
