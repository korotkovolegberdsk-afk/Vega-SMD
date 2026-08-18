using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.Components;

public partial class ComponentCard : UserControl
{
    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
        nameof(Component), typeof(ComponentDefinition), typeof(ComponentCard));

    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(
        nameof(Package), typeof(PackageDefinition), typeof(ComponentCard));

    public static readonly DependencyProperty TapeProfileProperty = DependencyProperty.Register(
        nameof(TapeProfile), typeof(ComponentTapeReelGeometry), typeof(ComponentCard));

    public static readonly DependencyProperty StencilRuleProperty = DependencyProperty.Register(
        nameof(StencilRule), typeof(StencilTechnologyRule), typeof(ComponentCard));

    public static readonly DependencyProperty RelatedComponentsProperty = DependencyProperty.Register(
        nameof(RelatedComponents), typeof(IEnumerable<ComponentDefinition>), typeof(ComponentCard));

    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
        nameof(Geometry), typeof(PackageGeometry), typeof(ComponentCard));

    public ComponentDefinition? Component
    {
        get => (ComponentDefinition?)GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }

    public PackageDefinition? Package
    {
        get => (PackageDefinition?)GetValue(PackageProperty);
        set => SetValue(PackageProperty, value);
    }

    public ComponentTapeReelGeometry? TapeProfile
    {
        get => (ComponentTapeReelGeometry?)GetValue(TapeProfileProperty);
        set => SetValue(TapeProfileProperty, value);
    }

    public StencilTechnologyRule? StencilRule
    {
        get => (StencilTechnologyRule?)GetValue(StencilRuleProperty);
        set => SetValue(StencilRuleProperty, value);
    }

    public IEnumerable<ComponentDefinition>? RelatedComponents
    {
        get => (IEnumerable<ComponentDefinition>?)GetValue(RelatedComponentsProperty);
        set => SetValue(RelatedComponentsProperty, value);
    }

    public PackageGeometry? Geometry
    {
        get => (PackageGeometry?)GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }

    public ComponentCard() => InitializeComponent();
}
