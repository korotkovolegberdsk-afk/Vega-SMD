using Vega.PnP.Models;

namespace Vega.PnP;

public class ComponentPasteAnalyzerService
{
    public ComponentPastePattern Analyze(MappedComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var primitives = component.PastePrimitives;
        if (primitives.Count == 0)
        {
            return new ComponentPastePattern
            {
                RefDes = component.RefDes,
                PackageName = component.PackageName,
                Rotation = component.Rotation
            };
        }

        var minX = primitives.Min(x => x.X - x.Width / 2);
        var maxX = primitives.Max(x => x.X + x.Width / 2);
        var minY = primitives.Min(x => x.Y - x.Height / 2);
        var maxY = primitives.Max(x => x.Y + x.Height / 2);

        return new ComponentPastePattern
        {
            RefDes = component.RefDes,
            PackageName = component.PackageName,
            PastePrimitives = primitives,
            PadCount = primitives.Count,
            TotalArea = primitives.Sum(x => x.Area),
            MinX = minX,
            MaxX = maxX,
            MinY = minY,
            MaxY = maxY,
            CenterX = (minX + maxX) / 2,
            CenterY = (minY + maxY) / 2,
            Width = maxX - minX,
            Height = maxY - minY,
            Rotation = component.Rotation
        };
    }
}
