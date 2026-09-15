using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Vega.StencilUI.Controls;

internal static class Sot23ApertureDuplicateLayerFix
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(
            typeof(Sot23ApertureReferencePreview),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(RemoveDuplicateLayers));
    }

    private static void RemoveDuplicateLayers(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject root)
            return;

        RemoveFilledRectangles(root);
    }

    private static void RemoveFilledRectangles(DependencyObject parent)
    {
        if (parent is Panel panel)
        {
            for (var i = panel.Children.Count - 1; i >= 0; i--)
            {
                if (panel.Children[i] is Rectangle rectangle && rectangle.Fill is not null && rectangle.Stroke is null)
                    panel.Children.RemoveAt(i);
            }
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            RemoveFilledRectangles(VisualTreeHelper.GetChild(parent, i));
    }
}
