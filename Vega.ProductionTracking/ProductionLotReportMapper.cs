using Vega.ProductionTracking.Models;
using Vega.Report.Models;

namespace Vega.ProductionTracking;

public static class ProductionLotReportMapper
{
    public static ProductionLotReportItem ToReportItem(ProductionLotReport report) => new()
    {
        OrderNumber = report.Lot.OrderNumber, Customer = report.Lot.Customer, BoardName = report.Lot.BoardName, BoardRevision = report.Lot.BoardRevision,
        StencilRevision = report.Stencil, Paste = report.Paste, ReflowProfile = report.Reflow, Yield = report.Yield,
        Equipment = report.Equipment.Select(item => $"{item.EquipmentType}: {item.Manufacturer} {item.Model}").ToList(),
        Defects = report.Defects.Select(item => $"{item.ComponentRef}: {item.DefectType} x{item.Quantity}").ToList(), Recommendations = report.Recommendations
    };
}