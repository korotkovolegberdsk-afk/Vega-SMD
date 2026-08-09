using Vega.StencilCAM.Models;
using Vega.StencilInput.Models;
using Vega.StencilViewer.Models;
using Vega.Gerber.Models;
using Vega.TechnologyDecision.Models;
using Vega.StencilWorkflow.Models;

namespace Vega.StencilProjects.Models;

public enum StencilProjectInputSource { PasteGerber, AltiumPcbDoc, PanelGerber }
public enum StencilProjectPasteSide { Top, Bottom }
public enum StencilProjectStatus { Created, InputLoaded, Analyzed, PreviewReady, ReportGenerated, Error }

public class StencilProject
{
    public int Id { get; init; }
    public string ProjectName { get; set; } = "";
    public string Customer { get; set; } = "";
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public string Revision { get; set; } = "V001";
    public StencilProjectInputSource InputSource { get; set; } = StencilProjectInputSource.PasteGerber;
    public StencilProjectPasteSide PasteSide { get; set; } = StencilProjectPasteSide.Top;
    public int? FrameId { get; set; }
    public StencilProjectStatus Status { get; set; } = StencilProjectStatus.Created;
    public List<StencilInputFile> InputFiles { get; } = [];
}

public class StencilInputFile
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public string FileName { get; init; } = "";
    public string FileType { get; init; } = "";
    public string Path { get; init; } = "";
    public DateTime ImportDate { get; init; } = DateTime.UtcNow;
}

public class StencilProjectSession
{
    public required StencilProject Project { get; init; }
    public StencilInputProject? Input { get; set; }
    public StencilManufacturingProject? WorkflowProject { get; set; }
    public StencilViewDocument? Preview { get; set; }
    public StencilAnalysisContext? AnalysisContext { get; set; }
    public TechnologyDecisionResult? TechnologyDecision { get; set; }
    public PasteAnalysisResult? AnalysisResult { get; set; }
    public StencilViewDocument? PreviewData { get; set; }
    public StencilProjectReportContext? ReportContext { get; set; }
    public CorrectedPasteLayer? CorrectedLayer { get; set; }
    public IReadOnlyList<PasteCorrectionChange> Changes { get; set; } = Array.Empty<PasteCorrectionChange>();
    public StencilFrame? Frame { get; set; }
    public int ApertureCount => WorkflowProject?.OriginalPaste?.Primitives.Count ?? 0;
    public double BoardWidth => Input?.BoardOutline?.Width ?? 0;
    public double BoardHeight => Input?.BoardOutline?.Height ?? 0;
}
public class StencilAnalysisContext
{
    public IReadOnlyList<GerberAperture> Apertures { get; init; } = Array.Empty<GerberAperture>();
    public IReadOnlyList<PastePrimitive> Pads { get; init; } = Array.Empty<PastePrimitive>();
    public StencilBounds? BoardBounds { get; init; }
    public string Side { get; init; } = "";
    public string GeometrySummary => $"{Pads.Count} pads; {Apertures.Count} apertures";
}
public class StencilProjectReportContext
{
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}