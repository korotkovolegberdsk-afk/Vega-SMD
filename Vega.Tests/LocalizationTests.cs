using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vega.Localization;
using Xunit;

namespace Vega.Tests;

public class LocalizationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "VegaLocalization", Guid.NewGuid().ToString("N"));
    private string SettingsFile => Path.Combine(_directory, "settings.json");

    [Fact]
    public void RussianLanguage_LoadsRussianStrings()
    {
        var service = CreateService();
        service.SetLanguage("ru-RU");

        Assert.Equal("ru-RU", service.GetCurrentLanguage());
        Assert.Equal("\u041E\u0442\u043A\u0440\u044B\u0442\u044C \u043F\u0440\u043E\u0435\u043A\u0442", service.GetString("OpenProject"));
    }

    [Fact]
    public void EnglishLanguage_LoadsEnglishStrings()
    {
        var service = CreateService();
        service.SetLanguage("en-US");

        Assert.Equal("en-US", service.GetCurrentLanguage());
        Assert.Equal("Open Project", service.GetString("OpenProject"));
    }

    [Fact]
    public void LanguageAndThemeChanges_ReplaceApplicationResources()
    {
        RunSta(() =>
        {
            var application = Application.Current ?? new Application();
            var service = CreateService();
            var languageNotifications = 0;
            var themeNotifications = 0;
            service.LanguageChanged += (_, _) => languageNotifications++;
            service.ThemeChanged += (_, _) => themeNotifications++;
            var button = new Button();
            button.SetResourceReference(ContentControl.ContentProperty, "OpenProject");
            var window = new Window { Content = button, Width = 1, Height = 1, ShowInTaskbar = false };
            window.Show();
            try
            {
                service.SetLanguage("en-US");
                Assert.Equal("Open Project", button.Content);
                service.SetLanguage("ru-RU");
                Assert.Equal("\u041E\u0442\u043A\u0440\u044B\u0442\u044C \u043F\u0440\u043E\u0435\u043A\u0442", button.Content);

                service.SetTheme("Dark");
                var background = Assert.IsType<SolidColorBrush>(application.TryFindResource("AppBackgroundBrush"));
                var text = Assert.IsType<SolidColorBrush>(application.TryFindResource("TextBrush"));
                var buttonForeground = Assert.IsType<SolidColorBrush>(application.TryFindResource("ButtonForegroundBrush"));
                var menuForeground = Assert.IsType<SolidColorBrush>(application.TryFindResource("MenuForegroundBrush"));
                var menuHover = Assert.IsType<SolidColorBrush>(application.TryFindResource("MenuItemHoverBrush"));
                var menuSelected = Assert.IsType<SolidColorBrush>(application.TryFindResource("MenuItemSelectedBrush"));
                var tabBackground = Assert.IsType<SolidColorBrush>(application.TryFindResource("TabBackgroundBrush"));
                var tabForeground = Assert.IsType<SolidColorBrush>(application.TryFindResource("TabForegroundBrush"));
                var tabSelected = Assert.IsType<SolidColorBrush>(application.TryFindResource("TabSelectedBrush"));
                var previewBackground = Assert.IsType<SolidColorBrush>(application.TryFindResource("PreviewBackgroundBrush"));
                var previewText = Assert.IsType<SolidColorBrush>(application.TryFindResource("PreviewTextBrush"));
                var gridHeader = Assert.IsType<SolidColorBrush>(application.TryFindResource("GridHeaderBrush"));
                var gridHeaderText = Assert.IsType<SolidColorBrush>(application.TryFindResource("GridHeaderTextBrush"));
                var gridBorder = Assert.IsType<SolidColorBrush>(application.TryFindResource("GridBorderBrush"));
                var windowBackground = Assert.IsType<SolidColorBrush>(application.TryFindResource("WindowBackgroundBrush"));
                Assert.Equal(Color.FromRgb(0x1E, 0x25, 0x2D), background.Color);
                Assert.Equal(Color.FromRgb(0xF1, 0xF5, 0xF9), text.Color);
                Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF), buttonForeground.Color);
                Assert.Equal(Color.FromRgb(0xF8, 0xFA, 0xFC), menuForeground.Color);
                Assert.Equal(Color.FromRgb(0x3A, 0x48, 0x56), menuHover.Color);
                Assert.Equal(Color.FromRgb(0x47, 0x5A, 0x6B), menuSelected.Color);
                Assert.Equal(Color.FromRgb(0x34, 0x42, 0x4F), tabBackground.Color);
                Assert.Equal(Color.FromRgb(0xF1, 0xF5, 0xF9), tabForeground.Color);
                Assert.Equal(Color.FromRgb(0x47, 0x5A, 0x6B), tabSelected.Color);
                Assert.Equal(Color.FromRgb(0x20, 0x2A, 0x33), previewBackground.Color);
                Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF), previewText.Color);
                Assert.Equal(Color.FromRgb(0x34, 0x42, 0x4F), gridHeader.Color);
                Assert.Equal(Color.FromRgb(0xF8, 0xFA, 0xFC), gridHeaderText.Color);
                Assert.Equal(Color.FromRgb(0x65, 0x76, 0x87), gridBorder.Color);
                Assert.Equal(Color.FromRgb(0x1E, 0x25, 0x2D), windowBackground.Color);
                service.SetTheme("Light");
                var lightMenuForeground = Assert.IsType<SolidColorBrush>(application.TryFindResource("MenuForegroundBrush"));
                var lightPreviewBackground = Assert.IsType<SolidColorBrush>(application.TryFindResource("PreviewBackgroundBrush"));
                var lightPreviewText = Assert.IsType<SolidColorBrush>(application.TryFindResource("PreviewTextBrush"));
                Assert.Equal(Color.FromRgb(0x1F, 0x29, 0x37), lightMenuForeground.Color);
                Assert.Equal(Color.FromRgb(0xF7, 0xF8, 0xFA), lightPreviewBackground.Color);
                Assert.Equal(Color.FromRgb(0x1F, 0x29, 0x37), lightPreviewText.Color);
                Assert.Equal(2, languageNotifications);
                Assert.Equal(2, themeNotifications);
            }
            finally
            {
                window.Close();
            }
        });
    }
    [Fact]
    public void SettingsPersistAfterServiceRestart()
    {
        var settings = new ApplicationSettingsService(SettingsFile);
        var service = new LocalizationService(settings);
        service.SetLanguage("en-US");
        service.SetTheme("Dark");
        service.UpdateSettings("Dark", "C:\\Projects\\Controller.gtp");

        var restarted = new LocalizationService(settings);
        Assert.Equal("en-US", restarted.GetCurrentLanguage());
        Assert.Equal("Dark", restarted.CurrentTheme);
        Assert.Equal("C:\\Projects\\Controller.gtp", restarted.Settings.LastProjectPath);
    }

    [Fact]
    public void SettingsWindow_LoadsFromXaml()
    {
        RunSta(() => _ = new Vega.StencilUI.SettingsWindow());
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private LocalizationService CreateService() => new(new ApplicationSettingsService(SettingsFile));

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { exception = error; }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (exception is not null) ExceptionDispatchInfo.Capture(exception).Throw();
    }
}