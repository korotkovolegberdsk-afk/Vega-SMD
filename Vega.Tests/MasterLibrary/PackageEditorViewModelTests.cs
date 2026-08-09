using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.StencilUI.ViewModels;
using Xunit;
namespace Vega.Tests.MasterLibrary;
public sealed class PackageEditorViewModelTests
{
 private static PackageDefinitionService S(){MasterLibraryMigrationRunner.Apply();return new();}
 private static PackageDefinition Seed(PackageDefinitionService s)=>s.GetAll().First(p=>p.PackageFamily=="QFN");
 [Fact] public void CreateModeLoadsDefaults(){var vm=new PackageEditorViewModel(S(),PackageEditorMode.Create);Assert.NotNull(vm.WorkingCopy);Assert.NotEmpty(vm.Families);}
 [Fact] public void EditModeLoadsPackage(){var s=S();var p=Seed(s);var vm=new PackageEditorViewModel(s,PackageEditorMode.Edit,p);Assert.Equal(p.PackageName,vm.PackageName);}
 [Fact] public void EditCancelDoesNotModifyOriginal(){var s=S();var p=Seed(s);var original=p.DisplayName;var vm=new PackageEditorViewModel(s,PackageEditorMode.Edit,p);vm.DisplayName="Changed";Assert.Equal(original,p.DisplayName);}
 [Fact] public void ValidationRejectsNegativeDimension(){var s=S();var vm=new PackageEditorViewModel(s,PackageEditorMode.Create);vm.Length=-1;Assert.False(s.ValidatePackage(vm.WorkingCopy).IsValid);}
 [Fact] public void AliasAddRemove(){var vm=new PackageEditorViewModel(S(),PackageEditorMode.Create);vm.AddAlias("a");Assert.Single(vm.Aliases);vm.RemoveAlias(vm.Aliases[0]);Assert.Empty(vm.Aliases);}
 [Fact] public void DirtyStateChangesAfterEdit(){var vm=new PackageEditorViewModel(S(),PackageEditorMode.Create);Assert.False(vm.IsDirty);vm.DisplayName="X";Assert.True(vm.IsDirty);}
 [Fact] public void DuplicateModeUsesClone(){var s=S();var p=Seed(s);var clone=s.ClonePackage(p.Id);var vm=new PackageEditorViewModel(s,PackageEditorMode.Duplicate,clone);Assert.Contains("_COPY",vm.PackageName);}
}