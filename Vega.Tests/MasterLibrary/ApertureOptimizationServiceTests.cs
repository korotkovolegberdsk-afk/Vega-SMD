using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class ApertureOptimizationServiceTests
{
    private readonly ApertureOptimizationService _service = new();

    [Fact]
    public void Optimize_R0603_Should_Recommend_Rectangle_Reduction()
    {
        var result = _service.Optimize(
            Primitive(0.9, 0.45),
            Rule("CHIP", 10, 10));

        Assert.Equal(AperturePatternType.RectangleReduction, result.PatternType);
        Assert.Equal(0.81, result.RecommendedWidth, 6);
        Assert.Equal(0.405, result.RecommendedHeight, 6);
        Assert.Equal(81, result.CoveragePercent, 6);
    }

    [Fact]
    public void Optimize_Large_Pad_Should_Recommend_Window_Pane()
    {
        var result = _service.Optimize(
            Primitive(5, 4),
            Rule("CHIP", 0, 0));

        Assert.Equal(AperturePatternType.WindowPane, result.PatternType);
        Assert.Equal(2, result.Rows);
        Assert.Equal(3, result.Columns);
        Assert.Equal(0.3, result.WebWidth, 6);
        Assert.True(result.CoveragePercent < 100);
    }

    [Fact]
    public void Optimize_Qfn_ThermalPad_Should_Recommend_Window_Pane()
    {
        var result = _service.Optimize(
            Primitive(2, 2),
            Rule("QFN", 10, 10, "Window-pane thermal pad aperture."));

        Assert.Equal(AperturePatternType.WindowPane, result.PatternType);
        Assert.Equal(2, result.Rows);
        Assert.Equal(2, result.Columns);
        Assert.Contains("QFN thermal pad", result.Reason);
    }

    private static PastePrimitive Primitive(double width, double height) => new()
    {
        Width = width,
        Height = height
    };

    private static StencilRecommendationRule Rule(
        string family,
        double reductionX,
        double reductionY,
        string thermalPadRule = "") => new()
    {
        PackageFamily = family,
        ReductionX = reductionX,
        ReductionY = reductionY,
        ThermalPadRule = thermalPadRule
    };
}
