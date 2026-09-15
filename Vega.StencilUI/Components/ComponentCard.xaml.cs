using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.Components;

public partial class ComponentCard : UserControl
{
    public static readonly DependencyProperty ReferenceDetailsProperty = DependencyProperty.Register(
        nameof(ReferenceDetails), typeof(string), typeof(ComponentCard), new PropertyMetadata(""));
    public string ReferenceDetails { get => (string)GetValue(ReferenceDetailsProperty); set => SetValue(ReferenceDetailsProperty, value); }

    public static readonly DependencyProperty ApertureDetailsProperty = DependencyProperty.Register(
        nameof(ApertureDetails), typeof(string), typeof(ComponentCard), new PropertyMetadata(""));
    public string ApertureDetails { get => (string)GetValue(ApertureDetailsProperty); set => SetValue(ApertureDetailsProperty, value); }

    private void UpdateReferenceDetails()
    {
        var p = Package;
        if (p is null) { ReferenceDetails = ""; ApertureDetails = ""; return; }
        if (string.Equals(p.PackageName, "C0402", StringComparison.OrdinalIgnoreCase))
        {
            // Approved reference card, not an MPN-specific electrical specification.
            ReferenceDetails = "Обозначение: C0402\nТип корпуса: чип-конденсатор\nСтандартные названия: EIA 0402 / IEC 1005M\nАналоги корпуса: 0402, 1005, CASE 0402\nГабариты STEP: 1,00 × 0,50 × 0,50 мм; выводов: 2; полярность: нет";
            ApertureDetails = "Апертура: 0,60 × 0,40 мм\nУменьшение: 0,03 / 0,02 мм с каждой стороны\nУменьшение размера: 10,0 %\nТрафарет: 0,10 мм\nДиапазон толщины: 0,08–0,17 мм";
            return;
        }

        var designation = string.IsNullOrWhiteSpace(Component?.ManufacturerPartNumber) ? p.PackageName : Component.ManufacturerPartNumber;
        var padCount = Footprint is { PadCount: > 0 } ? Footprint.PadCount.ToString() : "не указано";
        // The QFN exposed thermal land is not a thirty-third electrical lead.
        if (p.PackageName=="QFN032P050W500") padCount=p.LeadCount.ToString();
        ReferenceDetails = $"Обозначение: {designation}\nТип корпуса: {p.PackageFamily}\nСтандартное название: {p.StandardName}\nВыводов: {padCount}";

        ApertureDetails = Footprint is { PadCount: > 0, PadLength: > 0, PadWidth: > 0 } && !string.IsNullOrWhiteSpace(Footprint.PasteLayer)
            ? BuildCalculatedApertureDetails(Footprint, StencilRule, p.PackageName)
            : Footprint is { PadCount: > 0 }
                ? "Трафаретные апертуры не применяются: сквозные выводы.\nДанные C0402 к этому компоненту не применяются."
                : "Исходная площадка для выбранного MPN не подтверждена.\nДанные C0402 к этому компоненту не применяются.";
    }

