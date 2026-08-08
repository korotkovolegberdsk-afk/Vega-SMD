using System.Globalization;
using Vega.ReflowProfile.Models;

namespace Vega.ReflowProfile;

public class ReflowProfileImportService
{
    public List<ReflowProfilePoint> Import(string fileName, int profileId = 0, string sensorChannel = "Imported")
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Input file is required.", nameof(fileName));
        return Parse(File.ReadLines(fileName), profileId, sensorChannel);
    }

    public List<ReflowProfilePoint> Parse(IEnumerable<string> lines, int profileId = 0, string sensorChannel = "Imported")
    {
        ArgumentNullException.ThrowIfNull(lines);
        var result = new List<ReflowProfilePoint>();
        foreach (var line in lines.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var parts = line.Split([',', ';', '\t'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || !TryNumber(parts[0], out var time) || !TryNumber(parts[1], out var temperature)) continue;
            result.Add(new ReflowProfilePoint { ProfileId = profileId, TimeSeconds = time, TemperatureC = temperature, SensorChannel = sensorChannel });
        }
        return result.OrderBy(point => point.TimeSeconds).ToList();
    }

    private static bool TryNumber(string value, out double result) => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) || double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
}