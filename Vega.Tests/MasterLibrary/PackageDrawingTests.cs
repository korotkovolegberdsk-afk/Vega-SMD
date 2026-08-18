using Xunit;
using Vega.Models.MasterLibrary;
using Vega.StencilUI.PackageDrawing;

namespace Vega.Tests.MasterLibrary;

public sealed class PackageDrawingTests
{
    [Theory]
    [InlineData("SOT23", 3)]
    [InlineData("SOT25", 5)]
    [InlineData("SOT26", 6)]
    public void MiniMold_HasActualLeadCount(string name, int count)
    {
        var scene = ParametricPackageGeometryBuilder.Build(Package(name, "SOT", leads: count, bodyX: 3, bodyY: 1.5, pitch: .95, leadWidth: .3, leadLength: .6));
        Assert.Equal(count, scene.LeadCount);
    }

    [Fact]
    public void So8_HasEightLeads() =>
        Assert.Equal(8, ParametricPackageGeometryBuilder.Build(Package("SO08", "SOIC", leads: 8, bodyX: 5, bodyY: 4, pitch: 1.27, leadWidth: .4, leadLength: .8)).LeadCount);

    [Fact]
    public void Qfp32_HasThirtyTwoLeads() =>
        Assert.Equal(32, ParametricPackageGeometryBuilder.Build(Package("QFP032P065W092", "QFP", leads: 32, bodyX: 7, bodyY: 7, pitch: .8, leadWidth: .3, leadLength: .8)).LeadCount);

    [Fact]
    public void Qfn32_HasThirtyTwoPads() =>
        Assert.Equal(32, ParametricPackageGeometryBuilder.Build(Package("QFN032P050W500", "QFN", pads: 32, bodyX: 5, bodyY: 5, pitch: .5, leadWidth: .25)).PadCount);

    [Fact]
    public void Sod_HasTwoTerminals() =>
        Assert.Equal(2, ParametricPackageGeometryBuilder.Build(Package("SOD123", "SOD", pads: 2, bodyX: 2.7, bodyY: 1.6)).Primitives.Count(x => x.Kind == ScenePrimitiveKind.Terminal));

    [Fact]
    public void Dpak5_HasFiveLeadPositions() =>
        Assert.Equal(5, ParametricPackageGeometryBuilder.Build(Package("DPAK5", "DPAK", leads: 5, bodyX: 10, bodyY: 6, pitch: 1.27, leadWidth: .8, leadLength: 1.2)).LeadCount);

    [Fact]
    public void Bga64_HasSixtyFourBalls() =>
        Assert.Equal(64, ParametricPackageGeometryBuilder.Build(Package("BGA064P080W800", "BGA", pads: 64, bodyX: 8, bodyY: 8, ballPitch: .8, ballDiameter: .4)).BallCount);

    [Fact]
    public void AluminumCap_HasCircularTopView()
    {
        var scene = ParametricPackageGeometryBuilder.Build(Package("ALC5", "ALUMINUM_CAP", pads: 2, bodyX: 5, bodyY: 5, height: 7));
        Assert.Contains(scene.Primitives, x => x.Kind == ScenePrimitiveKind.Body && x.Marker == "Circular");
    }

    [Fact]
    public void UnreferencedPackagesUseHonestGenericOutline()
    {
        var scene = ParametricPackageGeometryBuilder.Build(Package("TANT", "TANTALUM_CAP", pads: 2, bodyX: 3.2, bodyY: 1.6));
        Assert.Equal("Generic", scene.TemplateId);
        Assert.Contains(scene.Warnings, warning => warning.Contains("template not defined", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PitchWidthAndLengthChangeSceneGeometry()
    {
        var p = Package("SOT23", "SOT", leads: 3, bodyX: 3, bodyY: 1.5, pitch: .95, leadWidth: .3, leadLength: .5);
        var first = ParametricPackageGeometryBuilder.Build(p);
        p.Pitch = 1.10; p.LeadWidth = .5; p.LeadLength = 1.0;
        var second = ParametricPackageGeometryBuilder.Build(p);
        Assert.NotEqual(first.Primitives.Where(x => x.Kind == ScenePrimitiveKind.Lead).Select(x => x.Bounds).ToArray(), second.Primitives.Where(x => x.Kind == ScenePrimitiveKind.Lead).Select(x => x.Bounds).ToArray());
    }

    [Theory]
    [InlineData(400, 300)]
    [InlineData(600, 500)]
    [InlineData(1000, 700)]
    public void Fit_StaysInsideViewport(double width, double height)
    {
        var scene = ParametricPackageGeometryBuilder.Build(Package("D2PAK", "D2PAK", leads: 3, bodyX: 10, bodyY: 9.8, height: 4.5, pitch: 2.54, leadWidth: .8, leadLength: 1.2));
        var fit = ParametricPackageGeometryBuilder.Fit(scene, new System.Windows.Size(width, height));
        Assert.True(fit.Bounds.Left >= 0 && fit.Bounds.Top >= 0);
        Assert.True(fit.Bounds.Right <= width && fit.Bounds.Bottom <= height);
    }

    private static PackageDefinition Package(string name, string family, int leads = 0, int pads = 0, double bodyX = 1, double bodyY = 1, double height = 0, double pitch = 0, double leadWidth = 0, double leadLength = 0, double ballPitch = 0, double ballDiameter = 0) =>
        new() { PackageName = name, PackageFamily = family, BodyLength = bodyX, BodyWidth = bodyY, Height = height, LeadCount = leads, PadCount = pads, Pitch = pitch, LeadWidth = leadWidth, LeadLength = leadLength, BallPitch = ballPitch, BallDiameter = ballDiameter };
}