using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Vega.Altium.Models;

namespace Vega.Altium;

public class AltiumPcbDocParserService
{
    private static readonly byte[] CompoundDocumentHeader = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
    private string? _fileName;
    private byte[] _fileBytes = Array.Empty<byte>();
    private List<AltiumComponent>? _components;

    public void Load(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("PcbDoc file path is required.", nameof(fileName));
        if (!File.Exists(fileName))
            throw new FileNotFoundException("PcbDoc file was not found.", fileName);

        _fileName = fileName;
        _fileBytes = File.ReadAllBytes(fileName);
        _components = null;
    }

    public List<AltiumComponent> ParseComponents()
    {
        EnsureLoaded();
        if (_components is not null)
            return _components.ToList();

        var records = ExtractRecords(ExtractSearchableText());
        _components = records
            .Select(ToComponent)
            .Where(component => !string.IsNullOrWhiteSpace(component.RefDes))
            .GroupBy(component => component.RefDes, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        return _components.ToList();
    }

    public List<AltiumBomItem> ExportBom()
    {
        return ParseComponents()
            .GroupBy(component => string.IsNullOrWhiteSpace(component.ManufacturerPartNumber)
                ? $"{component.Footprint}|{component.Value}|{component.Comment}"
                : component.ManufacturerPartNumber, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AltiumBomItem
            {
                PartNumber = group.First().ManufacturerPartNumber,
                Description = group.First().Description,
                Manufacturer = group.First().Manufacturer,
                Package = group.First().Footprint,
                Quantity = group.Sum(component => component.Quantity),
                Components = group.Select(component => component.RefDes).OrderBy(value => value).ToList()
            })
            .ToList();
    }

    public List<AltiumPnpItem> ExportPickAndPlace() => ParseComponents()
        .Select(component => new AltiumPnpItem
        {
            RefDes = component.RefDes, X = component.X, Y = component.Y, Rotation = component.Rotation,
            Layer = component.Layer, Footprint = component.Footprint, Comment = component.Comment
        })
        .ToList();

    public AltiumImportResult Parse() => new()
    {
        ProjectName = Path.GetFileNameWithoutExtension(_fileName!),
        Components = ParseComponents(), Bom = ExportBom(), PickAndPlace = ExportPickAndPlace()
    };

    public Vega.CAD.Models.PcbProject ImportPcbProject()
    {
        return new AltiumPcbProjectAdapter().Adapt(Parse(), _fileName!);
    }

    public string ExportBomCsv()
    {
        var rows = ParseComponents().Select(component => new[]
        {
            component.RefDes, component.Comment, component.Value, component.Footprint,
            component.Quantity.ToString(CultureInfo.InvariantCulture)
        });
        return ToCsv([ ["Designator", "Comment", "Value", "Footprint", "Quantity"], .. rows ]);
    }

    public string ExportPickAndPlaceCsv()
    {
        var rows = ExportPickAndPlace().Select(item => new[]
        {
            item.RefDes, Format(item.X), Format(item.Y), Format(item.Rotation), item.Layer, item.Footprint
        });
        return ToCsv([ ["RefDes", "X", "Y", "Rotation", "Layer", "Footprint"], .. rows ]);
    }

    private string ExtractSearchableText()
    {
        var ascii = Encoding.Latin1.GetString(_fileBytes);
        var utf16 = Encoding.Unicode.GetString(_fileBytes);
        return Normalize(ascii) + "\n" + Normalize(utf16);
    }

    private static string Normalize(string value) => new(value.Select(character =>
        character is >= ' ' and <= '~' || character is '\r' or '\n' or '|' or '=' or ';'
            ? character : '\n').ToArray());

    private static List<Dictionary<string, string>> ExtractRecords(string text)
    {
        var records = new List<Dictionary<string, string>>();
        foreach (var line in text.Split(['\r', '\n', '\0'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.Contains("Component", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("RECORD=Component", StringComparison.OrdinalIgnoreCase))
                continue;

            var fields = ParseFields(line);
            if (Get(fields, "RefDes", "Designator", "DesignatorText").Length > 0)
                records.Add(fields);
        }
        return records;
    }

    private static Dictionary<string, string> ParseFields(string record)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in record.Split(['|', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = token.IndexOf('=');
            if (separator <= 0)
                continue;
            fields[token[..separator].Trim()] = token[(separator + 1)..].Trim();
        }
        return fields;
    }

    private static AltiumComponent ToComponent(IReadOnlyDictionary<string, string> fields) => new()
    {
        RefDes = Get(fields, "RefDes", "Designator", "DesignatorText"),
        Comment = Get(fields, "Comment"), Value = Get(fields, "Value", "Comment"),
        Description = Get(fields, "Description"), Footprint = Get(fields, "Footprint", "Pattern", "PCBFootprint"),
        Manufacturer = Get(fields, "Manufacturer", "Mfr"),
        ManufacturerPartNumber = Get(fields, "ManufacturerPartNumber", "MPN", "PartNumber"),
        Layer = Get(fields, "Layer", "LayerName"), X = GetDouble(fields, "X", "XLocation"),
        Y = GetDouble(fields, "Y", "YLocation"), Rotation = GetDouble(fields, "Rotation", "RotationAngle"),
        Quantity = Math.Max(1, (int)GetDouble(fields, "Quantity"))
    };

    private static string Get(IReadOnlyDictionary<string, string> fields, params string[] keys) =>
        keys.Select(key => fields.TryGetValue(key, out var value) ? value : "").FirstOrDefault(value => value.Length > 0) ?? "";

    private static double GetDouble(IReadOnlyDictionary<string, string> fields, params string[] keys)
    {
        var value = Get(fields, keys);
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }

    private static string ToCsv(IEnumerable<string[]> rows) => string.Join(Environment.NewLine, rows.Select(row => string.Join(',', row.Select(EscapeCsv))));
    private static string EscapeCsv(string value) => value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    private static string Format(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private void EnsureLoaded()
    {
        if (_fileName is null)
            throw new InvalidOperationException("Load must be called before parsing.");
        if (_fileBytes.Length >= CompoundDocumentHeader.Length && _fileBytes[..CompoundDocumentHeader.Length].SequenceEqual(CompoundDocumentHeader))
            return;
    }
}


