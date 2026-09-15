using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vega.StencilUI.Controls;
using Vega.StencilUI.TapeReelDrawing;

namespace Vega.StencilUI.Components;

public partial class ComponentCard
{
    static ComponentCard()
    {
        EventManager.RegisterClassHandler(typeof(ComponentCard), FrameworkElement.LoadedEvent, new RoutedEventHandler(ApplyDeterministicDrawingLayout));
    }

    private static void ApplyDeterministicDrawingLayout(object sender, RoutedEventArgs args)
    {
        if (sender is not ComponentCard card) return;
        card.DataContextChanged -= Card_DataContextChanged;
        card.DataContextChanged += Card_DataContextChanged;
        ScheduleLayout(card);
    }

    private static void Card_DataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is ComponentCard card) ScheduleLayout(card);
    }

    private static void OnPackageChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is ComponentCard card) { card.UpdateReferenceDetails(); ScheduleLayout(card); }
    }

    private static void ScheduleLayout(ComponentCard card)
    {
        // The approved component-card template is rendered from the selected
        // component data.  Do not replace an individual package with a
        // pre-rendered drawing: that hides missing verification and breaks the
        // common card layout.
    }

    private static void WrapPackageDrawing(ComponentCard card)
    {
        var group = Descendants(card).OfType<GroupBox>().FirstOrDefault(item => item.Header?.ToString() == "ЧЕРТЁЖ КОРПУСА");
        if (group is null || group.Content is Grid) return;
        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.Children.Add(FixedViewport(Sot23ApprovedImage.CreatePackageDrawing(), 720, 560));
        var rec = new StackPanel { Margin = new Thickness(12, 6, 12, 8) };
        rec.Children.Add(new TextBlock { Text = "РЕКОМЕНДАЦИИ ПО ТРАФАРЕТУ", FontWeight = FontWeights.SemiBold, Foreground = Brushes.RoyalBlue, FontSize = 14 });
        rec.Children.Add(new TextBlock { Text = "Форма апертуры: прямоугольная", Margin = new Thickness(0, 4, 0, 0), Foreground = Brushes.Black });
        rec.Children.Add(new TextBlock { Text = "Толщина: 0,10–0,15 мм    |    рекомендуемая: 0,12 мм", Foreground = Brushes.Black });
        rec.Children.Add(new TextBlock { Text = "Aspect Ratio: 0,60 / 0,12 = 5,0", Foreground = Brushes.Black });
        Grid.SetRow(rec, 1); content.Children.Add(rec); group.Content = content;
    }
    private static void HideRightRecommendations(DependencyObject root)
    {
        foreach (var item in Descendants(root).OfType<TextBlock>())
        {
            if (item.Text.StartsWith("Рекомендация по форме:", StringComparison.Ordinal) || item.Text.StartsWith("Aspect Ratio:", StringComparison.Ordinal) || item.Text.StartsWith("Допустимая толщина", StringComparison.Ordinal))
                item.Visibility = Visibility.Collapsed;
        }
    }

    private static void WrapApertureDrawing(ComponentCard card)
    {
        var drawing = Descendants(card).OfType<StencilAperturePreview>().FirstOrDefault();
        if (drawing?.Parent is not Border border || border.Child is Viewbox) return;
        border.Child = FixedViewport(Sot23ApprovedImage.CreateAperture(), 520, 520);
    }

    private static void WrapTapeDrawing(ComponentCard card)
    {
        var drawing = Descendants(card).OfType<TapeReelPreview>().FirstOrDefault();
        if (drawing?.Parent is not Panel panel) return;
        var index = panel.Children.IndexOf(drawing);
        if (index < 0 || panel.Children[index] is Viewbox) return;
        panel.Children.RemoveAt(index);
        panel.Children.Insert(index, FixedViewport(Sot23ApprovedImage.Create("Sot23-Tape-Approved.png"), 620, 420));
    }

    private static Viewbox FixedViewport(UIElement child, double width, double height) => new()
    {
        MaxWidth = width, MaxHeight = height, Stretch = Stretch.Uniform, StretchDirection = StretchDirection.DownOnly,
        HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Child = child
    };

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
