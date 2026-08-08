using Vega.Gerber.Models;

namespace Vega.Services.Gerber;

public class CorrectedPasteLayerBuilderService
{
    public CorrectedPasteLayer Build(
        PasteLayer source,
        List<OptimizedAperturePattern> patterns)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Count != source.Primitives.Count)
        {
            throw new ArgumentException(
                "РљРѕР»РёС‡РµСЃС‚РІРѕ РѕРїС‚РёРјРёР·РёСЂРѕРІР°РЅРЅС‹С… РїР°С‚С‚РµСЂРЅРѕРІ РґРѕР»Р¶РЅРѕ СЃРѕРѕС‚РІРµС‚СЃС‚РІРѕРІР°С‚СЊ РєРѕР»РёС‡РµСЃС‚РІСѓ РёСЃС…РѕРґРЅС‹С… РїСЂРёРјРёС‚РёРІРѕРІ.",
                nameof(patterns));
        }

        var correctedPrimitives = new List<PastePrimitive>();
        var changes = new List<PasteCorrectionChange>();

        for (var index = 0; index < source.Primitives.Count; index++)
        {
            var original = source.Primitives[index];
            var pattern = patterns[index];

            if (pattern.PatternType == AperturePatternType.WindowPane)
            {
                var windows = CreateWindowPane(original, pattern);
                correctedPrimitives.AddRange(windows);
                changes.Add(CreateChange(
                    "ThermalPadSegmentation",
                    pattern.RefDes,
                    original,
                    $"WindowPane {pattern.Rows}x{pattern.Columns}",
                    pattern.Reason));
                continue;
            }

            var corrected = CreateReducedPrimitive(original, pattern);
            correctedPrimitives.Add(corrected);

            if (pattern.PatternType != AperturePatternType.Single)
            {
                changes.Add(CreateChange(
                    "ApertureResize",
                    pattern.RefDes,
                    original,
                    $"{pattern.RecommendedWidth:0.###}x{pattern.RecommendedHeight:0.###}",
                    pattern.Reason));
            }
        }

        return new CorrectedPasteLayer
        {
            OriginalFileName = source.FileName,
            Side = source.Side,
            OriginalPrimitiveCount = source.Primitives.Count,
            CorrectedPrimitiveCount = correctedPrimitives.Count,
            OriginalLayer = source,
            CorrectedPrimitives = correctedPrimitives,
            Changes = changes
        };
    }

    private static PastePrimitive CreateReducedPrimitive(
        PastePrimitive original,
        OptimizedAperturePattern pattern)
    {
        var width = pattern.PatternType == AperturePatternType.Single
            ? original.Width
            : pattern.RecommendedWidth;
        var height = pattern.PatternType == AperturePatternType.Single
            ? original.Height
            : pattern.RecommendedHeight;

        return new PastePrimitive
        {
            X = original.X,
            Y = original.Y,
            Rotation = original.Rotation,
            ApertureId = original.ApertureId,
            Width = width,
            Height = height,
            Area = width * height,
            Perimeter = 2 * (width + height)
        };
    }

    private static List<PastePrimitive> CreateWindowPane(
        PastePrimitive original,
        OptimizedAperturePattern pattern)
    {
        var result = new List<PastePrimitive>();
        var totalWidth = pattern.Columns * pattern.RecommendedWidth
            + (pattern.Columns - 1) * pattern.WebWidth;
        var totalHeight = pattern.Rows * pattern.RecommendedHeight
            + (pattern.Rows - 1) * pattern.WebWidth;
        var rotationRadians = original.Rotation * Math.PI / 180;
        var cosine = Math.Cos(rotationRadians);
        var sine = Math.Sin(rotationRadians);

        for (var row = 0; row < pattern.Rows; row++)
        {
            for (var column = 0; column < pattern.Columns; column++)
            {
                var localX = -totalWidth / 2
                    + pattern.RecommendedWidth / 2
                    + column * (pattern.RecommendedWidth + pattern.WebWidth);
                var localY = -totalHeight / 2
                    + pattern.RecommendedHeight / 2
                    + row * (pattern.RecommendedHeight + pattern.WebWidth);
                var x = original.X + localX * cosine - localY * sine;
                var y = original.Y + localX * sine + localY * cosine;

                result.Add(new PastePrimitive
                {
                    X = x,
                    Y = y,
                    Rotation = original.Rotation,
                    ApertureId = original.ApertureId,
                    Width = pattern.RecommendedWidth,
                    Height = pattern.RecommendedHeight,
                    Area = pattern.RecommendedWidth * pattern.RecommendedHeight,
                    Perimeter = 2 * (pattern.RecommendedWidth + pattern.RecommendedHeight)
                });
            }
        }

        return result;
    }

    private static PasteCorrectionChange CreateChange(
        string changeType,
        string refDes,
        PastePrimitive original,
        string newGeometry,
        string reason)
    {
        return new PasteCorrectionChange
        {
            RefDes = refDes,
            ChangeType = changeType,
            OriginalGeometry = $"{original.Width:0.###}x{original.Height:0.###} Solid",
            NewGeometry = newGeometry,
            Reason = reason
        };
    }
}


