using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.TapeReelDrawing;

/// <summary>Read-only logical positions used by <see cref="TapeReelPreview"/>.</summary>
public sealed record TapeReelGeometryLayout(double BaseLineX, IReadOnlyList<double> HoleXs, IReadOnlyList<double> PocketXs)
{
    public static TapeReelGeometryLayout Create(ComponentTapeReelGeometry geometry, double visibleTapeWidth, int pocketCount)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        if (visibleTapeWidth <= 0) throw new ArgumentOutOfRangeException(nameof(visibleTapeWidth));
        if (pocketCount < 1) throw new ArgumentOutOfRangeException(nameof(pocketCount));

        var p0 = geometry.SprocketHolePitch > 0 ? geometry.SprocketHolePitch : 4d;
        var p1 = geometry.PocketPitch > 0 ? geometry.PocketPitch : 4d;
        var baseLineX = p0 / 2d;
        var holeCount = Math.Max(1, (int)Math.Floor(visibleTapeWidth / p0) + 1);

        var holes = Enumerable.Range(0, holeCount).Select(index => index * p0).ToArray();
        var pockets = Enumerable.Range(0, pocketCount).Select(index => baseLineX + index * p1).ToArray();
        return new TapeReelGeometryLayout(baseLineX, holes, pockets);
    }
}
