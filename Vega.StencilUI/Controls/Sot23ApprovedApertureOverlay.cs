using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Vega.StencilUI.Controls;

internal sealed class Sot23ApprovedApertureAdorner : Adorner
{
    private readonly Image _image;

    public Sot23ApprovedApertureAdorner(UIElement adornedElement, ImageSource source) : base(adornedElement)
    {
        IsHitTestVisible = false;
        _image = new Image { Source = source, Stretch = Stretch.Uniform };
        AddVisualChild(_image);
    }

    protected override int VisualChildrenCount => 1;
    protected override Visual GetVisualChild(int index) => _image;
    protected override Size MeasureOverride(Size constraint) { _image.Measure(constraint); return constraint; }
    protected override Size ArrangeOverride(Size finalSize) { _image.Arrange(new Rect(finalSize)); return finalSize; }
}

internal static class Sot23ApprovedApertureOverlay
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Sot23ApertureReferencePreview), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not UIElement preview)
            return;

        preview.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
        {
            var layer = AdornerLayer.GetAdornerLayer(preview);
            var source = LoadApprovedImage();
            if (layer is null || source is null)
                return;

            foreach (var existing in AdornerLayer.GetAdorners(preview) ?? Array.Empty<Adorner>())
                if (existing is Sot23ApprovedApertureAdorner)
                    layer.Remove(existing);

            layer.Add(new Sot23ApprovedApertureAdorner(preview, source));
        }));
    }

    private static ImageSource? LoadApprovedImage()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "Images", "рисунок аперту.png"));
        if (!File.Exists(path))
            return null;

        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
