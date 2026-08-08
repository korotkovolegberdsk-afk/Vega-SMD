using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.PnP.Models;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class CorrectedPasteRecommendationServiceTests
{
    [Fact]
    public void Create_Should_Recommend_Reduction_Without_Changing_Original_Pattern()
    {
        var pattern = new ComponentPastePattern
        {
            RefDes = "U1",
            PackageName = "QFN-32",
            PastePrimitives = new[]
            {
                new PastePrimitive
                {
                    X = 1,
                    Y = 2,
                    Width = 0.9,
                    Height = 0.45,
                    Area = 0.405,
                    Perimeter = 2.7
                }
            },
            PadCount = 1,
            TotalArea = 0.405
        };
        var analysis = new StencilAnalysisResult
        {
            RefDes = "U1",
            PackageName = "QFN-32",
            Status = "WARNING",
            Warnings = new[] { "Тепловая площадка требует отдельной проверки." }
        };
        var rule = new StencilRecommendationRule
        {
            ReductionX = 10,
            ReductionY = 10,
            ThermalPadRule = "Window-pane thermal pad aperture."
        };

        var result = new CorrectedPasteRecommendationService()
            .Create(pattern, analysis, rule);

        Assert.Equal("Recommended", result.Status);
        Assert.Equal(0.9, pattern.PastePrimitives[0].Width, 6);
        Assert.Equal(0.45, pattern.PastePrimitives[0].Height, 6);
        Assert.Equal(0.81, result.RecommendedPattern.PastePrimitives[0].Width, 6);
        Assert.Equal(0.405, result.RecommendedPattern.PastePrimitives[0].Height, 6);
        Assert.Contains(result.Changes, x => x.Contains("Размеры апертур"));
        Assert.Contains(result.Changes, x => x.Contains("Thermal pad"));
    }
}
