using Vega.Data.MasterLibrary.Database;
using Vega.Localization;

namespace Vega.StencilUI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        MasterLibraryMigrationRunner.Apply();
        MasterLibrarySeeder.Seed();
        LocalizationService.Default.Initialize();
        base.OnStartup(e);
    }
}