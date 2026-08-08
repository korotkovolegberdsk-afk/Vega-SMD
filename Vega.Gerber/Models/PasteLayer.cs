namespace Vega.Gerber.Models;

public class PasteLayer
{
    public string FileName { get; init; } = "";
    public string Side { get; init; } = "Top";
    public List<GerberAperture> Apertures { get; } = new();
    public List<PastePrimitive> Primitives { get; } = new();
}
