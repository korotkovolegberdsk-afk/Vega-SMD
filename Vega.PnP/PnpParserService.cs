using System.Globalization;
using Vega.PnP.Models;

namespace Vega.PnP;

public class PnpParserService
{
    public List<PnpComponent> Parse(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Укажите путь к PnP-файлу.", nameof(fileName));
        }

        var lines = File.ReadLines(fileName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        if (lines.Count == 0)
        {
            return new List<PnpComponent>();
        }

        var separator = DetectSeparator(lines[0]);
        var headers = Split(lines[0], separator);
        var indexes = headers
            .Select((value, index) => new { Name = value.Trim(), Index = index })
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

        return lines.Skip(1)
            .Select(line => ParseComponent(Split(line, separator), indexes))
            .ToList();
    }

    private static PnpComponent ParseComponent(
        string[] values,
        IReadOnlyDictionary<string, int> indexes)
    {
        return new PnpComponent
        {
            RefDes = GetValue(values, indexes, "RefDes"),
            PartNumber = GetValue(values, indexes, "PartNumber"),
            PackageName = GetValue(values, indexes, "PackageName"),
            X = ParseDouble(GetValue(values, indexes, "X")),
            Y = ParseDouble(GetValue(values, indexes, "Y")),
            Rotation = ParseDouble(GetValue(values, indexes, "Rotation")),
            Side = GetValue(values, indexes, "Side")
        };
    }

    private static char DetectSeparator(string header)
    {
        return header.Contains('\t') ? '\t'
            : header.Contains(';') ? ';'
            : ',';
    }

    private static string[] Split(string line, char separator) =>
        line.Split(separator).Select(x => x.Trim()).ToArray();

    private static string GetValue(
        IReadOnlyList<string> values,
        IReadOnlyDictionary<string, int> indexes,
        string field)
    {
        if (!indexes.TryGetValue(field, out var index) || index >= values.Count)
        {
            return "";
        }

        return values[index];
    }

    private static double ParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            || double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
        {
            return parsed;
        }

        throw new FormatException($"Некорректное числовое значение PnP: {value}.");
    }
}
