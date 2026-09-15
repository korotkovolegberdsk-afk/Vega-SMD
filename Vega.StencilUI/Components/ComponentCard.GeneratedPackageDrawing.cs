using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Vega.StencilUI.Components;

internal static class ComponentCardGeneratedPackageDrawing
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(ComponentCard), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ComponentCard card)
            return;

        card.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            var image = FindImage(card);
            if (image is not null)
                image.Source = BuildSot23Drawing();
        }));
    }

    private static ImageSource BuildSot23Drawing()
    {
        const double w = 900, h = 500;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var blue = new SolidColorBrush(Color.FromRgb(0x07, 0x5B, 0xD8));
            var line = new Pen(new SolidColorBrush(Color.FromRgb(0xB7, 0xC9, 0xDF)), 1);
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
            dc.DrawLine(line, new Point(w / 2, 0), new Point(w / 2, h));
            dc.DrawLine(line, new Point(0, h / 2), new Point(w, h / 2));
            DrawText(dc, "SOT23 (3-ВЫВОДНОЙ)", blue, 24, 24, 22, true);
            DrawText(dc, "ЧЕРТЁЖ КОРПУСА", blue, 500, 24, 22, true);
            DrawText(dc, "1. ВИД СВЕРХУ", blue, 18, 52, 13, true);
            DrawText(dc, "2. ВИД СНИЗУ", blue, 468, 52, 13, true);
            DrawText(dc, "3. ВИД СБОКУ", blue, 18, 276, 13, true);
            DrawText(dc, "4. 3D ВИД", blue, 468, 276, 13, true);
            DrawTopView(dc, 95, 90);
            DrawTopView(dc, 555, 90);
            DrawSideView(dc, 95, 350);
            DrawIsometricView(dc, 535, 350);
        }
        var bitmap = new RenderTargetBitmap((int)w, (int)h, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private static void DrawTopView(DrawingContext dc, double x, double y)
    {
        var body = new SolidColorBrush(Color.FromRgb(0x56, 0x65, 0x72));
        var metal = new SolidColorBrush(Color.FromRgb(0xD9, 0xDC, 0xDE));
        var pen = new Pen(Brushes.Black, 2);
        dc.DrawRoundedRectangle(body, pen, new Rect(x, y, 245, 105), 6, 6);
        foreach (var pin in new[] { new Rect(x + 33, y - 32, 32, 36), new Rect(x + 180, y - 32, 32, 36), new Rect(x + 106, y + 105, 32, 36) })
            dc.DrawRectangle(metal, new Pen(Brushes.DimGray, 1), pin);
        dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, 1), new Point(x + 20, y + 22), 6, 6);
        DrawText(dc, "2,90", Brushes.Black, x + 100, y - 45, 12, false);
        DrawText(dc, "1,90", Brushes.Black, x + 110, y - 20, 12, false);
        DrawText(dc, "0,40", Brushes.Black, x + 108, y + 158, 12, false);
    }

    private static void DrawSideView(DrawingContext dc, double x, double y)
    {
        var body = new SolidColorBrush(Color.FromRgb(0x56, 0x65, 0x72));
        var pen = new Pen(Brushes.Black, 2);
        dc.DrawGeometry(body, pen, Geometry.Parse($"M {x + 15},{y + 30} L {x + 45},{y} L {x + 225},{y} L {x + 255},{y + 30} L {x + 240},{y + 82} L {x + 30},{y + 82} Z"));
        var metal = new Pen(Brushes.DimGray, 2);
        dc.DrawLine(metal, new Point(x + 15, y + 30), new Point(x - 20, y + 75));
        dc.DrawLine(metal, new Point(x - 20, y + 75), new Point(x - 55, y + 75));
        dc.DrawLine(metal, new Point(x + 255, y + 30), new Point(x + 290, y + 75));
        dc.DrawLine(metal, new Point(x + 290, y + 75), new Point(x + 325, y + 75));
        DrawText(dc, "0,95", Brushes.Black, x + 105, y + 120, 12, false);
        DrawText(dc, "1,10", Brushes.Black, x - 65, y + 35, 12, false);
        DrawText(dc, "0,15", Brushes.Black, x - 65, y + 92, 12, false);
    }

    private static void DrawIsometricView(DrawingContext dc, double x, double y)
    {
        var body = new SolidColorBrush(Color.FromRgb(0x56, 0x65, 0x72));
        var pen = new Pen(Brushes.Black, 2);
        dc.DrawGeometry(body, pen, Geometry.Parse($"M {x},{y + 55} L {x + 55},{y + 10} L {x + 250},{y + 32} L {x + 200},{y + 88} Z"));
        dc.DrawGeometry(body, pen, Geometry.Parse($"M {x},{y + 55} L {x},{y + 100} L {x + 200},{y + 135} L {x + 200},{y + 88} Z"));
        dc.DrawGeometry(body, pen, Geometry.Parse($"M {x + 200},{y + 88} L {x + 250},{y + 32} L {x + 250},{y + 77} L {x + 200},{y + 135} Z"));
    }

    private static void DrawText(DrawingContext dc, string text, Brush brush, double x, double y, double size, bool bold)
    {
        var ft = new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface(bold ? "Arial Bold" : "Arial"), size, brush, 96);
        dc.DrawText(ft, new Point(x, y));
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
