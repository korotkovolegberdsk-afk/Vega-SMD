namespace Vega.CAD;

public sealed record StepProjectionSet(string ModelPath, string MeshPath, IReadOnlyDictionary<StepProjectionKind, StepProjection> Views);

/// <summary>Single in-process pipeline used by the UI: STEP -> mesh -> four views.</summary>
public sealed class StepModelProjectionPipeline
{
    private readonly OcctStepModelService _step = new();
    private readonly GlbMeshReader _mesh = new();
    private readonly StepProjectionGeometry _projection = new();

    public StepProjectionSet Build(string stepPath, string workingDirectory, double pixelsPerMillimetre = 250)
    {
        var meshPath = _step.ConvertStepToGlb(stepPath, workingDirectory);
        return BuildFromGlb(stepPath, meshPath, pixelsPerMillimetre);
    }

    public StepProjectionSet BuildFromGlb(string modelPath, string meshPath, double pixelsPerMillimetre = 250)
    {
        var triangles = _mesh.ReadTriangles(meshPath);
        var views = Enum.GetValues<StepProjectionKind>().ToDictionary(kind => kind, kind => _projection.Project(triangles, kind, pixelsPerMillimetre));
        return new StepProjectionSet(modelPath, meshPath, views);
    }
}
