using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;
using Vega.StencilUI.PackageDrawing;
using Vega.Services.MasterLibrary;

namespace Vega.StencilUI.Controls;

public sealed class PackageDrawingPreview : FrameworkElement
{
    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(nameof(Package), typeof(PackageDefinition), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty RefreshVersionProperty = DependencyProperty.Register(nameof(RefreshVersion), typeof(int), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ShowDimensionsProperty = DependencyProperty.Register(nameof(ShowDimensions), typeof(bool), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ShowEngineeringViewsProperty = DependencyProperty.Register(nameof(ShowEngineeringViews), typeof(bool), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(nameof(Geometry), typeof(PackageGeometry), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ShowSideViewOnlyProperty = DependencyProperty.Register(nameof(ShowSideViewOnly), typeof(bool), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public PackageDefinition? Package { get => (PackageDefinition?)GetValue(PackageProperty); set => SetValue(PackageProperty, value); }
    public int RefreshVersion { get => (int)GetValue(RefreshVersionProperty); set => SetValue(RefreshVersionProperty, value); }
    public bool ShowDimensions { get => (bool)GetValue(ShowDimensionsProperty); set => SetValue(ShowDimensionsProperty, value); }
    public bool ShowEngineeringViews { get => (bool)GetValue(ShowEngineeringViewsProperty); set => SetValue(ShowEngineeringViewsProperty, value); }
    public PackageGeometry? Geometry { get => (PackageGeometry?)GetValue(GeometryProperty); set => SetValue(GeometryProperty, value); }
    public bool ShowSideViewOnly { get => (bool)GetValue(ShowSideViewOnlyProperty); set => SetValue(ShowSideViewOnlyProperty, value); }
    public PackageDrawingDiagnostics? Diagnostics { get; private set; }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (Package is null || ActualWidth < 100 || ActualHeight < 100) return;

        var package = Package;
        if (ShowEngineeringViews)
        {
            // Verified engineering views do not use parametric fallback geometry.

            DrawVerifiedEngineeringViews(dc, package);
            return;
        }

        var template = PackageDrawingTemplateResolver.Resolve(package);
        var scene = ParametricPackageGeometryBuilder.Build(package);
        var fit = ParametricPackageGeometryBuilder.Fit(scene, new Size(ActualWidth, ActualHeight), .12);
        var unknown = template.Parameters.Where(p => IsUnknown(package, p.SourceProperty)).Select(p => p.Key).ToArray();
        Diagnostics = new PackageDrawingDiagnostics(scene.TemplateId, scene.AlignmentType, scene.LeadCount, scene.PadCount, scene.BallCount, fit.Scale, unknown, scene.Warnings);

        if (ShowSideViewOnly)
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            var sideCell = new Rect(0, 0, ActualWidth, ActualHeight);
            DrawViewFrame(dc, sideCell, "2  ВИД СБОКУ");
            DrawSideView(dc, sideCell, package, template, scene);
            return;
        }

        dc.PushTransform(new TranslateTransform(fit.Offset.X, fit.Offset.Y));
        dc.PushTransform(new ScaleTransform(fit.Scale, fit.Scale));
        foreach (var primitive in scene.Primitives) DrawPrimitive(dc, primitive, template.TopologyType, true);
        dc.Pop();
        dc.Pop();

        if (ShowDimensions)
            DrawDimensionLines(dc, package, template, scene, fit);
#if DEBUG
        if (ShowDimensions)
            DrawDebugOverlay(dc, scene, fit);
#endif
    }

    private void DrawVerifiedEngineeringViews(DrawingContext dc, PackageDefinition package)
    {
        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
        var geometry = new PackageManufacturerDrawingGeometryService().GetVerifiedGeometry(package);
        if (geometry is null)
        {
            DrawMissingVerifiedViews(dc, package);
            return;
        }

        const double gap = 8;
        const double headerHeight = 34;
        DrawText(dc, "Vega-SMD  |  ЧЕРТЁЖ КОРПУСА  |  " + package.PackageName, new Point(12, 8), new SolidColorBrush(Color.FromRgb(31, 78, 121)));
        dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(31, 78, 121)), 1), new Point(0, headerHeight - 2), new Point(ActualWidth, headerHeight - 2));
        var cellWidth = (ActualWidth - gap) / 2;
        var cellHeight = (ActualHeight - headerHeight - gap) / 2;
        var cells = new[]
        {
            new Rect(0, headerHeight, cellWidth, cellHeight),
            new Rect(cellWidth + gap, headerHeight, cellWidth, cellHeight),
            new Rect(0, headerHeight + cellHeight + gap, cellWidth, cellHeight),
            new Rect(cellWidth + gap, headerHeight + cellHeight + gap, cellWidth, cellHeight)
        };
        var types = new[] { "Top", "End", "Side", "ThreeD" };
        var titles = new[] { "1  ВИД СВЕРХУ", "2  ВИД СНИЗУ", "3  ВИД СБОКУ", $"4  3D вид ({package.PackageName})" };
        var assetService = new PackageManufacturerDrawingAssetService();
        for (var i = 0; i < cells.Length; i++)
        {
            DrawViewFrame(dc, cells[i], titles[i]);
            var projection = geometry.Projections.FirstOrDefault(p => string.Equals(p.ProjectionType, types[i], StringComparison.OrdinalIgnoreCase) && p.IsAvailable);
            if (projection is null)
            {
                if (types[i].Equals("ThreeD", StringComparison.OrdinalIgnoreCase))
                {
                    var localImage = ResolveLocalComponentImage(package);
                    if (localImage is not null)
                    {
                        DrawLocalComponentImage(dc, cells[i], localImage);
                        continue;
                    }
                }
                var message = types[i].Equals("ThreeD", StringComparison.OrdinalIgnoreCase)
                    ? "3D модель отсутствует в чертеже производителя"
                    : "Вид отсутствует в чертеже производителя";
                DrawText(dc, message, new Point(cells[i].Left + 18, cells[i].Top + cells[i].Height / 2), Brushes.Black);
                continue;
            }
            var primitives = LoadDrawingPrimitives(projection.Id);
            if (HasRenderablePrimitives(primitives))
            {
                DrawVerifiedProjection(dc, cells[i], projection, primitives);
                continue;
            }

            if (string.Equals(package.PackageName?.Trim(), "SOT23", StringComparison.OrdinalIgnoreCase) && types[i].Equals("Top", StringComparison.OrdinalIgnoreCase))
            {
                var content = new Rect(cells[i].Left + 36, cells[i].Top + 42, Math.Max(1, cells[i].Width - 72), Math.Max(1, cells[i].Height - 76));
                DrawSotPlanProjection(dc, cells[i], content, package, false, false);
                DrawCleanNominalDimensions(dc, "Top", content, package);
                continue;
            }
            if (string.Equals(package.PackageName?.Trim(), "SOT23", StringComparison.OrdinalIgnoreCase) && types[i].Equals("Side", StringComparison.OrdinalIgnoreCase))
            {
                DrawSotSideProjection(dc, cells[i], package);
                DrawCleanNominalDimensions(dc, "Side", cells[i], package);
                continue;
            }
            var asset = assetService.GetVerifiedAsset(geometry, types[i]);
            if (asset is not null && DrawVerifiedAsset(dc, cells[i], asset, package))
                continue;

            DrawText(dc, "Недостаточно verified geometry данных для детальной проекции",
                new Point(cells[i].Left + 18, cells[i].Top + cells[i].Height / 2), Brushes.Black);
        }
    }

    private void DrawMissingVerifiedViews(DrawingContext dc, PackageDefinition package)
    {
        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
        const double gap = 8;
        var cellWidth = (ActualWidth - gap) / 2;
        var cellHeight = (ActualHeight - gap) / 2;
        var cells = new[]
        {
            new Rect(0, 0, cellWidth, cellHeight),
            new Rect(cellWidth + gap, 0, cellWidth, cellHeight),
            new Rect(0, cellHeight + gap, cellWidth, cellHeight),
            new Rect(cellWidth + gap, cellHeight + gap, cellWidth, cellHeight)
        };
        var titles = new[] { "1  ВИД СВЕРХУ", "2  ВИД СНИЗУ", "3  ВИД СБОКУ", $"4  3D вид ({package.PackageName})" };
        for (var i = 0; i < cells.Length; i++)
        {
            DrawViewFrame(dc, cells[i], titles[i]);
            if (i == 3)
            {
                var localImage = ResolveLocalComponentImage(package);
                if (localImage is not null)
                {
                    DrawLocalComponentImage(dc, cells[i], localImage);
                    continue;
                }
            }
            DrawText(dc, "Подтверждённые данные производителя отсутствуют",
                new Point(cells[i].Left + 18, cells[i].Top + cells[i].Height / 2), Brushes.Black);
        }
    }

    private static bool HasRenderablePrimitives(IReadOnlyList<PrimitiveSnapshot> primitives) =>
        primitives.Any(p => PrimitivePoints(p).Any());

    private bool DrawVerifiedAsset(DrawingContext dc, Rect cell, PackageManufacturerDrawingAsset asset, PackageDefinition package)
    {
        var path = asset.FilePath;
        if (!Path.IsPathRooted(path))
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Replace('\\', Path.DirectorySeparatorChar));
        if (!File.Exists(path)) return false;

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();

            var area = new Rect(cell.Left + 12, cell.Top + 30,
                Math.Max(1, cell.Width - 24), Math.Max(1, cell.Height - 42));
            // Use only the projection region recorded in the verified asset
            // metadata. This removes duplicate views and page footnotes while
            // preserving the manufacturer's dimensions and extension lines.
            var source = GetDisplaySource(asset, image.PixelWidth, image.PixelHeight);
            var scale = Math.Min(area.Width / source.Width, area.Height / source.Height);
            if (!double.IsFinite(scale) || scale <= 0) return false;
            var size = new Size(source.Width * scale, source.Height * scale);
            var destination = new Rect(
                area.Left + (area.Width - size.Width) / 2,
                area.Top + (area.Height - size.Height) / 2,
                size.Width, size.Height);
            BitmapSource renderImage = image;
            if (source.Width < image.PixelWidth || source.Height < image.PixelHeight || source.X > 0 || source.Y > 0)
            {
                var crop = new CroppedBitmap(image, new Int32Rect(
                    (int)Math.Floor(source.X), (int)Math.Floor(source.Y),
                    Math.Max(1, (int)Math.Ceiling(source.Width)), Math.Max(1, (int)Math.Ceiling(source.Height))));
                crop.Freeze();
                renderImage = crop;
            }
            dc.DrawImage(renderImage, destination);
            DrawDisplayRedactions(dc, asset.ProjectionType, source, destination, image.PixelWidth, image.PixelHeight);
            DrawCleanNominalDimensions(dc, asset.ProjectionType, destination, package);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static Rect GetCropSource(string cropReference, int pixelWidth, int pixelHeight)
    {
        if (string.IsNullOrWhiteSpace(cropReference)) return new Rect(0, 0, pixelWidth, pixelHeight);
        try
        {
            using var doc = JsonDocument.Parse(cropReference);
            var root = doc.RootElement;
            if (!root.TryGetProperty("coordinateSystem", out var cs) || cs.GetString() != "normalized-page")
                return new Rect(0, 0, pixelWidth, pixelHeight);
            var x = root.GetProperty("x").GetDouble();
            var y = root.GetProperty("y").GetDouble();
            var w = root.GetProperty("width").GetDouble();
            var h = root.GetProperty("height").GetDouble();
            if (x < 0 || y < 0 || w <= 0 || h <= 0 || x + w > 1 || y + h > 1)
                return new Rect(0, 0, pixelWidth, pixelHeight);
            return new Rect(x * pixelWidth, y * pixelHeight, w * pixelWidth, h * pixelHeight);
        }
        catch (JsonException)
        {
            return new Rect(0, 0, pixelWidth, pixelHeight);
        }
    }

    private static Rect GetDisplaySource(PackageManufacturerDrawingAsset asset, int pixelWidth, int pixelHeight)
    {
        // Keep the database/source evidence untouched. These display-only
        // regions omit Infineon's datum and GD&T callouts without painting over
        // or modifying the original manufacturer file.
        if (string.Equals(asset.ProjectionType, "Top", StringComparison.OrdinalIgnoreCase))
            return new Rect(pixelWidth * .035, pixelHeight * .08, pixelWidth * .405, pixelHeight * .66);
        if (string.Equals(asset.ProjectionType, "Side", StringComparison.OrdinalIgnoreCase))
            return new Rect(pixelWidth * .635, pixelHeight * .045, pixelWidth * .335, pixelHeight * .68);
        return GetCropSource(asset.CropReference, pixelWidth, pixelHeight);
    }

    private static void DrawDisplayRedactions(DrawingContext dc, string projectionType, Rect source, Rect destination, int pixelWidth, int pixelHeight)
    {
        var normalized = new List<Rect>();
        if (string.Equals(projectionType, "Top", StringComparison.OrdinalIgnoreCase))
        {
            // Remove only local service labels; dimension lines and values stay.
            normalized.Add(new Rect(.250, .205, .075, .145)); // lead number 3
            normalized.Add(new Rect(.135, .595, .085, .105)); // lead number 1
            normalized.Add(new Rect(.340, .595, .085, .105)); // lead number 2
            normalized.Add(new Rect(.035, .625, .160, .125)); // Pin1 label
            normalized.Add(new Rect(.385, .625, .105, .125)); // datum C
            normalized.Add(new Rect(.390, .045, .105, .125)); // datum B leader/box
            normalized.Add(new Rect(.390, .155, .170, .115)); // GD&T frame
        }
        else if (string.Equals(projectionType, "Side", StringComparison.OrdinalIgnoreCase))
        {
            // Keep every numeric dimension; remove only the GD&T frame and
            // datum A box that are not dimensions for the display preview.
            normalized.Add(new Rect(.490, .635, .185, .125)); // GD&T frame
            normalized.Add(new Rect(.835, .535, .145, .165)); // datum A leader/box
        }

        foreach (var region in normalized)
        {
            var sx = region.Left * pixelWidth;
            var sy = region.Top * pixelHeight;
            var sw = region.Width * pixelWidth;
            var sh = region.Height * pixelHeight;
            var clippedLeft = (sx - source.Left) / source.Width;
            var clippedTop = (sy - source.Top) / source.Height;
            var clippedRight = (sx + sw - source.Left) / source.Width;
            var clippedBottom = (sy + sh - source.Top) / source.Height;
            var left = destination.Left + Math.Max(0, clippedLeft) * destination.Width;
            var top = destination.Top + Math.Max(0, clippedTop) * destination.Height;
            var right = destination.Left + Math.Min(1, clippedRight) * destination.Width;
            var bottom = destination.Top + Math.Min(1, clippedBottom) * destination.Height;
            if (right > left && bottom > top)
                dc.DrawRectangle(Brushes.White, null, new Rect(left, top, right - left, bottom - top));
        }
    }

    private void DrawCleanNominalDimensions(DrawingContext dc, string projectionType, Rect destination, PackageDefinition package)
    {
        var brush = new SolidColorBrush(Color.FromRgb(74, 85, 104));
        brush.Freeze();
        if (string.Equals(projectionType, "Top", StringComparison.OrdinalIgnoreCase))
        {
            DrawText(dc, Millimetres(BodyLength(package)), new Point(destination.Left + destination.Width * .30, destination.Top - 18), brush);
            DrawText(dc, Millimetres(BodyWidth(package)), new Point(destination.Left - 2, destination.Top + destination.Height * .48), brush);
            if (GeometryPitch(package, 0) > 0)
                DrawText(dc, Millimetres(GeometryPitch(package, 0)), new Point(destination.Left + destination.Width * .30, destination.Bottom + 4), brush);
            if (GeometryLeadWidth(package, 0) > 0)
                DrawText(dc, Millimetres(GeometryLeadWidth(package, 0)), new Point(destination.Left + destination.Width * .30, destination.Bottom + 20), brush);
        }
        else if (string.Equals(projectionType, "Side", StringComparison.OrdinalIgnoreCase))
        {
            var height = GeometryBodyHeight(package);
            if (height > 0)
                DrawText(dc, Millimetres(height), new Point(destination.Right + 4, destination.Top + destination.Height * .44), brush);
        }
    }

    private void DrawVerifiedProjection(DrawingContext dc, Rect cell, ProjectionGeometry projection, IReadOnlyList<PrimitiveSnapshot> primitives)
    {
        var all = primitives.SelectMany(PrimitivePoints).ToList();
        if (all.Count == 0)
        {
            DrawText(dc, "Недостаточно verified geometry данных для детальной проекции", new Point(cell.Left + 18, cell.Top + cell.Height / 2), Brushes.Black);
            return;
        }

        var minX = all.Min(p => p.X) + projection.OriginX;
        var maxX = all.Max(p => p.X) + projection.OriginX;
        var minY = all.Min(p => p.Y) + projection.OriginY;
        var maxY = all.Max(p => p.Y) + projection.OriginY;
        var width = Math.Max(0.001, maxX - minX);
        var height = Math.Max(0.001, maxY - minY);
        var area = new Rect(cell.Left + 28, cell.Top + 34, Math.Max(1, cell.Width - 56), Math.Max(1, cell.Height - 74));
        var scale = Math.Min(area.Width / width, area.Height / height) * .78;
        Point Map(Point p) => new(area.Left + (p.X + projection.OriginX - minX) * scale,
            area.Top + (maxY - (p.Y + projection.OriginY)) * scale);

        foreach (var primitive in primitives)
        {
            var pen = new Pen(primitive.Layer == "Lead" ? MetalBrush() : Brushes.Black, Math.Max(.8, (primitive.StrokeWidth ?? .02) * scale));
            switch (primitive.PrimitiveType)
            {
                case "Line" when primitive.StartX is { } sx && primitive.StartY is { } sy && primitive.EndX is { } ex && primitive.EndY is { } ey:
                    dc.DrawLine(pen, Map(new Point(sx, sy)), Map(new Point(ex, ey))); break;
                case "Circle" when primitive.CenterX is { } cx && primitive.CenterY is { } cy && primitive.Radius is { } radius:
                    dc.DrawEllipse(primitive.Layer == "Pin1" ? Brushes.White : PackageBodyBrush(), pen, Map(new Point(cx, cy)), radius * scale, radius * scale); break;
                case "Polygon":
                    var points = ParseGeometry(primitive.GeometryData);
                    if (points.Count < 2) break;
                    var path = new StreamGeometry(); using (var context = path.Open()) { context.BeginFigure(Map(points[0]), primitive.Layer == "Body", primitive.Layer == "Body"); context.PolyLineTo(points.Skip(1).Select(Map).ToArray(), true, primitive.Layer == "Body"); }
                    dc.DrawGeometry(primitive.Layer == "Body" ? PackageBodyBrush() : null, pen, path); break;
            }
        }
    }

    private static IReadOnlyList<PrimitiveSnapshot> LoadDrawingPrimitives(int projectionId)
    {
        try
        {
            using var connection = MasterLibraryConnection.Create(); using var command = connection.CreateCommand();
            command.CommandText = "SELECT PrimitiveType, StartX, StartY, EndX, EndY, CenterX, CenterY, Radius, GeometryData, StrokeWidth, Layer, CoordinateSystem FROM PackageDrawingPrimitive WHERE ProjectionGeometryId=$id ORDER BY Id";
            command.Parameters.AddWithValue("$id", projectionId); using var reader = command.ExecuteReader(); var rows = new List<PrimitiveSnapshot>();
            while (reader.Read()) rows.Add(new PrimitiveSnapshot(reader.GetString(0), NullableDouble(reader, 1), NullableDouble(reader, 2), NullableDouble(reader, 3), NullableDouble(reader, 4), NullableDouble(reader, 5), NullableDouble(reader, 6), NullableDouble(reader, 7), reader.GetString(8), reader.IsDBNull(9) ? null : reader.GetDouble(9), reader.GetString(10), reader.GetString(11)));
            return rows;
        }
        catch (SqliteException) { return []; }
    }

    private static IEnumerable<Point> PrimitivePoints(PrimitiveSnapshot p)
    {
        if (!string.Equals(p.CoordinateSystem, "mm", StringComparison.OrdinalIgnoreCase)) yield break;
        if (p.StartX is { } sx && p.StartY is { } sy) yield return new Point(sx, sy);
        if (p.EndX is { } ex && p.EndY is { } ey) yield return new Point(ex, ey);
        if (p.CenterX is { } cx && p.CenterY is { } cy && p.Radius is { } r) { yield return new Point(cx - r, cy - r); yield return new Point(cx + r, cy + r); }
        foreach (var point in ParseGeometry(p.GeometryData)) yield return point;
    }

    private static double? NullableDouble(SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
    private sealed record PrimitiveSnapshot(string PrimitiveType, double? StartX, double? StartY, double? EndX, double? EndY, double? CenterX, double? CenterY, double? Radius, string GeometryData, double? StrokeWidth, string Layer, string CoordinateSystem);

    private static List<Point> ParseGeometry(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        try
        {
            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;
            var points = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("points", out var property) ? property : root;
            if (points.ValueKind == JsonValueKind.Array)
            {
                var result = new List<Point>();
                foreach (var item in points.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() < 2) continue;
                    result.Add(new Point(item[0].GetDouble(), item[1].GetDouble()));
                }
                return result;
            }
        }
        catch (JsonException)
        {
            // The compact text form is accepted for imported manufacturer drawings.
        }
        return value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.Split(',', StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length >= 2 && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out _) && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            .Select(parts => new Point(double.Parse(parts[0], CultureInfo.InvariantCulture), double.Parse(parts[1], CultureInfo.InvariantCulture)))
            .ToList();
    }

    private void DrawEngineeringViews(DrawingContext dc, PackageDefinition package, PackageDrawingTemplate template, DrawingScene scene)
    {
        const double gap = 8;
        dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
        var cellWidth = (ActualWidth - gap) / 2;
        var cellHeight = (ActualHeight - gap) / 2;
        var cells = new[]
        {
            new Rect(0, 0, cellWidth, cellHeight),
            new Rect(cellWidth + gap, 0, cellWidth, cellHeight),
            new Rect(0, cellHeight + gap, cellWidth, cellHeight),
            new Rect(cellWidth + gap, cellHeight + gap, cellWidth, cellHeight)
        };

        DrawViewFrame(dc, cells[0], "1  ВИД СВЕРХУ");
        DrawViewFrame(dc, cells[1], "2  ВИД СБОКУ");
        // The first approved SOT rule is intentionally limited to SOT23.  Keep
        // the topology guard, but tolerate package names coming from legacy
        // rows with incidental whitespace.
        var isSot23 = template.TopologyType == PackageTopologyType.MiniMold
            && string.Equals(package.PackageName?.Trim(), "SOT23", StringComparison.OrdinalIgnoreCase);
        DrawViewFrame(dc, cells[2], isSot23 ? "3  ВИД С ТОРЦА" : "Вид снизу");
        DrawViewFrame(dc, cells[3], $"3D вид ({package.PackageName})");
        var orthographicScale = template.TopologyType == PackageTopologyType.MiniMold
            ? ComputeSotOrthographicScale(package, cells)
            : (double?)null;
        DrawPlanView(dc, cells[0], package, template, scene, true, false, orthographicScale);
        DrawSideView(dc, cells[1], package, template, scene, orthographicScale);
        if (isSot23)
            DrawSotEndProjection(dc, cells[2], package, orthographicScale);
        else
            DrawPlanView(dc, cells[2], package, template, scene, template.TopologyType == PackageTopologyType.MiniMold, true);
        DrawThreeDimensionalView(dc, cells[3], package, template, scene);
    }

    private void DrawViewFrame(DrawingContext dc, Rect cell, string title)
    {
        var border = new SolidColorBrush(Color.FromRgb(190, 196, 202));
        var heading = new SolidColorBrush(Color.FromRgb(31, 78, 121));
        border.Freeze();
        heading.Freeze();
        dc.DrawRectangle(Brushes.White, new Pen(border, .8), cell);
        DrawText(dc, title, new Point(cell.Left + 10, cell.Top + 7), heading);
    }

    private void DrawPlanView(DrawingContext dc, Rect cell, PackageDefinition package, PackageDrawingTemplate template, DrawingScene scene, bool showDimensions, bool bottomView, double? sharedScale = null)
    {
        var content = new Rect(cell.Left + 42, cell.Top + 34, Math.Max(1, cell.Width - 62), Math.Max(1, cell.Height - 58));
        if (template.TopologyType == PackageTopologyType.MiniMold)
        {
            DrawSotPlanProjection(dc, cell, content, package, showDimensions, bottomView, sharedScale);
            return;
        }
        var fit = ParametricPackageGeometryBuilder.Fit(scene, content.Size, showDimensions ? .18 : .12);
        var translatedFit = fit with { Offset = fit.Offset + new Vector(content.Left, content.Top) };

        dc.PushTransform(new TranslateTransform(translatedFit.Offset.X, translatedFit.Offset.Y));
        dc.PushTransform(new ScaleTransform(translatedFit.Scale, translatedFit.Scale));
        foreach (var primitive in scene.Primitives)
            DrawPrimitive(dc, primitive, template.TopologyType, true);
        dc.Pop();
        dc.Pop();

        if (!showDimensions) return;
        var body = scene.Primitives.FirstOrDefault(p => p.Kind == ScenePrimitiveKind.Body);
        if (body is null) return;
        var bodyScreen = ToScreen(body.Bounds, translatedFit);
        var pen = EngineeringDimensionPen();
        var length = BodyLength(package);
        var width = BodyWidth(package);
        if (length > 0)
            HorizontalDimension(dc, bodyScreen.Left, bodyScreen.Right, Math.Max(cell.Top + 31, bodyScreen.Top - 20), $"A = {Millimetres(length)}", pen);
        if (width > 0)
            VerticalDimension(dc, bodyScreen.Top, bodyScreen.Bottom, Math.Max(cell.Left + 17, bodyScreen.Left - 20), $"B = {Millimetres(width)}", pen);
    }

    private void DrawSideView(DrawingContext dc, Rect cell, PackageDefinition package, PackageDrawingTemplate template, DrawingScene scene, double? sharedScale = null)
    {
        if (template.TopologyType == PackageTopologyType.MiniMold)
        {
            DrawSotSideProjection(dc, cell, package, sharedScale);
            return;
        }
        var length = BodyLength(package);
        var height = package.Height;
        if (length <= 0 || height <= 0) return;
        var drawingArea = new Rect(cell.Left + 44, cell.Top + 46, Math.Max(1, cell.Width - 76), Math.Max(1, cell.Height - 82));
        var scale = Math.Min(drawingArea.Width / length, drawingArea.Height / height) * .68;
        var body = new Rect(drawingArea.Left + (drawingArea.Width - length * scale) / 2,
            drawingArea.Top + (drawingArea.Height - height * scale) / 2, length * scale, height * scale);
        var isChip = template.TopologyType == PackageTopologyType.Chip;

        dc.DrawRoundedRectangle(isChip ? CeramicBrush() : PackageBodyBrush(), EngineeringOutlinePen(), body, isChip ? 2 : 4, isChip ? 2 : 4);
        if (isChip)
        {
            var cap = Math.Min(body.Width * .18, body.Height * .45);
            dc.DrawRectangle(MetalBrush(), EngineeringOutlinePen(), new Rect(body.Left, body.Top, cap, body.Height));
            dc.DrawRectangle(MetalBrush(), EngineeringOutlinePen(), new Rect(body.Right - cap, body.Top, cap, body.Height));
        }
        else
        {
            DrawSideContacts(dc, body, scene, template.TopologyType);
        }

        var pen = EngineeringDimensionPen();
        HorizontalDimension(dc, body.Left, body.Right, Math.Max(cell.Top + 31, body.Top - 20), $"A = {Millimetres(length)}", pen);
        VerticalDimension(dc, body.Top, body.Bottom, Math.Min(cell.Right - 17, body.Right + 20), $"H = {Millimetres(height)}", pen);
    }

    private void DrawSideContacts(DrawingContext dc, Rect body, DrawingScene scene, PackageTopologyType topology)
    {
        var contactCount = scene.Primitives.Count(p => p.Kind is ScenePrimitiveKind.Lead or ScenePrimitiveKind.Pad or ScenePrimitiveKind.Ball or ScenePrimitiveKind.Terminal);
        if (contactCount == 0) return;
        var visible = Math.Clamp((contactCount + 1) / 2, 1, 12);
        var contactWidth = Math.Max(3, body.Width / Math.Max(8, visible * 2.5));
        var contactHeight = Math.Max(3, body.Height * .16);
        for (var i = 0; i < visible; i++)
        {
            var x = body.Left + (i + 1) * body.Width / (visible + 1);
            if (topology == PackageTopologyType.Bga)
                dc.DrawEllipse(MetalBrush(), EngineeringOutlinePen(), new Point(x, body.Bottom + contactHeight / 2), contactHeight / 2, contactHeight / 2);
            else
                dc.DrawRectangle(MetalBrush(), EngineeringOutlinePen(), new Rect(x - contactWidth / 2, body.Bottom, contactWidth, contactHeight));
        }
    }

    private void DrawThreeDimensionalView(DrawingContext dc, Rect cell, PackageDefinition package, PackageDrawingTemplate template, DrawingScene scene)
    {
        var localImage = ResolveLocalComponentImage(package);
        if (localImage is null)
        {
            DrawText(dc, "3D-модель из документации производителя отсутствует",
                new Point(cell.Left + 18, cell.Top + cell.Height / 2), Brushes.Black);
            return;
        }

        DrawLocalComponentImage(dc, cell, localImage);
        return;

        /*
        if (template.TopologyType == PackageTopologyType.MiniMold)
        {
            DrawSotThreeDimensional(dc, cell, package);
            return;
        }
        var area = new Rect(cell.Left + 24, cell.Top + 42, Math.Max(1, cell.Width - 48), Math.Max(1, cell.Height - 62));
        var bodyLength = BodyLength(package);
        var bodyWidth = BodyWidth(package);
        if (bodyLength <= 0 || bodyWidth <= 0) return;
        var topWidth = area.Width * .62;
        var topHeight = Math.Min(area.Height * .42, topWidth * bodyWidth / bodyLength * .48);
        var depth = Math.Min(area.Height * .22, Math.Max(10, package.Height > 0 ? topHeight * package.Height / bodyWidth * .55 : topHeight * .35));
        var left = area.Left + (area.Width - topWidth) / 2;
        var top = area.Top + (area.Height - topHeight - depth) / 2;
        var skew = topHeight * .38;
        var isChip = template.TopologyType == PackageTopologyType.Chip;
        var capWidth = isChip ? topWidth * .17 : 0;
        var topFace = isChip
            ? Parallelogram(new Point(left + skew + capWidth, top), new Point(left + topWidth - capWidth, top), new Point(left + topWidth - skew - capWidth, top + topHeight), new Point(left + capWidth, top + topHeight))
            : Parallelogram(new Point(left + skew, top), new Point(left + topWidth, top), new Point(left + topWidth - skew, top + topHeight), new Point(left, top + topHeight));
        var frontFace = isChip
            ? Parallelogram(new Point(left + capWidth, top + topHeight), new Point(left + topWidth - skew - capWidth, top + topHeight), new Point(left + topWidth - skew - capWidth, top + topHeight + depth), new Point(left + capWidth, top + topHeight + depth))
            : Parallelogram(new Point(left, top + topHeight), new Point(left + topWidth - skew, top + topHeight), new Point(left + topWidth - skew, top + topHeight + depth), new Point(left, top + topHeight + depth));
        dc.DrawGeometry(isChip ? CeramicBrush() : PackageBodyBrush(), EngineeringOutlinePen(), topFace);
        dc.DrawGeometry(isChip ? CeramicShadowBrush() : PackageBodyShadowBrush(), EngineeringOutlinePen(), frontFace);
        if (isChip)
        {
            DrawChipThreeDimensionalCaps(dc, left, top, topWidth, topHeight, depth, skew);
            return;
        }
        DrawThreeDimensionalContacts(dc, left, top, topWidth, topHeight, depth, skew, scene, template.TopologyType);
        */
    }

    private static string? ResolveLocalComponentImage(PackageDefinition package)
    {
        var name = (package.PackageName ?? string.Empty).Trim().ToLowerInvariant();
        var family = (package.PackageFamily ?? string.Empty).Trim().ToLowerInvariant();
        var suffix = name.TrimStart('r', 'c', 'l');
        var key = family switch
        {
            "chip" when package.ComponentType.Contains("res", StringComparison.OrdinalIgnoreCase) => $"resistor_{suffix}",
            "chip" when package.ComponentType.Contains("ind", StringComparison.OrdinalIgnoreCase) => $"inductor_{suffix}",
            "chip" => $"capacitor_{suffix}",
            _ => name.Replace("-", "_")
        };
        var mapPath = Path.Combine(AppContext.BaseDirectory, "Assets", "components_images.json");
        if (!File.Exists(mapPath)) return null;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(mapPath));
            if (!document.RootElement.TryGetProperty(key, out var value)) return null;
            var relative = value.GetString();
            if (string.IsNullOrWhiteSpace(relative)) return null;
            var path = Path.Combine(AppContext.BaseDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(path) ? path : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void DrawLocalComponentImage(DrawingContext dc, Rect cell, string path)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            var area = new Rect(cell.Left + 24, cell.Top + 34, Math.Max(1, cell.Width - 48), Math.Max(1, cell.Height - 62));
            var scale = Math.Min(area.Width / image.PixelWidth, area.Height / image.PixelHeight) * .82;
            var size = new Size(image.PixelWidth * scale, image.PixelHeight * scale);
            var destination = new Rect(area.Left + (area.Width - size.Width) / 2, area.Top + (area.Height - size.Height) / 2, size.Width, size.Height);
            dc.DrawImage(image, destination);
        }
        catch (Exception)
        {
            DrawText(dc, "Не удалось загрузить локальную 3D-модель", new Point(cell.Left + 18, cell.Top + cell.Height / 2), Brushes.Black);
        }
    }

