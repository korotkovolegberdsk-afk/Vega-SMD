using Vega.Gerber.Models;
using Vega.StencilCAM.Models;

namespace Vega.StencilViewer.Models;

public class StencilViewDocument
{
    public string ProjectName { get; init; } = "";
    public StencilFrame? Frame { get; init; }
    public PasteLayer? OriginalPasteLayer { get; init; }
    public CorrectedPasteLayer? CorrectedPasteLayer { get; init; }
    public IReadOnlyList<StencilFiducial> Fiducials { get; init; } = Array.Empty<StencilFiducial>();
    public IReadOnlyList<StencilMarking> MarkingLayer { get; init; } = Array.Empty<StencilMarking>();
    public StencilTransformation? Transformations { get; init; }
}
