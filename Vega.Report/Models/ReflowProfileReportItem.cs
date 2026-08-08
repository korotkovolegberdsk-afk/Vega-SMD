namespace Vega.Report.Models;

public class ReflowProfileChartPoint
{
    public double TimeSeconds { get; init; }
    public double TemperatureC { get; init; }
    public string SensorChannel { get; init; } = "";
}

public class ReflowProfileReportItem
{
    public int ProfileId { get; init; }
    public string Name { get; init; } = "";
    public string EquipmentName { get; init; } = "";
    public string SolderPaste { get; init; } = "";
    public double PeakTemperature { get; init; }
    public double TimeAboveLiquidus { get; init; }
    public double RampRate { get; init; }
    public double CoolingRate { get; init; }
    public IReadOnlyList<ReflowProfileChartPoint> ChartPoints { get; init; } = Array.Empty<ReflowProfileChartPoint>();
}