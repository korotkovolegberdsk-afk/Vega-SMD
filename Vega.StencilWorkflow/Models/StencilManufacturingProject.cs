using Vega.Gerber.Models;
using Vega.StencilCAM.Models;
using Vega.StencilInput.Models;
using Vega.StencilViewer.Models;

namespace Vega.StencilWorkflow.Models;

public class StencilManufacturingProject
{
    public int Id { get; init; }
    public string ProjectName { get; set; } = "";
    public StencilInputProject? InputProject { get; set; }
    public StencilFrame? Frame { get; set; }
    public string PasteSource { get; set; } = "";
    public PasteLayer? OriginalPaste { get; set; }
    public CorrectedPasteLayer? CorrectedPaste { get; set; }
    public IReadOnlyList<StencilFiducial> Fiducials { get; set; } = Array.Empty<StencilFiducial>();
    public IReadOnlyList<StencilMarking> Marking { get; set; } = Array.Empty<StencilMarking>();
    public PasteAnalysisResult? AnalysisResult { get; set; }
    public IReadOnlyList<string> OutputFiles { get; set; } = Array.Empty<string>();
    public StencilWorkflowStatus Status { get; set; } = StencilWorkflowStatus.Created;
    public StencilTransformation? Transformations { get; set; }
    public StencilPlacementResult? Placement { get; set; }
    public StencilViewDocument? Preview { get; set; }
}