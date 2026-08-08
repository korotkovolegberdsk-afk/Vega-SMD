using Vega.Models.MasterLibrary;
using Vega.PnP.Models;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class StencilRecommendationAnalyzerServiceTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _packageService = new();
    private readonly StencilRecommendationAnalyzerService _analyzer = new();

    [Fact]
    public void Analyze_ChipResistor_Should_Return_Ok()
    {
        var package = AddPackageWithFootprint("CHIP", padCount: 2, thermalPadCount: 0);
        var pattern = CreatePattern("R1", package.PackageName, padCount: 2);
        var rule = CreateRule("CHIP", "Not applicable");

        var result = _analyzer.Analyze(pattern, package, rule);

        Assert.Equal("OK", result.Status);
        Assert.Equal(0.81, result.CurrentPasteArea, 6);
        Assert.Equal(0.81, result.ExpectedPasteArea, 6);
        Assert.Equal(2, result.PadCount);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Analyze_Qfn_Should_Warn_For_Thermal_Pad()
    {
        var package = AddPackageWithFootprint("QFN", padCount: 4, thermalPadCount: 1);
        var pattern = CreatePattern("U1", package.PackageName, padCount: 4);
        var rule = CreateRule("QFN", "Window-pane thermal pad aperture.");

        var result = _analyzer.Analyze(pattern, package, rule);

        Assert.Equal("WARNING", result.Status);
        Assert.Contains(result.Warnings, x => x.Contains("Тепловая площадка"));
        Assert.Contains(result.Recommendations, x => x.Contains("Проверьте"));
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    private PackageDefinition AddPackageWithFootprint(
        string prefix,
        int padCount,
        int thermalPadCount)
    {
        var package = _database.CreatePackage(prefix);
        package.PadCount = padCount;
        package.ThermalPadCount = thermalPadCount;
        _packageService.Add(package);
        var savedPackage = _packageService.GetAll()
            .Single(x => x.PackageName == package.PackageName);

        _packageService.AddFootprint(new PackageFootprint
        {
            PackageId = savedPackage.Id,
            PadCount = padCount,
            PadLength = 1,
            PadWidth = 0.5,
            PadPitch = 1.2,
            PatternName = $"{prefix}-PATTERN"
        });

        return savedPackage;
    }

    private static ComponentPastePattern CreatePattern(
        string refDes,
        string packageName,
        int padCount)
    {
        var primitives = Enumerable.Range(0, padCount)
            .Select(index => new Vega.Gerber.Models.PastePrimitive
            {
                X = index * 1.2,
                Y = 0,
                Width = 0.9,
                Height = 0.45,
                Area = 0.405,
                Perimeter = 2.7
            })
            .ToList();

        return new ComponentPastePattern
        {
            RefDes = refDes,
            PackageName = packageName,
            PastePrimitives = primitives,
            PadCount = primitives.Count,
            TotalArea = primitives.Sum(x => x.Area)
        };
    }

    private static StencilRecommendationRule CreateRule(
        string family,
        string thermalPadRule)
    {
        return new StencilRecommendationRule
        {
            PackageFamily = family,
            ComponentType = "Resistor",
            RecommendedStencilThickness = 0.12,
            ApertureShape = ApertureShape.Rectangle,
            ReductionX = 10,
            ReductionY = 10,
            ThermalPadRule = thermalPadRule,
            AreaRatioMinimum = 0.66,
            AspectRatioMinimum = 1.5
        };
    }
}