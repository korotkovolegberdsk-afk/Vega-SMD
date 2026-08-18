using System.Windows;
using System.Windows.Media;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.StencilUI.PackageDrawing;

namespace Vega.StencilUI.Controls;

/// <summary>Read-only aperture preview derived from package contacts and the selected stencil rule.</summary>
public sealed class StencilAperturePreview : FrameworkElement
{
    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(
        nameof(Package), typeof(PackageDefinition), typeof(StencilAperturePreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RuleProperty = DependencyProperty.Register(
        nameof(Rule), typeof(StencilTechnologyRule), typeof(StencilAperturePreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public PackageDefinition? Package
    {
        get => (PackageDefinition?)GetValue(PackageProperty);
        set => SetValue(PackageProperty, value);
    }

    public StencilTechnologyRule? Rule
    {
        get => (StencilTechnologyRule?)GetValue(RuleProperty);
        set => SetValue(RuleProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
        if (Package is null || Rule is null || ActualWidth < 40 || ActualHeight < 40) return;

        var contacts = ParametricPackageGeometryBuilder.Build(Package).Primitives
            .Where(primitive => primitive.Kind is ScenePrimitiveKind.Lead
                or ScenePrimitiveKind.Pad
                or ScenePrimitiveKind.Terminal
                or ScenePrimitiveKind.Ball
                or ScenePrimitiveKind.Tab
                or ScenePrimitiveKind.ExposedPad)
            .ToArray();
        if (contacts.Length == 0) return;

        var bounds = contacts.Select(contact => contact.Bounds).Aggregate(Rect.Union);
        if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0) return;
        const double margin = 14;
        var scale = Math.Min((ActualWidth - margin * 2) / bounds.Width, (ActualHeight - margin * 2) / bounds.Height);
        if (!double.IsFinite(scale) || scale <= 0) return;
        var offset = new Vector((ActualWidth - bounds.Width * scale) / 2 - bounds.Left * scale,
            (ActualHeight - bounds.Height * scale) / 2 - bounds.Top * scale);
        var shape = ApertureShapeSelectorService.TrySelectTechnologyShape(Rule, out var selected)
            ? selected
            : ApertureShapeType.Rectangle;
        var reductionX = Math.Clamp(Rule.PreferredReductionX, 0, 95) / 100d;
        var reductionY = Math.Clamp(Rule.PreferredReductionY, 0, 95) / 100d;

        foreach (var contact in contacts)
        {
            var pad = ToScreen(contact.Bounds, scale, offset);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(229, 232, 235)), new Pen(Brushes.Gray, .7), pad, 2, 2);
            var aperture = InsetByReduction(pad, reductionX, reductionY);
            DrawAperture(dc, aperture, contact.Kind == ScenePrimitiveKind.Ball ? ApertureShapeType.Round : shape);
        }
    }

    private static Rect ToScreen(Rect source, double scale, Vector offset) =>
        new(source.X * scale + offset.X, source.Y * scale + offset.Y, source.Width * scale, source.Height * scale);

    private static Rect InsetByReduction(Rect source, double reductionX, double reductionY)
    {
        var width = Math.Max(2, source.Width * (1 - reductionX));
        var height = Math.Max(2, source.Height * (1 - reductionY));
        return new Rect(source.Left + (source.Width - width) / 2, source.Top + (source.Height - height) / 2, width, height);
    }

    private static void DrawAperture(DrawingContext dc, Rect aperture, ApertureShapeType shape)
    {
        var fill = new SolidColorBrush(Color.FromRgb(205, 133, 63));
        var outline = new Pen(Brushes.Black, .8);
        switch (shape)
        {
            case ApertureShapeType.Round:
            case ApertureShapeType.Ellipse:
                dc.DrawEllipse(fill, outline, new Point(aperture.Left + aperture.Width / 2, aperture.Top + aperture.Height / 2), aperture.Width / 2, aperture.Height / 2);
                break;
            case ApertureShapeType.Square:
                var side = Math.Min(aperture.Width, aperture.Height);
                dc.DrawRectangle(fill, outline, new Rect(aperture.Left + (aperture.Width - side) / 2, aperture.Top + (aperture.Height - side) / 2, side, side));
                break;
            case ApertureShapeType.Oblong:
                dc.DrawRoundedRectangle(fill, outline, aperture, Math.Min(aperture.Width, aperture.Height) / 2, Math.Min(aperture.Width, aperture.Height) / 2);
                break;
            case ApertureShapeType.Array:
                var gap = Math.Max(1.5, Math.Min(aperture.Width, aperture.Height) * .08);
                var cellWidth = Math.Max(1, (aperture.Width - gap) / 2);
                var cellHeight = Math.Max(1, (aperture.Height - gap) / 2);
                for (var row = 0; row < 2; row++)
                for (var column = 0; column < 2; column++)
                    dc.DrawRectangle(fill, outline, new Rect(aperture.Left + column * (cellWidth + gap), aperture.Top + row * (cellHeight + gap), cellWidth, cellHeight));
                break;
            case ApertureShapeType.HomePlate:
            case ApertureShapeType.InvertedHomePlate:
                var inset = aperture.Width * .18;
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(new Point(aperture.Left + inset, aperture.Top), true, true);
                    context.LineTo(new Point(aperture.Right, aperture.Top), true, false);
                    context.LineTo(new Point(aperture.Right, aperture.Bottom), true, false);
                    context.LineTo(new Point(aperture.Left + inset, aperture.Bottom), true, false);
                    context.LineTo(new Point(aperture.Left, aperture.Top + aperture.Height / 2), true, false);
                }
                dc.DrawGeometry(fill, outline, geometry);
                break;
            default:
                dc.DrawRectangle(fill, outline, aperture);
                break;
        }
    }
}
