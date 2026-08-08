using System.Globalization;
using System.Text.RegularExpressions;
using Vega.Gerber.Models;

namespace Vega.Gerber;

public class GerberPasteParserService
{
    private static readonly Regex FormatRegex = new(
        @"%FSL.AX(?<xInteger>\d)(?<xDecimal>\d)Y(?<yInteger>\d)(?<yDecimal>\d)\*%",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ApertureRegex = new(
        @"%ADD(?<id>\d+)(?<shape>[CRO]),?(?<first>[\d.]+)?(?:X(?<second>[\d.]+))?\*%",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ApertureSelectRegex = new(
        @"^D(?<id>\d+)\*$",
        RegexOptions.Compiled);

    private static readonly Regex FlashRegex = new(
        @"^(?:X(?<x>[+-]?\d+))?(?:Y(?<y>[+-]?\d+))?D03\*$",
        RegexOptions.Compiled);

    private string? _fileName;
    private string[] _lines = Array.Empty<string>();

    public void Load(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Укажите путь к Gerber-файлу.", nameof(fileName));
        }

        _fileName = fileName;
        _lines = File.ReadAllLines(fileName);
    }

    public PasteLayer Parse()
    {
        if (_fileName is null)
        {
            throw new InvalidOperationException("Сначала вызовите Load.");
        }

        var layer = new PasteLayer
        {
            FileName = Path.GetFileName(_fileName),
            Side = GetSide(_fileName)
        };
        var apertures = new Dictionary<int, GerberAperture>();
        var coordinateFormat = new CoordinateFormat(2, 4, 2, 4);
        var selectedApertureId = 0;
        var currentX = 0d;
        var currentY = 0d;

        foreach (var rawLine in _lines)
        {
            var line = rawLine.Trim();
            var formatMatch = FormatRegex.Match(line);
            if (formatMatch.Success)
            {
                coordinateFormat = new CoordinateFormat(
                    int.Parse(formatMatch.Groups["xInteger"].Value),
                    int.Parse(formatMatch.Groups["xDecimal"].Value),
                    int.Parse(formatMatch.Groups["yInteger"].Value),
                    int.Parse(formatMatch.Groups["yDecimal"].Value));
                continue;
            }

            var apertureMatch = ApertureRegex.Match(line);
            if (apertureMatch.Success)
            {
                var aperture = CreateAperture(apertureMatch);
                apertures.Add(aperture.ApertureId, aperture);
                layer.Apertures.Add(aperture);
                continue;
            }

            var selectMatch = ApertureSelectRegex.Match(line);
            if (selectMatch.Success)
            {
                selectedApertureId = int.Parse(selectMatch.Groups["id"].Value);
                continue;
            }

            var flashMatch = FlashRegex.Match(line);
            if (!flashMatch.Success || !apertures.TryGetValue(selectedApertureId, out var selectedAperture))
            {
                continue;
            }

            if (flashMatch.Groups["x"].Success)
            {
                currentX = ParseCoordinate(flashMatch.Groups["x"].Value, coordinateFormat.XDecimal);
            }

            if (flashMatch.Groups["y"].Success)
            {
                currentY = ParseCoordinate(flashMatch.Groups["y"].Value, coordinateFormat.YDecimal);
            }

            layer.Primitives.Add(CreatePrimitive(currentX, currentY, selectedAperture));
        }

        return layer;
    }

    private static GerberAperture CreateAperture(Match match)
    {
        var id = int.Parse(match.Groups["id"].Value);
        var shape = match.Groups["shape"].Value.ToUpperInvariant();
        var first = ParseDimension(match.Groups["first"].Value);
        var second = match.Groups["second"].Success
            ? ParseDimension(match.Groups["second"].Value)
            : first;

        return shape switch
        {
            "C" => new GerberAperture
            {
                ApertureId = id,
                Shape = "Circle",
                Width = first,
                Height = first,
                Diameter = first
            },
            "R" => new GerberAperture
            {
                ApertureId = id,
                Shape = "Rectangle",
                Width = first,
                Height = second
            },
            "O" => new GerberAperture
            {
                ApertureId = id,
                Shape = "Obround",
                Width = first,
                Height = second
            },
            _ => throw new FormatException("Неподдерживаемая форма апертуры.")
        };
    }

    private static PastePrimitive CreatePrimitive(
        double x,
        double y,
        GerberAperture aperture)
    {
        var (area, perimeter) = aperture.Shape switch
        {
            "Circle" =>
                (Math.PI * aperture.Diameter * aperture.Diameter / 4,
                 Math.PI * aperture.Diameter),
            "Obround" => ObroundMetrics(aperture.Width, aperture.Height),
            _ =>
                (aperture.Width * aperture.Height,
                 2 * (aperture.Width + aperture.Height))
        };

        return new PastePrimitive
        {
            X = x,
            Y = y,
            Rotation = 0,
            ApertureId = aperture.ApertureId,
            Width = aperture.Width,
            Height = aperture.Height,
            Area = area,
            Perimeter = perimeter
        };
    }

    private static (double Area, double Perimeter) ObroundMetrics(double width, double height)
    {
        var minor = Math.Min(width, height);
        var major = Math.Max(width, height);
        return (
            Math.PI * minor * minor / 4 + minor * (major - minor),
            Math.PI * minor + 2 * (major - minor));
    }

    private static double ParseDimension(string value) =>
        double.Parse(value, CultureInfo.InvariantCulture);

    private static double ParseCoordinate(string value, int decimalPlaces)
    {
        var sign = value.StartsWith('-') ? -1 : 1;
        var digits = value.TrimStart('+', '-');
        return sign * long.Parse(digits, CultureInfo.InvariantCulture)
            / Math.Pow(10, decimalPlaces);
    }

    private static string GetSide(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return extension.Equals(".gbs", StringComparison.OrdinalIgnoreCase)
            || fileName.Contains("bottom", StringComparison.OrdinalIgnoreCase)
            ? "Bottom"
            : "Top";
    }

    private readonly record struct CoordinateFormat(
        int XInteger,
        int XDecimal,
        int YInteger,
        int YDecimal);
}
