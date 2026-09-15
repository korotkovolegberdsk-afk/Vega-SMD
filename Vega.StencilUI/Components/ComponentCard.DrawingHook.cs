using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Vega.StencilUI.Components;

internal static class ComponentCardDrawingHook
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(ComponentCard), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnCardLoaded));
    }

    private static void OnCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ComponentCard card)
            return;

        var image = FindImage(card);
        if (image is null)
            return;

        image.Source = new BitmapImage(new Uri(
            "pack://application:,,,/Vega.StencilUI;component/Assets/Approved/Sot23-Package-Approved.png",
            UriKind.Absolute));
    }

    private static Image? FindImage(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Image image)
                return image;

            var nested = FindImage(child);
            if (nested is not null)
                return nested;
        }

        return null;
    }
}
