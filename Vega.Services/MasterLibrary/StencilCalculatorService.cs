using Vega.Models.MasterLibrary;

namespace Vega.Services.MasterLibrary;

public class StencilCalculatorService
{
    public StencilCalculationResult Calculate(
        double padLength,
        double padWidth,
        double stencilThickness,
        double reductionPercent,
        ApertureShape apertureShape = ApertureShape.Rectangle)
    {
        if (padLength <= 0 || padWidth <= 0 || stencilThickness <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(padLength),
                "Размеры площадки и толщина трафарета должны быть больше нуля.");
        }

        if (reductionPercent < 0 || reductionPercent >= 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reductionPercent),
                "Уменьшение апертуры должно быть в диапазоне от 0 до 100 процентов.");
        }

        var scale = 1 - reductionPercent / 100d;
        var apertureLength = padLength * scale;
        var apertureWidth = padWidth * scale;
        var minimumDimension = Math.Min(apertureLength, apertureWidth);
        var (area, perimeter) = CalculateShapeMetrics(
            apertureLength,
            apertureWidth,
            minimumDimension,
            apertureShape);

        // Area Ratio = площадь апертуры / (периметр апертуры × толщина трафарета).
        var areaRatio = area / (perimeter * stencilThickness);

        // Aspect Ratio = минимальный размер апертуры / толщина трафарета.
        var aspectRatio = minimumDimension / stencilThickness;
        var status = GetStatus(areaRatio, aspectRatio);

        return new StencilCalculationResult
        {
            ApertureLength = apertureLength,
            ApertureWidth = apertureWidth,
            AreaRatio = areaRatio,
            AspectRatio = aspectRatio,
            ApertureShape = apertureShape,
            CalculationStatus = status,
            Recommendation = GetRecommendation(status)
        };
    }

    private static (double Area, double Perimeter) CalculateShapeMetrics(
        double length,
        double width,
        double minimumDimension,
        ApertureShape shape)
    {
        return shape switch
        {
            ApertureShape.Rectangle =>
                (length * width, 2 * (length + width)),
            ApertureShape.Circle =>
                (Math.PI * minimumDimension * minimumDimension / 4,
                 Math.PI * minimumDimension),
            ApertureShape.Square =>
                (minimumDimension * minimumDimension, 4 * minimumDimension),
            ApertureShape.RoundedRectangle => RoundedRectangleMetrics(
                length,
                width,
                minimumDimension / 4),
            _ => throw new ArgumentOutOfRangeException(nameof(shape))
        };
    }

    private static (double Area, double Perimeter) RoundedRectangleMetrics(
        double length,
        double width,
        double cornerRadius)
    {
        var area = length * width
            - (4 - Math.PI) * cornerRadius * cornerRadius;
        var perimeter = 2 * (length + width)
            - (8 - 2 * Math.PI) * cornerRadius;

        return (area, perimeter);
    }

    private static CalculationStatus GetStatus(double areaRatio, double aspectRatio)
    {
        if (areaRatio >= 0.66 && aspectRatio >= 1.5)
        {
            return CalculationStatus.Good;
        }

        if (areaRatio >= 0.5 && aspectRatio >= 1.2)
        {
            return CalculationStatus.Warning;
        }

        return CalculationStatus.Fail;
    }

    private static string GetRecommendation(CalculationStatus status)
    {
        return status switch
        {
            CalculationStatus.Good =>
                "Параметры апертуры соответствуют рекомендуемым соотношениям.",
            CalculationStatus.Warning =>
                "Проверьте перенос пасты: соотношения близки к предельным.",
            _ =>
                "Увеличьте апертуру или уменьшите толщину трафарета."
        };
    }
}