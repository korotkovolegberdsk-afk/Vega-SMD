using Vega.Gerber.Models;
using Vega.PnP.Models;

namespace Vega.PnP;

public class ComponentMappingService
{
    private readonly double _matchRadius;

    public ComponentMappingService(double matchRadius = 1.0)
    {
        if (matchRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(matchRadius),
                "Радиус сопоставления должен быть больше нуля.");
        }

        _matchRadius = matchRadius;
    }

    public List<MappedComponent> Map(PnpComponent[] components, PasteLayer layer)
    {
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(layer);
        return components.Select(component => MapComponent(component, layer)).ToList();
    }

    private MappedComponent MapComponent(PnpComponent component, PasteLayer layer)
    {
        var distances = layer.Primitives
            .Select(primitive => new { Primitive = primitive, Distance = Distance(component, primitive) })
            .ToList();
        var matchedPrimitives = distances
            .Where(x => x.Distance <= _matchRadius)
            .OrderBy(x => x.Distance)
            .Select(x => x.Primitive)
            .ToList();
        var nearestDistance = distances.Count == 0
            ? double.PositiveInfinity
            : distances.Min(x => x.Distance);

        return new MappedComponent
        {
            RefDes = component.RefDes,
            PackageName = component.PackageName,
            PastePrimitives = matchedPrimitives,
            PastePrimitiveCount = matchedPrimitives.Count,
            MatchDistance = nearestDistance,
            Rotation = component.Rotation,
            Status = matchedPrimitives.Count > 0 ? "Matched" : "NoPaste"
        };
    }

    private static double Distance(PnpComponent component, PastePrimitive primitive)
    {
        var dx = component.X - primitive.X;
        var dy = component.Y - primitive.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}