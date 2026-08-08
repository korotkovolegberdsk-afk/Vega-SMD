using Vega.EngineeringDashboard.Models;
using Vega.Report.Models;

namespace Vega.EngineeringDashboard;

public static class EngineeringDashboardReportMapper
{
    public static EngineeringSummaryReportItem ToReportItem(EngineeringDashboardData dashboard) => new()
    {
        ProjectOverview = $"{dashboard.ProjectName}; customer: {dashboard.Customer}; board revision: {dashboard.BoardRevision}",
        StencilSummary = $"Revision: {dashboard.StencilRevision}; frame: {dashboard.Frame}; changes: {dashboard.ChangeCount}",
        TechnologyDecisions = dashboard.TechnologyDecisions,
        QualityKpis = new Dictionary<string, double> { ["Yield"] = dashboard.Yield, ["FPY"] = dashboard.FPY },
        Warnings = dashboard.Warnings.Select(item => $"{item.Severity}: {item.Message}").ToList(),
        Recommendations = dashboard.Recommendations.Select(item => $"{item.Type}: {item.Recommendation}; {item.Reason}").ToList()
    };
}