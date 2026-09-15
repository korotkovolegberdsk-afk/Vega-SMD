using System.Windows.Media;

namespace Vega.StencilUI.Controls;

/// <summary>Native light engineering palette shared by component-card previews.</summary>
internal static class ComponentCardPalette
{
    public static readonly Brush Page = Brush("#F5F7F9");
    public static readonly Brush Panel = Brushes.White;
    public static readonly Brush Projection = Brush("#F8FAFB");
    public static readonly Brush Title = Brush("#1769C2");
    public static readonly Brush Text = Brush("#1B1F23");
    public static readonly Brush Dimension = Brush("#3F4850");
    public static readonly Brush Border = Brush("#AEBAC6");
    public static readonly Brush Separator = Brush("#D9E0E6");
    public static readonly Brush Tape = Brush("#ADB8C4");
    public static readonly Brush Pocket = Brush("#E1E7ED");
    public static readonly Brush Aperture = Brush("#D88432");

    private static Brush Brush(string value)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        brush.Freeze();
        return brush;
    }
}
