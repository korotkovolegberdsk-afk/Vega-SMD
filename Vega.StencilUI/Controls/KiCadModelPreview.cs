using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Vega.CAD;
using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.Controls;

public sealed class KiCadModelPreview : UserControl
{
    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(nameof(Package), typeof(PackageDefinition), typeof(KiCadModelPreview), new PropertyMetadata(null, OnPackageChanged));
    public PackageDefinition? Package { get => (PackageDefinition?)GetValue(PackageProperty); set => SetValue(PackageProperty, value); }
    public static readonly DependencyProperty ModelPathProperty = DependencyProperty.Register(nameof(ModelPath), typeof(string), typeof(KiCadModelPreview), new PropertyMetadata("", OnPackageChanged));
    public string ModelPath { get => (string)GetValue(ModelPathProperty); set => SetValue(ModelPathProperty, value); }
    public static readonly DependencyProperty CadModelProperty = DependencyProperty.Register(nameof(CadModel), typeof(ComponentCadModel), typeof(KiCadModelPreview), new PropertyMetadata(null, OnPackageChanged));
    public ComponentCadModel? CadModel { get => (ComponentCadModel?)GetValue(CadModelProperty); set => SetValue(CadModelProperty, value); }

    private readonly Grid _grid = new() { Width = 922, Height = 589, Background = ComponentCardPalette.Separator, ClipToBounds = true };
    private readonly TextBlock _status = new() { Margin = new Thickness(12), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly StepModelProjectionPipeline _pipeline = new();
    private ModelVisual3D? _modelVisual;
    private Point _lastMouse;
    private bool _isRotating;
    private int _refreshGeneration;
    private AxisAngleRotation3D _yaw = new(new Vector3D(0, 1, 0), -35);
    private AxisAngleRotation3D _pitch = new(new Vector3D(1, 0, 0), 20);

    public KiCadModelPreview()
    {
        ClipToBounds = true;
        Content = new Viewbox { Stretch = Stretch.Uniform, Child = _grid };
        _grid.RowDefinitions.Add(new RowDefinition()); _grid.RowDefinitions.Add(new RowDefinition());
        _grid.ColumnDefinitions.Add(new ColumnDefinition()); _grid.ColumnDefinitions.Add(new ColumnDefinition());
        _grid.Children.Add(_status);
        Loaded += (_, _) => Refresh();
    }

    private static void OnPackageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((KiCadModelPreview)d).Refresh();

    private async void Refresh()
    {
        var refreshGeneration = ++_refreshGeneration;
        if (!IsLoaded || Package is null) return;
        var package = Package;
        var requestedModelPath = ModelPath ?? string.Empty;
        var dispatcher = Dispatcher;
        if (SodPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSodViews(package.PackageName);
            return;
        }
        if (MelfPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedMelfViews();
            return;
        }
        if (SotPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSotViews();
            return;
        }
        if (Sot23MultiLeadPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSot23MultiLeadViews(package.PackageName);
            return;
        }
        if (SotMicroPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSotMicroViews(package.PackageName);
            return;
        }
        if (Sot523PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSot523Views();
            return;
        }
        if (Sot563PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSot563Views();
            return;
        }
        if (Sot723PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSot723Views();
            return;
        }
        if (SotPowerPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSotPowerViews(package.PackageName);
            return;
        }
        if (DpakPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedDpakViews(package.PackageName);
            return;
        }
        if (SoicPackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSoicViews();
            return;
        }
        if (Soic16PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedSoic16Views(package.PackageName);
            return;
        }
        if (Qfp32PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedQfp32Views();
            return;
        }
        if (Qfn32PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedQfn32Views();
            return;
        }
        if (Bga64PackageDrawing.Supports(package.PackageName))
        {
            ShowDocumentedBga64Views();
            return;
        }
        var model = string.IsNullOrWhiteSpace(requestedModelPath)
            ? KiCadModelRenderService.ResolveModelPath(package.PackageName, package.Model3DFile)
            : Path.IsPathRooted(requestedModelPath) ? requestedModelPath : Path.Combine(AppContext.BaseDirectory, requestedModelPath);
        if (model is null)
        {
            if (IsCurrentRefresh(refreshGeneration, package, requestedModelPath))
            {
                _grid.Children.Clear();
                _grid.Children.Add(_status);
                _status.Text = "STEP-модель корпуса не найдена";
            }
            return;
        }
        _status.Text = "Построение видов из STEP-модели…";
        try
        {
            var folder = Path.Combine(Path.GetTempPath(), "Vega-SMD", "InProcessStep", package.PackageName);
            StepProjectionSet result;
            try
            {
                result = await Task.Run(() => _pipeline.Build(model, folder, 250));
            }
            catch when (TryGetCachedMesh(model, out var cachedMesh))
            {
                result = await Task.Run(() => _pipeline.BuildFromGlb(model, cachedMesh!, 250));
            }
            await dispatcher.InvokeAsync(() =>
            {
                if (IsCurrentRefresh(refreshGeneration, package, requestedModelPath)) ShowViews(result);
            });
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(Path.GetTempPath(), "Vega-SMD", "KiCadModelPreview.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"{DateTime.Now:O}\nPackage={package.PackageName}\nModel={model}\n{ex}\n\n");
            await dispatcher.InvokeAsync(() =>
            {
                if (IsCurrentRefresh(refreshGeneration, package, requestedModelPath))
                    _status.Text = "Ошибка загрузки STEP-модели. Подробности записаны в журнал.";
            });
        }
    }

    private bool IsCurrentRefresh(int generation, PackageDefinition package, string requestedModelPath) =>
        generation == _refreshGeneration &&
        ReferenceEquals(Package, package) &&
        string.Equals(ModelPath ?? string.Empty, requestedModelPath, StringComparison.Ordinal);

    private static bool TryGetCachedMesh(string modelPath, out string? meshPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(modelPath) + ".glb";
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "Generated", fileName),
            Path.Combine(AppContext.BaseDirectory, "checked", fileName),
            Path.Combine(AppContext.BaseDirectory, "net8.0-windows", "checked", fileName),
        };
        meshPath = candidates.FirstOrDefault(File.Exists);
        return meshPath is not null;
    }

