using Vega.Data.MasterLibrary.Database;
using Vega.Services.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class DiscreteLibraryTests
{
 static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 [Theory][InlineData("SOT23",3)][InlineData("SOT25",5)][InlineData("SOT26",6)][InlineData("SOD123",2)][InlineData("SMA",2)][InlineData("DPAK",3)] public void PhysicalPackageExists(string name,int leads){var p=S().GetPackageByName(name);Assert.NotNull(p);Assert.Equal(leads,p!.LeadCount);Assert.Equal("Other",p.ComponentType);}
 [Theory][InlineData("SOT-23","SOT23")][InlineData("SC59","SOT23")][InlineData("SOT23-5","SOT25")][InlineData("SOT23-6","SOT26")] public void AliasRecognized(string alias,string expected){var p=S().Search(alias);Assert.Single(p.Where(x=>x.PackageName==expected));}
 [Fact] public void NoDuplicatesOrElectricalVariants(){var p=S().GetAll().Where(x=>new[]{"SOD","SOT","DPAK"}.Contains(x.PackageFamily)).ToList();Assert.Equal(p.Count,p.Select(x=>x.PackageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());Assert.DoesNotContain(p,x=>x.PackageName.Contains('_'));Assert.All(p,x=>Assert.True(x.Length>0&&x.Width>0&&x.Height>0&&x.LeadCount>=0));}
}
