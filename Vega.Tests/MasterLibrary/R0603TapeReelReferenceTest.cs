using Vega.Models.MasterLibrary;
using Vega.StencilUI.TapeReelDrawing;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public sealed class R0603TapeReelReferenceTest
{
    [Fact]
    public void R0603TapeReelReference_UsesExpectedCarrierTapeGeometry()
    {
        var component = new ComponentDefinition
        {
            Id = 601,
            ManufacturerPartNumber = "R0603_TEST",
            PackageId = 603,
            Package = new PackageDefinition
            {
                Id = 603,
                PackageName = "CHIP_0603",
                PackageFamily = "CHIP",
                ComponentType = "Resistor"
            }
        };
        var tape = new ComponentTapeReelGeometry
        {
            ComponentDefinitionId = component.Id,
            CarrierTapeWidth = 8,
            SprocketHolePitch = 4,
            SprocketHoleDiameter = 1.5,
            PocketPitch = 4,
            PocketOrientation = TapePocketOrientation.Deg0
        };

        var layout = TapeReelGeometryLayout.Create(tape, visibleTapeWidth: 24, pocketCount: 4);
        var sprocketHoleCenterY = tape.CarrierTapeWidth * .18;
        var pocketCenterY = (sprocketHoleCenterY + tape.CarrierTapeWidth) / 2;

        Assert.Equal("R0603_TEST", component.ManufacturerPartNumber);
        Assert.Equal("CHIP_0603", component.Package!.PackageName);
        Assert.Equal(2, layout.BaseLineX);
        Assert.Equal(2, layout.PocketXs[0]);
        Assert.Equal(new[] { 2d, 6d, 10d, 14d }, layout.PocketXs);
        Assert.Equal(4, tape.PocketPitch);
        Assert.All(layout.HoleXs.Zip(layout.HoleXs.Skip(1)), pair => Assert.Equal(4, pair.Second - pair.First));
        Assert.True(layout.HoleXs.Count > layout.PocketXs.Count);
        Assert.Equal((sprocketHoleCenterY + tape.CarrierTapeWidth) / 2, pocketCenterY);
        Assert.Equal(TapePocketOrientation.Deg0, tape.PocketOrientation);
    }
}
