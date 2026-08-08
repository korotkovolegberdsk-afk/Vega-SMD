using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class ApertureOptimizationService
{
    private const double SmallApertureDimension = 0.6;
    private const double LargeApertureArea = 4;
    private const double TargetWindowPaneSize = 2;
    private const double DefaultWebWidth = 0.3;

    public OptimizedAperturePattern Optimize(
        PastePrimitive primitive,
        StencilRecommendationRule rule)
    {
        ArgumentNullException.ThrowIfNull(primitive);
        ArgumentNullException.ThrowIfNull(rule);

        if (primitive.Width <= 0 || primitive.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(primitive),
                "Р Р°Р·РјРµСЂС‹ Р°РїРµСЂС‚СѓСЂС‹ РґРѕР»Р¶РЅС‹ Р±С‹С‚СЊ Р±РѕР»СЊС€Рµ РЅСѓР»СЏ.");
        }

        var isQfnThermalPad = rule.PackageFamily.Equals(
                "QFN",
                StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(rule.ThermalPadRule);
        var area = primitive.Width * primitive.Height;

        if (isQfnThermalPad || area >= LargeApertureArea)
        {
            return CreateWindowPane(primitive, isQfnThermalPad);
        }

        if (Math.Min(primitive.Width, primitive.Height) <= SmallApertureDimension)
        {
            return CreateRectangleReduction(primitive, rule);
        }

        return new OptimizedAperturePattern
        {
            PatternType = AperturePatternType.Single,
            ApertureShapeType = ApertureShapeType.Rectangle,
            ApertureGeometry = new ApertureGeometry { ShapeType = ApertureShapeType.Rectangle, Width = primitive.Width, Height = primitive.Height },
            OriginalWidth = primitive.Width,
            OriginalHeight = primitive.Height,
            RecommendedWidth = primitive.Width,
            RecommendedHeight = primitive.Height,
            Rows = 1,
            Columns = 1,
            CoveragePercent = 100,
            Reason = "Р Р°Р·РјРµСЂ Р°РїРµСЂС‚СѓСЂС‹ РЅРµ С‚СЂРµР±СѓРµС‚ СЃРµРєС†РёРѕРЅРёСЂРѕРІР°РЅРёСЏ РёР»Рё РґРѕРїРѕР»РЅРёС‚РµР»СЊРЅРѕРіРѕ СѓРјРµРЅСЊС€РµРЅРёСЏ."
        };
    }

    private static OptimizedAperturePattern CreateRectangleReduction(
        PastePrimitive primitive,
        StencilRecommendationRule rule)
    {
        var width = primitive.Width * (1 - rule.ReductionX / 100d);
        var height = primitive.Height * (1 - rule.ReductionY / 100d);

        return new OptimizedAperturePattern
        {
            PatternType = AperturePatternType.RectangleReduction,
            ApertureShapeType = ApertureShapeType.Rectangle,
            ApertureGeometry = new ApertureGeometry { ShapeType = ApertureShapeType.Rectangle, Width = width, Height = height },
            OriginalWidth = primitive.Width,
            OriginalHeight = primitive.Height,
            RecommendedWidth = width,
            RecommendedHeight = height,
            Rows = 1,
            Columns = 1,
            CoveragePercent = width * height / (primitive.Width * primitive.Height) * 100,
            Reason = "РњР°Р»РµРЅСЊРєР°СЏ Р°РїРµСЂС‚СѓСЂР°: РїСЂРёРјРµРЅРµРЅРѕ РїСЂСЏРјРѕСѓРіРѕР»СЊРЅРѕРµ СѓРјРµРЅСЊС€РµРЅРёРµ РґР»СЏ СЃС‚Р°Р±РёР»СЊРЅРѕРіРѕ РїРµСЂРµРЅРѕСЃР° РїР°СЃС‚С‹."
        };
    }

    private static OptimizedAperturePattern CreateWindowPane(
        PastePrimitive primitive,
        bool isQfnThermalPad)
    {
        var columns = isQfnThermalPad
            ? Math.Max(2, (int)Math.Ceiling(primitive.Width / TargetWindowPaneSize))
            : Math.Max(2, (int)Math.Ceiling(primitive.Width / TargetWindowPaneSize));
        var rows = isQfnThermalPad
            ? Math.Max(2, (int)Math.Ceiling(primitive.Height / TargetWindowPaneSize))
            : Math.Max(2, (int)Math.Ceiling(primitive.Height / TargetWindowPaneSize));
        var width = (primitive.Width - (columns - 1) * DefaultWebWidth) / columns;
        var height = (primitive.Height - (rows - 1) * DefaultWebWidth) / rows;
        var coverage = rows * columns * width * height
            / (primitive.Width * primitive.Height) * 100;

        return new OptimizedAperturePattern
        {
            PatternType = AperturePatternType.WindowPane,
            ApertureShapeType = ApertureShapeType.Array,
            ApertureGeometry = new ApertureGeometry { ShapeType = ApertureShapeType.Array, Width = primitive.Width, Height = primitive.Height, Rows = rows, Columns = columns, WebWidth = DefaultWebWidth, Coverage = coverage },
            OriginalWidth = primitive.Width,
            OriginalHeight = primitive.Height,
            RecommendedWidth = width,
            RecommendedHeight = height,
            Rows = rows,
            Columns = columns,
            WebWidth = DefaultWebWidth,
            CoveragePercent = coverage,
            Reason = isQfnThermalPad
                ? "QFN thermal pad: СЂРµРєРѕРјРµРЅРґСѓРµС‚СЃСЏ window-pane РґР»СЏ СЃРЅРёР¶РµРЅРёСЏ voiding."
                : "Р‘РѕР»СЊС€Р°СЏ Р°РїРµСЂС‚СѓСЂР°: СЂРµРєРѕРјРµРЅРґСѓРµС‚СЃСЏ window-pane РґР»СЏ РєРѕРЅС‚СЂРѕР»РёСЂСѓРµРјРѕРіРѕ РѕР±СЉС‘РјР° РїР°СЃС‚С‹."
        };
    }
}

