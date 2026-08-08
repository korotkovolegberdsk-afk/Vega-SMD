using Vega.ProcessLearning.Models;
using Vega.QualityAnalytics.Models;

namespace Vega.EngineeringDashboard.Models;

public enum DashboardWarningSeverity { Info, Warning, Critical }
public enum DashboardWarningCategory { Stencil, Technology, Quality, Production, Reflow }

public class DashboardWarning
{
    public DashboardWarningSeverity Severity { get; init; }
    public DashboardWarningCategory Category { get; init; }
    public string Message { get; init; } = "";
    public string Source { get; init; } = "";
}

public class EngineeringRecommendation
{
    public string Type { get; init; } = "";
    public string Component { get; init; } = "";
    public string CurrentState { get; init; } = "";
    public string Recommendation { get; init; } = "";
    public string Reason { get; init; } = "";
    public string Confidence { get; init; } = "";
}

public class EngineeringDashboardData
{
    public string ProjectName { get; init; } = "";
    public string BoardRevision { get; init; } = "";
    public string Customer { get; init; } = "";
    public string StencilRevision { get; init; } = "";
    public string StencilStatus { get; init; } = "";
    public string TechnologyStatus { get; init; } = "";
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public int ComponentCount { get; init; }
    public int ApertureCount { get; init; }
    public int ChangeCount { get; init; }
    public double Yield { get; init; }
    public double FPY { get; init; }
    public IReadOnlyList<string> MainDefects { get; init; } = Array.Empty<string>();
    public IReadOnlyList<EngineeringRecommendation> Recommendations { get; init; } = Array.Empty<EngineeringRecommendation>();
    public DateTime ProjectDate { get; init; }
    public string Frame { get; init; } = "";
    public double StencilThickness { get; init; }
    public string OriginalPaste { get; init; } = "";
    public string CorrectedPaste { get; init; } = "";
    public IReadOnlyList<string> TechnologyDecisions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ConfirmedImprovements { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> FailedDecisions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<DashboardWarning> Warnings { get; init; } = Array.Empty<DashboardWarning>();
}

public class EngineeringDashboardExport
{
    public IReadOnlyDictionary<string, double> KPIs { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, IReadOnlyList<object>> Charts { get; init; } = new Dictionary<string, IReadOnlyList<object>>();
    public IReadOnlyDictionary<string, IReadOnlyList<object>> Tables { get; init; } = new Dictionary<string, IReadOnlyList<object>>();
    public IReadOnlyList<string> Images { get; init; } = Array.Empty<string>();
    public IReadOnlyList<DashboardWarning> Warnings { get; init; } = Array.Empty<DashboardWarning>();
}

public class TechnologyDashboardSummary
{
    public string Status { get; init; } = "";
    public IReadOnlyList<string> Decisions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<DashboardWarning> Warnings { get; init; } = Array.Empty<DashboardWarning>();
}

public class QualityDashboardSummary
{
    public double Yield { get; init; }
    public double FPY { get; init; }
    public double PPM { get; init; }
    public IReadOnlyList<DefectStatistic> Pareto { get; init; } = Array.Empty<DefectStatistic>();
}

public class ProductionDashboardSummary
{
    public string Summary { get; init; } = "";
    public IReadOnlyList<string> Equipment { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProcessDefectRecord> Defects { get; init; } = Array.Empty<ProcessDefectRecord>();
}