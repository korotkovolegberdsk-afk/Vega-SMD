using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;

namespace Vega.StencilUI.Controls;

// ST VFQFPN32, 5 x 5 mm, 0.50 mm pitch; DS14186 Rev. 1.
// This is a leadless package: the terminal metallisation is shown at the
// body perimeter and on the bottom view, never as gull-wing leads.
internal static class Qfn32PackageDrawing
{
    private const double Body = 5.00, Height = .90, Pitch = .50, TerminalWidth = .25,
        TerminalLength = .40, ThermalPad = 3.60, LandSpan = 5.30;
    private const int PinsPerSide = 8;
    private static readonly Brush BodyBrush = new SolidColorBrush(Color.FromRgb(74, 91, 108));
    private static readonly Brush Outline = new SolidColorBrush(Color.FromRgb(52, 67, 82));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(211, 214, 211));

    public static bool Supports(string? name) => name?.Equals("QFN032P050W500", StringComparison.OrdinalIgnoreCase) == true;

    private static IEnumerable<double> Pins()
    {
        var center = (PinsPerSide - 1) / 2d;
        for (var i = 0; i < PinsPerSide; i++) yield return (i - center) * Pitch;
    }

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh()
    {
        var mesh = new List<GlbTriangle>();
        var body = new Vector4(.29f, .36f, .43f, 1); var metal = new Vector4(.83f, .84f, .82f, 1); var mark = new Vector4(.98f, .98f, .96f, 1);
        static Vector4 Shade(Vector4 c, float f) => new(c.X * f, c.Y * f, c.Z * f, c.W);
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 color) { mesh.Add(new(a * .001f, b * .001f, c * .001f, color)); mesh.Add(new(a * .001f, c * .001f, d * .001f, color)); }
        void Box(float x0, float x1, float y0, float y1, float z0, float z1, Vector4 color, bool faceted = false)
        {
            var top = faceted ? Shade(color, 1.18f) : color; var side = faceted ? Shade(color, .68f) : color;
            Face(new(x0, y0, z0), new(x1, y0, z0), new(x1, y1, z0), new(x0, y1, z0), side);
            Face(new(x0, y0, z1), new(x0, y1, z1), new(x1, y1, z1), new(x1, y0, z1), top);
            Face(new(x0, y0, z0), new(x0, y0, z1), new(x1, y0, z1), new(x1, y0, z0), color);
            Face(new(x0, y1, z0), new(x1, y1, z0), new(x1, y1, z1), new(x0, y1, z1), side);
            Face(new(x0, y0, z0), new(x0, y1, z0), new(x0, y1, z1), new(x0, y0, z1), side);
            Face(new(x1, y0, z0), new(x1, y0, z1), new(x1, y1, z1), new(x1, y1, z0), color);
        }
        Box(-(float)Body / 2, (float)Body / 2, -(float)Body / 2, (float)Body / 2, .12f, (float)Height, body, true);
        foreach (var p in Pins())
        {
            var q = (float)p; var half = (float)TerminalWidth / 2;
            Box(-(float)LandSpan / 2, -(float)Body / 2 + .12f, q - half, q + half, 0, .14f, metal);
            Box((float)Body / 2 - .12f, (float)LandSpan / 2, q - half, q + half, 0, .14f, metal);
            Box(q - half, q + half, -(float)LandSpan / 2, -(float)Body / 2 + .12f, 0, .14f, metal);
            Box(q - half, q + half, (float)Body / 2 - .12f, (float)LandSpan / 2, 0, .14f, metal);
        }
        Box(-(float)ThermalPad / 2, (float)ThermalPad / 2, -(float)ThermalPad / 2, (float)ThermalPad / 2, 0, .14f, metal);
        Box(-(float)Body * .43f, -(float)Body * .34f, -(float)Body * .44f, -(float)Body * .35f, (float)Height, (float)Height + .01f, mark);
        return mesh;
    }

    public static void PaintTop(Canvas canvas, double cx, double cy, double scale, bool bottom = false)
    {
        foreach (var p in Pins())
        {
            Add(canvas, cx - LandSpan * scale / 2, cy + (p - TerminalWidth / 2) * scale, TerminalLength * scale, TerminalWidth * scale);
            Add(canvas, cx + (LandSpan / 2 - TerminalLength) * scale, cy + (p - TerminalWidth / 2) * scale, TerminalLength * scale, TerminalWidth * scale);
            Add(canvas, cx + (p - TerminalWidth / 2) * scale, cy - LandSpan * scale / 2, TerminalWidth * scale, TerminalLength * scale);
            Add(canvas, cx + (p - TerminalWidth / 2) * scale, cy + (LandSpan / 2 - TerminalLength) * scale, TerminalWidth * scale, TerminalLength * scale);
        }
        var body = new Rectangle { Width = Body * scale, Height = Body * scale, Fill = BodyBrush, Stroke = Outline, StrokeThickness = .8 };
        Canvas.SetLeft(body, cx - Body * scale / 2); Canvas.SetTop(body, cy - Body * scale / 2); canvas.Children.Add(body);
        if (bottom)
        {
            var pad = new Rectangle { Width = ThermalPad * scale, Height = ThermalPad * scale, Fill = Metal, Stroke = ComponentCardPalette.Border, StrokeThickness = .8 };
            Canvas.SetLeft(pad, cx - ThermalPad * scale / 2); Canvas.SetTop(pad, cy - ThermalPad * scale / 2); canvas.Children.Add(pad);
        }
        else
        {
            var dot = new Ellipse { Width = .28 * scale, Height = .28 * scale, Fill = Brushes.White };
            Canvas.SetLeft(dot, cx - Body * .44 * scale); Canvas.SetTop(dot, cy - Body * .44 * scale); canvas.Children.Add(dot);
        }
    }

    private static void Add(Canvas canvas, double x, double y, double width, double height)
    {
        var terminal = new Rectangle { Width = width, Height = height, Fill = Metal, Stroke = ComponentCardPalette.Border, StrokeThickness = .55 };
        Canvas.SetLeft(terminal, x); Canvas.SetTop(terminal, y); canvas.Children.Add(terminal);
    }

    public static UIElement Create(StepProjectionKind kind)
    {
        var c = new Canvas { Width = 512, Height = 370, Background = Brushes.Transparent };
        const double cx = 256, cy = 178, s = 34, floor = 222;
        string F(double value) => value.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
        if (kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c, cx, cy, s, kind == StepProjectionKind.Bottom);
            if (kind == StepProjectionKind.Top)
            {
                var left = cx - Body * s / 2; var right = cx + Body * s / 2; var top = cy - Body * s / 2; var bottom = cy + Body * s / 2;
                H(c, left, right, top - 28, top, F(Body)); V(c, left - 34, top, bottom, left, F(Body));
                V(c, right + 27, cy - 3.5 * Pitch * s, cy - 2.5 * Pitch * s, right, F(Pitch), true);
                SmallV(c, right + 31, cy + 3.5 * Pitch * s - TerminalWidth * s / 2, cy + 3.5 * Pitch * s + TerminalWidth * s / 2, right, cy + 3.5 * Pitch * s - 32, F(TerminalWidth));
            }
        }
        else
        {
            var body = new Rectangle { Width = Body * s, Height = Height * s, Fill = BodyBrush, Stroke = Outline, StrokeThickness = .8 };
            Canvas.SetLeft(body, cx - Body * s / 2); Canvas.SetTop(body, floor - Height * s); c.Children.Add(body);
            var leftPad = new Rectangle { Width = TerminalLength * s, Height = .14 * s, Fill = Metal, Stroke = ComponentCardPalette.Border, StrokeThickness = .6 };
            Canvas.SetLeft(leftPad, cx - LandSpan * s / 2); Canvas.SetTop(leftPad, floor - .14 * s); c.Children.Add(leftPad);
            var rightPad = new Rectangle { Width = TerminalLength * s, Height = .14 * s, Fill = Metal, Stroke = ComponentCardPalette.Border, StrokeThickness = .6 };
            Canvas.SetLeft(rightPad, cx + (LandSpan / 2 - TerminalLength) * s); Canvas.SetTop(rightPad, floor - .14 * s); c.Children.Add(rightPad);
            V(c, cx - LandSpan * s / 2 - 34, floor - Height * s, floor, cx - LandSpan * s / 2, F(Height));
            SideH(c, cx + (LandSpan / 2 - TerminalLength) * s, cx + LandSpan * s / 2, floor + 48, floor, F(TerminalLength));
        }
        return new Viewbox { Stretch = Stretch.Uniform, Child = c, Margin = new Thickness(8) };
    }

    private static void Line(Canvas c, double x, double y, double xx, double yy) => c.Children.Add(new Line { X1 = x, Y1 = y, X2 = xx, Y2 = yy, Stroke = ComponentCardPalette.Dimension, StrokeThickness = 1.1 });
    private static void Arrow(Canvas c, Point tip, System.Windows.Vector d) { d.Normalize(); var n = new System.Windows.Vector(-d.Y, d.X); c.Children.Add(new Polygon { Fill = ComponentCardPalette.Dimension, Points = new PointCollection { tip, tip + d * 8 + n * 2.5, tip + d * 8 - n * 2.5 } }); }
    private static void Label(Canvas c, string value, double x, double y, bool vertical = false) { var t = new TextBlock { Text = value, FontFamily = new FontFamily("Arial"), FontSize = 25, Foreground = ComponentCardPalette.Dimension, Background = ComponentCardPalette.Projection, Padding = new Thickness(3, 0, 3, 0) }; if (vertical) t.LayoutTransform = new RotateTransform(-90); t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); Canvas.SetLeft(t, vertical ? x - t.DesiredSize.Width - 8 : x - t.DesiredSize.Width / 2); Canvas.SetTop(t, vertical ? y - t.DesiredSize.Height / 2 : y - t.DesiredSize.Height - 10); c.Children.Add(t); }
    private static void H(Canvas c, double a, double b, double y, double origin, string value) { var d = Math.Sign(y - origin); Line(c, a, origin, a, y + d * 7); Line(c, b, origin, b, y + d * 7); Line(c, a, y, b, y); Arrow(c, new(a, y), new(1, 0)); Arrow(c, new(b, y), new(-1, 0)); Label(c, value, (a + b) / 2, y); }
    private static void V(Canvas c, double x, double a, double b, double origin, string value, bool right = false) { var d = Math.Sign(x - origin); Line(c, origin, a, x + d * 7, a); Line(c, origin, b, x + d * 7, b); Line(c, x, a, x, b); Arrow(c, new(x, a), new(0, 1)); Arrow(c, new(x, b), new(0, -1)); Label(c, value, right ? x + 48 : x, (a + b) / 2, true); }
    private static void SmallV(Canvas c, double x, double a, double b, double origin, double shelfY, string value) { Line(c, origin, a, x + 7, a); Line(c, origin, b, x + 7, b); Line(c, x, shelfY, x, b + 16); Arrow(c, new(x, a), new(0, -1)); Arrow(c, new(x, b), new(0, 1)); Line(c, x, shelfY, x + 76, shelfY); Label(c, value, x + 38, shelfY); }
    private static void SideH(Canvas c, double a, double b, double y, double origin, string value) { Line(c, a, origin, a, y + 7); Line(c, b, origin, b, y + 7); Line(c, a - 25, y, b + 18, y); Arrow(c, new(a, y), new(-1, 0)); Arrow(c, new(b, y), new(1, 0)); Label(c, value, b + 52, y); }
}
