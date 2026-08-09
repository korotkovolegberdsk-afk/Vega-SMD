using Vega.Data.MasterLibrary.Database;
using Vega.StencilUI.ViewModels;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class MasterLibraryViewModelTests
{
    private static MasterLibraryViewModel Create()
    {
        MasterLibraryMigrationRunner.Apply();
        return new MasterLibraryViewModel();
    }
    [Fact] public void LoadPackages_Loads_MasterLibrary() => Assert.NotEmpty(Create().Packages);
    [Fact] public void FilterByFamily_Filters_Table() { var vm=Create(); vm.SelectFamily("QFN"); Assert.All(vm.Packages,p=>Assert.Equal("QFN",p.PackageFamily)); }
    [Fact] public void SearchByAlias_0603_Finds_R0603() { var vm=Create(); vm.SearchText="0603"; Assert.Contains(vm.Packages,p=>p.PackageName=="R0603"); }
    [Fact] public void SelectPackage_Updates_SelectedPackage() { var vm=Create(); var package=vm.Packages.First(); vm.SelectedPackage=package; Assert.Same(package,vm.SelectedPackage); }
    [Fact] public void LoadAliases_Loads_R0603_Aliases() { var vm=Create(); vm.SearchText="R0603"; vm.SelectedPackage=vm.Packages.First(p=>p.PackageName=="R0603"); Assert.Contains(vm.Aliases,a=>a.Alias=="0603"); }
}