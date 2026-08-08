using Vega.Gerber.Models;

namespace Vega.PnP.Models;

public class MappedComponent
{
    public string RefDes { get; init; } = "";
    public string PackageName { get; init; } = "";
    public IReadOnlyList<PastePrimitive> PastePrimitives { get; init; }
        = Array.Empty<PastePrimitive>();
    public int PastePrimitiveCount { get; init; }
    public double MatchDistance { get; init; }
    public double Rotation { get; init; }
    public string Status { get; init; } = "";
}