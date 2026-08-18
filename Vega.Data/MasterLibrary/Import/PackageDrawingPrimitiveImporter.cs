using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Import;

public sealed class PackageDrawingPrimitiveImporter
{
    private static readonly Regex PathToken = new(@"[MLHVZmlhvz]|[-+]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][-+]?\d+)?", RegexOptions.Compiled);

    public IReadOnlyList<PackageDrawingPrimitive> ImportSvg(int projectionId, string filePath)
    {
        if (projectionId <= 0) throw new ArgumentOutOfRangeException(nameof(projectionId));
        var root = XDocument.Load(filePath, LoadOptions.PreserveWhitespace).Root ?? throw new InvalidDataException("SVG root is missing.");
        var scale = ReadScale(root);
        var result = new List<PackageDrawingPrimitive>();
        foreach (var e in root.Descendants())
        {
            var layer = ResolveLayer(e); var stroke = ReadOptional(e.Attribute("stroke-width")?.Value, scale);
            switch (e.Name.LocalName)
            {
                case "line": result.Add(new PackageDrawingPrimitive { ProjectionGeometryId = projectionId, PrimitiveType = "Line", Layer = layer, CoordinateSystem = "mm", StrokeWidth = stroke, StartX = Required(e, "x1", scale), StartY = Required(e, "y1", scale), EndX = Required(e, "x2", scale), EndY = Required(e, "y2", scale) }); break;
                case "circle": result.Add(new PackageDrawingPrimitive { ProjectionGeometryId = projectionId, PrimitiveType = "Circle", Layer = layer, CoordinateSystem = "mm", StrokeWidth = stroke, CenterX = Required(e, "cx", scale), CenterY = Required(e, "cy", scale), Radius = Required(e, "r", scale) }); break;
                case "path":
                    var points = ParsePath(e.Attribute("d")?.Value);
                    if (points.Count < 2) throw new InvalidDataException("SVG path must contain at least two line points.");
                    result.Add(new PackageDrawingPrimitive { ProjectionGeometryId = projectionId, PrimitiveType = "Polygon", Layer = layer, CoordinateSystem = "mm", StrokeWidth = stroke, GeometryData = JsonSerializer.Serialize(points.Select(p => new[] { p.X * scale, p.Y * scale })) });
                    break;
            }
        }
        return result;
    }

    private static double ReadScale(XElement root)
    {
        static double Physical(string? raw, string name)
        {
            if (string.IsNullOrWhiteSpace(raw)) throw new InvalidDataException($"SVG {name} must declare mm units.");
            var m = Regex.Match(raw, @"^\s*([-+]?(?:\d+(?:\.\d*)?|\.\d+))\s*([A-Za-z]+)\s*$");
            if (!m.Success || !m.Groups[2].Value.Equals("mm", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException($"SVG {name} must use mm; pixels are rejected.");
            return double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }
        var width = Physical(root.Attribute("width")?.Value, "width"); var height = Physical(root.Attribute("height")?.Value, "height");
        var v = (root.Attribute("viewBox")?.Value ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (v.Length != 4 || !double.TryParse(v[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) || !double.TryParse(v[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) || !double.TryParse(v[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var w) || !double.TryParse(v[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var h) || w <= 0 || h <= 0 || Math.Abs(x) > 1e-9 || Math.Abs(y) > 1e-9) throw new InvalidDataException("SVG viewBox must be numeric and start at 0,0.");
        var sx = width / w; var sy = height / h; if (Math.Abs(sx - sy) > 1e-9) throw new InvalidDataException("SVG scale is not preserved."); return sx;
    }

    private static double Required(XElement e, string name, double scale)
    { var raw = e.Attribute(name)?.Value ?? throw new InvalidDataException($"SVG {name} is required."); if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var n)) throw new InvalidDataException($"SVG {name} is not numeric."); return n * scale; }
    private static double? ReadOptional(string? raw, double scale) => double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n * scale : null;

    private static string ResolveLayer(XElement e)
    { var s = string.Join(" ", new[] { e.Attribute("data-layer")?.Value, e.Attribute("layer")?.Value, e.Attribute("id")?.Value, e.Attribute("class")?.Value }).ToLowerInvariant(); if (s.Contains("pin1") || s.Contains("pin-1")) return "Pin1"; if (s.Contains("body")) return "Body"; if (s.Contains("lead")) return "Lead"; if (s.Contains("dim")) return "Dimension"; return "Auxiliary"; }

    private static List<(double X, double Y)> ParsePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return [];
        var t = PathToken.Matches(path).Select(x => x.Value).ToArray(); var r = new List<(double X, double Y)>(); var i = 0; var cmd = 'M'; var cur = (X: 0d, Y: 0d); var start = cur;
        while (i < t.Length)
        {
            if (t[i].Length == 1 && char.IsLetter(t[i][0])) cmd = t[i++][0];
            if (char.ToUpperInvariant(cmd) == 'Z') { cur = start; cmd = 'L'; continue; }
            var rel = char.IsLower(cmd); var c = char.ToUpperInvariant(cmd);
            if (c is 'H' or 'V') { if (i >= t.Length) throw new InvalidDataException("Invalid SVG path."); var n = double.Parse(t[i++], CultureInfo.InvariantCulture); cur = c == 'H' ? (rel ? cur.X + n : n, cur.Y) : (cur.X, rel ? cur.Y + n : n); r.Add(cur); continue; }
            if (c is not ('M' or 'L') || i + 1 >= t.Length) throw new InvalidDataException("Only SVG line paths are supported.");
            var px = double.Parse(t[i++], CultureInfo.InvariantCulture); var py = double.Parse(t[i++], CultureInfo.InvariantCulture); cur = rel ? (cur.X + px, cur.Y + py) : (px, py); if (c == 'M') start = cur; r.Add(cur); cmd = c == 'M' ? (rel ? 'l' : 'L') : cmd;
        }
        return r;
    }
}