    private double ComputeSotOrthographicScale(PackageDefinition package, IReadOnlyList<Rect> cells)
    {
        var length = BodyLength(package);
        var width = BodyWidth(package);
        var height = GeometryBodyHeight(package);
        if (length <= 0 || width <= 0 || height <= 0) return 1;

        var lead = GeometryLeadLength(package, Math.Max(width * .35, height * .30));
        var topPhysicalWidth = length;
        var topPhysicalHeight = width + lead * 2;
        var sideProjection = lead;
        // Gull-wing profile extends beyond the body on both sides. Reserve
        // the full orthogonal lead envelope so SIDE uses the same mm scale.
        var sidePhysicalWidth = length + sideProjection * 2.4;
        var sidePhysicalHeight = height + sideProjection * .65;
        var endPhysicalWidth = width + lead * 1.8;
        var endPhysicalHeight = height + lead * .45;

        var top = new Rect(cells[0].Left + 42, cells[0].Top + 34, Math.Max(1, cells[0].Width - 62), Math.Max(1, cells[0].Height - 58));
        var side = new Rect(cells[1].Left + 44, cells[1].Top + 48, Math.Max(1, cells[1].Width - 80), Math.Max(1, cells[1].Height - 88));
        var end = new Rect(cells[2].Left + 46, cells[2].Top + 46, Math.Max(1, cells[2].Width - 86), Math.Max(1, cells[2].Height - 88));

        // One physical px/mm scale is shared by TOP, SIDE and END.  The
        // smallest fit wins so no projection is independently enlarged.
        return Math.Min(
            Math.Min(top.Width / topPhysicalWidth, top.Height / topPhysicalHeight),
            Math.Min(
                Math.Min(side.Width / sidePhysicalWidth, side.Height / sidePhysicalHeight),
                Math.Min(end.Width / endPhysicalWidth, end.Height / endPhysicalHeight))) * .70;
    }

