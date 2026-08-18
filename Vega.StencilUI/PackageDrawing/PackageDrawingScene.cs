using System.Windows;
using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.PackageDrawing;

public enum PackageTopologyType { Chip, Melf, MiniMold, Sot223, Sod, PowerDiode, Sop, Soj, Qfp, Plcc, Qfn, Bga, Lga, PowerPackage, AluminumCap, Tantalum, Led, Inductor, Crystal, Oscillator, Connector, Generic }
public enum ScenePrimitiveKind { Body, Lead, Pad, Ball, Terminal, Tab, ExposedPad, Marker, SideBody, SideLead }
public sealed record ScenePrimitive(ScenePrimitiveKind Kind, Rect Bounds, string? Marker = null);
public sealed record DrawingScene(string TemplateId, string AlignmentType, IReadOnlyList<ScenePrimitive> Primitives, IReadOnlyList<string> Warnings)
{
    public Rect Bounds => Primitives.Count == 0 ? Rect.Empty : Primitives.Select(p => p.Bounds).Aggregate(Rect.Union);
    public int LeadCount => Primitives.Count(p => p.Kind == ScenePrimitiveKind.Lead);
    public int PadCount => Primitives.Count(p => p.Kind == ScenePrimitiveKind.Pad);
    public int BallCount => Primitives.Count(p => p.Kind == ScenePrimitiveKind.Ball);
}
public sealed record FitToViewportResult(double Scale, Vector Offset, Rect Bounds);
public sealed record PackageDrawingDiagnostics(string Template, string AlignmentType, int RenderedLeads, int RenderedPads, int RenderedBalls, double FitScale, IReadOnlyList<string> UnknownRequiredFields, IReadOnlyList<string> Warnings);

public static class ParametricPackageGeometryBuilder
{
    public static DrawingScene Build(PackageDefinition package)
    {
        var template = PackageDrawingTemplateResolver.Resolve(package);
        var warnings = new List<string>();
        var parts = new List<ScenePrimitive>();
        var x = Size(package.BodyLength, package.Length, "Body X", warnings);
        var y = Size(package.BodyWidth, package.Width, "Body Y", warnings);
        var body = new Rect(-x / 2, -y / 2, x, y);
        var topology = template.TopologyType;

        switch (topology)
        {
            case PackageTopologyType.Chip:
                Body(parts, body); ChipTerminations(parts, body); break;
            case PackageTopologyType.Melf:
                Body(parts, body); Terminals(parts, body, 2, false); break;
            case PackageTopologyType.MiniMold:
                Body(parts, body); MiniMold(parts, package, body, warnings); Marker(parts, body, "Pin1Dot"); break;
            case PackageTopologyType.Sot223:
                Body(parts, body); Sot223(parts, package, body, warnings); break;
            case PackageTopologyType.Sod:
            case PackageTopologyType.PowerDiode:
                Body(parts, body); Terminals(parts, body, 2, false); Marker(parts, body, "CathodeBar"); break;
            case PackageTopologyType.Sop:
            case PackageTopologyType.Soj:
                Body(parts, body); TwoSides(parts, package, body, warnings, topology == PackageTopologyType.Soj); break;
            case PackageTopologyType.Qfp:
            case PackageTopologyType.Plcc:
                Body(parts, body); FourSides(parts, package, body, warnings, false); Marker(parts, body, "Pin1Dot"); break;
            case PackageTopologyType.Qfn:
                Body(parts, body); FourSides(parts, package, body, warnings, true); ExposedPad(parts, package, body); Marker(parts, body, "Pin1Dot"); break;
            case PackageTopologyType.Bga:
            case PackageTopologyType.Lga:
                Body(parts, body); Array(parts, package, body, warnings, topology == PackageTopologyType.Bga); break;
            case PackageTopologyType.PowerPackage:
                Body(parts, body); Power(parts, package, body, warnings); break;
            case PackageTopologyType.AluminumCap:
                parts.Add(new(ScenePrimitiveKind.Body, body, "Circular")); Terminals(parts, body, 2, true); Marker(parts, body, "PlusMark"); break;
            case PackageTopologyType.Tantalum:
                Body(parts, body); Terminals(parts, body, 2, false); Marker(parts, body, "PlusMark"); break;
            case PackageTopologyType.Led:
                Body(parts, body); Terminals(parts, body, Math.Max(2, package.PadCount), false); Marker(parts, body, "CathodeBar"); break;
            case PackageTopologyType.Inductor:
            case PackageTopologyType.Crystal:
            case PackageTopologyType.Oscillator:
                Body(parts, body); Terminals(parts, body, Math.Max(2, package.PadCount), false); Marker(parts, body, "Pin1Dot"); break;
            default:
                Body(parts, body); warnings.Add("Drawing template not defined."); break;
        }

        if (package.LeadWidth > 0 && package.Pitch > 0 && package.LeadWidth > package.Pitch)
            warnings.Add("Lead width exceeds pitch; verify package data.");
        return new DrawingScene(template.Name, template.AlignmentType, parts, warnings);
    }

