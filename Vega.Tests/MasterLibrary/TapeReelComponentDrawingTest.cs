using Vega.Models.MasterLibrary;
using Vega.StencilUI.PackageDrawing;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class TapeReelComponentDrawingTest
{
    [Fact]
    public void R0603_DrawsChipPackageWithZeroRotation()
    {
        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "R0603_TEST",
            Package = new PackageDefinition
            {
                PackageName = "R0603",
                PackageFamily = "CHIP",
                BodyLength = 1.6,
                BodyWidth = 0.8,
                PadCount = 2
            }
        };
        var tape = new ComponentTapeReelGeometry { PickupRotation = 0 };

        var scene = ParametricPackageGeometryBuilder.Build(component.Package!);

        Assert.Contains(scene.Primitives, primitive => primitive.Kind == ScenePrimitiveKind.Body);
        Assert.Contains(scene.Primitives, primitive => primitive.Kind == ScenePrimitiveKind.Terminal);
        Assert.Equal(0, tape.PickupRotation);
    }

    [Fact]
    public void Sot23_DrawsPackageWithPin1Marker()
    {
        var component = new ComponentDefinition
        {
            ManufacturerPartNumber = "SOT23_TEST",
            Package = new PackageDefinition
            {
                PackageName = "SOT23",
                PackageFamily = "SOT",
                BodyLength = 2.9,
                BodyWidth = 1.3,
                Pitch = .95,
                LeadCount = 3,
                LeadLength = .4,
                LeadWidth = .4
            }
        };

        var scene = ParametricPackageGeometryBuilder.Build(component.Package!);

        Assert.Contains(scene.Primitives, primitive => primitive.Kind == ScenePrimitiveKind.Body);
        Assert.Contains(scene.Primitives, primitive => primitive.Kind == ScenePrimitiveKind.Lead);
        Assert.Contains(scene.Primitives, primitive => primitive.Kind == ScenePrimitiveKind.Marker && primitive.Marker == "Pin1Dot");
    }
}
