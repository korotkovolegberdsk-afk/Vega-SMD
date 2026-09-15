using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Vega.StencilUI.Controls;

public sealed class Sot23ApertureReferencePreview : FrameworkElement
{
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));

        var scale = Math.Min(RenderSize.Width / 520d, RenderSize.Height / 350d);
        if (scale <= 0)
            return;

        dc.PushTransform(new ScaleTransform(scale, scale));

        var outline = new Pen(Brushes.Black, 1.5);
        var centerline = new Pen(Brushes.Black, 1);
        var dimension = new Pen(Brushes.Black, 1);
        var centers = new[]
        {
            new Point(180, 240),
            new Point(340, 240),
            new Point(260, 95)
        };

        foreach (var center in centers)
        {
            dc.DrawRectangle(null, outline, new Rect(center.X - 18, center.Y - 30, 36, 60));
            dc.DrawLine(centerline, new Point(center.X - 42, center.Y), new Point(center.X + 42, center.Y));
            dc.DrawLine(centerline, new Point(center.X, center.Y - 55), new Point(center.X, center.Y + 55));
        }

        DrawHorizontalDimension(dc, dimension, 242, 278, 35, "0,6");
        DrawVerticalDimension(dc, dimension, 65, 125, 390, "0,7");
        DrawVerticalDimension(dc, dimension, 95, 240, 55, "2,0");
        DrawHorizontalDimension(dc, dimension, 180, 260, 305, "0,95");
        DrawHorizontalDimension(dc, dimension, 260, 340, 330, "0,95");
        DrawHorizontalDimension(dc, dimension, 180, 340, 350, "1,9");

        dc.Pop();
    }

    private static void DrawHorizontalDimension(DrawingContext dc, Pen pen, double x1, double x2, double y, string text)
    {
        dc.DrawLine(pen, new Point(x1, y), new Point(x2, y));
        dc.DrawLine(pen, new Point(x1, y), new Point(x1 + 7, y - 4));
        dc.DrawLine(pen, new Point(x1, y), new Point(x1 + 7, y + 4));
        dc.DrawLine(pen, new Point(x2, y), new Point(x2 - 7, y - 4));
        dc.DrawLine(pen, new Point(x2, y), new Point(x2 - 7, y + 4));
        dc.DrawText(CreateText(text), new Point((x1 + x2) / 2 - 12, y - 20));
    }

    private static void DrawVerticalDimension(DrawingContext dc, Pen pen, double y1, double y2, double x, string text)
    {
        dc.DrawLine(pen, new Point(x, y1), new Point(x, y2));
        dc.DrawLine(pen, new Point(x, y1), new Point(x - 4, y1 + 7));
        dc.DrawLine(pen, new Point(x, y1), new Point(x + 4, y1 + 7));
        dc.DrawLine(pen, new Point(x, y2), new Point(x - 4, y2 - 7));
        dc.DrawLine(pen, new Point(x, y2), new Point(x + 4, y2 - 7));
        dc.DrawText(CreateText(text), new Point(x + 8, (y1 + y2) / 2 - 8));
    }

    private static FormattedText CreateText(string text)
    {
        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            15,
            Brushes.Black,
            1);
    }
}
