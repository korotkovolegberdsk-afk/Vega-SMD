using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests;

public class ApertureStrategyEngineTests
{
    private readonly ApertureStrategySelectorService _strategySelector = new();
    private readonly ApertureShapeSelectorService _shapeSelector = new();

    [Fact]
    public void R0603_Standard_SelectsStandardStrategyAndRectangle()
    {
        var (strategy, shape) = Select("R0603", 0.8, 0, new ProcessCondition());

        Assert.Equal(ApertureStrategy.StandardPasteRelease, strategy);
        Assert.Equal(ApertureShapeType.Rectangle, shape);
    }

    [Fact]
    public void R0603_AntiSolderBall_SelectsSnubnose()
    {
        var (strategy, shape) = Select("R0603", 0.8, 0, new ProcessCondition
        {
            DefectRisks = [StencilDefectType.SolderBall]
        });

        Assert.Equal(ApertureStrategy.AntiSolderBall, strategy);
        Assert.Equal(ApertureShapeType.Snubnose, shape);
    }

    [Fact]
    public void Qfp_FinePitch_SelectsHomePlate()
    {
        var (strategy, shape) = Select("QFP-64", 0.5, 0, new ProcessCondition { IsFinePitch = true });

        Assert.Equal(ApertureStrategy.FinePitch, strategy);
        Assert.Equal(ApertureShapeType.HomePlate, shape);
    }

    [Fact]
    public void Melf_SelectsMelfShape()
    {
        var (strategy, shape) = Select("MELF-1206", 1, 0, new ProcessCondition());

        Assert.Equal(ApertureStrategy.StandardPasteRelease, strategy);
        Assert.Equal(ApertureShapeType.MELF, shape);
    }

    [Fact]
    public void QfnThermal_SelectsThermalPadAndWindowPaneWithoutChangingGerberPrimitive()
    {
        var sourcePrimitive = new PastePrimitive { X = 12, Y = 8, Width = 6, Height = 6 };
        var before = (sourcePrimitive.X, sourcePrimitive.Y, sourcePrimitive.Width, sourcePrimitive.Height);
        var (strategy, shape) = Select("QFN-32", 0.5, 1, new ProcessCondition { HasThermalPad = true });

        Assert.Equal(ApertureStrategy.ThermalPad, strategy);
        Assert.Equal(ApertureShapeType.Array, shape);
        Assert.Equal(before, (sourcePrimitive.X, sourcePrimitive.Y, sourcePrimitive.Width, sourcePrimitive.Height));
    }

    private (ApertureStrategy Strategy, ApertureShapeType Shape) Select(
        string packageName,
        double pitch,
        int thermalPadCount,
        ProcessCondition condition)
    {
        var package = new PackageDefinition
        {
            PackageName = packageName,
            Pitch = pitch,
            ThermalPadCount = thermalPadCount
        };
        var component = new ComponentDefinition { ComponentType = "IC" };
        var rule = new StencilRecommendationRule { ApertureShape = ApertureShape.Rectangle };
        var strategy = _strategySelector.SelectStrategy(component, package, condition);
        return (strategy, _shapeSelector.SelectShape(package, component, rule, strategy));
    }
}
