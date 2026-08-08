using System.Globalization;
using Vega.Gerber.Models;

namespace Vega.Gerber;

public class GerberPasteWriterService
{
    private const int CoordinateDecimalPlaces = 4;

    public void Write(CorrectedPasteLayer layer, string outputFile)
    {
        ArgumentNullException.ThrowIfNull(layer);
        if (string.IsNullOrWhiteSpace(outputFile))
        {
            throw new ArgumentException("Output file path is required.", nameof(outputFile));
        }

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputFile));
        Directory.CreateDirectory(outputDirectory!);

        var apertures = CreateApertures(layer.CorrectedPrimitives);
        var lines = new List<string>
        {
            "G04 Vega-SMD corrected paste layer*",
            "%FSLAX24Y24*%",
            "%MOMM*%"
        };

        foreach (var aperture in apertures)
        {
            lines.Add($"%ADD{aperture.Id}R,{FormatDimension(aperture.Width)}X{FormatDimension(aperture.Height)}*%");
        }

        int? selectedAperture = null;
        foreach (var primitive in layer.CorrectedPrimitives)
        {
            var aperture = apertures.Single(item => item.Width == primitive.Width && item.Height == primitive.Height);
            if (selectedAperture != aperture.Id)
            {
                lines.Add($"D{aperture.Id}*");
                selectedAperture = aperture.Id;
            }

            lines.Add($"X{FormatCoordinate(primitive.X)}Y{FormatCoordinate(primitive.Y)}D03*");
        }

        lines.Add("M02*");
        File.WriteAllLines(outputFile, lines);
    }

    public GerberCompareReport CreateCompareReport(CorrectedPasteLayer layer, string correctedFile)
    {
        ArgumentNullException.ThrowIfNull(layer);

        var originalCount = layer.OriginalPrimitiveCount;
        var correctedCount = layer.CorrectedPrimitiveCount;
        return new GerberCompareReport
        {
            OriginalFile = layer.OriginalFileName,
            CorrectedFile = correctedFile,
            ModifiedCount = layer.Changes.Count,
            AddedCount = Math.Max(0, correctedCount - originalCount),
            RemovedCount = Math.Max(0, originalCount - correctedCount)
        };
    }

    private static List<OutputAperture> CreateApertures(IReadOnlyList<PastePrimitive> primitives)
    {
        return primitives
            .Select(primitive => new { primitive.Width, primitive.Height })
            .Distinct()
            .Select((geometry, index) => new OutputAperture(index + 10, geometry.Width, geometry.Height))
            .ToList();
    }

    private static string FormatDimension(double value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);

    private static string FormatCoordinate(double value)
    {
        var scaled = Math.Round(value * Math.Pow(10, CoordinateDecimalPlaces), MidpointRounding.AwayFromZero);
        return ((long)scaled).ToString("+000000;-000000;000000", CultureInfo.InvariantCulture);
    }

    private sealed record OutputAperture(int Id, double Width, double Height);
}

