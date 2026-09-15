using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Vega.StencilUI.Controls;

internal static class Sot23ApertureGrayOverlayFix
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Sot23ApertureReferencePreview), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject root)
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => RemoveGrayOverlays(root)));
    }

    private static void RemoveGrayOverlays(DependencyObject parent)
    {
        if (parent is Panel panel)
        {
            for (var i = panel.Children.Count - 1; i >= 0; i--)
            {
                if (panel.Children[i] is Rectangle rectangle && IsGray(rectangle.Fill))
                    panel.Children.RemoveAt(i);
            }
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            RemoveGrayOverlays(VisualTreeHelper.GetChild(parent, i));
    }

    private static bool IsGray(Brush? brush)
    {
        if (brush is not SolidColorBrush solid)
            return false;

        var c = solid.Color;
        return c.A > 0 && Math.Abs(c.R - c.G) < 4 && Math.Abs(c.G - c.B) < 4 && c.R >= 130 && c.R <= 230;
    }
}
