using Vega.Gerber.Models;

namespace Vega.Gerber;

public class PasteAnalyzerService
{
    private const double MinimumApertureSize = 0.2;
    private const double SuspiciousMaximumApertureSize = 10;
    private const double MinimumAreaRatio = 0.66;
    private const double MinimumAspectRatio = 1.5;
    private readonly double _stencilThickness;

    public PasteAnalyzerService(double stencilThickness = 0.12)
    {
        if (stencilThickness <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stencilThickness),
                "Толщина трафарета должна быть больше нуля.");
        }

        _stencilThickness = stencilThickness;
    }

    public PasteAnalysisResult Analyze(PasteLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        var sizes = layer.Apertures
            .SelectMany(x => new[] { x.Width, x.Height })
            .Where(x => x > 0)
            .ToList();
        var warnings = 0;

        foreach (var primitive in layer.Primitives)
        {
            var minimumDimension = Math.Min(primitive.Width, primitive.Height);
            var maximumDimension = Math.Max(primitive.Width, primitive.Height);
            var areaRatio = primitive.Perimeter <= 0
                ? 0
                : primitive.Area / (primitive.Perimeter * _stencilThickness);
            var aspectRatio = minimumDimension / _stencilThickness;

            if (minimumDimension < MinimumApertureSize
                || maximumDimension > SuspiciousMaximumApertureSize
                || areaRatio < MinimumAreaRatio
                || aspectRatio < MinimumAspectRatio)
            {
                warnings++;
            }
        }

        return new PasteAnalysisResult
        {
            PrimitiveCount = layer.Primitives.Count,
            ApertureCount = layer.Apertures.Count,
            ShapeStatistics = layer.Apertures
                .GroupBy(x => x.Shape)
                .ToDictionary(x => x.Key, x => x.Count()),
            MinApertureSize = sizes.Count == 0 ? 0 : sizes.Min(),
            MaxApertureSize = sizes.Count == 0 ? 0 : sizes.Max(),
            WarningCount = warnings
        };
    }
}