    public static FitToViewportResult Fit(DrawingScene scene, Size viewport, double safeMargin = .10)
    {
        var b = scene.Bounds;
        if (b.IsEmpty || viewport.Width <= 0 || viewport.Height <= 0) return new FitToViewportResult(1, new Vector(), Rect.Empty);
        var marginX = viewport.Width * safeMargin;
        var marginY = viewport.Height * safeMargin;
        var scale = Math.Min(Math.Max(1, viewport.Width - marginX * 2) / b.Width, Math.Max(1, viewport.Height - marginY * 2) / b.Height);
        var offset = new Vector((viewport.Width - b.Width * scale) / 2 - b.Left * scale, (viewport.Height - b.Height * scale) / 2 - b.Top * scale);
        return new FitToViewportResult(scale, offset, new Rect(b.X * scale + offset.X, b.Y * scale + offset.Y, b.Width * scale, b.Height * scale));
    }

    private static double Size(double primary, double secondary, string name, ICollection<string> warnings)
    {
        if (primary > 0) return primary;
        if (secondary > 0) return secondary;
        warnings.Add($"{name} is unknown.");
        return 1;
    }
    private static void Body(List<ScenePrimitive> p, Rect body) => p.Add(new(ScenePrimitiveKind.Body, body));
    private static void Marker(List<ScenePrimitive> p, Rect body, string marker) => p.Add(new(ScenePrimitiveKind.Marker, new Rect(body.Left + body.Width * .08, body.Top + body.Height * .08, body.Width * .10, body.Height * .10), marker));
    private static void ChipTerminations(List<ScenePrimitive> p, Rect body)
    {
        // Metallized end caps are within the ceramic body outline, not IC-like leads.
        var length = Math.Min(body.Width * .18, body.Width / 3);
        p.Add(new(ScenePrimitiveKind.Terminal, new Rect(body.Left, body.Top, length, body.Height)));
        p.Add(new(ScenePrimitiveKind.Terminal, new Rect(body.Right - length, body.Top, length, body.Height)));
    }
    private static void Terminals(List<ScenePrimitive> p, Rect body, int count, bool circular)
    {
        var terminal = Math.Max(body.Width, body.Height) * .12;
        if (count <= 2)
        {
            p.Add(new(ScenePrimitiveKind.Terminal, new Rect(body.Left - terminal, body.Top + body.Height * .25, terminal, body.Height * .5)));
            p.Add(new(ScenePrimitiveKind.Terminal, new Rect(body.Right, body.Top + body.Height * .25, terminal, body.Height * .5)));
            return;
        }
        for (var i = 0; i < count; i++)
        {
            var x = body.Left + (i + .5) * body.Width / count;
            p.Add(new(ScenePrimitiveKind.Terminal, new Rect(x - terminal / 2, body.Bottom - terminal, terminal, terminal)));
        }
    }
    private static void MiniMold(List<ScenePrimitive> p, PackageDefinition d, Rect body, List<string> warnings)
    {
        // SOT package outline: the asymmetric lead rows are on the long N/S sides,
        // while Body X is horizontal and Body Y is vertical.
        var total = Contacts(d);
        var north = (total + 1) / 2;
        var south = total - north;
        var projection = d.LeadLength > 0 ? d.LeadLength : body.Height * .12;
        var width = d.LeadWidth > 0 ? d.LeadWidth : Math.Min(body.Width / Math.Max(4, north), body.Width * .12);
        LeadRowHorizontal(p, body.Left, body.Width, body.Top, north, projection, width, -1, d.Pitch);
        LeadRowHorizontal(p, body.Left, body.Width, body.Bottom, south, projection, width, 1, d.Pitch);
    }

