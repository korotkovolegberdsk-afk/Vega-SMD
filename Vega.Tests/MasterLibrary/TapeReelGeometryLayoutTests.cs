using Vega.Models.MasterLibrary;
using Vega.StencilUI.TapeReelDrawing;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class TapeReelGeometryLayoutTests
{
    [Fact]
    public void EqualP0AndP1_UsesBaselineBetweenFirstAndSecondHole()
    {
        var layout = TapeReelGeometryLayout.Create(new ComponentTapeReelGeometry { SprocketHolePitch = 4, PocketPitch = 4 }, 12, 4);

        Assert.Equal(2, layout.BaseLineX);
        Assert.Equal(new[] { 0d, 4d, 8d, 12d }, layout.HoleXs);
        Assert.Equal(new[] { 2d, 6d, 10d, 14d }, layout.PocketXs);
        Assert.True(layout.HoleXs[0] < layout.BaseLineX && layout.BaseLineX < layout.HoleXs[1]);
        Assert.Equal(layout.BaseLineX, layout.PocketXs[0]);
        Assert.Equal(4, layout.PocketXs[1] - layout.PocketXs[0]);
    }

    [Fact]
    public void DifferentP0AndP1_KeepsBaselineFixedAndUsesPocketPitch()
    {
        var layout = TapeReelGeometryLayout.Create(new ComponentTapeReelGeometry { SprocketHolePitch = 4, PocketPitch = 8 }, 20, 3);

        Assert.Equal(2, layout.BaseLineX);
        Assert.Equal(new[] { 0d, 4d, 8d, 12d, 16d, 20d }, layout.HoleXs);
        Assert.Equal(new[] { 2d, 10d, 18d }, layout.PocketXs);
        Assert.Equal(layout.BaseLineX, layout.PocketXs[0]);
        Assert.Equal(8, layout.PocketXs[1] - layout.PocketXs[0]);
        Assert.Equal(16, layout.PocketXs[2] - layout.PocketXs[0]);
    }

    [Fact]
    public void SprocketHoles_ContinuePastLastPocket_IndependentlyOfPocketCount()
    {
        var layout = TapeReelGeometryLayout.Create(new ComponentTapeReelGeometry { SprocketHolePitch = 4, PocketPitch = 8 }, 24, 3);

        Assert.Equal(2, layout.BaseLineX);
        Assert.Equal(layout.BaseLineX, layout.PocketXs[0]);
        Assert.Equal(new[] { 0d, 4d, 8d, 12d, 16d, 20d, 24d }, layout.HoleXs);
        Assert.True(layout.HoleXs[^1] > layout.PocketXs[^1]);
    }
}
