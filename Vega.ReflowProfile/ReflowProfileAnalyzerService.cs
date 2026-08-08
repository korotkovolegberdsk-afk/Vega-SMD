using Vega.ReflowProfile.Models;

namespace Vega.ReflowProfile;

public class ReflowProfileAnalyzerService
{
    public ReflowProfileAnalysis Analyze(int profileId, IReadOnlyList<ReflowProfilePoint> points, double liquidusTemperature = 217)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 2) throw new ArgumentException("At least two profile points are required.", nameof(points));
        var ordered = points.OrderBy(point => point.TimeSeconds).ToArray();
        var peak = CalculatePeak(ordered);
        var analysis = new ReflowProfileAnalysis
        {
            ProfileId = profileId,
            RampRate = CalculateRampRate(ordered),
            SoakStart = 150,
            SoakEnd = 180,
            SoakTime = CalculateSoak(ordered),
            LiquidusTemperature = liquidusTemperature,
            TimeAboveLiquidus = CalculateTAL(ordered, liquidusTemperature),
            PeakTemperature = peak,
            CoolingRate = CalculateCoolingRate(ordered),
        };
        analysis.Status = analysis.PeakTemperature is < 235 or > 250 || analysis.TimeAboveLiquidus is < 40 or > 90
            ? ReflowProfileStatus.Warning : ReflowProfileStatus.OK;
        return analysis;
    }

    public double CalculateRampRate(IReadOnlyList<ReflowProfilePoint> points)
    {
        var slopes = Segments(points).Where(segment => segment.Start.TemperatureC < 150 && segment.End.TemperatureC <= 180 && segment.Slope > 0).Select(segment => segment.Slope);
        return slopes.DefaultIfEmpty(0).Max();
    }

    public double CalculateSoak(IReadOnlyList<ReflowProfilePoint> points) => TimeInRange(points, 150, 180);

    public double CalculateTAL(IReadOnlyList<ReflowProfilePoint> points, double liquidusTemperature = 217) => TimeAbove(points, liquidusTemperature);

    public double CalculatePeak(IReadOnlyList<ReflowProfilePoint> points) => points.Count == 0 ? 0 : points.Max(point => point.TemperatureC);

    public double CalculateCoolingRate(IReadOnlyList<ReflowProfilePoint> points)
    {
        var peakIndex = points.Select((point, index) => (point, index)).OrderByDescending(item => item.point.TemperatureC).First().index;
        var slopes = Segments(points.Skip(peakIndex).ToArray()).Where(segment => segment.Slope < 0).Select(segment => -segment.Slope);
        return slopes.DefaultIfEmpty(0).Max();
    }

    public IReadOnlyList<ReflowProfileRecommendation> GetRecommendations(ReflowProfileAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        return
        [
            Recommendation(analysis.ProfileId, "Peak Temperature", analysis.PeakTemperature, "235-250 °C", analysis.PeakTemperature is >= 235 and <= 250),
            Recommendation(analysis.ProfileId, "TAL", analysis.TimeAboveLiquidus, "40-90 sec", analysis.TimeAboveLiquidus is >= 40 and <= 90),
            Recommendation(analysis.ProfileId, "Ramp Rate", analysis.RampRate, "0.5-3.0 °C/sec", analysis.RampRate is >= .5 and <= 3),
            Recommendation(analysis.ProfileId, "Cooling Rate", analysis.CoolingRate, "1.0-4.0 °C/sec", analysis.CoolingRate is >= 1 and <= 4)
        ];
    }

    private static ReflowProfileRecommendation Recommendation(int id, string parameter, double value, string range, bool ok) => new()
    {
        ProfileId = id, Parameter = parameter, CurrentValue = value, RecommendedRange = range,
        Status = ok ? ReflowProfileStatus.OK : ReflowProfileStatus.Warning,
        Message = ok ? "Within recommended range." : "Outside recommended range."
    };

    private static double TimeInRange(IReadOnlyList<ReflowProfilePoint> points, double low, double high) =>
        Segments(points).Sum(segment => DurationInRange(segment, low, high));

    private static double TimeAbove(IReadOnlyList<ReflowProfilePoint> points, double threshold) =>
        Segments(points).Sum(segment => DurationInRange(segment, threshold, double.PositiveInfinity));

    private static double DurationInRange(Segment segment, double low, double high)
    {
        var duration = segment.End.TimeSeconds - segment.Start.TimeSeconds;
        if (duration <= 0) return 0;
        var start = segment.Start.TemperatureC;
        var end = segment.End.TemperatureC;
        var delta = end - start;
        if (Math.Abs(delta) < double.Epsilon) return start >= low && start <= high ? duration : 0;

        double intervalStart = 0;
        double intervalEnd = 1;
        if (delta > 0)
        {
            if (end < low || start > high) return 0;
            if (start < low) intervalStart = (low - start) / delta;
            if (!double.IsPositiveInfinity(high) && end > high) intervalEnd = (high - start) / delta;
        }
        else
        {
            if (start < low || end > high) return 0;
            if (!double.IsPositiveInfinity(high) && start > high) intervalStart = (high - start) / delta;
            if (end < low) intervalEnd = (low - start) / delta;
        }

        return Math.Max(0, Math.Min(1, intervalEnd) - Math.Max(0, intervalStart)) * duration;
    }
    private static IEnumerable<Segment> Segments(IReadOnlyList<ReflowProfilePoint> points) => points.OrderBy(point => point.TimeSeconds).Zip(points.OrderBy(point => point.TimeSeconds).Skip(1), (start, end) => new Segment(start, end, (end.TemperatureC - start.TemperatureC) / (end.TimeSeconds - start.TimeSeconds))).Where(segment => !double.IsNaN(segment.Slope) && !double.IsInfinity(segment.Slope));

    private sealed record Segment(ReflowProfilePoint Start, ReflowProfilePoint End, double Slope);
}