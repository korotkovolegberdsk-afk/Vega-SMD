using Vega.Data.MasterLibrary.Database;
using Vega.Models;
using Vega.Services.MasterLibrary.Import;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class ProjectComponentTests
{
 [Fact] public void BomAndPnpMergeByRefDes(){var c=new ProjectComponentNormalizer().Normalize([new(){RefDes="R1",PartNumber="P",SourceKind="BOM"},new(){RefDes="r1",Footprint="R0603",X=1,Y=2,SourceKind="PNP"}]).Single();Assert.Equal("P",c.PartNumber);Assert.Equal("R0603",c.NormalizedFootprint);Assert.Equal(1,c.X);}
 [Fact] public void YgxAndBomMerge(){var c=new ProjectComponentNormalizer().Normalize([new(){RefDes="U1",Comment="IC",SourceKind="BOM"},new(){RefDes="U1",Footprint="QFN-32",SourceKind="YGX"}]).Single();Assert.Equal("IC",c.Comment);Assert.Equal("QFN-32",c.SourceFootprint);}
 [Fact] public void FootprintAliasAssignsPackage(){MasterLibraryMigrationRunner.Apply();var c=new ProjectComponentNormalizer().Normalize([new(){RefDes="R1",Footprint="0603"}]).Single();new ProjectComponentRecognitionService().Recognize(c);Assert.Equal(ProjectComponentRecognitionStatus.Matched,c.RecognitionStatus);Assert.NotNull(c.PackageDefinitionId);}
 [Fact] public void UnknownIsNotFound(){var c=new ProjectComponent{SourceFootprint="UNKNOWN",NormalizedFootprint="UNKNOWN"};new ProjectComponentRecognitionService().Recognize(c);Assert.Equal(ProjectComponentRecognitionStatus.NotFound,c.RecognitionStatus);}
 [Fact] public void ManualOverrideIsManual(){MasterLibraryMigrationRunner.Apply();var package=new Vega.Services.MasterLibrary.PackageDefinitionService().GetAll().First();var c=new ProjectComponent{};new ProjectComponentRecognitionService().Recognize(c,package);Assert.Equal(ProjectComponentRecognitionStatus.Manual,c.RecognitionStatus);Assert.Equal(package.Id,c.PackageDefinitionId);Assert.Equal(1,c.RecognitionConfidence);}
}