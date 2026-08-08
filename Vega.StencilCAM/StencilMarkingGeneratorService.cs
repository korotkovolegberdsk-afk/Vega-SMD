using Vega.StencilCAM.Models;

namespace Vega.StencilCAM;

public class StencilMarkingGeneratorService
{
    public StencilMarking Generate(string text, double positionX, double positionY, double height, string font)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Marking text is required.", nameof(text));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        return new StencilMarking
        {
            Text = text, PositionX = positionX, PositionY = positionY, Height = height, Font = font,
            Mirror = true, Rotation = 0
        };
    }
}
