using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// LQFP-32, 7 x 7 mm, 0.80 mm pitch.  The nominal outline follows
// JEDEC MS-026 ABA; the selected ST reference is STM32L071KBT6TR.
internal static class Qfp32PackageDrawing
{
    private const double Body = 7.00, Overall = 9.10, Height = 1.40, Pitch = .80,
        LeadWidth = .375, LeadLength = .60, LeadThickness = .15;
    private const int PinsPerSide = 8;
    private static readonly Brush BodyBrush = new SolidColorBrush(Color.FromRgb(74, 91, 108));
    private static readonly Brush Outline = new SolidColorBrush(Color.FromRgb(52, 67, 82));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(211, 214, 211));

    public static bool Supports(string? name) => name?.Equals("LQFP032P080W091", StringComparison.OrdinalIgnoreCase) == true;

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
        Box(-(float)Body / 2, (float)Body / 2, -(float)Body / 2, (float)Body / 2, .10f, (float)Height, body, true);
        foreach (var p in Pins())
        {
            var q = (float)p; var half = (float)LeadWidth / 2;
            Box(-(float)Overall / 2, -(float)Body / 2, q - half, q + half, 0, (float)LeadThickness, metal);
            Box((float)Body / 2, (float)Overall / 2, q - half, q + half, 0, (float)LeadThickness, metal);
            Box(q - half, q + half, -(float)Overall / 2, -(float)Body / 2, 0, (float)LeadThickness, metal);
            Box(q - half, q + half, (float)Body / 2, (float)Overall / 2, 0, (float)LeadThickness, metal);
        }
        Box((float)Body * .30f, (float)Body * .38f, -(float)Body * .46f, -(float)Body * .42f, (float)Height, (float)Height + .01f, mark);
        return mesh;
    }

    public static void PaintTop(Canvas canvas, double cx, double cy, double scale, bool bottom = false)
    {
        var body = new Rectangle { Width = Body * scale, Height = Body * scale, Fill = BodyBrush, Stroke = Outline, StrokeThickness = .8 };
        Canvas.SetLeft(body, cx - Body * scale / 2); Canvas.SetTop(body, cy - Body * scale / 2); canvas.Children.Add(body);
        foreach (var p in Pins())
        {
            PaintHorizontalLead(canvas, cx, cy + p * scale, scale, -1); PaintHorizontalLead(canvas, cx, cy + p * scale, scale, 1);
            PaintVerticalLead(canvas, cx + p * scale, cy, scale, -1); PaintVerticalLead(canvas, cx + p * scale, cy, scale, 1);
        }
        if (!bottom)
        {
            var dot = new Ellipse { Width = .28 * scale, Height = .28 * scale, Fill = Brushes.White };
            Canvas.SetLeft(dot, cx + Body * .34 * scale); Canvas.SetTop(dot, cy - Body * .47 * scale); canvas.Children.Add(dot);
        }
    }

    private static void PaintHorizontalLead(Canvas canvas, double cx, double y, double scale, double side)
    {
        var edge = side * Body / 2; var shoulder = edge + side * .20; var tip = side * Overall / 2; var half = LeadWidth / 2; var neck = half * .56;
        Point P(double x, double yy) => new(cx + x * scale, yy);
        var g = new StreamGeometry(); using (var x = g.Open())
        {
            x.BeginFigure(P(edge, y - neck * scale), true, true); x.LineTo(P(shoulder, y - neck * scale), true, false);
            x.BezierTo(P(shoulder + side * .09, y - neck * scale), P(shoulder + side * .13, y - half * scale), P(shoulder + side * .22, y - half * scale), true, false);
            x.LineTo(P(tip, y - half * scale), true, false); x.LineTo(P(tip, y + half * scale), true, false); x.LineTo(P(shoulder + side * .22, y + half * scale), true, false);
            x.BezierTo(P(shoulder + side * .13, y + half * scale), P(shoulder + side * .09, y + neck * scale), P(shoulder, y + neck * scale), true, false); x.LineTo(P(edge, y + neck * scale), true, false);
        }
        canvas.Children.Add(new Path { Data = g, Fill = Metal, Stroke = ComponentCardPalette.Border, StrokeThickness = .8 });
    }

    private static void PaintVerticalLead(Canvas canvas, double x, double cy, double scale, double side)
    {
        var edge = side * Body / 2; var shoulder = edge + side * .20; var tip = side * Overall / 2; var half = LeadWidth / 2; var neck = half * .56;
        Point P(double xx, double y) => new(xx, cy + y * scale);
        var g = new StreamGeometry(); using (var q = g.Open())
        {
            q.BeginFigure(P(x - neck * scale, edge), true, true); q.LineTo(P(x - neck * scale, shoulder), true, false);
            q.BezierTo(P(x - neck * scale, shoulder + side * .09), P(x - half * scale, shoulder + side * .13), P(x - half * scale, shoulder + side * .22), true, false);
            q.LineTo(P(x - half * scale, tip), true, false); q.LineTo(P(x + half * scale, tip), true, false); q.LineTo(P(x + half * scale, shoulder + side * .22), true, false);
            q.BezierTo(P(x + half * scale, shoulder + side * .13), P(x + neck * scale, shoulder + side * .09), P(x + neck * scale, shoulder), true, false); q.LineTo(P(x + neck * scale, edge), true, false);
        }
        canvas.Children.Add(new Path { Data = g, Fill = Metal, Stroke = ComponentCardPalette.Border, StrokeThickness = .8 });
    }

    public static UIElement Create(StepProjectionKind kind)
    {
        var c = new Canvas { Width = 512, Height = 370, Background = Brushes.Transparent }; const double cx = 256, cy = 178; const double s = 25;
        string F(double value) => value.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
        if (kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c, cx, cy, s, kind == StepProjectionKind.Bottom);
            if (kind == StepProjectionKind.Top)
            {
                var outerLeft = cx - Overall * s / 2; var outerRight = cx + Overall * s / 2; var top = cy - Overall * s / 2; var bottom = cy + Overall * s / 2;
                H(c, outerLeft, outerRight, Math.Max(36, top - 22), top, F(Overall)); HBelow(c, cx - Body * s / 2, cx + Body * s / 2, bottom + 27, bottom, F(Body));
                V(c, outerLeft - 31, top, bottom, outerLeft, F(Overall));
                V(c, outerRight + 23, cy - 3.5 * Pitch * s, cy - 2.5 * Pitch * s, outerRight, F(Pitch), true);
                SmallV(c, outerRight + 30, cy + 3.5 * Pitch * s - LeadWidth * s / 2, cy + 3.5 * Pitch * s + LeadWidth * s / 2, outerRight, cy + 3.5 * Pitch * s - 30, F(LeadWidth));
            }
        }
        else
        {
            const double floor = 220; var body = new Rectangle { Width = Body * s, Height = Height * s, Fill = BodyBrush, Stroke = Outline, StrokeThickness = .8 };
            Canvas.SetLeft(body, cx - Body * s / 2); Canvas.SetTop(body, floor - Height * s); c.Children.Add(body);
            GullWingLeadDrawing.PaintHorizontalSide(c, cx / s - Body / 2, cx / s + Body / 2, floor, cx / s - Overall / 2, cx / s + Overall / 2, Height, LeadThickness, s, Metal);
            V(c, cx - Overall * s / 2 - 32, floor - Height * s, floor, cx - Overall * s / 2, F(Height)); SideH(c, cx + Overall * s / 2 - LeadLength * s, cx + Overall * s / 2, floor + 46, floor, F(LeadLength));
        }
        return new Viewbox { Stretch = Stretch.Uniform, Child = c, Margin = new Thickness(8) };
    }

    private static void Line(Canvas c, double x, double y, double xx, double yy) => c.Children.Add(new Line { X1 = x, Y1 = y, X2 = xx, Y2 = yy, Stroke = ComponentCardPalette.Dimension, StrokeThickness = 1.1 });
    private static void Arrow(Canvas c, Point tip, Vector d) { d.Normalize(); var n = new Vector(-d.Y, d.X); c.Children.Add(new Polygon { Fill = ComponentCardPalette.Dimension, Points = new PointCollection { tip, tip + d * 8 + n * 2.5, tip + d * 8 - n * 2.5 } }); }
    private static void Label(Canvas c, string value, double x, double y, bool vertical = false) { var t = new TextBlock { Text = value, FontFamily = new FontFamily("Arial"), FontSize = 25, Foreground = ComponentCardPalette.Dimension, Background = ComponentCardPalette.Projection, Padding = new Thickness(3, 0, 3, 0) }; if (vertical) t.LayoutTransform = new RotateTransform(-90); t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); Canvas.SetLeft(t, vertical ? x - t.DesiredSize.Width - 8 : x - t.DesiredSize.Width / 2); Canvas.SetTop(t, vertical ? y - t.DesiredSize.Height / 2 : y - t.DesiredSize.Height - 10); c.Children.Add(t); }
    private static void H(Canvas c, double a, double b, double y, double origin, string value) { var d = Math.Sign(y - origin); Line(c, a, origin, a, y + d * 7); Line(c, b, origin, b, y + d * 7); Line(c, a, y, b, y); Arrow(c, new(a, y), new(1, 0)); Arrow(c, new(b, y), new(-1, 0)); Label(c, value, (a + b) / 2, y); }
    private static void HBelow(Canvas c, double a, double b, double y, double origin, string value) { Line(c, a, origin, a, y + 7); Line(c, b, origin, b, y + 7); Line(c, a, y, b, y); Arrow(c, new(a, y), new(1, 0)); Arrow(c, new(b, y), new(-1, 0)); var t = new TextBlock { Text = value, FontFamily = new FontFamily("Arial"), FontSize = 25, Foreground = ComponentCardPalette.Dimension, Background = ComponentCardPalette.Projection, Padding = new Thickness(3, 0, 3, 0) }; t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); Canvas.SetLeft(t, (a + b - t.DesiredSize.Width) / 2); Canvas.SetTop(t, y + 5); c.Children.Add(t); }
    private static void V(Canvas c, double x, double a, double b, double origin, string value, bool right = false) { var d = Math.Sign(x - origin); Line(c, origin, a, x + d * 7, a); Line(c, origin, b, x + d * 7, b); Line(c, x, a, x, b); Arrow(c, new(x, a), new(0, 1)); Arrow(c, new(x, b), new(0, -1)); Label(c, value, right ? x + 48 : x, (a + b) / 2, true); }
    private static void SmallV(Canvas c, double x, double a, double b, double origin, double shelfY, string value) { Line(c, origin, a, x + 7, a); Line(c, origin, b, x + 7, b); Line(c, x, shelfY, x, b + 16); Arrow(c, new(x, a), new(0, -1)); Arrow(c, new(x, b), new(0, 1)); Line(c, x, shelfY, x + 76, shelfY); Label(c, value, x + 38, shelfY); }
    private static void SideH(Canvas c, double a, double b, double y, double origin, string value) { Line(c, a, origin, a, y + 7); Line(c, b, origin, b, y + 7); Line(c, a - 70, y, b + 15, y); Arrow(c, new(a, y), new(-1, 0)); Arrow(c, new(b, y), new(1, 0)); Label(c, value, a - 34, y); }
}
