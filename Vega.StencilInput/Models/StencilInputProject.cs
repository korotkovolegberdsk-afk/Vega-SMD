using Vega.CAD.Models;
using Vega.Gerber.Models;
using Vega.StencilCAM.Models;

namespace Vega.StencilInput.Models;

public enum StencilInputSourceType
{
    PasteOnly,
    AltiumProject,
    PanelGerber,
    Manual
}

public class StencilInputProject
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = "";
    public StencilInputSourceType SourceType { get; set; }
    public List<string> SourceFiles { get; } = [];
    public List<PasteLayer> PasteLayers { get; } = [];
    public StencilBounds? BoardOutline { get; set; }
    public string AssemblyDrawing { get; set; } = "";
    public PcbProject? PcbProject { get; set; }
    public List<BomItem> BomItems { get; } = [];
    public List<Placement> Placements { get; } = [];
    public List<Component> Components { get; } = [];
    public List<Fiducial> Fiducials { get; } = [];
}
