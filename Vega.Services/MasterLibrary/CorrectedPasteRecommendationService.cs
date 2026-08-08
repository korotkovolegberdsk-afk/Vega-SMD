using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.PnP.Models;

namespace Vega.Services.MasterLibrary;

public class CorrectedPasteRecommendationService
{
    public CorrectedPasteRecommendation Create(
        ComponentPastePattern pattern,
        StencilAnalysisResult analysis,
        StencilRecommendationRule rule)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(rule);

        if (analysis.Status == "OK")
        {
            return new CorrectedPasteRecommendation
            {
                RefDes = pattern.RefDes,
                OriginalPattern = pattern,
                RecommendedPattern = pattern,
                Reason = "Фактический паттерн соответствует технологическим правилам.",
                Status = "NoChange"
            };
        }

        var scaleX = 1 - rule.ReductionX / 100d;
        var scaleY = 1 - rule.ReductionY / 100d;
        var correctedPrimitives = pattern.PastePrimitives
            .Select(primitive => CreateCorrectedPrimitive(primitive, scaleX, scaleY))
            .ToList();
        var recommendedPattern = CreatePattern(pattern, correctedPrimitives);
        var changes = new List<string>();

        if (rule.ReductionX != 0 || rule.ReductionY != 0)
        {
            changes.Add(
                $"Размеры апертур: X {rule.ReductionX:0.###}% и Y {rule.ReductionY:0.###}%.");
        }

        if (analysis.Warnings.Any(x => x.Contains("Тепловая площадка"))
            && !string.IsNullOrWhiteSpace(rule.ThermalPadRule))
        {
            changes.Add("Thermal pad: " + rule.ThermalPadRule);
        }

        return new CorrectedPasteRecommendation
        {
            RefDes = pattern.RefDes,
            OriginalPattern = pattern,
            RecommendedPattern = recommendedPattern,
            Changes = changes,
            Reason = analysis.Warnings.Count == 0
                ? "Применено технологическое правило reduction."
                : string.Join(" ", analysis.Warnings),
            Status = "Recommended"
        };
    }

    private static PastePrimitive CreateCorrectedPrimitive(
        PastePrimitive primitive,
        double scaleX,
        double scaleY)
    {
        var width = primitive.Width * scaleX;
        var height = primitive.Height * scaleY;

        return new PastePrimitive
        {
            X = primitive.X,
            Y = primitive.Y,
            Rotation = primitive.Rotation,
            ApertureId = primitive.ApertureId,
            Width = width,
            Height = height,
            Area = width * height,
            Perimeter = 2 * (width + height)
        };
    }

    private static ComponentPastePattern CreatePattern(
        ComponentPastePattern source,
        IReadOnlyList<PastePrimitive> primitives)
    {
        if (primitives.Count == 0)
        {
            return new ComponentPastePattern
            {
                RefDes = source.RefDes,
                PackageName = source.PackageName,
                Rotation = source.Rotation
            };
        }

        var minX = primitives.Min(x => x.X - x.Width / 2);
        var maxX = primitives.Max(x => x.X + x.Width / 2);
        var minY = primitives.Min(x => x.Y - x.Height / 2);
        var maxY = primitives.Max(x => x.Y + x.Height / 2);

        return new ComponentPastePattern
        {
            RefDes = source.RefDes,
            PackageName = source.PackageName,
            PastePrimitives = primitives,
            PadCount = primitives.Count,
            TotalArea = primitives.Sum(x => x.Area),
            MinX = minX,
            MaxX = maxX,
            MinY = minY,
            MaxY = maxY,
            CenterX = (minX + maxX) / 2,
            CenterY = (minY + maxY) / 2,
            Width = maxX - minX,
            Height = maxY - minY,
            Rotation = source.Rotation
        };
    }
}
