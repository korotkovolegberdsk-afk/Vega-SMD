using Vega.Mirtec;
using Vega.Models;
using Vega.Models.MasterLibrary;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class MirtecMmdGeneratorTests
{
 private static PackageDefinition P(int id,string name)=>new(){Id=id,PackageName=name,MirtecAoiClass="LIB_"+name};
 private static ProjectComponent C(string r,int id,ProjectComponentRecognitionStatus s=ProjectComponentRecognitionStatus.Matched)=>new(){RefDes=r,PartNumber="PN-"+r,Comment="Comment",X=1,Y=2,Rotation=-90,Side="Bottom",PackageDefinitionId=id,RecognitionStatus=s};
 [Fact] public void GenerateSingleComponent(){var r=new MirtecMmdGenerator().Generate("T",[C("R1",1)],[P(1,"R0603")]);Assert.True(r.Success);Assert.Single(r.Preview);Assert.Equal(270,r.Preview[0].Rotation);}
 [Fact] public void PackageAssignedAndDataPreserved(){var r=new MirtecMmdGenerator().Generate("T",[C("U1",1)],[P(1,"SO08P127W078")]);var p=r.Preview.Single();Assert.Equal("SO08P127W078",p.PackageName);Assert.Equal("PN-U1",p.PartNumber);Assert.Equal("Comment",p.Comment);Assert.Equal("Bottom",p.Side);}
 [Fact] public void ManualAllowed(){var r=new MirtecMmdGenerator().Generate("T",[C("U1",1,ProjectComponentRecognitionStatus.Manual)],[P(1,"QFN")]);Assert.True(r.Success);Assert.Equal(1,r.ManualCount);}
 [Theory][InlineData(ProjectComponentRecognitionStatus.NotFound)][InlineData(ProjectComponentRecognitionStatus.Ambiguous)] public void UnknownBlocks(ProjectComponentRecognitionStatus status){var r=new MirtecMmdGenerator().Generate("T",[C("X1",1,status)],[P(1,"R")]);Assert.False(r.Success);Assert.NotEmpty(r.Errors);}
 [Fact] public void WriterCreatesFile(){var dir=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);var project=new MirtecMmdProject{ProjectName="T"};project.Parts.Add(new MirtecMmdPart{RefDes="R1",PackageName="R0603"});var file=Path.Combine(dir,"T.mmd");new MirtecMmdWriter().Write(project,file);Assert.True(File.Exists(file));Assert.True(File.Exists(Path.Combine(dir,"PROJECT_MMD_REPORT.txt")));Directory.Delete(dir,true);}
}