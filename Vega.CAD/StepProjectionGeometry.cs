using System.Numerics;

namespace Vega.CAD;

public enum StepProjectionKind { Top, Bottom, Side, Isometric, Longitudinal }

public sealed record ProjectedTriangle(Vector2 A, Vector2 B, Vector2 C, Vector4 Color, double Depth);

public sealed record StepProjection(IReadOnlyList<ProjectedTriangle> Triangles, double Width, double Height);

/// <summary>Projects the imported STEP mesh without an external CAD process.</summary>
public sealed class StepProjectionGeometry
{
    public StepProjection Project(IReadOnlyList<GlbTriangle> triangles, StepProjectionKind kind, double pixelsPerMillimetre)
    {
        if (triangles.Count == 0) throw new ArgumentException("No mesh triangles were supplied.", nameof(triangles));
        var points = triangles.SelectMany(t => new[] { t.A, t.B, t.C }).Select(p => p * 1000f).ToArray();
        var extents = new[]
        {
            points.Max(p => p.X) - points.Min(p => p.X),
            points.Max(p => p.Y) - points.Min(p => p.Y),
            points.Max(p => p.Z) - points.Min(p => p.Z)
        };
        var planarAxes = Enumerable.Range(0, 3).OrderByDescending(i => extents[i]).ToArray();
        var axes = new ProjectionAxes(planarAxes[0], planarAxes[1], planarAxes[2]);
        var projected = triangles.Select(t => ProjectTriangle(t, kind, axes)).ToArray();
        var minX = projected.SelectMany(t => new[] { t.A.X, t.B.X, t.C.X }).Min();
        var maxX = projected.SelectMany(t => new[] { t.A.X, t.B.X, t.C.X }).Max();
        var minY = projected.SelectMany(t => new[] { t.A.Y, t.B.Y, t.C.Y }).Min();
        var maxY = projected.SelectMany(t => new[] { t.A.Y, t.B.Y, t.C.Y }).Max();
        var result = projected.Select(t => t with
        {
            A = new Vector2((float)((t.A.X - minX) * pixelsPerMillimetre), (float)((maxY - t.A.Y) * pixelsPerMillimetre)),
            B = new Vector2((float)((t.B.X - minX) * pixelsPerMillimetre), (float)((maxY - t.B.Y) * pixelsPerMillimetre)),
            C = new Vector2((float)((t.C.X - minX) * pixelsPerMillimetre), (float)((maxY - t.C.Y) * pixelsPerMillimetre))
        }).OrderBy(t => t.Depth).ToArray();
        return new StepProjection(result, (maxX - minX) * pixelsPerMillimetre, (maxY - minY) * pixelsPerMillimetre);
    }

    private static ProjectedTriangle ProjectTriangle(GlbTriangle t, StepProjectionKind kind, ProjectionAxes axes)
    {
        var a = ProjectPoint(t.A, kind, axes); var b = ProjectPoint(t.B, kind, axes); var c = ProjectPoint(t.C, kind, axes);
        return new ProjectedTriangle(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y), new Vector2(c.X, c.Y), t.Color, (a.Z + b.Z + c.Z) / 3.0);
    }

    private readonly record struct ProjectionAxes(int Long, int Cross, int Thickness);

    private static float Axis(Vector3 p, int axis) => axis switch { 0 => p.X, 1 => p.Y, _ => p.Z };

    private static Vector3 ProjectPoint(Vector3 p, StepProjectionKind kind, ProjectionAxes axes)
    {
        // glTF/GLB uses metres; the drawing API uses millimetres.
        p *= 1000f;
        return kind switch
        {
            // Select axes from the actual STEP extents.  This keeps the plan
            // horizontal for both C0402 (1.00 x 0.50) and C2220 (6.10 x 5.40).
            StepProjectionKind.Top => new Vector3(Axis(p, axes.Long), Axis(p, axes.Cross), Axis(p, axes.Thickness)),
            StepProjectionKind.Bottom => new Vector3(Axis(p, axes.Long), -Axis(p, axes.Cross), -Axis(p, axes.Thickness)),
            // The side view is the end face: the second-largest plan axis is
            // horizontal and the smallest STEP axis is the 1.80 mm height.
            StepProjectionKind.Side => new Vector3(Axis(p, axes.Cross), Axis(p, axes.Thickness), Axis(p, axes.Long)),
            StepProjectionKind.Longitudinal => new Vector3(Axis(p, axes.Long), Axis(p, axes.Thickness), -Axis(p, axes.Cross)),
            // Orthonormal camera axes: the same physical scale as all other views.
            // Draw far faces first so hidden rear edges cannot show on the body.
            _ => new Vector3((p.X - p.Y) / MathF.Sqrt(2), (2 * p.Z - p.X - p.Y) / MathF.Sqrt(6), p.X + p.Y + p.Z),
        };
    }
}