    private static void LeadRowHorizontal(List<ScenePrimitive> p, double left, double bodyWidth, double edge, int count, double projection, double width, int direction, double pitch)
    {
        if (count <= 0) return;
        var step = pitch > 0 ? pitch : bodyWidth / count;
        for (var i = 0; i < count; i++)
        {
            var x = left + bodyWidth / 2 - (count - 1) * step / 2 + i * step;
            p.Add(new(ScenePrimitiveKind.Lead, new Rect(x - width / 2, direction < 0 ? edge - projection : edge, width, projection)));
        }
    }
    private static void Sot223(List<ScenePrimitive> p, PackageDefinition d, Rect body, List<string> warnings)
    {
        var total = Contacts(d);
        var small = Math.Max(1, total - 1);
        var projection = d.LeadLength > 0 ? d.LeadLength : body.Height * .15;
        var width = d.LeadWidth > 0 ? d.LeadWidth : body.Width * .08;
        for (var i = 0; i < small; i++)
        {
            var x = body.Left + (i + .5) * body.Width / small;
            p.Add(new(ScenePrimitiveKind.Lead, new Rect(x - width / 2, body.Bottom, width, projection)));
        }
        p.Add(new(ScenePrimitiveKind.Tab, new Rect(body.Left + body.Width * .18, body.Top - projection * 1.5, body.Width * .64, projection * 1.5)));
    }
    private static void TwoSides(List<ScenePrimitive> p, PackageDefinition d, Rect body, List<string> warnings, bool jLead)
    {
        var total = Contacts(d);
        if (total % 2 != 0) { warnings.Add("Lead distribution requires verification."); return; }
        var perSide = total / 2;
        var projection = d.LeadLength > 0 ? d.LeadLength : body.Width * .12;
        var width = d.LeadWidth > 0 ? d.LeadWidth : body.Height / Math.Max(8, perSide * 3);
        LeadRow(p, body.Left, body.Top, body.Height, perSide, projection, width, -1, d.Pitch);
        LeadRow(p, body.Right, body.Top, body.Height, perSide, projection, width, 1, d.Pitch);
    }
    private static void FourSides(List<ScenePrimitive> p, PackageDefinition d, Rect body, List<string> warnings, bool pads)
    {
        var total = Contacts(d);
        if (total % 4 != 0) { warnings.Add("Lead distribution requires verification."); return; }
        var perSide = total / 4;
        var projection = pads ? Math.Min(body.Width, body.Height) * .06 : (d.LeadLength > 0 ? d.LeadLength : body.Width * .12);
        var width = d.LeadWidth > 0 ? d.LeadWidth : Math.Min(body.Width, body.Height) / Math.Max(10, perSide * 3);
        for (var i = 0; i < perSide; i++)
        {
            var x = body.Left + (i + .5) * body.Width / perSide;
            var y = body.Top + (i + .5) * body.Height / perSide;
            var kind = pads ? ScenePrimitiveKind.Pad : ScenePrimitiveKind.Lead;
            p.Add(new(kind, new Rect(x - width / 2, body.Top - projection, width, projection)));
            p.Add(new(kind, new Rect(x - width / 2, body.Bottom, width, projection)));
            p.Add(new(kind, new Rect(body.Left - projection, y - width / 2, projection, width)));
            p.Add(new(kind, new Rect(body.Right, y - width / 2, projection, width)));
        }
    }
    private static void Power(List<ScenePrimitive> p, PackageDefinition d, Rect body, List<string> warnings)
    {
        var count = Contacts(d);
        var projection = d.LeadLength > 0 ? d.LeadLength : body.Height * .15;
        var width = d.LeadWidth > 0 ? d.LeadWidth : body.Width / Math.Max(8, count * 3);
        var spacing = d.Pitch > 0 ? d.Pitch : body.Width / count;
        for (var i = 0; i < count; i++)
        {
            var x = body.Left + body.Width / 2 - (count - 1) * spacing / 2 + i * spacing;
            p.Add(new(ScenePrimitiveKind.Lead, new Rect(x - width / 2, body.Bottom, width, projection)));
        }
        // TO-252 / TO-263 exposed power tab is an electrical lead and must visibly extend beyond the molded body.
        var tabHeight = Math.Max(projection * 1.35, body.Height * .24);
        p.Add(new(ScenePrimitiveKind.Tab, new Rect(body.Left + body.Width * .12, body.Top - tabHeight, body.Width * .76, tabHeight)));
        p.Add(new(ScenePrimitiveKind.SideBody, new Rect(body.Left, body.Bottom + projection * 3, body.Width * .55, Math.Max(.2, d.Height))));
        p.Add(new(ScenePrimitiveKind.SideLead, new Rect(body.Left + body.Width * .45, body.Bottom + projection * 3 + Math.Max(.2, d.Height), body.Width * .45, Math.Max(.1, projection * .25))));
    }
    private static void ExposedPad(List<ScenePrimitive> p, PackageDefinition d, Rect body)
    {
        if (d.ThermalPadLength <= 0 || d.ThermalPadWidth <= 0) return;
        p.Add(new(ScenePrimitiveKind.ExposedPad, new Rect(-d.ThermalPadLength / 2, -d.ThermalPadWidth / 2, d.ThermalPadLength, d.ThermalPadWidth)));
    }
    private static void Array(List<ScenePrimitive> p, PackageDefinition d, Rect body, List<string> warnings, bool balls)
    {
        var total = Math.Max(1, d.PadCount);
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(total)));
        var rows = (int)Math.Ceiling(total / (double)columns);
        var pitch = d.BallPitch > 0 ? d.BallPitch : Math.Min(body.Width / Math.Max(2, columns), body.Height / Math.Max(2, rows));
        var diameter = d.BallDiameter > 0 ? d.BallDiameter : Math.Min(pitch * .45, Math.Min(body.Width, body.Height) * .12);
        var firstX = -(columns - 1) * pitch / 2;
        var firstY = -(rows - 1) * pitch / 2;
        for (var i = 0; i < total; i++)
        {
            var x = firstX + i % columns * pitch;
            var y = firstY + i / columns * pitch;
            p.Add(new(balls ? ScenePrimitiveKind.Ball : ScenePrimitiveKind.Pad, new Rect(x - diameter / 2, y - diameter / 2, diameter, diameter)));
        }
    }
    private static void LeadRow(List<ScenePrimitive> p, double edge, double top, double bodyHeight, int count, double projection, double width, int direction, double pitch)
    {
        if (count <= 0) return;
        for (var i = 0; i < count; i++)
        {
            var step = pitch > 0 ? pitch : bodyHeight / count;
            var y = top + bodyHeight / 2 - (count - 1) * step / 2 + i * step;
            p.Add(new(ScenePrimitiveKind.Lead, new Rect(direction < 0 ? edge - projection : edge, y - width / 2, projection, width)));
        }
    }
    private static int Contacts(PackageDefinition d) => Math.Max(1, d.LeadCount > 0 ? d.LeadCount : d.PadCount);
}

