using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests;

public class ApertureShapeEngineTests
{
    private readonly ApertureShapeSelectorService _selector = new();
    private readonly ApertureShapeGeneratorService _generator = new();

    [Theory]
    [InlineData("R0603", 0.8, 0, "Resistor", ApertureShapeType.Rectangle)]
    [InlineData("QFP-64", 0.5, 0, "IC", ApertureShapeType.HomePlate)]
    [InlineData("MELF-1206", 1.0, 0, "Diode", ApertureShapeType.MELF)]
    [InlineData("BGA-144", 0.8, 0, "IC", ApertureShapeType.Round)]
    [InlineData("QFN-32", 0.5, 1, "IC", ApertureShapeType.Array)]
    public void SelectShape_ChoosesExpectedTechnologyShape(
        string packageName,
        double pitch,
        int thermalPadCount,
        string componentType,
        ApertureShapeType expected)
    {
        var actual = _selector.SelectShape(
            new PackageDefinition
            {
                PackageName = packageName,
                Pitch = pitch,
                ThermalPadCount = thermalPadCount
            },
            new ComponentDefinition { ComponentType = componentType },
            new StencilRecommendationRule { ApertureShape = ApertureShape.Rectangle });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Generate_Rectangle_CreatesSingleGerberCompatiblePrimitive()
    {
        var primitives = _generator.Generate(new ApertureGeometry
        {
            ShapeType = ApertureShapeType.Rectangle,
            Width = 0.81,
            Height = 0.405,
            Rotation = 90
        });

        var primitive = Assert.Single(primitives);
        Assert.Equal(ApertureShapeType.Rectangle, primitive.ShapeType);
        Assert.Equal(0.81, primitive.Width, 6);
        Assert.Equal(0.405, primitive.Height, 6);
        Assert.Equal(90, primitive.Rotation, 6);
    }

    [Fact]
    public void Generate_QfnWindowPane_CreatesArrayWithRequestedCoverageWithoutChangingSource()
    {
        var source = new PastePrimitive { Width = 6, Height = 6, X = 10, Y = 20 };
        var primitiveBefore = (source.Width, source.Height, source.X, source.Y);
        var geometry = new ApertureGeometry
        {
            ShapeType = ApertureShapeType.Array,
            Width = source.Width,
            Height = source.Height,
            Rows = 4,
            Columns = 4,
            WebWidth = 0.3,
            Coverage = 72.25
        };

        var primitives = _generator.Generate(geometry);
        var coverage = primitives.Sum(primitive => primitive.Area) / (source.Width * source.Height) * 100;

        Assert.Equal(16, primitives.Count);
        Assert.All(primitives, primitive => Assert.Equal(ApertureShapeType.Array, primitive.ShapeType));
        Assert.Equal(72.25, coverage, 6);
        Assert.Equal(primitiveBefore, (source.Width, source.Height, source.X, source.Y));
    }
}