    private void ShowViews(StepProjectionSet result)
    {
        _grid.Children.Clear();
        var package = Package;
        var sharedProjectionScale = GetSharedProjectionScale(result, package);
        var neutralReferenceAppearance = string.Equals(CadModel?.VerificationStatus, "ReferenceValidated", StringComparison.OrdinalIgnoreCase);
        if (SodPackageDrawing.Supports(package?.PackageName))
        {
            ShowDocumentedSodViews(package!.PackageName);
            return;
        }
        AddPanel(0, 0, "1 ВИД СВЕРХУ", DrawProjection(result.Views[StepProjectionKind.Top], StepProjectionKind.Top, true, neutralReferenceAppearance, package, fitScaleOverride: sharedProjectionScale));
        AddPanel(1, 0, "2 ВИД СНИЗУ", DrawProjection(result.Views[StepProjectionKind.Bottom], StepProjectionKind.Bottom, false, neutralReferenceAppearance, package, fitScaleOverride: sharedProjectionScale));
        // The end view of a two-terminal chip is the metallised electrode.
        // Keep it light, matching the terminal faces visible in the top and 3D views.
        AddPanel(0, 1, "3 ВИД СБОКУ", DrawProjection(result.Views[StepProjectionKind.Side], StepProjectionKind.Side, true, neutralReferenceAppearance, package, forceLightFill: true, fitScaleOverride: sharedProjectionScale));
        AddPanel(1, 1, "4 3D ВИД", CreateViewport(result.MeshPath, result.Views[StepProjectionKind.Isometric], neutralReferenceAppearance));
    }

