using Vega.Gerber.Models;
using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Xunit;

namespace Vega.Tests;

public class StencilFrameLibraryServiceTests
{
    private readonly StencilFrameLibraryService _service = new();

    [Fact]
    public void FrameLibrary_ChangesDefaultAndKeepsPreviousProjectAssignment()
    {
        var firstDefault = _service.GetDefaultFrame();
        Assert.NotNull(firstDefault);
        var second = new StencilFrame
        {
            Name = $"TEST_FRAME_{Guid.NewGuid():N}", PrinterModel = "Test", FrameWidth = 400, FrameHeight = 500,
            StencilWidth = 400, StencilHeight = 500, IsActive = true, IsDefault = false, SortOrder = 10
        };
        var secondId = _service.Add(second);
        var projectId = Random.Shared.Next(1_000_000, int.MaxValue);
        try
        {
            Assert.Contains(_service.GetFrames(), frame => frame.Id == secondId);
            _service.SetDefaultFrame(secondId);
            Assert.False(_service.SelectFrame(firstDefault!.Id)!.IsDefault);
            Assert.True(_service.SelectFrame(secondId)!.IsDefault);

            var savedProjectFrame = _service.SaveProjectFrame(projectId);
            _service.SetDefaultFrame(firstDefault!.Id);
            var restoredProjectFrame = _service.GetProjectFrame(projectId);
            Assert.NotNull(restoredProjectFrame);

            Assert.Equal(secondId, savedProjectFrame.FrameId);
            Assert.Equal(secondId, restoredProjectFrame!.FrameId);
            Assert.Equal(second.Name, restoredProjectFrame!.FrameName);
        }
        finally
        {
            _service.SetDefaultFrame(firstDefault!.Id);
        }
    }

    [Fact]
    public void Placement_UsesDefaultFrameWhenNoFrameIsSelected()
    {
        var layer = new PasteLayer { Side = "Top" };
        layer.Primitives.Add(new PastePrimitive { X = 1, Y = 1, Width = 1, Height = 1, Area = 1, Perimeter = 4 });

        var result = new StencilPlacementService().PlacePasteLayer(layer, new StencilTransformation());

        Assert.Equal(_service.GetDefaultFrame()!.Name, result.FrameName);
    }
}

