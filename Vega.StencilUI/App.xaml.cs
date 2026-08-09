using Vega.Localization;

namespace Vega.StencilUI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        LocalizationService.Default.Initialize();
        base.OnStartup(e);
    }
}