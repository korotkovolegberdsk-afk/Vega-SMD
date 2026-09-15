using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Vega.StencilUI.Controls;

internal static class Sot23ApertureDuplicateLayerLateFix
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Sot23ApertureReferencePreview), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject root)
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => RemoveGrayDuplicates(root)));
    }

    private static void RemoveGrayDuplicates(DependencyObject parent)
    {
        if (parent is Panel panel)
        {
            for (var i = panel.Children.Count - 1; i >= 0; i--)
            {
                if (panel.Children[i] is Rectangle rectangle && rectangle.Fill is SolidColorBrush && rectangle.Stroke is null)
                    panel.Children.RemoveAt(i);
            }
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            RemoveGrayDuplicates(VisualTreeHelper.GetChild(parent, i));
    }
}
