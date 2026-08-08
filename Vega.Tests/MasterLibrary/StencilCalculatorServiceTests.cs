using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class StencilCalculatorServiceTests
{
    private readonly StencilCalculatorService _service = new();

    [Fact]
    public void Calculate_Rectangle_Should_Return_Expected_Engineering_Values()
    {
        var result = _service.Calculate(
            padLength: 1.5,
            padWidth: 0.6,
            stencilThickness: 0.12,
            reductionPercent: 10,
            apertureShape: ApertureShape.Rectangle);

        Assert.Equal(1.35, result.ApertureLength, 6);
        Assert.Equal(0.54, result.ApertureWidth, 6);
        Assert.Equal(1.607142857, result.AreaRatio, 6);
        Assert.Equal(4.5, result.AspectRatio, 6);
        Assert.Equal(ApertureShape.Rectangle, result.ApertureShape);
        Assert.Equal(CalculationStatus.Good, result.CalculationStatus);
    }

    [Theory]
    [InlineData(ApertureShape.Rectangle)]
    [InlineData(ApertureShape.RoundedRectangle)]
    [InlineData(ApertureShape.Circle)]
    [InlineData(ApertureShape.Square)]
    public void Calculate_Should_Support_All_Aperture_Shapes(ApertureShape shape)
    {
        var result = _service.Calculate(1.5, 0.6, 0.12, 10, shape);

        Assert.Equal(shape, result.ApertureShape);
        Assert.True(result.AreaRatio > 0);
        Assert.Equal(4.5, result.AspectRatio, 6);
    }

    [Fact]
    public void Calculate_Should_Return_Fail_For_Insufficient_Ratios()
    {
        var result = _service.Calculate(0.2, 0.2, 0.2, 0);

        Assert.Equal(CalculationStatus.Fail, result.CalculationStatus);
    }

    [Fact]
    public void Calculate_Should_Reject_Invalid_Reduction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _service.Calculate(1.5, 0.6, 0.12, 100));
    }
}