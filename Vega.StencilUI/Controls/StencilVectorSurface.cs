using System.Windows;
using System.Windows.Media;
using Vega.Gerber.Models;
using Vega.StencilUI.Models;
using Vega.StencilCAM.Models;
using Vega.StencilViewer.Models;

namespace Vega.StencilUI.Controls;

/// <summary>Resolution-independent stencil geometry surface with hit-test and window selection.</summary>
public sealed class StencilVectorSurface : FrameworkElement
{
    public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(StencilViewDocument), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(null, Redraw));
    public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(nameof(ViewMode), typeof(StencilViewMode), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(StencilViewMode.Overlay, Redraw));
    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(1d, Redraw));
    public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(0d, Redraw));
    public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(0d, Redraw));
    public static readonly DependencyProperty ShowFrameProperty = DependencyProperty.Register(nameof(ShowFrame), typeof(bool), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(true, Redraw));
    public static readonly DependencyProperty ShowBoardProperty = DependencyProperty.Register(nameof(ShowBoard), typeof(bool), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(true, Redraw));
    public static readonly DependencyProperty ShowAperturesProperty = DependencyProperty.Register(nameof(ShowApertures), typeof(bool), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(true, Redraw));
    public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(nameof(EditMode), typeof(bool), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(false, Redraw));
    public static readonly DependencyProperty SelectedPrimitivesProperty = DependencyProperty.Register(nameof(SelectedPrimitives), typeof(IReadOnlyList<PastePrimitive>), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(Array.Empty<PastePrimitive>(), Redraw));
    public static readonly DependencyProperty EditedWidthProperty = DependencyProperty.Register(nameof(EditedWidth), typeof(double), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(0d, Redraw));
    public static readonly DependencyProperty EditedHeightProperty = DependencyProperty.Register(nameof(EditedHeight), typeof(double), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(0d, Redraw));
    public static readonly DependencyProperty ApertureOverridesProperty = DependencyProperty.Register(nameof(ApertureOverrides), typeof(IReadOnlyDictionary<int, EditableAperture>), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(null, Redraw));
    public static readonly DependencyProperty SelectionRectangleProperty = DependencyProperty.Register(nameof(SelectionRectangle), typeof(Rect), typeof(StencilVectorSurface), new FrameworkPropertyMetadata(Rect.Empty, Redraw));

    public StencilViewDocument? Document { get => (StencilViewDocument?)GetValue(DocumentProperty); set => SetValue(DocumentProperty, value); }
    public StencilViewMode ViewMode { get => (StencilViewMode)GetValue(ViewModeProperty); set => SetValue(ViewModeProperty, value); }
    public double Zoom { get => (double)GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }
    public double OffsetX { get => (double)GetValue(OffsetXProperty); set => SetValue(OffsetXProperty, value); }
    public double OffsetY { get => (double)GetValue(OffsetYProperty); set => SetValue(OffsetYProperty, value); }
    public bool ShowFrame { get => (bool)GetValue(ShowFrameProperty); set => SetValue(ShowFrameProperty, value); }
    public bool ShowBoard { get => (bool)GetValue(ShowBoardProperty); set => SetValue(ShowBoardProperty, value); }
    public bool ShowApertures { get => (bool)GetValue(ShowAperturesProperty); set => SetValue(ShowAperturesProperty, value); }
    public bool EditMode { get => (bool)GetValue(EditModeProperty); set => SetValue(EditModeProperty, value); }
    public IReadOnlyList<PastePrimitive> SelectedPrimitives { get => (IReadOnlyList<PastePrimitive>)GetValue(SelectedPrimitivesProperty); set => SetValue(SelectedPrimitivesProperty, value); }
    public double EditedWidth { get => (double)GetValue(EditedWidthProperty); set => SetValue(EditedWidthProperty, value); }
    public double EditedHeight { get => (double)GetValue(EditedHeightProperty); set => SetValue(EditedHeightProperty, value); }
    public IReadOnlyDictionary<int, EditableAperture>? ApertureOverrides { get => (IReadOnlyDictionary<int, EditableAperture>?)GetValue(ApertureOverridesProperty); set => SetValue(ApertureOverridesProperty, value); }
    public Rect SelectionRectangle { get => (Rect)GetValue(SelectionRectangleProperty); set => SetValue(SelectionRectangleProperty, value); }

    public PastePrimitive? HitTestPrimitive(Point point)
    {
        var scene = BuildScene();
        if (!TryCreateViewport(scene, out var viewport)) return null;
        foreach (var item in scene.Where(item => item.Primitive is not null).Reverse())
            if (ScreenBounds(item, viewport).InflateCopy(3).Contains(point)) return item.Primitive;
        return null;
    }

    public IReadOnlyList<PastePrimitive> GetPrimitivesInRectangle(Rect rectangle, bool fullyInside)
    {
        var scene = BuildScene();
        if (!TryCreateViewport(scene, out var viewport)) return [];
        return scene.Where(item => item.Primitive is not null)
            .Where(item => fullyInside ? rectangle.Contains(ScreenBounds(item, viewport)) : rectangle.IntersectsWith(ScreenBounds(item, viewport)))
            .Select(item => item.Primitive!).Distinct().ToList();
    }

    protected override void OnRender(DrawingContext context)
    {
        base.OnRender(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        var scene = BuildScene();
        if (!TryCreateViewport(scene, out var viewport)) return;
        foreach (var item in scene)
        {
            var bounds = ScreenBounds(item, viewport);
            var center = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
            var selected = EditMode && item.Primitive is not null && SelectedPrimitives.Contains(item.Primitive);
            var brush = BrushFor(item.Layer);
            var pen = selected ? new Pen(Brushes.Gold, 2.5) : new Pen(brush, item.Layer == StencilOverlayLayerType.Frame ? 1.5 : 0.5);
            context.PushTransform(new RotateTransform(-item.Rotation, center.X, center.Y));
            if (item.IsRound) context.DrawEllipse(item.Layer == StencilOverlayLayerType.Frame ? null : brush, pen, center, bounds.Width / 2, bounds.Height / 2);
            else context.DrawRectangle(item.Layer == StencilOverlayLayerType.Frame ? null : brush, pen, bounds);
            context.Pop();
        }
        if (!SelectionRectangle.IsEmpty)
        {
            var selectionPen = new Pen(Brushes.Gold, 1) { DashStyle = DashStyles.Dash };
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(30, 255, 215, 0)), selectionPen, SelectionRectangle);
        }
    }

    private List<ScenePrimitive> BuildScene()
    {
        if (Document is null) return [];
        var scene = new List<ScenePrimitive>();
        void Add(IEnumerable<PastePrimitive> primitives, StencilOverlayLayerType layer)
        {
            foreach (var primitive in primitives) { var aperture = ApertureOverrides is not null && ApertureOverrides.TryGetValue(primitive.ApertureId, out var modified) ? modified : null; scene.Add(new ScenePrimitive(primitive, layer, primitive.X, primitive.Y, aperture?.Width ?? primitive.Width, aperture?.Height ?? primitive.Height, primitive.Rotation, aperture?.Shape ?? primitive.ShapeType?.ToString() ?? "Rectangle")); }
        }
        var original = Document.OriginalPasteLayer?.Primitives ?? [];
        var corrected = Document.CorrectedPasteLayer?.CorrectedPrimitives ?? [];
        switch (ViewMode)
        {
            case StencilViewMode.Original: Add(original, StencilOverlayLayerType.OriginalPaste); break;
            case StencilViewMode.Corrected: Add(corrected, StencilOverlayLayerType.CorrectedPaste); break;
            case StencilViewMode.Overlay: Add(original, StencilOverlayLayerType.OriginalPaste); Add(corrected, StencilOverlayLayerType.CorrectedPaste); break;
            case StencilViewMode.Production:
                if (ShowFrame && Document.Frame is not null) AddFrame(scene, Document.Frame);
                if (ShowBoard) Add(original, StencilOverlayLayerType.OriginalPaste);
                if (ShowApertures) Add(corrected, StencilOverlayLayerType.CorrectedPaste);
                break;
        }
        return scene.Where(item => item.Width > 0 || item.Height > 0).ToList();
    }

    private static void AddFrame(List<ScenePrimitive> scene, StencilFrame frame)
    {
        var width = frame.StencilWidth > 0 ? frame.StencilWidth : frame.FrameWidth;
        var height = frame.StencilHeight > 0 ? frame.StencilHeight : frame.FrameHeight;
        scene.Add(new ScenePrimitive(null, StencilOverlayLayerType.Frame, frame.OriginX + width / 2, frame.OriginY + height / 2, width, height, 0, "Frame"));
    }

    private bool TryCreateViewport(IReadOnlyList<ScenePrimitive> scene, out Viewport viewport)
    {
        viewport = default;
        if (scene.Count == 0 || ActualWidth <= 0 || ActualHeight <= 0) return false;
        var minX = scene.Min(item => item.X - DisplayWidth(item) / 2); var maxX = scene.Max(item => item.X + DisplayWidth(item) / 2);
        var minY = scene.Min(item => item.Y - DisplayHeight(item) / 2); var maxY = scene.Max(item => item.Y + DisplayHeight(item) / 2);
        var scale = 0.88 * Math.Min(ActualWidth / Math.Max(maxX - minX, 0.01), ActualHeight / Math.Max(maxY - minY, 0.01)) * Math.Clamp(Zoom, 0.1, 10);
        viewport = new Viewport((minX + maxX) / 2, (minY + maxY) / 2, scale, OffsetX, OffsetY, ActualWidth, ActualHeight);
        return true;
    }

    private Rect ScreenBounds(ScenePrimitive item, Viewport viewport)
    {
        var center = viewport.ToScreen(item.X, item.Y);
        return new Rect(center.X - Math.Max(DisplayWidth(item) * viewport.Scale, 1) / 2, center.Y - Math.Max(DisplayHeight(item) * viewport.Scale, 1) / 2, Math.Max(DisplayWidth(item) * viewport.Scale, 1), Math.Max(DisplayHeight(item) * viewport.Scale, 1));
    }
    private double DisplayWidth(ScenePrimitive item) => item.Primitive is not null && SelectedPrimitives.Contains(item.Primitive) && EditedWidth > 0 ? EditedWidth : item.Width;
    private double DisplayHeight(ScenePrimitive item) => item.Primitive is not null && SelectedPrimitives.Contains(item.Primitive) && EditedHeight > 0 ? EditedHeight : item.Height;
    private static void Redraw(DependencyObject source, DependencyPropertyChangedEventArgs e) => ((StencilVectorSurface)source).InvalidateVisual();
    private static Brush BrushFor(StencilOverlayLayerType layer) => layer switch
    {
        StencilOverlayLayerType.OriginalPaste => new SolidColorBrush(Color.FromArgb(150, 80, 150, 255)),
        StencilOverlayLayerType.CorrectedPaste => new SolidColorBrush(Color.FromArgb(180, 70, 205, 130)),
        StencilOverlayLayerType.Frame => new SolidColorBrush(Color.FromRgb(240, 180, 60)), _ => new SolidColorBrush(Color.FromRgb(190, 190, 190))
    };
    private sealed record ScenePrimitive(PastePrimitive? Primitive, StencilOverlayLayerType Layer, double X, double Y, double Width, double Height, double Rotation, string Shape)
    { public bool IsRound => Shape.Contains("Round", StringComparison.OrdinalIgnoreCase) || Shape.Contains("Circle", StringComparison.OrdinalIgnoreCase); }
    private readonly record struct Viewport(double CenterX, double CenterY, double Scale, double OffsetX, double OffsetY, double Width, double Height)
    { public Point ToScreen(double x, double y) => new(Width / 2 + (x - CenterX) * Scale + OffsetX, Height / 2 - (y - CenterY) * Scale + OffsetY); }
}

file static class RectExtensions
{
    public static Rect InflateCopy(this Rect source, double amount) { source.Inflate(amount, amount); return source; }
}