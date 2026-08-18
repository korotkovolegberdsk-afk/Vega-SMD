using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.Models.MasterLibrary;
using Vega.StencilUI.Controls;

namespace Vega.StencilUI.TapeReelDrawing;

/// <summary>Read-only schematic viewer of carrier tape geometry.</summary>
public partial class TapeReelPreview : UserControl
{
    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
        nameof(Geometry), typeof(ComponentTapeReelGeometry), typeof(TapeReelPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));

    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
        nameof(Component), typeof(ComponentDefinition), typeof(TapeReelPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    private double _zoom = 1.0;
    private double _fitScale = 1.0;

    public ComponentTapeReelGeometry? Geometry
    {
        get => (ComponentTapeReelGeometry?)GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }

    public ComponentDefinition? Component
    {
        get => (ComponentDefinition?)GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }
    public TapeReelPreview()
    {
        InitializeComponent();
        Loaded += (_, _) => Fit();
    }

    public void ZoomIn()
    {
        _zoom = Math.Min(10, _zoom * 1.25);
        Render();
    }

    public void ZoomOut()
    {
        _zoom = Math.Max(.1, _zoom / 1.25);
        Render();
    }

    public void Fit()
    {
        var geometry = Geometry;
        if (!HasDrawingGeometry(geometry) || DrawingCanvas.ActualWidth <= 0 || DrawingCanvas.ActualHeight <= 0) return;

        var p0 = geometry!.SprocketHolePitch;
        var p1 = geometry.PocketPitch;
        var tapeWidth = geometry.CarrierTapeWidth;
        var widthMm = Math.Max(p0 * .5 + p1 * 3.5, 20);
        _fitScale = Math.Min((DrawingCanvas.ActualWidth - 70) / widthMm, (DrawingCanvas.ActualHeight - 80) / tapeWidth);
        _zoom = 1;
        Render();
    }

    private static void GeometryChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) => ((TapeReelPreview)source).Fit();
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => ZoomOut();
    private void Fit_Click(object sender, RoutedEventArgs e) => Fit();
    private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => Fit();

    private void Render()
    {
        DrawingCanvas.Children.Clear();
        var geometry = Geometry;
        if (!HasDrawingGeometry(geometry) || DrawingCanvas.ActualWidth <= 0 || DrawingCanvas.ActualHeight <= 0) return;

        var p1 = geometry!.PocketPitch;
        var tapeWidth = geometry.CarrierTapeWidth;
        var pocketLength = geometry.PocketLength;
        var pocketWidth = geometry.PocketWidth;
        var holeDiameter = geometry.SprocketHoleDiameter;
        var scale = _fitScale * _zoom;
        var originX = 25d;
        var originY = (DrawingCanvas.ActualHeight - tapeWidth * scale) / 2;
        var visibleTapeWidth = Math.Max(1, (DrawingCanvas.ActualWidth - 50) / scale);
        var layout = TapeReelGeometryLayout.Create(geometry, visibleTapeWidth, 4);
        var package = Component?.Package;

        AddRectangle(originX, originY, Math.Max(10, DrawingCanvas.ActualWidth - 50), tapeWidth * scale,
            new SolidColorBrush(Color.FromRgb(225, 232, 238)), Brushes.DimGray);

        var sprocketHoleCenterY = originY + tapeWidth * scale * .18;
        var tapeBottomY = originY + tapeWidth * scale;
        foreach (var holePosition in layout.HoleXs)
        {
            var holeX = originX + holePosition * scale;
            AddEllipse(holeX - holeDiameter * scale / 2, sprocketHoleCenterY - holeDiameter * scale / 2,
                holeDiameter * scale, holeDiameter * scale, Brushes.White, Brushes.DimGray);
        }

        var pocketCenterY = (sprocketHoleCenterY + tapeBottomY) / 2;
        foreach (var pocketPosition in layout.PocketXs)
        {
            var pocketX = originX + pocketPosition * scale;
            var pocketY = pocketCenterY - pocketWidth * scale / 2;
            AddRectangle(pocketX - pocketLength * scale / 2, pocketY, pocketLength * scale, pocketWidth * scale,
                new SolidColorBrush(Color.FromRgb(94, 131, 159)), Brushes.Navy);

            AddPackageComponent(package, pocketX, pocketCenterY, pocketLength * scale, pocketWidth * scale, geometry.PickupRotation);
        }

        if (layout.PocketXs.Count > 1)
        {
            var firstPocketX = originX + layout.PocketXs[0] * scale;
            var secondPocketX = originX + layout.PocketXs[1] * scale;
            AddDimension(firstPocketX, secondPocketX, originY + tapeWidth * scale + 22, $"P1 {p1:0.###} мм");
        }

        AddVerticalDimension(originX + DrawingCanvas.ActualWidth - 36, originY, originY + tapeWidth * scale, $"W {tapeWidth:0.###} мм");
    }
    private void AddPackageComponent(PackageDefinition? package, double centerX, double centerY, double pocketLength, double pocketWidth, double rotation)
    {
        if (package is null) return;

        var size = Math.Max(20, Math.Min(pocketLength, pocketWidth) * .72);
        var drawing = new PackageDrawingPreview
        {
            Package = package,
            ShowDimensions = false,
            Width = 160,
            Height = 160,
            IsHitTestVisible = false
        };
        var component = new Viewbox
        {
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            Child = drawing,
            IsHitTestVisible = false,
            RenderTransform = new RotateTransform(NormalizeRotation(rotation), size / 2, size / 2)
        };
        Canvas.SetLeft(component, centerX - size / 2);
        Canvas.SetTop(component, centerY - size / 2);
        DrawingCanvas.Children.Add(component);
    }

    private static double NormalizeRotation(double rotation)
    {
        var normalized = rotation % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }
    private static bool HasDrawingGeometry(ComponentTapeReelGeometry? geometry) =>
        geometry is not null &&
        geometry.CarrierTapeWidth > 0 &&
        geometry.PocketPitch > 0 &&
        geometry.PocketLength > 0 &&
        geometry.PocketWidth > 0 &&
        geometry.SprocketHolePitch > 0 &&
        geometry.SprocketHoleDiameter > 0;
    private void AddRectangle(double x, double y, double width, double height, Brush fill, Brush stroke) { var shape = new Rectangle { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = 1 }; Canvas.SetLeft(shape, x); Canvas.SetTop(shape, y); DrawingCanvas.Children.Add(shape); }
    private void AddEllipse(double x, double y, double width, double height, Brush fill, Brush? stroke) { var shape = new Ellipse { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = stroke is null ? 0 : 1 }; Canvas.SetLeft(shape, x); Canvas.SetTop(shape, y); DrawingCanvas.Children.Add(shape); }
    private void AddLine(double x1, double y1, double x2, double y2, Brush brush, double thickness) { DrawingCanvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness }); }
    private void AddText(string text, double x, double y, Brush brush) { var block = new TextBlock { Text = text, Foreground = brush, FontSize = 11, FontWeight = FontWeights.SemiBold }; Canvas.SetLeft(block, x); Canvas.SetTop(block, y); DrawingCanvas.Children.Add(block); }
    private void AddDimension(double from, double to, double y, string label)
    {
        AddLine(from, y, to, y, Brushes.Black, 1);
        AddLine(from, y - 4, from, y + 4, Brushes.Black, 1);
        AddLine(to, y - 4, to, y + 4, Brushes.Black, 1);
        AddText(label, (from + to) / 2 - 26, y - 17, Brushes.Black);
    }
    private void AddVerticalDimension(double x, double from, double to, string label)
    {
        AddLine(x, from, x, to, Brushes.Black, 1);
        AddLine(x - 4, from, x + 4, from, Brushes.Black, 1);
        AddLine(x - 4, to, x + 4, to, Brushes.Black, 1);
        AddText(label, x - 62, (from + to) / 2 - 6, Brushes.Black);
    }
    private void AddOrientationArrow(double x, double y, TapePocketOrientation orientation)
    {
        var length = 14d;
        var direction = orientation switch { TapePocketOrientation.Deg90 => new Vector(0, -1), TapePocketOrientation.Deg180 => new Vector(-1, 0), TapePocketOrientation.Deg270 => new Vector(0, 1), _ => new Vector(1, 0) };
        var end = new Point(x + direction.X * length, y + direction.Y * length);
        AddLine(x, y, end.X, end.Y, Brushes.White, 2);
        AddLine(end.X, end.Y, end.X - direction.X * 5 - direction.Y * 3, end.Y - direction.Y * 5 + direction.X * 3, Brushes.White, 2);
        AddLine(end.X, end.Y, end.X - direction.X * 5 + direction.Y * 3, end.Y - direction.Y * 5 - direction.X * 3, Brushes.White, 2);
    }
}