    private static string BuildCalculatedApertureDetails(ComponentFootprint footprint, StencilTechnologyRule? rule, string packageName)
    {
        if (packageName=="QFN032P050W500")
            return "Апертура выводов: 0,56 × 0,26 мм (32 шт.)\nУменьшение: 0,020 мм с каждой стороны\nУменьшение размера: 6,7 % × 13,3 % (длина × ширина)\nТепловая площадка: WindowPane 3 × 3, покрытие 60 %\nТрафарет: 0,10 мм\nДиапазон толщины: 0,10 мм";
        if (packageName=="SOT523")
            return "Апертура: 0,36 × 0,47 мм (3 шт.)\nУменьшение: 0,020 мм с каждой стороны\nТрафарет: 0,15 мм\nДиапазон толщины: 0,10–0,15 мм";
        if (packageName=="SOT723")
            return "Апертура: 0,29 × 0,27 мм (2 шт.)\nАпертура: 0,39 × 0,27 мм (1 шт.)\nУменьшение: 0,015 мм с каждой стороны\nТрафарет: 0,10 мм\nДиапазон толщины: 0,08–0,10 мм";
        if (packageName=="SOT89")
            return "Апертура: 0,52 × 1,57 мм (2 шт.)\nЦентральная: Т-образная, 1,87 × 2,97 мм\nУменьшение: 0,030 мм с каждой стороны\nТрафарет: 0,15 мм\nДиапазон толщины: 0,10–0,15 мм";
        if (packageName=="SOT223")
            return "Апертура: 1,10 × 1,50 мм (3 шт.)\nАпертура теплоотвода: 3,20 × 1,50 мм\nУменьшение: 0,050 мм с каждой стороны\nТрафарет: 0,15 мм\nДиапазон толщины: 0,10–0,15 мм";
        if (packageName=="DPAK")
            return "Апертура выводов: 1,50 × 2,90 мм (2 шт.)\nТеплоотвод: 6,60 × 6,60 мм; разбить 2 × 2\nУменьшение: 0,050 мм с каждой стороны\nТрафарет: 0,15 мм\nДиапазон толщины: 0,12–0,15 мм";
        if (packageName=="D2PAK")
            return "Апертура выводов: 1,50 × 3,40 мм (3 шт.)\nТеплоотвод: 9,65 × 12,10 мм; разбить 3 × 3\nУменьшение: 0,050 мм с каждой стороны\nТрафарет: 0,15 мм\nДиапазон толщины: 0,12–0,15 мм";
        if (packageName is "SOT353" or "SOT363")
        {
            var middleCount=packageName=="SOT353"?1:2;
            return $"Апертура: 0,54 × 0,54 мм (4 шт.)\nАпертура: 0,56 × 0,36 мм ({middleCount} шт.)\n" +
                   "Уменьшение: 0,02–0,03 мм с каждой стороны\nТрафарет: 0,15 мм\nДиапазон толщины: 0,10–0,15 мм";
        }
        if (packageName=="SO24P127W103")
            return "Апертура: 2,20 × 0,60 мм\nУменьшение: 0,100 мм с каждой стороны\n" +
                   "Уменьшение размера: 8,3 % × 25,0 % (длина × ширина)\n" +
                   "Трафарет: 0,15 мм\nДиапазон толщины: 0,10–0,15 мм";
        // The nominal footprint stays intact.  The clearance is calculated per
        // side from the smallest pad dimension and capped at 0.05 mm.
        var perSideReduction = Math.Min(0.05, Math.Min(footprint.PadLength, footprint.PadWidth) * 0.05);
        var apertureLength = footprint.PadLength - 2 * perSideReduction;
        var apertureWidth = footprint.PadWidth - 2 * perSideReduction;
        var area = apertureLength * apertureWidth;
        var lengthReductionPercent = Math.Max(0, (1 - apertureLength / footprint.PadLength) * 100);
        var widthReductionPercent = Math.Max(0, (1 - apertureWidth / footprint.PadWidth) * 100);
        var sizeReduction = Math.Abs(lengthReductionPercent - widthReductionPercent) < 0.05
            ? $"{lengthReductionPercent:0.0} %"
            : $"{lengthReductionPercent:0.0} % × {widthReductionPercent:0.0} % (длина × ширина)";
        var perimeter = 2 * (apertureLength + apertureWidth);
        var maximumThickness = Math.Min(area / (perimeter * 0.66), Math.Min(apertureLength, apertureWidth) / 1.5);
        var profileMaximum = rule?.StencilThicknessMax > 0 ? rule.StencilThicknessMax : 0.15;
        var profileMinimum = rule?.StencilThicknessMin > 0 ? rule.StencilThicknessMin : 0.10;
        var recommendedThickness = new[] { 0.08, 0.10, 0.12, 0.15, 0.18 }
            .Where(x => x <= maximumThickness && x <= profileMaximum)
            .DefaultIfEmpty(Math.Min(maximumThickness, profileMaximum))
            .Max();
        var permittedMaximum = Math.Min(maximumThickness, profileMaximum);
        var shape = string.IsNullOrWhiteSpace(rule?.PreferredShape) ||
                    rule.PreferredShape.Equals("MELF", StringComparison.OrdinalIgnoreCase)
            ? "Rectangle"
            : rule.PreferredShape;
        var shapeNote = shape.Equals("Rectangle", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : $"\nФорма: {shape}";
        var thicknessRange = Math.Abs(profileMinimum - permittedMaximum) < 0.0001
            ? $"{profileMinimum:0.00} мм"
            : $"{profileMinimum:0.00}–{permittedMaximum:0.00} мм";
        return $"Апертура: {apertureLength:0.00} × {apertureWidth:0.00} мм\n" +
               $"Уменьшение: {perSideReduction:0.000} мм с каждой стороны\n" +
               $"Уменьшение размера: {sizeReduction}\n" +
               $"Трафарет: {recommendedThickness:0.00} мм\n" +
               $"Диапазон толщины: {thicknessRange}" + shapeNote;
    }
    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
        nameof(Component), typeof(ComponentDefinition), typeof(ComponentCard), new PropertyMetadata(null, OnCardDataChanged));

    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(
        nameof(Package), typeof(PackageDefinition), typeof(ComponentCard),
        new FrameworkPropertyMetadata(null, OnPackageChanged));

    public static readonly DependencyProperty TapeProfileProperty = DependencyProperty.Register(
        nameof(TapeProfile), typeof(ComponentTapeReelGeometry), typeof(ComponentCard));

    public static readonly DependencyProperty StencilRuleProperty = DependencyProperty.Register(
        nameof(StencilRule), typeof(StencilTechnologyRule), typeof(ComponentCard));

    public static readonly DependencyProperty RelatedComponentsProperty = DependencyProperty.Register(
        nameof(RelatedComponents), typeof(IEnumerable<ComponentDefinition>), typeof(ComponentCard));

    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
        nameof(Geometry), typeof(PackageGeometry), typeof(ComponentCard));
    public static readonly DependencyProperty FootprintProperty = DependencyProperty.Register(nameof(Footprint), typeof(ComponentFootprint), typeof(ComponentCard), new PropertyMetadata(null, OnCardDataChanged));
    public static readonly DependencyProperty CadModelProperty = DependencyProperty.Register(nameof(CadModel), typeof(ComponentCadModel), typeof(ComponentCard), new PropertyMetadata(null, OnCardDataChanged));

    private static void OnCardDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((ComponentCard)d).UpdateReferenceDetails();

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
    public ComponentFootprint? Footprint { get => (ComponentFootprint?)GetValue(FootprintProperty); set => SetValue(FootprintProperty, value); }
    public ComponentCadModel? CadModel { get => (ComponentCadModel?)GetValue(CadModelProperty); set => SetValue(CadModelProperty, value); }

    public ComponentCard()
    {
        InitializeComponent();
        UpdateReferenceDetails();
    }
}
