using Vega.Gerber.Models;
using Vega.PnP;
using Vega.PnP.Models;
using Xunit;

namespace Vega.Tests;

public class ComponentPasteAnalyzerServiceTests
{
    [Fact]
    public void Analyze_Should_Build_Pattern_From_Multiple_Paste_Primitives()
    {
        var component = new MappedComponent
        {
            RefDes = "U1",
            PackageName = "SOIC-8",
            Rotation = 90,
            PastePrimitives = new[]
            {
                new PastePrimitive
                {
                    X = 1,
                    Y = 2,
                    Width = 0.6,
                    Height = 0.4,
                    Area = 0.24,
                    Perimeter = 2
                },
                new PastePrimitive
                {
                    X = 3,
                    Y = 4,
                    Width = 1.2,
                    Height = 0.6,
                    Area = 0.72,
                    Perimeter = 3.6
                }
            }
        };

        var pattern = new ComponentPasteAnalyzerService().Analyze(component);

        Assert.Equal(2, pattern.PadCount);
        Assert.Equal(0.96, pattern.TotalArea, 6);
        Assert.Equal(0.7, pattern.MinX, 6);
        Assert.Equal(3.6, pattern.MaxX, 6);
        Assert.Equal(1.8, pattern.MinY, 6);
        Assert.Equal(4.3, pattern.MaxY, 6);
        Assert.Equal(2.15, pattern.CenterX, 6);
        Assert.Equal(3.05, pattern.CenterY, 6);
        Assert.Equal(2.9, pattern.Width, 6);
        Assert.Equal(2.5, pattern.Height, 6);
        Assert.Equal(90, pattern.Rotation, 6);
    }
}
