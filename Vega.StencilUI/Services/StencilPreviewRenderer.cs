using System.Windows;
using System.Windows.Media;
using Vega.StencilViewer;
using Vega.StencilViewer.Models;

namespace Vega.StencilUI.Services;

/// <summary>Renders the Gerber overlay geometry prepared by Vega.StencilViewer for WPF preview.</summary>
public sealed class StencilPreviewLayerVisibility
{
    public bool ShowFrame { get; init; } = true;
    public bool ShowBoard { get; init; } = true;
    public bool ShowApertures { get; init; } = true;
}

public sealed class StencilPreviewRenderer
{
    private const double CanvasWidth = 800;
    private const double CanvasHeight = 320;
    private const double FitCoverage = 0.88;

    public ImageSource? Render(StencilViewDocument document, StencilViewMode mode, StencilPreviewLayerVisibility? visibility = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        var overlay = new StencilOverlayService();
        overlay.LoadProject(document);
        visibility ??= new StencilPreviewLayerVisibility();
        var layers = overlay.CreateOverlay(mode).Where(layer => IsVisible(layer, mode, visibility)).ToList();
        var geometry = layers.SelectMany(layer => layer.Geometry.Select(item => (Layer: layer.LayerType, Geometry: item)))
            .Where(item => item.Geometry.Width > 0 || item.Geometry.Height > 0)
            .ToList();

        System.Diagnostics.Debug.WriteLine("=== PREVIEW RENDER ===");
        System.Diagnostics.Debug.WriteLine($"Mode: {mode}");
        System.Diagnostics.Debug.WriteLine($"Input geometry count: {geometry.Count}");
        System.Diagnostics.Debug.WriteLine($"Source primitives: original={document.OriginalPasteLayer?.Primitives.Count ?? 0}; corrected={document.CorrectedPasteLayer?.CorrectedPrimitives.Count ?? 0}; layers={layers.Count}");
        if (geometry.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("Rendered objects: 0 (no overlay geometry received)");
            return null;
        }

        var minX = geometry.Min(item => item.Geometry.X - item.Geometry.Width / 2);
        var maxX = geometry.Max(item => item.Geometry.X + item.Geometry.Width / 2);
        var minY = geometry.Min(item => item.Geometry.Y - item.Geometry.Height / 2);
        var maxY = geometry.Max(item => item.Geometry.Y + item.Geometry.Height / 2);
        var sourceWidth = Math.Max(maxX - minX, 0.01);
        var sourceHeight = Math.Max(maxY - minY, 0.01);
        var scale = FitCoverage * Math.Min(CanvasWidth / sourceWidth, CanvasHeight / sourceHeight);
        var offsetX = (CanvasWidth - sourceWidth * scale) / 2 - minX * scale;
        var offsetY = (CanvasHeight - sourceHeight * scale) / 2 - minY * scale;
        System.Diagnostics.Debug.WriteLine($"BoundingBox: MinX={minX:0.####}; MaxX={maxX:0.####}; MinY={minY:0.####}; MaxY={maxY:0.####}");
        System.Diagnostics.Debug.WriteLine($"Canvas size: Width={CanvasWidth}; Height={CanvasHeight}");
        System.Diagnostics.Debug.WriteLine($"Scale: {scale:0.######}");

        var drawing = new DrawingGroup();
        using (var context = drawing.Open())
        {
            // Defines the full viewport so fit-to-view remains stable for narrow geometry.
            context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, CanvasWidth, CanvasHeight));
            foreach (var item in geometry)
            {
                var value = item.Geometry;
                var brush = BrushFor(item.Layer);
                var pen = new Pen(brush, item.Layer == StencilOverlayLayerType.Frame ? 1.5 : 0.5);
                var width = Math.Max(value.Width * scale, 1);
                var height = Math.Max(value.Height * scale, 1);
                var x = value.X * scale + offsetX;
                var y = CanvasHeight - (value.Y * scale + offsetY);
                context.PushTransform(new RotateTransform(-value.Rotation, x, y));
                if (value.Shape.Contains("Round", StringComparison.OrdinalIgnoreCase) || value.Shape.Contains("Circle", StringComparison.OrdinalIgnoreCase))
                    context.DrawEllipse(brush, pen, new Point(x, y), width / 2, height / 2);
                else
                    context.DrawRectangle(item.Layer == StencilOverlayLayerType.Frame ? null : brush, pen, new Rect(x - width / 2, y - height / 2, width, height));
                context.Pop();
            }
        }
        System.Diagnostics.Debug.WriteLine($"Rendered objects: {geometry.Count}");
        drawing.Freeze();
        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    private static bool IsVisible(StencilOverlayLayer layer, StencilViewMode mode, StencilPreviewLayerVisibility visibility)
    {
        if (mode != StencilViewMode.Production) return true;
        return layer.LayerType switch
        {
            StencilOverlayLayerType.Frame => visibility.ShowFrame,
            StencilOverlayLayerType.OriginalPaste => visibility.ShowBoard,
            StencilOverlayLayerType.CorrectedPaste => visibility.ShowApertures,
            _ => true
        };
    }
    private static Brush BrushFor(StencilOverlayLayerType layer) => layer switch
    {
        StencilOverlayLayerType.OriginalPaste => new SolidColorBrush(Color.FromArgb(150, 80, 150, 255)),
        StencilOverlayLayerType.CorrectedPaste => new SolidColorBrush(Color.FromArgb(180, 70, 205, 130)),
        StencilOverlayLayerType.Frame => new SolidColorBrush(Color.FromRgb(240, 180, 60)),
        StencilOverlayLayerType.Fiducial => new SolidColorBrush(Color.FromRgb(235, 100, 100)),
        _ => new SolidColorBrush(Color.FromRgb(190, 190, 190))
    };
}