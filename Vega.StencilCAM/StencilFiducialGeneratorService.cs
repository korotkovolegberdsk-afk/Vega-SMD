using Vega.StencilCAM.Models;

namespace Vega.StencilCAM;

public class StencilFiducialGeneratorService
{
    public StencilFiducial GeneratePcbFiducial(string shape, double diameter, double x, double y, string layer = "Fiducial") =>
        Create(StencilFiducialType.Pcb, shape, diameter, x, y, layer);

    public StencilFiducial GenerateLocalFiducial(string shape, double diameter, double x, double y, string layer = "Fiducial") =>
        Create(StencilFiducialType.Local, shape, diameter, x, y, layer);

    private static StencilFiducial Create(StencilFiducialType type, string shape, double diameter, double x, double y, string layer)
    {
        if (diameter <= 0) throw new ArgumentOutOfRangeException(nameof(diameter));
        return new StencilFiducial { Type = type, Shape = shape, Diameter = diameter, X = x, Y = y, Layer = layer };
    }
}
