using System.Windows;
using System.Windows.Media;

namespace Vega.StencilUI.Controls;

public sealed class Sot23TapeReferencePreview : FrameworkElement
{
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
        var scale = Math.Min(RenderSize.Width / 620d, RenderSize.Height / 220d);
        if (scale <= 0) return;
        dc.PushTransform(new ScaleTransform(scale, scale));
        var tapePen = new Pen(Brushes.Black, 1.6);
        var tape = new SolidColorBrush(Color.FromRgb(220, 224, 228));
        var path = new StreamGeometry();
        using (var g = path.Open())
        {
            g.BeginFigure(new Point(35, 25), true, false);
            g.LineTo(new Point(555, 25), true, false);
            g.BezierTo(new Point(585, 55), new Point(585, 165), new Point(555, 195), true, false);
            g.LineTo(new Point(35, 195), true, false);
            g.BezierTo(new Point(5, 165), new Point(5, 55), new Point(35, 25), true, false);
        }
        dc.DrawGeometry(tape, tapePen, path);

        var holeBrush = Brushes.White;
        foreach (var x in new[] { 95d, 255d, 415d, 575d })
            dc.DrawEllipse(holeBrush, tapePen, new Point(x, 67), 18, 18);

        foreach (var x in new[] { 175d, 335d, 495d })
        {
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(205, 215, 224)), new Pen(new SolidColorBrush(Color.FromRgb(30, 70, 120)), 1.2), new Rect(x - 43, 105, 86, 55), 2, 2);
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(180, 185, 190)), new Pen(Brushes.Black, 1), new Rect(x - 30, 120, 60, 25));
            dc.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(x - 6, 108, 12, 12));
            dc.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(x - 29, 145, 12, 12));
            dc.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(x + 17, 145, 12, 12));
        }

        DrawHorizontalDimension(dc, tapePen, 175, 335, 214, "Шаг карманов 4,0");
        DrawVerticalDimension(dc, tapePen, 25, 195, 610, "8,0");
        dc.DrawText(new FormattedText("Направление подачи", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 15, Brushes.Black, 1), new Point(415, 198));
        dc.DrawLine(tapePen, new Point(390, 215), new Point(500, 215));
        dc.DrawLine(tapePen, new Point(500, 215), new Point(490, 210));
        dc.DrawLine(tapePen, new Point(500, 215), new Point(490, 220));
        dc.Pop();
    }

    private static void DrawHorizontalDimension(DrawingContext dc, Pen pen, double x1, double x2, double y, string text)
    {
        dc.DrawLine(pen, new Point(x1, y), new Point(x2, y));
        dc.DrawLine(pen, new Point(x1, y), new Point(x1 + 7, y - 4));
        dc.DrawLine(pen, new Point(x1, y), new Point(x1 + 7, y + 4));
        dc.DrawLine(pen, new Point(x2, y), new Point(x2 - 7, y - 4));
        dc.DrawLine(pen, new Point(x2, y), new Point(x2 - 7, y + 4));
        dc.DrawText(new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 14, Brushes.Black, 1), new Point((x1 + x2) / 2 - 50, y - 19));
    }

    private static void DrawVerticalDimension(DrawingContext dc, Pen pen, double y1, double y2, double x, string text)
    {
        dc.DrawLine(pen, new Point(x, y1), new Point(x, y2));
        dc.DrawLine(pen, new Point(x, y1), new Point(x - 4, y1 + 7));
        dc.DrawLine(pen, new Point(x, y1), new Point(x + 4, y1 + 7));
        dc.DrawLine(pen, new Point(x, y2), new Point(x - 4, y2 - 7));
        dc.DrawLine(pen, new Point(x, y2), new Point(x + 4, y2 - 7));
        dc.DrawText(new FormattedText(text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 15, Brushes.Black, 1), new Point(x - 28, (y1 + y2) / 2 - 8));
    }
}
