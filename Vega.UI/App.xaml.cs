using System.Windows;
using Vega.Data.MasterLibrary.Database;

namespace Vega.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(
            StartupEventArgs e)
        {
            MasterLibraryMigrationRunner.Apply();

            MasterLibrarySeeder.Seed();

            base.OnStartup(e);
        }
    }
}