    private void DrawSotPlanProjection(DrawingContext dc, Rect cell, Rect content, PackageDefinition package, bool showDimensions, bool bottomView, double? sharedScale = null)
    {
        var length = BodyLength(package);
        var width = BodyWidth(package);
        if (length <= 0 || width <= 0) return;
        var leadLength = GeometryLeadLength(package, width * .35);
        var leadWidth = GeometryLeadWidth(package, length * .12);
        var totalLeads = GeometryLeadCount(package);
        var topLeads = (totalLeads + 1) / 2;
        var bottomLeads = totalLeads - topLeads;
        var pitch = GeometryPitch(package, length / Math.Max(2, topLeads));
        var overallHeight = width + leadLength * 2;
        var scale = sharedScale ?? Math.Min(content.Width / length, content.Height / overallHeight) * .72;
        var body = new Rect(content.Left + (content.Width - length * scale) / 2,
            content.Top + (content.Height - overallHeight * scale) / 2 + leadLength * scale,
            length * scale, width * scale);
        var metal = MetalBrush();
        var outline = EngineeringOutlinePen();
        var leadW = Math.Max(3, leadWidth * scale);
        var leadL = Math.Max(5, leadLength * scale);
        var topPositions = SotLeadPositions(body, topLeads, pitch * scale);
        var bottomPositions = SotLeadPositions(body, bottomLeads, pitch * scale);
        foreach (var x in topPositions)
            DrawSotPlanLead(dc, x, body.Top, leadW, leadL, true, metal, outline);
        foreach (var x in bottomPositions)
            DrawSotPlanLead(dc, x, body.Bottom, leadW, leadL, false, metal, outline);

        var bodyBrush = bottomView ? new SolidColorBrush(Color.FromRgb(78, 96, 108)) : PackageBodyBrush();
        dc.DrawRoundedRectangle(bodyBrush, outline, body, 3, 3);
        if (bottomView)
        {
            var underside = new Rect(body.Left + body.Width * .12, body.Top + body.Height * .18, body.Width * .76, body.Height * .64);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(42, 58, 69)), new Pen(Brushes.Black, .6), underside, 2, 2);
            var markerSize = Math.Max(4, Math.Min(10, leadW * .55));
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(35, 42, 47)), null,
                new Point(topPositions.FirstOrDefault(body.Left + body.Width * .18), body.Top + markerSize), markerSize / 2, markerSize / 2);
        }
        else
        {
            var markerSize = Math.Max(5, Math.Min(12, body.Height * .12));
            dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, .6),
                new Point(body.Left + body.Width * .12, body.Top + body.Height * .18), markerSize / 2, markerSize / 2);
        }

        if (!showDimensions) return;
        var pen = EngineeringDimensionPen();
        if (!bottomView)
        {
            // Dedicated dimension lanes keep every label outside the body and
            // prevent the short lead-width/pitch dimensions from colliding.
            HorizontalDimension(dc, body.Left, body.Right, Math.Max(cell.Top + 34, body.Top - leadL - 28), $"A = {Millimetres(length)}", pen);
            VerticalDimension(dc, body.Top, body.Bottom, Math.Max(cell.Left + 28, body.Left - 34), $"B = {Millimetres(width)}", pen);
            if (leadL > 0)
                VerticalDimension(dc, body.Top - leadL, body.Bottom + leadL, Math.Min(cell.Right - 28, body.Right + 48), $"O = {Millimetres(width + leadLength * 2)}", pen);
            if (topPositions.Count > 1)
                HorizontalDimension(dc, topPositions[0], topPositions[1], Math.Max(cell.Top + 58, body.Top - leadL - 56), $"H = {Millimetres(pitch)}", pen);
            if (leadWidth > 0)
                HorizontalDimension(dc, topPositions[0] - leadW / 2, topPositions[0] + leadW / 2, Math.Max(cell.Top + 82, body.Top - leadL - 84), $"J = {Millimetres(leadWidth)}", pen);
            if (GeometryLeadLength(package, 0) > 0)
                VerticalDimension(dc, body.Top - leadL, body.Top, Math.Max(cell.Left + 12, body.Left - 68), $"K = {Millimetres(GeometryLeadLength(package, 0))}", pen);
        }
        else
        {
            var pitchPositions = topPositions.Count > 1 ? topPositions : bottomPositions;
            if (pitchPositions.Count > 1)
                HorizontalDimension(dc, pitchPositions[0], pitchPositions[1], Math.Min(cell.Bottom - 17, body.Bottom + leadL + 20), $"H = {Millimetres(pitch)}", pen);
            if (leadWidth > 0)
            {
                var widthPosition = bottomPositions.Count > 0 ? bottomPositions[0] : topPositions[0];
                HorizontalDimension(dc, widthPosition - leadW / 2, widthPosition + leadW / 2, Math.Min(cell.Bottom - 38, body.Bottom + leadL + 42), $"J = {Millimetres(leadWidth)}", pen);
            }
            if (GeometryLeadLength(package, 0) > 0)
            {
                VerticalDimension(dc, body.Bottom, body.Bottom + leadL, Math.Max(cell.Left + 17, body.Left - 38), $"K = {Millimetres(GeometryLeadLength(package, 0))}", pen);
            }
        }
    }

    private static void DrawSotPlanLead(DrawingContext dc, double centerX, double bodyEdge, double width, double length, bool above, Brush fill, Pen outline)
    {
        var rect = above
            ? new Rect(centerX - width / 2, bodyEdge - length, width, length)
            : new Rect(centerX - width / 2, bodyEdge, width, length);
        dc.DrawRoundedRectangle(fill, outline, rect, 1, 1);
    }

    private static IReadOnlyList<double> SotLeadPositions(Rect body, int count, double pitch)
    {
        if (count <= 0) return [];
        return Enumerable.Range(0, count)
            .Select(index => body.Left + body.Width / 2 - (count - 1) * pitch / 2 + index * pitch)
            .ToArray();
    }

    private void DrawSotSideProjection(DrawingContext dc, Rect cell, PackageDefinition package, double? sharedScale = null)
    {
        var length = BodyLength(package);
        var height = GeometryBodyHeight(package);
        if (length <= 0 || height <= 0) return;
        var area = new Rect(cell.Left + 44, cell.Top + 48, Math.Max(1, cell.Width - 80), Math.Max(1, cell.Height - 88));
        var projectionMm = GeometryLeadLength(package, 0);
        var projection = projectionMm > 0 ? projectionMm : Math.Max(length * .12, height * .30);
        var totalHeight = height + projection * .65;
        var horizontalMm = Math.Max(height * .30, projection * .52);
        var shelfMm = Math.Max(height * .30, projection * .68);
        var profileWidthMm = length + (horizontalMm + shelfMm) * 2;
        var scale = sharedScale ?? Math.Min(area.Width / profileWidthMm, area.Height / totalHeight) * .66;
        var body = new Rect(area.Left + (area.Width - length * scale) / 2,
            area.Top + (area.Height - totalHeight * scale) / 2,
            length * scale, height * scale);
        var outline = EngineeringOutlinePen();
        var leadWidthMm = GeometryLeadWidth(package, 0);
        var leadThickness = leadWidthMm > 0
            ? Math.Max(2.5, leadWidthMm * scale * .35)
            : Math.Max(2.5, body.Height * .055);
        var leadOutlinePen = new Pen(Brushes.Black, leadThickness + 1.2);
        var leadPen = new Pen(MetalBrush(), leadThickness);
        leadPen.StartLineCap = PenLineCap.Square;
        leadPen.EndLineCap = PenLineCap.Square;
        leadPen.LineJoin = PenLineJoin.Round;
        var rootY = body.Bottom - body.Height * .18;
        var horizontal = horizontalMm * scale;
        var drop = Math.Max(body.Height * .20, projection * scale * .48);
        var shelf = shelfMm * scale;
        DrawGullWingSideLead(dc, new Point(body.Left, rootY), -1, horizontal, drop, shelf, leadOutlinePen, leadPen);
        DrawGullWingSideLead(dc, new Point(body.Right, rootY), 1, horizontal, drop, shelf, leadOutlinePen, leadPen);
        dc.DrawRectangle(PackageBodyBrush(), outline, body);
        var pen = EngineeringDimensionPen();
        VerticalDimension(dc, body.Top, body.Bottom, Math.Min(cell.Right - 34, body.Right + 56), $"C = {Millimetres(height)}", pen);
    }

    private static void DrawGullWingSideLead(
        DrawingContext dc,
        Point root,
        int direction,
        double horizontal,
        double drop,
        double shelf,
        Pen outline,
        Pen metal)
    {
        var p1 = new Point(root.X + direction * horizontal, root.Y);
        var p2 = new Point(p1.X + direction * horizontal * .16, root.Y + drop * .70);
        var p3 = new Point(p2.X + direction * horizontal * .08, root.Y + drop);
        var p4 = new Point(p3.X + direction * shelf, p3.Y);
        DrawLeadSegment(dc, outline, root, p1);
        DrawLeadSegment(dc, outline, p1, p2);
        DrawLeadSegment(dc, outline, p2, p3);
        DrawLeadSegment(dc, outline, p3, p4);
        DrawLeadSegment(dc, metal, root, p1);
        DrawLeadSegment(dc, metal, p1, p2);
        DrawLeadSegment(dc, metal, p2, p3);
        DrawLeadSegment(dc, metal, p3, p4);
    }

    private static void DrawLeadSegment(DrawingContext dc, Pen pen, Point from, Point to) => dc.DrawLine(pen, from, to);

    private void DrawSotEndProjection(DrawingContext dc, Rect cell, PackageDefinition package, double? sharedScale = null)
    {
        var width = BodyWidth(package);
        var height = GeometryBodyHeight(package);
        if (width <= 0 || height <= 0) return;

        var area = new Rect(cell.Left + 46, cell.Top + 46, Math.Max(1, cell.Width - 86), Math.Max(1, cell.Height - 88));
        var leadProjection = GeometryLeadLength(package, 0);
        var projection = leadProjection > 0 ? leadProjection : Math.Max(width * .22, height * .28);
        var totalWidth = width + projection * 1.8;
        var totalHeight = height + projection * .45;
        var scale = sharedScale ?? Math.Min(area.Width / totalWidth, area.Height / totalHeight) * .70;
        var body = new Rect(
            area.Left + (area.Width - width * scale) / 2,
            area.Top + (area.Height - height * scale - projection * scale * .45) / 2,
            width * scale,
            height * scale);
        var outline = EngineeringOutlinePen();
        var leadWidth = GeometryLeadWidth(package, width * .12) * scale;
        var leadThickness = Math.Max(3, leadWidth * .35);
        var leadOutline = new Pen(Brushes.Black, leadThickness + 1.1);
        var metal = new Pen(MetalBrush(), leadThickness);
        var rootY = body.Bottom - body.Height * .18;
        var leftRoot = new Point(body.Left + body.Width * .25, rootY);
        var rightRoot = new Point(body.Left + body.Width * .75, rootY);

        DrawEndLead(dc, leftRoot, -1, projection * scale * .55, projection * scale * .42, leadOutline, metal);
        DrawEndLead(dc, rightRoot, 1, projection * scale * .55, projection * scale * .42, leadOutline, metal);
        dc.DrawRectangle(PackageBodyBrush(), outline, body);
        var pen = EngineeringDimensionPen();
        VerticalDimension(dc, body.Top, body.Bottom, Math.Min(cell.Right - 18, body.Right + 28), $"C = {Millimetres(height)}", pen);
        HorizontalDimension(dc, body.Left, body.Right, Math.Min(cell.Bottom - 18, body.Bottom + projection * scale * .70), $"B = {Millimetres(width)}", pen);
    }

    private static void DrawEndLead(DrawingContext dc, Point root, int direction, double horizontal, double drop, Pen outline, Pen metal)
    {
        var p1 = new Point(root.X + direction * horizontal, root.Y);
        var p2 = new Point(p1.X + direction * horizontal * .12, root.Y + drop);
        var p3 = new Point(p2.X + direction * horizontal * .55, p2.Y);
        DrawLeadSegment(dc, outline, root, p1);
        DrawLeadSegment(dc, outline, p1, p2);
        DrawLeadSegment(dc, outline, p2, p3);
        DrawLeadSegment(dc, metal, root, p1);
        DrawLeadSegment(dc, metal, p1, p2);
        DrawLeadSegment(dc, metal, p2, p3);
    }

    private void DrawSotThreeDimensional(DrawingContext dc, Rect cell, PackageDefinition package)
    {
        var length = BodyLength(package);
        var width = BodyWidth(package);
        if (length <= 0 || width <= 0) return;
        var totalLeads = GeometryLeadCount(package);
        var backLeads = (totalLeads + 1) / 2;
        var frontLeads = totalLeads - backLeads;
        var pitch = GeometryPitch(package, length / Math.Max(2, backLeads));
        var area = new Rect(cell.Left + 28, cell.Top + 46, Math.Max(1, cell.Width - 56), Math.Max(1, cell.Height - 72));
        var topWidth = area.Width * .62;
        var topHeight = Math.Min(area.Height * .38, topWidth * width / length * .55);
        var height = GeometryBodyHeight(package);
        var depth = Math.Min(area.Height * .18, Math.Max(10, height > 0 ? topHeight * height / width * .45 : topHeight * .4));
        var left = area.Left + (area.Width - topWidth) / 2;
        var top = area.Top + (area.Height - topHeight - depth) / 2;
        // Keep the 3D body close to an orthographic package view; the former
        // large skew made the SOT look like a thin wedge.
        var skew = topHeight * .22;
        var centerX = left + topWidth / 2;
        var pitchPixels = topWidth * pitch / length;
        var leadSize = Math.Clamp(topWidth * Math.Max(GeometryLeadWidth(package, length * .10), length * .10) / length, 6, 14);
        var leadPen = EngineeringOutlinePen(.7);

        foreach (var x in Enumerable.Range(0, backLeads)
                     .Select(index => centerX - (backLeads - 1) * pitchPixels / 2 + index * pitchPixels))
        {
            var lead = Parallelogram(new Point(x - leadSize / 2 + skew * .35, top + 2), new Point(x + leadSize / 2 + skew * .35, top + 2),
                new Point(x + leadSize / 2 + skew * .58, top - topHeight * .32), new Point(x - leadSize / 2 + skew * .58, top - topHeight * .32));
            dc.DrawGeometry(MetalBrush(), leadPen, lead);
        }

        var topFace = Parallelogram(new Point(left + skew, top), new Point(left + topWidth, top), new Point(left + topWidth - skew, top + topHeight), new Point(left, top + topHeight));
        var frontFace = Parallelogram(new Point(left, top + topHeight), new Point(left + topWidth - skew, top + topHeight),
            new Point(left + topWidth - skew, top + topHeight + depth), new Point(left, top + topHeight + depth));
        dc.DrawGeometry(PackageBodyTopBrush(), EngineeringOutlinePen(), topFace);
        dc.DrawGeometry(PackageBodyShadowBrush(), EngineeringOutlinePen(), frontFace);

        foreach (var x in Enumerable.Range(0, frontLeads)
                     .Select(index => centerX - skew / 2 - (frontLeads - 1) * pitchPixels / 2 + index * pitchPixels))
        {
            var frontLead = Parallelogram(new Point(x - leadSize / 2, top + topHeight + depth - 2), new Point(x + leadSize / 2, top + topHeight + depth - 2),
                new Point(x + leadSize / 2 - skew * .18, top + topHeight + depth + topHeight * .34), new Point(x - leadSize / 2 - skew * .18, top + topHeight + depth + topHeight * .34));
            dc.DrawGeometry(MetalBrush(), leadPen, frontLead);
        }
        dc.DrawEllipse(Brushes.White, new Pen(Brushes.Black, .6), new Point(left + skew + topWidth * .10, top + topHeight * .18), 4, 4);
    }

    private static StreamGeometry Parallelogram(Point a, Point b, Point c, Point d)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(a, true, true);
            context.LineTo(b, true, false);
            context.LineTo(c, true, false);
            context.LineTo(d, true, false);
        }
        geometry.Freeze();
        return geometry;
    }

    private void DrawChipThreeDimensionalCaps(DrawingContext dc, double left, double top, double width, double height, double depth, double skew)
    {
        var capWidth = width * .17;
        var leftCap = Parallelogram(new Point(left + skew, top), new Point(left + skew + capWidth, top), new Point(left + capWidth, top + height), new Point(left, top + height));
        var rightCap = Parallelogram(new Point(left + width - capWidth, top), new Point(left + width, top), new Point(left + width - skew, top + height), new Point(left + width - skew - capWidth, top + height));
        dc.DrawGeometry(ChipCapBrush(), EngineeringOutlinePen(), leftCap);
        dc.DrawGeometry(ChipCapBrush(), EngineeringOutlinePen(), rightCap);
        dc.DrawRoundedRectangle(ChipCapShadowBrush(), EngineeringOutlinePen(), new Rect(left, top + height, capWidth, depth), 2, 2);
        dc.DrawRoundedRectangle(ChipCapShadowBrush(), EngineeringOutlinePen(), new Rect(left + width - skew - capWidth, top + height, capWidth, depth), 2, 2);
    }

    private void DrawThreeDimensionalContacts(DrawingContext dc, double left, double top, double width, double height, double depth, double skew, DrawingScene scene, PackageTopologyType topology)
    {
        var contacts = scene.Primitives.Where(p => p.Kind is ScenePrimitiveKind.Lead or ScenePrimitiveKind.Pad or ScenePrimitiveKind.Ball or ScenePrimitiveKind.Terminal or ScenePrimitiveKind.Tab).ToArray();
        if (contacts.Length == 0) return;
        var bounds = scene.Bounds;
        foreach (var contact in contacts)
        {
            var nx = bounds.Width > 0 ? (contact.Bounds.Left + contact.Bounds.Width / 2 - bounds.Left) / bounds.Width : .5;
            var ny = bounds.Height > 0 ? (contact.Bounds.Top + contact.Bounds.Height / 2 - bounds.Top) / bounds.Height : .5;
            var x = left + skew + nx * (width - skew * 2) - ny * skew;
            var y = top + ny * height + (ny > .5 ? depth : 0);
            var size = Math.Clamp(Math.Min(width, height) / 18, 3, 8);
            if (topology == PackageTopologyType.Bga || contact.Kind == ScenePrimitiveKind.Ball)
                dc.DrawEllipse(MetalBrush(), EngineeringOutlinePen(), new Point(x, y), size, size);
            else
                dc.DrawRoundedRectangle(MetalBrush(), EngineeringOutlinePen(), new Rect(x - size, y - size / 2, size * 2, size), 1, 1);
        }
    }

    private void DrawPrimitive(DrawingContext dc, ScenePrimitive p, PackageTopologyType topology, bool engineeringMaterial = false)
    {
        var body = engineeringMaterial && topology == PackageTopologyType.Chip ? CeramicBrush() : PackageBodyBrush();
        var metal = engineeringMaterial ? MetalBrush() : GeometryBrush();
        var outline = engineeringMaterial ? EngineeringOutlinePen(.018) : new Pen(GeometryBrush(), 0.018);
        switch (p.Kind)
        {
            case ScenePrimitiveKind.Body:
                if (p.Marker == "Circular") dc.DrawEllipse(body, outline, Center(p.Bounds), p.Bounds.Width / 2, p.Bounds.Height / 2);
                else dc.DrawRectangle(body, outline, p.Bounds);
                break;
            case ScenePrimitiveKind.Lead:
                dc.DrawRectangle(metal, engineeringMaterial ? outline : null, p.Bounds); break;
            case ScenePrimitiveKind.Pad:
            case ScenePrimitiveKind.Terminal:
                dc.DrawRectangle(metal, engineeringMaterial ? outline : null, p.Bounds); break;
            case ScenePrimitiveKind.Ball:
                dc.DrawEllipse(metal, engineeringMaterial ? outline : null, Center(p.Bounds), p.Bounds.Width / 2, p.Bounds.Height / 2); break;
            case ScenePrimitiveKind.Tab:
            case ScenePrimitiveKind.ExposedPad:
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(120, 132, 144)), new Pen(Brushes.Silver, .025), p.Bounds); break;
            case ScenePrimitiveKind.SideBody:
                dc.DrawRoundedRectangle(body, outline, p.Bounds, .08, .08); break;
            case ScenePrimitiveKind.SideLead:
                dc.DrawRectangle(metal, engineeringMaterial ? outline : null, p.Bounds); break;
            case ScenePrimitiveKind.Marker:
                DrawMarker(dc, p); break;
        }
    }

    private static Point Center(Rect r) => new(r.Left + r.Width / 2, r.Top + r.Height / 2);

    private void DrawMarker(DrawingContext dc, ScenePrimitive p)
    {
        switch (p.Marker)
        {
            case "CathodeBar":
                dc.DrawRectangle(Brushes.White, null, p.Bounds); break;
            case "PlusMark":
                var c = Center(p.Bounds); var pen = new Pen(Brushes.White, Math.Max(.03, p.Bounds.Width * .12));
                dc.DrawLine(pen, new Point(p.Bounds.Left, c.Y), new Point(p.Bounds.Right, c.Y));
                dc.DrawLine(pen, new Point(c.X, p.Bounds.Top), new Point(c.X, p.Bounds.Bottom)); break;
            default:
                dc.DrawEllipse(Brushes.White, null, Center(p.Bounds), p.Bounds.Width / 2, p.Bounds.Height / 2); break;
        }
    }

    private void DrawDimensionLines(DrawingContext dc, PackageDefinition package, PackageDrawingTemplate template, DrawingScene scene, FitToViewportResult fit)
    {
        var body = scene.Primitives.FirstOrDefault(p => p.Kind == ScenePrimitiveKind.Body);
        if (body is null) return;
        var screen = ToScreen(body.Bounds, fit);
        var pen = new Pen(DimensionBrush(), 1);
        var x = template.Parameters.FirstOrDefault(p => p.SourceProperty == "BodyLength");
        var y = template.Parameters.FirstOrDefault(p => p.SourceProperty == "BodyWidth");
        var h = template.Parameters.FirstOrDefault(p => p.SourceProperty == "Height");
        if (x is not null && !IsUnknown(package, x.SourceProperty))
            HorizontalDimension(dc, screen.Left, screen.Right, screen.Top - 22, $"{x.Key} {Value(package, x.SourceProperty, x.IsCount)}", pen);
        if (y is not null && !IsUnknown(package, y.SourceProperty))
            VerticalDimension(dc, screen.Top, screen.Bottom, screen.Left - 22, $"{y.Key} {Value(package, y.SourceProperty, y.IsCount)}", pen);

        var lead = scene.Primitives.FirstOrDefault(p => p.Kind is ScenePrimitiveKind.Lead or ScenePrimitiveKind.Pad);
        var width = template.Parameters.FirstOrDefault(p => p.SourceProperty == "LeadWidth");
        if (lead is not null && width is not null && !IsUnknown(package, width.SourceProperty))
        {
            var r = ToScreen(lead.Bounds, fit);
            HorizontalDimension(dc, r.Left, r.Right, r.Bottom + 16, $"{width.Key} {Value(package, width.SourceProperty, width.IsCount)}", pen);
        }

        var contacts = scene.Primitives.Where(p => p.Kind is ScenePrimitiveKind.Lead or ScenePrimitiveKind.Pad).OrderBy(p => p.Bounds.Top).ToArray();
        var pitch = template.Parameters.FirstOrDefault(p => p.SourceProperty == "Pitch" || p.SourceProperty == "BallPitch");
        if (contacts.Length > 1 && pitch is not null && !IsUnknown(package, pitch.SourceProperty))
        {
            var first = ToScreen(contacts[0].Bounds, fit);
            var second = ToScreen(contacts[1].Bounds, fit);
            if (Math.Abs(second.Top - first.Top) > 3)
                VerticalDimension(dc, first.Top + first.Height / 2, second.Top + second.Height / 2, Math.Min(first.Left, second.Left) - 34, $"{pitch.Key} {Value(package, pitch.SourceProperty, pitch.IsCount)}", pen);
        }

        var ball = scene.Primitives.FirstOrDefault(p => p.Kind == ScenePrimitiveKind.Ball);
        var diameter = template.Parameters.FirstOrDefault(p => p.SourceProperty == "BallDiameter");
        if (ball is not null && diameter is not null && !IsUnknown(package, diameter.SourceProperty))
        {
            var r = ToScreen(ball.Bounds, fit);
            HorizontalDimension(dc, r.Left, r.Right, r.Top - 14, $"{diameter.Key} {Value(package, diameter.SourceProperty, diameter.IsCount)}", pen);
        }

        if (h is not null && !IsUnknown(package, h.SourceProperty))
        {
            var side = scene.Primitives.FirstOrDefault(p => p.Kind == ScenePrimitiveKind.SideBody);
            if (side is not null)
            {
                var r = ToScreen(side.Bounds, fit);
                VerticalDimension(dc, r.Top, r.Bottom, r.Right + 18, $"{h.Key} {Value(package, h.SourceProperty, h.IsCount)}", pen);
            }
        }
    }

    private static Rect ToScreen(Rect value, FitToViewportResult fit) => new(value.X * fit.Scale + fit.Offset.X, value.Y * fit.Scale + fit.Offset.Y, value.Width * fit.Scale, value.Height * fit.Scale);

    private void HorizontalDimension(DrawingContext dc, double from, double to, double y, string label, Pen pen)
    {
        if (to <= from) return;
        dc.DrawLine(pen, new Point(from, y), new Point(to, y));
        dc.DrawLine(pen, new Point(from, y - 4), new Point(from, y + 4));
        dc.DrawLine(pen, new Point(to, y - 4), new Point(to, y + 4));
        Arrow(dc, new Point(from, y), 1, 0, pen); Arrow(dc, new Point(to, y), -1, 0, pen);
        DrawText(dc, label, new Point((from + to) / 2 - 18, y - 16), pen.Brush);
    }

    private void VerticalDimension(DrawingContext dc, double from, double to, double x, string label, Pen pen)
    {
        if (to <= from) return;
        dc.DrawLine(pen, new Point(x, from), new Point(x, to));
        dc.DrawLine(pen, new Point(x - 4, from), new Point(x + 4, from));
        dc.DrawLine(pen, new Point(x - 4, to), new Point(x + 4, to));
        Arrow(dc, new Point(x, from), 0, 1, pen); Arrow(dc, new Point(x, to), 0, -1, pen);
        var labelPoint = new Point(x - 13, (from + to) / 2);
        dc.PushTransform(new RotateTransform(-90, labelPoint.X, labelPoint.Y));
        DrawText(dc, label, new Point(labelPoint.X - 18, labelPoint.Y - 7), pen.Brush);
        dc.Pop();
    }

    private static void Arrow(DrawingContext dc, Point p, double dx, double dy, Pen pen)
    {
        var normal = new Vector(-dy, dx);
        var direction = new Vector(dx, dy);
        dc.DrawLine(pen, p, p + direction * 6 + normal * 3);
        dc.DrawLine(pen, p, p + direction * 6 - normal * 3);
    }

    private Brush GeometryBrush() => TryFindResource("GridBorderBrush") as Brush ?? Brushes.DimGray;
    private Brush DimensionBrush() => TryFindResource("TextBrush") as Brush ?? Brushes.Black;
    private static Brush PackageBodyBrush() => new SolidColorBrush(Color.FromRgb(92, 105, 116));
    private static Brush PackageBodyTopBrush() => new SolidColorBrush(Color.FromRgb(111, 126, 138));
    private static Brush PackageBodyShadowBrush() => new SolidColorBrush(Color.FromRgb(70, 82, 92));
    private static Brush CeramicBrush() => new LinearGradientBrush(
        Color.FromRgb(124, 126, 116), Color.FromRgb(87, 99, 108),
        new Point(0, 0), new Point(1, 1));
    private static Brush CeramicShadowBrush() => new SolidColorBrush(Color.FromRgb(52, 64, 75));
    private static Brush MetalBrush() => new SolidColorBrush(Color.FromRgb(220, 224, 226));
    private static Brush ChipCapBrush() => new LinearGradientBrush(
        Color.FromRgb(232, 231, 222), Color.FromRgb(174, 178, 177),
        new Point(0, 0), new Point(1, 1));
    private static Brush ChipCapShadowBrush() => new LinearGradientBrush(
        Color.FromRgb(205, 210, 208), Color.FromRgb(150, 158, 161),
        new Point(0, 0), new Point(0, 1));
    private static Brush MetalShadowBrush() => new SolidColorBrush(Color.FromRgb(176, 183, 187));
    private static Pen EngineeringOutlinePen(double thickness = .8) => new(Brushes.Black, thickness);
    private static Pen EngineeringDimensionPen() => new(Brushes.Black, .8);
    private double BodyLength(PackageDefinition package) => Geometry?.BodyLength > 0 ? Geometry.BodyLength : package.BodyLength > 0 ? package.BodyLength : package.Length;
    private double BodyWidth(PackageDefinition package) => Geometry?.BodyWidth > 0 ? Geometry.BodyWidth : package.BodyWidth > 0 ? package.BodyWidth : package.Width;
    private double GeometryBodyHeight(PackageDefinition package) => Geometry?.BodyHeight > 0 ? Geometry.BodyHeight : package.Height;
    private double GeometryLeadLength(PackageDefinition package, double fallback) => Geometry?.LeadLength > 0 ? Geometry.LeadLength : package.LeadLength > 0 ? package.LeadLength : fallback;
    private double GeometryLeadWidth(PackageDefinition package, double fallback) => Geometry?.LeadWidth > 0 ? Geometry.LeadWidth : package.LeadWidth > 0 ? package.LeadWidth : fallback;
    private int GeometryLeadCount(PackageDefinition package) => Geometry?.LeadCount > 0 ? Geometry.LeadCount : Math.Max(1, package.LeadCount > 0 ? package.LeadCount : package.PadCount);
    private double GeometryPitch(PackageDefinition package, double fallback) => Geometry?.LeadPitch > 0 ? Geometry.LeadPitch : package.Pitch > 0 ? package.Pitch : fallback;
    private static string Millimetres(double value) => $"{value:0.00} мм";
    private static bool IsUnknown(PackageDefinition p, string property) => property switch
    {
        "BodyLength" => p.BodyLength <= 0 && p.Length <= 0,
        "BodyWidth" => p.BodyWidth <= 0 && p.Width <= 0,
        "Height" => p.Height <= 0,
        "LeadCount" => p.LeadCount <= 0 && p.PadCount <= 0,
        "PadCount" => p.PadCount <= 0,
        "Pitch" => p.Pitch <= 0,
        "LeadLength" => p.LeadLength <= 0,
        "LeadWidth" => p.LeadWidth <= 0,
        "ThermalPadLength" => p.ThermalPadLength <= 0,
        "ThermalPadWidth" => p.ThermalPadWidth <= 0,
        "BallPitch" => p.BallPitch <= 0,
        "BallDiameter" => p.BallDiameter <= 0,
        _ => true
    };

    private static string Value(PackageDefinition p, string property, bool count) 
    {
        var v = property switch
        {
            "BodyLength" => p.BodyLength > 0 ? p.BodyLength : p.Length,
            "BodyWidth" => p.BodyWidth > 0 ? p.BodyWidth : p.Width,
            "Height" => p.Height, "LeadCount" => p.LeadCount > 0 ? p.LeadCount : p.PadCount, "PadCount" => p.PadCount,
            "Pitch" => p.Pitch, "LeadLength" => p.LeadLength, "LeadWidth" => p.LeadWidth,
            "ThermalPadLength" => p.ThermalPadLength, "ThermalPadWidth" => p.ThermalPadWidth,
            "BallPitch" => p.BallPitch, "BallDiameter" => p.BallDiameter, _ => 0d
        };
        return count ? $"{v:0}" : $"{v:0.###}";
    }

    private void DrawText(DrawingContext dc, string text, Point point, Brush brush)
    {
        var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI Semibold"), 11, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(ft, point);
    }

#if DEBUG
    private void DrawDebugOverlay(DrawingContext dc, DrawingScene scene, FitToViewportResult fit)
    {
        var text = $"Template: {scene.TemplateId}  Scale: {fit.Scale:0.0} px/mm  Leads: {scene.LeadCount}  Pads: {scene.PadCount}  Balls: {scene.BallCount}";
        DrawText(dc, text, new Point(12, ActualHeight - 20), Brushes.DimGray);
    }
#endif
}
