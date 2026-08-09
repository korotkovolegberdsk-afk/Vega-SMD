using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using Vega.StencilUI;
using Xunit;

namespace Vega.Tests;

public class ThemeVisualTests
{
    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void ThemeDictionary_ContainsRequiredBrushesAndControlStyles(string theme)
    {
        RunSta(() =>
        {
            var dictionary = (ResourceDictionary)Application.LoadComponent(
                new Uri($"/Vega.StencilUI;component/Themes/Theme.{theme}.xaml", UriKind.Relative));

            foreach (var brush in new[]
                     {
                         "WindowBackgroundBrush", "PanelBackgroundBrush", "TextBrush", "SecondaryTextBrush",
                         "ButtonBackgroundBrush", "ButtonForegroundBrush", "ButtonHoverBrush",
                         "MenuBackgroundBrush", "MenuForegroundBrush", "MenuHoverBrush", "MenuSelectedBrush",
                         "TabBackgroundBrush", "TabForegroundBrush", "TabSelectedBrush",
                         "InputBackgroundBrush", "InputForegroundBrush",
                         "GridBackgroundBrush", "GridHeaderBrush", "GridHeaderTextBrush", "GridBorderBrush"
                     })
            {
                Assert.NotNull(FindResource(dictionary, brush));
            }

            foreach (var controlType in new[]
                     {
                         typeof(Button), typeof(MenuItem), typeof(TabItem), typeof(TextBox), typeof(DataGrid),
                         typeof(ComboBox), typeof(ListBox), typeof(TreeView), typeof(CheckBox)
                     })
            {
                Assert.IsType<Style>(FindResource(dictionary, controlType));
            }
        });
    }

    private static object? FindResource(ResourceDictionary dictionary, object key)
    {
        if (dictionary.Contains(key)) return dictionary[key];
        foreach (ResourceDictionary mergedDictionary in dictionary.MergedDictionaries)
        {
            var result = FindResource(mergedDictionary, key);
            if (result is not null) return result;
        }

        return null;
    }

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