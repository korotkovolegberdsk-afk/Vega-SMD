using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Vega.StencilUI.Controls;

public static class Sot23ApprovedImage
{
    public static Image Create(string fileName)
    {
        return new Image
        {
            Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/Approved/{fileName}", UriKind.Absolute)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
    }

    public static UIElement CreatePackageDrawing() => Create("Sot23-Package-Approved.png");

    public static UIElement CreateAperture() => Create("Sot23-Aperture-Approved.png");
}
