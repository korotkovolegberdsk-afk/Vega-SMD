using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;
public sealed class PackageCrudTests
{
 private static PackageDefinitionService Service(){MasterLibraryMigrationRunner.Apply();return new PackageDefinitionService();}
 private static PackageDefinition NewPackage(PackageDefinitionService s){var seed=s.GetAll().First(p=>p.PackageFamily=="QFN");return new PackageDefinition{PackageName="CRUD_"+Guid.NewGuid().ToString("N"),DisplayName="CRUD",StandardName="CRUD",PackageFamily=seed.PackageFamily,ComponentType="IC",CategoryId=seed.CategoryId,FamilyId=seed.FamilyId,Length=1,Width=1,Height=1};}
 [Fact] public void CreatePackage(){var s=Service();var p=s.CreatePackage(NewPackage(s));Assert.True(p.Id>0);Assert.True(s.DeletePackage(p.Id).CanDelete);}
 [Fact] public void UpdatePackage(){var s=Service();var p=s.CreatePackage(NewPackage(s));p.DisplayName="Updated";s.Update(p);Assert.Equal("Updated",s.GetById(p.Id)!.DisplayName);s.DeletePackage(p.Id);}
 [Fact] public void ClonePackageGeneratesUniqueName(){var s=Service();var p=s.CreatePackage(NewPackage(s));var a=s.ClonePackage(p.Id);var b=s.ClonePackage(p.Id);Assert.EndsWith("_COPY",a.PackageName);Assert.EndsWith("_COPY",b.PackageName);s.DeletePackage(p.Id);}
 [Fact] public void RejectDuplicatePackageNameCaseInsensitive(){var s=Service();var p=s.CreatePackage(NewPackage(s));var duplicate=NewPackage(s);duplicate.PackageName=p.PackageName.ToLowerInvariant();Assert.False(s.ValidatePackage(duplicate).IsValid);s.DeletePackage(p.Id);}
 [Fact] public void RejectEmptyAndNegative(){var s=Service();var p=NewPackage(s);p.PackageName=" ";p.Length=-1;Assert.False(s.ValidatePackage(p).IsValid);}
 [Fact] public void AliasCreateAndDelete(){var s=Service();var p=s.CreatePackage(NewPackage(s));var a=s.CreateAlias(new PackageAlias{PackageId=p.Id,Alias=" A ",AliasType="Test"});Assert.Equal("A",a.Alias);s.DeleteAlias(a.Id);Assert.Empty(s.GetAliases(p.Id));s.DeletePackage(p.Id);}
 [Fact] public void RejectDuplicateAlias(){var s=Service();var p=s.CreatePackage(NewPackage(s));s.CreateAlias(new PackageAlias{PackageId=p.Id,Alias="A"});Assert.Throws<InvalidOperationException>(()=>s.CreateAlias(new PackageAlias{PackageId=p.Id,Alias=" a "}));s.DeletePackage(p.Id);}
 [Fact] public void DeleteUnusedPackage(){var s=Service();var p=s.CreatePackage(NewPackage(s));Assert.True(s.DeletePackage(p.Id).CanDelete);Assert.Null(s.GetById(p.Id));}
 [Fact] public void RejectDeleteUsedPackage(){var s=Service();var p=s.CreatePackage(NewPackage(s));using(var c=MasterLibraryConnection.Create()){using var cmd=c.CreateCommand();cmd.CommandText="INSERT INTO ComponentDefinition(ManufacturerPartNumber,PackageId) VALUES($n,$id);";cmd.Parameters.AddWithValue("$n","CRUD-"+Guid.NewGuid());cmd.Parameters.AddWithValue("$id",p.Id);cmd.ExecuteNonQuery();}Assert.False(s.DeletePackage(p.Id).CanDelete);}
 [Fact] public void RejectUnknownFamilyAndType(){var s=Service();var p=NewPackage(s);p.PackageFamily="BAD";p.ComponentType="BAD";Assert.False(s.ValidatePackage(p).IsValid);}
}