    private void ShowDocumentedSodViews(string name)
    {
        _grid.Children.Clear();
        var documentedMesh = SodPackageDrawing.BuildDocumentedMesh(name);
        var projector = new StepProjectionGeometry();
        AddPanel(0, 0, "1 ВИД СВЕРХУ", SodPackageDrawing.Create(projector.Project(documentedMesh, StepProjectionKind.Top, 250), StepProjectionKind.Top, name));
        AddPanel(1, 0, "2 ВИД СНИЗУ", SodPackageDrawing.Create(projector.Project(documentedMesh, StepProjectionKind.Bottom, 250), StepProjectionKind.Bottom, name));
        AddPanel(0, 1, "3 ВИД СБОКУ", SodPackageDrawing.Create(projector.Project(documentedMesh, StepProjectionKind.Longitudinal, 250), StepProjectionKind.Longitudinal, name));
        AddPanel(1, 1, "4 3D ВИД", CreateViewport(string.Empty, projector.Project(documentedMesh, StepProjectionKind.Isometric, 250), false, documentedMesh));
    }

    private void ShowDocumentedMelfViews()
    {
        _grid.Children.Clear();
        var mesh=MelfPackageDrawing.BuildDocumentedMesh();
        var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",MelfPackageDrawing.Create(projector.Project(mesh,StepProjectionKind.Top,250),StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",MelfPackageDrawing.Create(projector.Project(mesh,StepProjectionKind.Bottom,250),StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",MelfPackageDrawing.Create(projector.Project(mesh,StepProjectionKind.Side,250),StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedSotViews()
    {
        _grid.Children.Clear();
        var mesh=SotPackageDrawing.BuildDocumentedMesh();var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",SotPackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",SotPackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",SotPackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedSot23MultiLeadViews(string name)
    {
        _grid.Children.Clear();
        var mesh=Sot23MultiLeadPackageDrawing.BuildDocumentedMesh(name); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Sot23MultiLeadPackageDrawing.Create(name,StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Sot23MultiLeadPackageDrawing.Create(name,StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Sot23MultiLeadPackageDrawing.Create(name,StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedSotMicroViews(string name)
    {
        _grid.Children.Clear();
        var mesh=SotMicroPackageDrawing.BuildDocumentedMesh(name); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",SotMicroPackageDrawing.Create(name,StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",SotMicroPackageDrawing.Create(name,StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",SotMicroPackageDrawing.Create(name,StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedSot523Views()
    {
        _grid.Children.Clear();
        var mesh=Sot523PackageDrawing.BuildDocumentedMesh(); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Sot523PackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Sot523PackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Sot523PackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }
    private void ShowDocumentedSot563Views()
    {
        _grid.Children.Clear();
        var mesh=Sot563PackageDrawing.BuildDocumentedMesh(); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Sot563PackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Sot563PackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Sot563PackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }
    private void ShowDocumentedSot723Views()
    {
        _grid.Children.Clear();
        var mesh=Sot723PackageDrawing.BuildDocumentedMesh(); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Sot723PackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Sot723PackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Sot723PackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }
    private void ShowDocumentedSotPowerViews(string name)
    {
        _grid.Children.Clear();
        var mesh=SotPowerPackageDrawing.BuildDocumentedMesh(name); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",SotPowerPackageDrawing.Create(name,StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",SotPowerPackageDrawing.Create(name,StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",SotPowerPackageDrawing.Create(name,StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }
    private void ShowDocumentedDpakViews(string name)
    {
        _grid.Children.Clear();
        var mesh=DpakPackageDrawing.BuildDocumentedMesh(name); var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",DpakPackageDrawing.Create(name,StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",DpakPackageDrawing.Create(name,StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",DpakPackageDrawing.Create(name,StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }
    private void ShowDocumentedSoicViews()
    {
        _grid.Children.Clear();
        var mesh=SoicPackageDrawing.BuildDocumentedMesh();var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",SoicPackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",SoicPackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",SoicPackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedSoic16Views(string packageName)
    {
        _grid.Children.Clear();
        var mesh=Soic16PackageDrawing.BuildDocumentedMesh(packageName);var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Soic16PackageDrawing.Create(StepProjectionKind.Top,packageName));
        AddPanel(1,0,"2 ВИД СНИЗУ",Soic16PackageDrawing.Create(StepProjectionKind.Bottom,packageName));
        AddPanel(0,1,"3 ВИД СБОКУ",Soic16PackageDrawing.Create(StepProjectionKind.Side,packageName));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedQfp32Views()
    {
        _grid.Children.Clear();
        var mesh=Qfp32PackageDrawing.BuildDocumentedMesh();var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Qfp32PackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Qfp32PackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Qfp32PackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedQfn32Views()
    {
        _grid.Children.Clear();
        var mesh=Qfn32PackageDrawing.BuildDocumentedMesh();var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Qfn32PackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Qfn32PackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Qfn32PackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private void ShowDocumentedBga64Views()
    {
        _grid.Children.Clear();
        var mesh=Bga64PackageDrawing.BuildDocumentedMesh();var projector=new StepProjectionGeometry();
        AddPanel(0,0,"1 ВИД СВЕРХУ",Bga64PackageDrawing.Create(StepProjectionKind.Top));
        AddPanel(1,0,"2 ВИД СНИЗУ",Bga64PackageDrawing.Create(StepProjectionKind.Bottom));
        AddPanel(0,1,"3 ВИД СБОКУ",Bga64PackageDrawing.Create(StepProjectionKind.Side));
        AddPanel(1,1,"4 3D ВИД",CreateViewport(string.Empty,projector.Project(mesh,StepProjectionKind.Isometric,250),false,mesh));
    }

    private static double GetSharedProjectionScale(StepProjectionSet result, PackageDefinition? package)
    {
        const double leftDimensionReserve = 140;
        const double rightReserve = 20;
        var isSod323 = package?.PackageName.Equals("SOD323", StringComparison.OrdinalIgnoreCase) == true;
        var topBodyReserve = isSod323 ? 110d : 112d;
        var bottomBodyReserve = isSod323 ? 100d : 20d;
        var projections = new[]
        {
            result.Views[StepProjectionKind.Top],
            result.Views[StepProjectionKind.Bottom],
            result.Views[StepProjectionKind.Side]
        };
        return Math.Min(1d, projections.Min(projection => Math.Min(
            (512d - leftDimensionReserve - rightReserve) / projection.Width,
            (320d - topBodyReserve - bottomBodyReserve) / projection.Height)));
    }

    private void AddPanel(int column, int row, string title, UIElement content)
    {
        var panel = new Grid { Background = ComponentCardPalette.Projection, Margin = new Thickness(4), ClipToBounds = true };
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); panel.RowDefinitions.Add(new RowDefinition());
        panel.Children.Add(new TextBlock { Text = title, Foreground = ComponentCardPalette.Title, FontFamily = new FontFamily("Arial"), FontSize = 28, Margin = new Thickness(18, 10, 8, 2) });
        Grid.SetRow(content, 1); panel.Children.Add(content); Grid.SetColumn(panel, column); Grid.SetRow(panel, row); _grid.Children.Add(panel);
    }

    private static UIElement DrawProjection(StepProjection projection, StepProjectionKind projectionKind, bool showDimensions, bool neutralReferenceAppearance, PackageDefinition? package = null, double scale = 1.0, bool forceLightFill = false, double? fitScaleOverride = null)
    {
        var canvas = new Canvas { Width = 512, Height = 320, Background = Brushes.Transparent };
        // C0402 fits at the approved 250 px/mm scale. Larger exact-MPN STEP
        // models use one uniform reduction so their true dimensions, labels
        // and extension lines remain inside the same projection canvas.
        const double leftDimensionReserve = 140;
        const double rightReserve = 20;
        // 56 units are used by the horizontal dimension line and the Arial
        // dimension label requires a small clear gap above it.
        var isSod323 = package?.PackageName.Equals("SOD323", StringComparison.OrdinalIgnoreCase) == true;
        var topBodyReserve = isSod323 ? 110d : 112d;
        var bottomBodyReserve = isSod323 ? 100d : 20d;
        var fitScale = fitScaleOverride ?? Math.Min(
            1d,
            Math.Min(
                (512d - leftDimensionReserve - rightReserve) / projection.Width,
                (320d - topBodyReserve - bottomBodyReserve) / projection.Height));
        var drawingScale = scale * fitScale;
        var offsetX = fitScale < 1
            ? 256 + (leftDimensionReserve - 256) / drawingScale
            : (512 - projection.Width) / 2;
        var offsetY = fitScale < 1
            ? (topBodyReserve / drawingScale) - (160 / drawingScale) + 160
            : (320 - projection.Height) / 2 + 36;
        // Fill adjacent triangles of one material together: no internal tessellation seams.
        GeometryGroup? batch = null;
        Color? batchColor = null;
        foreach (var triangle in projection.Triangles)
        {
            if (IsDecorativeManufacturerMarking(triangle.Color)) continue;
            var geometry = new StreamGeometry();
            using (var context = geometry.Open()) { context.BeginFigure(ToPoint(triangle.A, offsetX, offsetY, drawingScale), true, true); context.LineTo(ToPoint(triangle.B, offsetX, offsetY, drawingScale), true, false); context.LineTo(ToPoint(triangle.C, offsetX, offsetY, drawingScale), true, false); }
            geometry.Freeze(); var color = forceLightFill ? Color.FromRgb(210, 210, 200) : ToDisplayColor(triangle.Color, neutralReferenceAppearance);
            if (batch is null || batchColor != color)
            {
                batch = new GeometryGroup { FillRule = FillRule.Nonzero };
                batchColor = color;
                canvas.Children.Add(new System.Windows.Shapes.Path { Data = batch, Fill = new SolidColorBrush(color), StrokeThickness = 0 });
            }
            batch.Children.Add(geometry);
        }
        var x1 = 256 + (offsetX - 256) * drawingScale; var x2 = 256 + (offsetX + projection.Width - 256) * drawingScale;
        var y1 = 160 + (offsetY - 160) * drawingScale; var y2 = 160 + (offsetY + projection.Height - 160) * drawingScale;
        if (forceLightFill)
        {
            // A side view is the terminal end face. Render it as one solid
            // light contour so tessellation/material gaps cannot make it black
            // or transparent while the dimensions remain tied to the STEP bounds.
            var endFace = new System.Windows.Shapes.Rectangle
            {
                Width = x2 - x1,
                Height = y2 - y1,
                Fill = new SolidColorBrush(Color.FromRgb(210, 210, 200)),
                Stroke = ComponentCardPalette.Dimension,
                StrokeThickness = 1
            };
            Canvas.SetLeft(endFace, x1);
            Canvas.SetTop(endFace, y1);
            canvas.Children.Add(endFace);
        }
        if (showDimensions)
        {
            if (isSod323)
                AddSod323Dimensions(canvas, projectionKind, x1, x2, y1, y2);
            else
            {
                // StepProjection stores geometry in pixels; the pipeline uses 250 px/mm.
                // Dimension labels come from the actual projected bounds.
                AddHorizontal(canvas, x1, x2, y1 - 56, y1, FormatDimension(projection.Width / 250));
                AddVertical(canvas, x1 - 56, y1, y2, x1, FormatDimension(projection.Height / 250));
            }
        }
        return new Viewbox
        {
            Stretch = Stretch.Uniform,
            Child = canvas,
            Margin = new Thickness(8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static void AddSod323Dimensions(Canvas canvas, StepProjectionKind projectionKind, double x1, double x2, double y1, double y2)
    {
        if (projectionKind == StepProjectionKind.Top)
        {
            AddHorizontal(canvas, x1, x2, y1 - 56, y1, "2,50");
            AddVertical(canvas, x1 - 56, y1, y2, x1, "1,30");
            AddCompactHorizontal(canvas, x1, x1 + (x2 - x1) * .12, y2 + 54, y2, "0,30");
            return;
        }

        if (projectionKind == StepProjectionKind.Side)
        {
            AddHorizontal(canvas, x1, x2, y1 - 56, y1, "1,70");
            AddVertical(canvas, x1 - 56, y1, y2, x1, "1,05");
            AddCompactHorizontal(canvas, x1, x1 + (x2 - x1) * .22, y2 + 54, y2, "0,11");
        }
    }

    private static string FormatDimension(double millimetres) =>
        millimetres.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

    private UIElement CreateViewport(string meshPath, StepProjection isometric, bool neutralReferenceAppearance, IReadOnlyList<GlbTriangle>? documentedMesh = null)
    {
        var triangles = (documentedMesh ?? new GlbMeshReader().ReadTriangles(meshPath))
            .Where(triangle => !IsDecorativeManufacturerMarking(triangle.Color))
            .ToArray();
        var points = triangles.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray();
        var min = new Vector3(points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z));
        var max = new Vector3(points.Max(p => p.X), points.Max(p => p.Y), points.Max(p => p.Z));
        var center = (min + max) * .5f;
        var radius = Math.Max(Math.Max(max.X - min.X, max.Y - min.Y), max.Z - min.Z);
        var group = new Model3DGroup();
        foreach (var materialGroup in triangles.GroupBy(t => ToDisplayColor(t.Color, neutralReferenceAppearance)))
        {
            var mesh = new MeshGeometry3D();
            foreach (var triangle in materialGroup)
            {
                var index = mesh.Positions.Count;
                mesh.Positions.Add(ToPoint3D(triangle.A)); mesh.Positions.Add(ToPoint3D(triangle.B)); mesh.Positions.Add(ToPoint3D(triangle.C));
                mesh.TriangleIndices.Add(index); mesh.TriangleIndices.Add(index + 1); mesh.TriangleIndices.Add(index + 2);
            }
            var material = new DiffuseMaterial(new SolidColorBrush(materialGroup.Key));
            group.Children.Add(new GeometryModel3D(mesh, material) { BackMaterial = material });
        }
        _yaw = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);
        _pitch = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
        var transform = new Transform3DGroup();
        transform.Children.Add(new RotateTransform3D(_yaw, center.X, center.Y, center.Z));
        transform.Children.Add(new RotateTransform3D(_pitch, center.X, center.Y, center.Z));
        _modelVisual = new ModelVisual3D { Content = group, Transform = transform };
        var viewport = new Viewport3D { Width = 512, Height = 320, ClipToBounds = true };
        viewport.Children.Add(new ModelVisual3D { Content = new AmbientLight(Color.FromRgb(180, 180, 180)) });
        viewport.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Color.FromRgb(90, 90, 90), new Vector3D(-1, 1, -2)) });
        viewport.Children.Add(_modelVisual);
        var distance = Math.Max(radius * 8, .001);
        var position = new Point3D(center.X + distance, center.Y - distance, center.Z + distance);
        // C0402 uses the approved 2.048 mm orthographic field exactly.
        // Exact larger models expand uniformly from their real mesh span,
        // so they fit without changing their physical proportions.
        var approvedC0402Field = 512.0 / 250 / 1000;
        var cameraWidth = string.Equals(Package?.PackageName, "C0402", StringComparison.OrdinalIgnoreCase)
            ? approvedC0402Field
            : documentedMesh is not null && SodPackageDrawing.Supports(Package?.PackageName)
                ? SodPackageDrawing.CameraFieldMetres(Package!.PackageName)
                : documentedMesh is not null && MelfPackageDrawing.Supports(Package?.PackageName)
                    ? MelfPackageDrawing.CameraFieldMetres
                : Math.Max(approvedC0402Field, radius * 2.25);
        // Hardware depth testing hides rear surfaces; no painter-order seams.
        var camera = new OrthographicCamera(position,
            new Vector3D(center.X - position.X, center.Y - position.Y, center.Z - position.Z),
            new Vector3D(0, 0, 1), cameraWidth) { NearPlaneDistance = .000001, FarPlaneDistance = 10 };
        viewport.Camera = camera;
        // Viewport3D can route input through an underlying 3D visual, which
        // makes drag handling inconsistent.  A transparent input surface is
        // deliberately placed above it so every point of the 3D cell accepts
        // rotation and wheel zoom in exactly the same way.
        var interactionSurface = new Border { Background = Brushes.Transparent, Cursor = Cursors.SizeAll };
        interactionSurface.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ClickCount == 2)
            {
                _yaw.Angle = 0;
                _pitch.Angle = 0;
                camera.Width = cameraWidth;
                e.Handled = true;
                return;
            }
            _lastMouse = e.GetPosition(interactionSurface);
            _isRotating = true;
            interactionSurface.CaptureMouse();
            e.Handled = true;
        };
        interactionSurface.MouseMove += (_, e) =>
        {
            if (!interactionSurface.IsMouseCaptured || !_isRotating) return;
            var p = e.GetPosition(interactionSurface);
            _yaw.Angle += (p.X - _lastMouse.X) * .6;
            _pitch.Angle -= (p.Y - _lastMouse.Y) * .6;
            _lastMouse = p;
            e.Handled = true;
        };
        interactionSurface.MouseLeftButtonUp += (_, e) =>
        {
            _isRotating = false;
            if (interactionSurface.IsMouseCaptured) interactionSurface.ReleaseMouseCapture();
            e.Handled = true;
        };
        interactionSurface.LostMouseCapture += (_, _) => _isRotating = false;
        // Zoom is local to the 3D cell: the wheel changes only the
        // orthographic camera field and cannot reach a parent ScrollViewer.
        interactionSurface.MouseWheel += (_, e) =>
        {
            camera.Width = Math.Clamp(camera.Width * (e.Delta > 0 ? .9 : 1.1), .0001, .1);
            e.Handled = true;
        };
        var viewportHost = new Grid();
        viewportHost.Children.Add(viewport);
        viewportHost.Children.Add(interactionSurface);
        return new Viewbox { Child = viewportHost, Stretch = Stretch.Uniform, Margin = new Thickness(8),
            HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
    }

    private static Point ToPoint(Vector2 point, double x, double y, double scale = 1) => new(256 + (point.X + x - 256) * scale, 160 + (point.Y + y - 160) * scale);
    private static Point3D ToPoint3D(Vector3 point) => new(point.X, point.Y, point.Z);
    private static Color ToColor(Vector4 color)
    {
        var r = (byte)(Math.Clamp(color.X, 0, 1) * 255);
        var g = (byte)(Math.Clamp(color.Y, 0, 1) * 255);
        var b = (byte)(Math.Clamp(color.Z, 0, 1) * 255);
        // Preserve the source material colours from the selected STEP model.
        // The approved C0402 appearance comes from its own model; other
        // components are not recoloured to brown or to a common electrode tone.
        return Color.FromArgb(255, r, g, b);
    }

    private static Color ToDisplayColor(Vector4 sourceColor, bool neutralReferenceAppearance)
    {
        var color = ToColor(sourceColor);
        // A KiCad reference model proves only the envelope.  Its dark ceramic
        // material must not be presented as the selected manufacturer's colour.
        // Exact MPN models retain every material authored in the model file.
        if (!neutralReferenceAppearance) return color;
        // KiCad terminal metallisation is frequently dark orange and disappears
        // against the engineering-card background. Make reference terminals
        // readable without claiming an MPN-specific material colour.
        if (color.R > color.G * 1.15 && color.G > color.B * 1.15)
            return Color.FromRgb(235, 180, 55);
        return color.R + color.G + color.B < 180 ? Color.FromRgb(118, 121, 128) : color;
    }

    private static bool IsDecorativeManufacturerMarking(Vector4 color)
    {
        // The KEMET STEP model contains its logo as a separate pale-blue
        // material.  It conveys no package geometry and is omitted from the
        // neutral engineering card, while the brown body and light electrodes
        // retain their original materials.
        return color.Z > color.Y + .12f && color.Z > color.X + .15f && color.Y >= color.X - .05f;
    }
    private static Color Darken(Color c) => Color.FromRgb((byte)(c.R * .65), (byte)(c.G * .65), (byte)(c.B * .65));
    private static void AddHorizontal(Canvas canvas, double x1, double x2, double y, double sourceY, string text, double labelGap = 8) { AddLine(canvas, x1, y, x2, y); AddLine(canvas, x1, sourceY, x1, y - 6); AddLine(canvas, x2, sourceY, x2, y - 6); AddArrow(canvas, x1, y, true, false); AddArrow(canvas, x2, y, false, false); AddLabel(canvas, text, (x1 + x2) / 2, y, false, labelGap); }
    private static void AddCompactHorizontal(Canvas canvas, double x1, double x2, double y, double sourceY, string text)
    {
        var leaderEnd = x2 + 112;
        AddLine(canvas, x1, y, leaderEnd, y);
        AddLine(canvas, x1, sourceY, x1, y - 6);
        AddLine(canvas, x2, sourceY, x2, y - 6);
        AddArrow(canvas, x1, y, true, false);
        AddArrow(canvas, x2, y, false, false);
        AddLabel(canvas, text, x2 + 64, y, false);
    }
    private static void AddVertical(Canvas canvas, double x, double y1, double y2, double sourceX, string text) { AddLine(canvas, x, y1, x, y2); AddLine(canvas, sourceX, y1, x - 6, y1); AddLine(canvas, sourceX, y2, x - 6, y2); AddArrow(canvas, x, y1, true, true); AddArrow(canvas, x, y2, false, true); AddLabel(canvas, text, x, (y1 + y2) / 2, true); }
    private static void AddLine(Canvas c, double x1, double y1, double x2, double y2) => c.Children.Add(new System.Windows.Shapes.Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = ComponentCardPalette.Dimension, StrokeThickness = 1.2 });
    private static void AddArrow(Canvas c, double x, double y, bool inward, bool vertical) { var p = new System.Windows.Shapes.Polygon { Fill = ComponentCardPalette.Dimension }; p.Points = vertical ? (inward ? new PointCollection { new(x, y), new(x - 4, y + 8), new(x + 4, y + 8) } : new PointCollection { new(x, y), new(x - 4, y - 8), new(x + 4, y - 8) }) : (inward ? new PointCollection { new(x, y), new(x + 8, y - 4), new(x + 8, y + 4) } : new PointCollection { new(x, y), new(x - 8, y - 4), new(x - 8, y + 4) }); c.Children.Add(p); }
    private static void AddLabel(Canvas c, string text, double x, double y, bool vertical, double labelGap = 8) { var label = new TextBlock { Text = text, FontFamily = new FontFamily("Arial"), FontSize = 34, Foreground = ComponentCardPalette.Dimension }; if (vertical) label.LayoutTransform = new RotateTransform(-90); label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); Canvas.SetLeft(label, vertical ? x - label.DesiredSize.Width - 8 : x - label.DesiredSize.Width / 2); Canvas.SetTop(label, vertical ? y - label.DesiredSize.Height / 2 : y - label.DesiredSize.Height - labelGap); c.Children.Add(label); }
}
