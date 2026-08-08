using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.StencilCAM.Models;

namespace Vega.StencilCAM;

public class StencilFrameService
{
    public StencilFrame LoadFrame(string gerberFile)
    {
        if (string.IsNullOrWhiteSpace(gerberFile))
            throw new ArgumentException("Gerber template path is required.", nameof(gerberFile));

        var parser = new GerberPasteParserService();
        parser.Load(gerberFile);
        var bounds = CalculateBounds(parser.Parse());
        return new StencilFrame
        {
            Name = Path.GetFileNameWithoutExtension(gerberFile), GerberTemplateFile = gerberFile,
            FrameWidth = bounds.Width, FrameHeight = bounds.Height,
            StencilWidth = bounds.Width, StencilHeight = bounds.Height,
            OriginX = bounds.MinX, OriginY = bounds.MinY
        };
    }

    public StencilBounds CalculateBounds(PasteLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        if (layer.Primitives.Count == 0)
            return new StencilBounds(0, 0, 0, 0);
        return new StencilBounds(
            layer.Primitives.Min(primitive => primitive.X - primitive.Width / 2),
            layer.Primitives.Min(primitive => primitive.Y - primitive.Height / 2),
            layer.Primitives.Max(primitive => primitive.X + primitive.Width / 2),
            layer.Primitives.Max(primitive => primitive.Y + primitive.Height / 2));
    }

    public (double X, double Y) GetCenter(StencilFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var width = frame.StencilWidth > 0 ? frame.StencilWidth : frame.FrameWidth;
        var height = frame.StencilHeight > 0 ? frame.StencilHeight : frame.FrameHeight;
        return (frame.OriginX + width / 2, frame.OriginY + height / 2);
    }

    public bool ValidateFit(StencilBounds bounds, StencilFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var width = frame.StencilWidth > 0 ? frame.StencilWidth : frame.FrameWidth;
        var height = frame.StencilHeight > 0 ? frame.StencilHeight : frame.FrameHeight;
        return bounds.MinX >= frame.OriginX && bounds.MinY >= frame.OriginY
            && bounds.MaxX <= frame.OriginX + width && bounds.MaxY <= frame.OriginY + height;
    }
}
