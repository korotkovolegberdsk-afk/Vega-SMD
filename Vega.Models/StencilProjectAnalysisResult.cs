using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.PnP.Models;

namespace Vega.Models;

public enum StencilAnalysisStatus
{
    OK,
    Warning,
    Critical
}

public class StencilProjectAnalysisResult
{
    public string ProjectName { get; init; } = "";
    public int TotalComponents { get; init; }
    public int AnalyzedComponents { get; init; }
    public int OkCount { get; init; }
    public int WarningCount { get; init; }
    public int CriticalCount { get; init; }
    public IReadOnlyList<ComponentStencilAnalysisResult> ComponentResults { get; init; }
        = Array.Empty<ComponentStencilAnalysisResult>();
}

public class ComponentStencilAnalysisResult
{
    public string RefDes { get; init; } = "";
    public string PackageName { get; init; } = "";
    public string Side { get; init; } = "";
    public ComponentPastePattern? PastePattern { get; init; }
    public StencilTechnologyRule? TechnologyRule { get; init; }
    public ApertureShapeType? CurrentShape { get; init; }
    public ApertureShapeType? RecommendedShape { get; init; }
    public double CurrentArea { get; init; }
    public double RecommendedArea { get; init; }
    public double AreaRatio { get; init; }
    public double AspectRatio { get; init; }
    public StencilAnalysisStatus Status { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}

public class StencilAnalysisReport
{
    public string Summary { get; init; } = "";
    public IReadOnlyList<ComponentStencilAnalysisResult> Components { get; init; }
        = Array.Empty<ComponentStencilAnalysisResult>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}
