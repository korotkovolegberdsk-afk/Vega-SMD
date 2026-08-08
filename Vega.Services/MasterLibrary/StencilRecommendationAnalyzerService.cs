using Vega.Models.MasterLibrary;
using Vega.PnP.Models;

namespace Vega.Services.MasterLibrary;

public class StencilRecommendationAnalyzerService
{
    private const double AreaTolerance = 0.15;
    private const double DimensionTolerance = 0.15;
    private readonly PackageDefinitionService _packageService;

    public StencilRecommendationAnalyzerService(
        PackageDefinitionService? packageService = null)
    {
        _packageService = packageService ?? new PackageDefinitionService();
    }

    public StencilAnalysisResult Analyze(
        ComponentPastePattern pattern,
        PackageDefinition package,
        StencilRecommendationRule rule)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(rule);

        var warnings = new List<string>();
        var recommendations = new List<string>();
        var footprint = _packageService.GetFootprint(package.Id);
        var currentArea = pattern.TotalArea;
        var expectedPadCount = footprint?.PadCount > 0
            ? footprint.PadCount
            : package.PadCount;
        var expectedArea = currentArea;

        if (footprint is null || footprint.PadLength <= 0 || footprint.PadWidth <= 0)
        {
            warnings.Add("В MasterLibrary отсутствует посадочное место для расчёта ожидаемой площади.");
        }
        else
        {
            var reductionX = 1 - rule.ReductionX / 100d;
            var reductionY = 1 - rule.ReductionY / 100d;
            expectedArea = footprint.PadCount
                * footprint.PadLength * reductionX
                * footprint.PadWidth * reductionY;

            if (!WithinTolerance(currentArea, expectedArea, AreaTolerance))
            {
                warnings.Add("Площадь Gerber Paste не соответствует рекомендуемому уменьшению апертур.");
            }

            CheckApertureDimensions(
                pattern,
                footprint.PadLength * reductionX,
                footprint.PadWidth * reductionY,
                warnings);
        }

        if (expectedPadCount > 0 && pattern.PadCount != expectedPadCount)
        {
            warnings.Add("Количество paste-площадок не соответствует посадочному месту.");
        }

        foreach (var primitive in pattern.PastePrimitives)
        {
            var areaRatio = primitive.Perimeter <= 0
                ? 0
                : primitive.Area / (primitive.Perimeter * rule.RecommendedStencilThickness);
            var aspectRatio = Math.Min(primitive.Width, primitive.Height)
                / rule.RecommendedStencilThickness;

            if (areaRatio < rule.AreaRatioMinimum)
            {
                warnings.Add("Area Ratio одной или нескольких апертур ниже технологического минимума.");
                break;
            }

            if (aspectRatio < rule.AspectRatioMinimum)
            {
                warnings.Add("Aspect Ratio одной или нескольких апертур ниже технологического минимума.");
                break;
            }
        }

        if (package.ThermalPadCount > 0 && !string.IsNullOrWhiteSpace(rule.ThermalPadRule))
        {
            warnings.Add("Тепловая площадка требует отдельной проверки: " + rule.ThermalPadRule);
        }

        var status = GetStatus(warnings, expectedPadCount, pattern.PadCount);
        if (status == "OK")
        {
            recommendations.Add("Gerber Paste Pattern соответствует правилам MasterLibrary.");
        }
        else
        {
            recommendations.Add("Проверьте размеры апертур, reduction и технологические ограничения.");
        }

        return new StencilAnalysisResult
        {
            RefDes = pattern.RefDes,
            PackageName = package.PackageName,
            CurrentPasteArea = currentArea,
            ExpectedPasteArea = expectedArea,
            PadCount = pattern.PadCount,
            Status = status,
            Warnings = warnings,
            Recommendations = recommendations
        };
    }

    private static void CheckApertureDimensions(
        ComponentPastePattern pattern,
        double expectedLength,
        double expectedWidth,
        List<string> warnings)
    {
        foreach (var primitive in pattern.PastePrimitives)
        {
            var directMatch = WithinTolerance(primitive.Width, expectedLength, DimensionTolerance)
                && WithinTolerance(primitive.Height, expectedWidth, DimensionTolerance);
            var rotatedMatch = WithinTolerance(primitive.Width, expectedWidth, DimensionTolerance)
                && WithinTolerance(primitive.Height, expectedLength, DimensionTolerance);

            if (!directMatch && !rotatedMatch)
            {
                warnings.Add("Размеры апертур не соответствуют рекомендуемому reduction.");
                return;
            }
        }
    }

    private static bool WithinTolerance(double actual, double expected, double tolerance)
    {
        return expected > 0 && Math.Abs(actual - expected) / expected <= tolerance;
    }

    private static string GetStatus(
        IReadOnlyCollection<string> warnings,
        int expectedPadCount,
        int actualPadCount)
    {
        if (expectedPadCount > 0 && actualPadCount != expectedPadCount)
        {
            return "FAIL";
        }

        return warnings.Count == 0 ? "OK" : "WARNING";
    }
}
