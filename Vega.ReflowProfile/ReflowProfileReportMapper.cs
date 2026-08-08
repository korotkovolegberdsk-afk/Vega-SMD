using Vega.ReflowProfile.Models;
using ReflowProfileModel = Vega.ReflowProfile.Models.ReflowProfile;
using Vega.Report.Models;

namespace Vega.ReflowProfile;

public static class ReflowProfileReportMapper
{
    public static ReflowProfileReportItem ToReportItem(ReflowProfileModel profile, ReflowProfileAnalysis analysis, IReadOnlyList<ReflowProfilePoint> points) => new()
    {
        ProfileId = profile.Id, Name = profile.Name, EquipmentName = profile.EquipmentName, SolderPaste = profile.SolderPaste,
        PeakTemperature = analysis.PeakTemperature, TimeAboveLiquidus = analysis.TimeAboveLiquidus, RampRate = analysis.RampRate,
        CoolingRate = analysis.CoolingRate,
        ChartPoints = points.Select(point => new ReflowProfileChartPoint { TimeSeconds = point.TimeSeconds, TemperatureC = point.TemperatureC, SensorChannel = point.SensorChannel }).ToArray()
    };
}