using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.Models.MasterLibrary;
using Vega.StencilUI.Controls;

namespace Vega.StencilUI.TapeReelDrawing;

/// <summary>Read-only schematic viewer of carrier tape geometry.</summary>
public partial class TapeReelPreview : UserControl
{
    public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.Register(
        nameof(ShowTitle), typeof(bool), typeof(TapeReelPreview), new PropertyMetadata(true, GeometryChanged));
    public bool ShowTitle { get => (bool)GetValue(ShowTitleProperty); set => SetValue(ShowTitleProperty, value); }
    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
        nameof(Geometry), typeof(ComponentTapeReelGeometry), typeof(TapeReelPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));

    public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
        nameof(Component), typeof(ComponentDefinition), typeof(TapeReelPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));

    // ComponentDefinition instances loaded for a card do not always carry their
    // Package navigation property.  Accept the card package explicitly so tape
    // presentation is chosen from the same package that the card displays.
    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(
        nameof(Package), typeof(PackageDefinition), typeof(TapeReelPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    private double _zoom = 1.0;
    private double _fitScale = 1.0;

    public ComponentTapeReelGeometry? Geometry
    {
        get => (ComponentTapeReelGeometry?)GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }

    public ComponentDefinition? Component
    {
        get => (ComponentDefinition?)GetValue(ComponentProperty);
        set => SetValue(ComponentProperty, value);
    }

    public PackageDefinition? Package
    {
        get => (PackageDefinition?)GetValue(PackageProperty);
        set => SetValue(PackageProperty, value);
    }
    public TapeReelPreview()
    {
        InitializeComponent();
        Loaded += (_, _) => Fit();
    }

    public void ZoomIn()
    {
        _zoom = Math.Min(10, _zoom * 1.25);
        Render();
    }

    public void ZoomOut()
    {
        _zoom = Math.Max(.1, _zoom / 1.25);
        Render();
    }

    public void Fit()
    {
        var geometry = Geometry;
        if (!HasDrawingGeometry(geometry) || DrawingCanvas.ActualWidth <= 0 || DrawingCanvas.ActualHeight <= 0) return;

        var p0 = geometry!.SprocketHolePitch;
        var p1 = geometry.PocketPitch;
        var tapeWidth = geometry.CarrierTapeWidth;
        var widthMm = Math.Max(p0 * .5 + p1 * 3.5, 20);
        _fitScale = Math.Min((DrawingCanvas.ActualWidth - 70) / widthMm, (DrawingCanvas.ActualHeight - 80) / tapeWidth);
        _zoom = 1;
        Render();
    }

    private static void GeometryChanged(DependencyObject source, DependencyPropertyChangedEventArgs args) => ((TapeReelPreview)source).Fit();
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => ZoomOut();
    private void Fit_Click(object sender, RoutedEventArgs e) => Fit();
    private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => Fit();

    private void Render()
    {
        DrawingCanvas.Children.Clear();
        var geometry = Geometry;
        if (!HasDrawingGeometry(geometry) || DrawingCanvas.ActualWidth <= 0 || DrawingCanvas.ActualHeight <= 0) return;

        var p1 = geometry!.PocketPitch;
        var tapeWidth = geometry.CarrierTapeWidth;
        var pocketLength = geometry.PocketLength;
        var pocketWidth = geometry.PocketWidth;
        var holeDiameter = geometry.SprocketHoleDiameter;
        var scale = _fitScale * _zoom;
        var originX = 25d;
        var originY = (DrawingCanvas.ActualHeight - tapeWidth * scale) / 2;
        var visibleTapeWidth = Math.Max(1, (DrawingCanvas.ActualWidth - 50) / scale);
        var package = Package ?? Component?.Package;
        var referenceC0402 = package?.PackageName.Equals("C0402", StringComparison.OrdinalIgnoreCase) == true;
        var isTwoTerminalChip = package is not null && (package.LeadCount == 2 || package.PadCount == 2);
        var documentedCardPackage = Vega.StencilUI.Controls.SodPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.MelfPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.SotPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.Sot23MultiLeadPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.SotMicroPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.Sot523PackageDrawing.Supports(package?.PackageName) || Vega.StencilUI.Controls.Sot563PackageDrawing.Supports(package?.PackageName) || Sot723PackageDrawing.Supports(package?.PackageName) || SotPowerPackageDrawing.Supports(package?.PackageName) || DpakPackageDrawing.Supports(package?.PackageName) || SoicPackageDrawing.Supports(package?.PackageName) || Soic16PackageDrawing.Supports(package?.PackageName) || Qfp32PackageDrawing.Supports(package?.PackageName) || Qfn32PackageDrawing.Supports(package?.PackageName);
        if (referenceC0402 || isTwoTerminalChip || documentedCardPackage)
        {
            RenderCardTape(geometry, package);
            return;
        }
        var layout = TapeReelGeometryLayout.Create(geometry, visibleTapeWidth, 4);

        if (referenceC0402) AddReferenceTape(originX, originY, Math.Max(10, DrawingCanvas.ActualWidth - 50), tapeWidth * scale);
        else AddRectangle(originX, originY, Math.Max(10, DrawingCanvas.ActualWidth - 50), tapeWidth * scale,
            new SolidColorBrush(Color.FromRgb(225, 232, 238)), Brushes.DimGray);

        var sprocketHoleCenterY = originY + tapeWidth * scale * .18;
        var tapeBottomY = originY + tapeWidth * scale;
        var referenceHoleXs = new[] { visibleTapeWidth * .08, visibleTapeWidth * .34, visibleTapeWidth * .60, visibleTapeWidth * .86 };
        foreach (var holePosition in (referenceC0402 ? referenceHoleXs : layout.HoleXs))
        {
            var holeX = originX + holePosition * scale;
            AddEllipse(holeX - holeDiameter * scale / 2, sprocketHoleCenterY - holeDiameter * scale / 2,
                holeDiameter * scale, holeDiameter * scale, Brushes.White, Brushes.DimGray);
        }

        var pocketCenterY = (sprocketHoleCenterY + tapeBottomY) / 2;
        var referencePocketXs = new[] { visibleTapeWidth * .22, visibleTapeWidth * .50, visibleTapeWidth * .78 };
        foreach (var pocketPosition in (referenceC0402 ? referencePocketXs : layout.PocketXs))
        {
            var pocketX = originX + pocketPosition * scale;
            var pocketY = pocketCenterY - pocketWidth * scale / 2;
            AddRectangle(pocketX - pocketLength * scale / 2, pocketY, pocketLength * scale, pocketWidth * scale,
                referenceC0402 ? new SolidColorBrush(Color.FromRgb(38, 39, 44)) : new SolidColorBrush(Color.FromRgb(94, 131, 159)),
                referenceC0402 ? new SolidColorBrush(Color.FromRgb(180, 184, 190)) : Brushes.Navy);

            // All two-terminal chip packages are placed vertically across the feed direction.
            // This is a card presentation rule; physical pocket dimensions remain source data.
            var rotation = package is not null && (package.LeadCount == 2 || package.PadCount == 2)
                ? 90d
                : geometry.PickupRotation;
            AddPackageComponent(package, pocketX, pocketCenterY, pocketLength * scale, pocketWidth * scale, rotation);
        }

        if ((referenceC0402 ? layout.PocketXs.Take(3).Count() : layout.PocketXs.Count) > 1)
        {
            var firstPocketX = originX + (referenceC0402 ? referencePocketXs[0] : layout.PocketXs[0]) * scale;
            var secondPocketX = originX + (referenceC0402 ? referencePocketXs[1] : layout.PocketXs[1]) * scale;
            AddDimension(firstPocketX, secondPocketX, originY + tapeWidth * scale + 22, referenceC0402 ? "2" : $"P1 {p1:0.###} мм");
        }

        AddVerticalDimension(originX + DrawingCanvas.ActualWidth - 36, originY, originY + tapeWidth * scale, referenceC0402 ? "8" : $"W {tapeWidth:0.###} мм");
        if (referenceC0402) AddText("Направление подачи →", originX + DrawingCanvas.ActualWidth - 260, originY + tapeWidth * scale - 34, ComponentCardPalette.Dimension, 18);
    }
    private void RenderCardTape(ComponentTapeReelGeometry geometry, PackageDefinition? package)
    {
        // Construct every physical length in the SAME mm coordinate system.
        // The final Viewbox scales the whole drawing uniformly, including dimensions.
        const double pixelsPerMm = 60;
        const double tapeX = 125;
        const double tapeY = 130;
        var pitchPixels = geometry.PocketPitch * pixelsPerMm;
        var holePitchPixels = geometry.SprocketHolePitch * pixelsPerMm;
        // Large DPAK pockets need balanced carrier material on both sides.
        // Keep the package orientation and pitch unchanged; only lengthen the
        // displayed fragment so neither outer pocket touches a cut edge.
        var firstHoleX = tapeX + (DpakPackageDrawing.Supports(package?.PackageName) ? 295 : Qfp32PackageDrawing.Supports(package?.PackageName) ? 235 : Qfn32PackageDrawing.Supports(package?.PackageName) ? 160 : 125);
        // The first pocket is always centred in the first P0 interval, never
        // directly below a sprocket hole.  P1 controls the following pockets;
        // it does not change the datum between the two initial holes.
        var firstPocketX = firstHoleX + holePitchPixels / 2;
        var holeDiameter = geometry.SprocketHoleDiameter * pixelsPerMm;
        var holeRadius = holeDiameter / 2;
        var pocketWidth = geometry.PocketWidth * pixelsPerMm;
        var pocketHeight = geometry.PocketLength * pixelsPerMm;
        var isSoic8 = SoicPackageDrawing.Supports(package?.PackageName);
        var isSoic16 = Soic16PackageDrawing.Supports(package?.PackageName);
        var isSoic = isSoic8 || isSoic16;
        var isQfp = Qfp32PackageDrawing.Supports(package?.PackageName);
        var isQfn = Qfn32PackageDrawing.Supports(package?.PackageName);
        var isLeadIc = isSoic || isQfp || isQfn;
        var documentedPackage = Vega.StencilUI.Controls.SodPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.MelfPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.SotPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.Sot23MultiLeadPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.SotMicroPackageDrawing.Supports(package?.PackageName) ||
            Vega.StencilUI.Controls.Sot523PackageDrawing.Supports(package?.PackageName) || Vega.StencilUI.Controls.Sot563PackageDrawing.Supports(package?.PackageName) || Sot723PackageDrawing.Supports(package?.PackageName) || SotPowerPackageDrawing.Supports(package?.PackageName) || DpakPackageDrawing.Supports(package?.PackageName) || isLeadIc;
        // Use a fixed four-hole-interval physical fragment. It contains five
        // pockets at P=4, nine at P=2 and three at P=8, with no false pitch
        // and without shrinking the annotations on wide-package tapes.
        var physicalSpan = isLeadIc ? 2 * geometry.PocketPitch : documentedPackage ? Math.Max(4 * geometry.SprocketHolePitch, 2 * geometry.PocketPitch) : 3 * geometry.SprocketHolePitch;
        var holeCount = documentedPackage ? (int)Math.Round(physicalSpan / geometry.SprocketHolePitch) + 1 : 4;
        var pocketCount = isLeadIc ? 3 : documentedPackage ? (int)Math.Round(physicalSpan / geometry.PocketPitch) + 1 : 5;

        // Keep a constant right margin while allowing the fifth pocket to be
        // fully visible for packages where P1 equals P0.
        var minimumTapeWidth = physicalSpan * pixelsPerMm + 230;
        var lastPocketRight = firstPocketX + (pocketCount - 1) * pitchPixels + pocketWidth / 2;
        if(DpakPackageDrawing.Supports(package?.PackageName))
            holeCount=(int)Math.Ceiling((lastPocketRight-firstHoleX)/holePitchPixels)+1;
        if(isLeadIc)
        {
            // Continue P0 perforation to the right cut.  Three wide P1 pockets
            // determine the fragment length; the holes then fill that length.
            var requiredRight=lastPocketRight+40;
            holeCount=(int)Math.Ceiling((requiredRight-firstHoleX-holeRadius)/holePitchPixels)+1;
        }
        var lastHoleRight = firstHoleX + (holeCount - 1) * holePitchPixels + holeRadius;
        var tapeWidth = isLeadIc
            ? Math.Max(minimumTapeWidth,lastHoleRight+40-tapeX)
            : Math.Max(minimumTapeWidth, Math.Max(lastHoleRight+40, lastPocketRight+105) - tapeX);
        var tapeHeight = geometry.CarrierTapeWidth * pixelsPerMm;
        var tapeBottom = tapeY + tapeHeight;
        var holeY = tapeY + 86;

        // Keep only complete pockets inside the illustrated tape fragment.  Wide
        // SO pockets can cross the curved left cut even though their centres are
        // on the documented P1 grid; in that case omit the incomplete pocket
        // instead of clipping it or changing the physical pitch.
        // The wave enters the carrier by 30 px at its deepest point.  A pocket
        // whose edge is at that tangent remains fully inside the tape; using
        // 40 px here unnecessarily removed the first documented TSSOP pocket.
        var cutEdgeClearance = isLeadIc ? 30d : 40d;
        var visiblePocketCenters = Enumerable.Range(0, pocketCount)
            .Select(i => firstPocketX + i * pitchPixels)
            .Where(centerX => centerX - pocketWidth / 2 >= tapeX + cutEdgeClearance &&
                              centerX + pocketWidth / 2 <= tapeX + tapeWidth - cutEdgeClearance)
            .ToArray();

        if (ShowTitle) AddText($"{package?.PackageName ?? "КОМПОНЕНТ"}  |  ЛЕНТА И КАРМАНЫ", 28, 25, ComponentCardPalette.Title, 18);
        AddReferenceTape(tapeX, tapeY, tapeWidth, tapeHeight);

        for (var i = 0; i < holeCount; i++)
        {
            var x = firstHoleX + i * holePitchPixels;
            AddEllipse(x - holeRadius, holeY - holeRadius, holeDiameter, holeDiameter, Brushes.White, Brushes.DimGray);
        }

        // The pocket centre is the middle of the space between the lower edge
        // of the sprocket hole and the lower tape edge.
        var pocketCenterY = (holeY + holeRadius + tapeBottom) / 2;
        foreach (var centerX in visiblePocketCenters)
        {
            var x = centerX - pocketWidth / 2;
            var y = pocketCenterY - pocketHeight / 2;
            AddRectangle(x, y, pocketWidth, pocketHeight, ComponentCardPalette.Pocket, ComponentCardPalette.Border);
            AddChipPackage(package, centerX, pocketCenterY, pocketWidth, pocketHeight);
        }

        var dimensionY = tapeBottom + 190;
        var culture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        var annotationSize=DpakPackageDrawing.Supports(package?.PackageName)?84d:isSoic?72d:isQfp?60d:48d;
        if (visiblePocketCenters.Length > 1)
            AddReferenceHorizontalDimension(visiblePocketCenters[0], visiblePocketCenters[1], pocketCenterY + pocketHeight / 2, dimensionY, geometry.PocketPitch.ToString("0.###", culture),annotationSize);
        AddReferenceVerticalDimension(tapeX + tapeWidth + (DpakPackageDrawing.Supports(package?.PackageName)?190:73), tapeY, tapeBottom, geometry.CarrierTapeWidth.ToString("0.###", culture),annotationSize);
        AddText("Направление подачи  →", tapeX + tapeWidth - (DpakPackageDrawing.Supports(package?.PackageName)?900:isSoic?760:isQfp?650:465), dimensionY+(annotationSize>48?18:0), ComponentCardPalette.Dimension, annotationSize);

        var contentWidth=tapeX+tapeWidth+275;var contentHeight=tapeBottom+330;
        var drawing = new Canvas { Width = contentWidth+360, Height = contentHeight+100,
            Background = ComponentCardPalette.Panel };
        var centeredContent=new Canvas{Width=contentWidth,Height=contentHeight,Background=Brushes.Transparent};
        Canvas.SetLeft(centeredContent,180);Canvas.SetTop(centeredContent,35);
        var elements = DrawingCanvas.Children.Cast<UIElement>().ToArray();
        DrawingCanvas.Children.Clear();
        foreach (var element in elements) centeredContent.Children.Add(element);
        drawing.Children.Add(centeredContent);
        DrawingCanvas.Children.Add(new Viewbox { Width = DrawingCanvas.ActualWidth,
            Height = DrawingCanvas.ActualHeight, Stretch = Stretch.Uniform, Child = drawing });
    }

    private static bool IsSodPackage(PackageDefinition? package) => package is not null &&
        (string.Equals(package.PackageFamily, "SOD", StringComparison.OrdinalIgnoreCase) ||
         package.PackageName.StartsWith("SOD", StringComparison.OrdinalIgnoreCase));

    private void AddChipPackage(PackageDefinition? package, double centerX, double centerY, double pocketWidth, double pocketHeight)
    {
        if(Vega.StencilUI.Controls.SodPackageDrawing.Supports(package?.PackageName))
        {
            var top=new Vega.CAD.StepProjectionGeometry().Project(Vega.StencilUI.Controls.SodPackageDrawing.BuildDocumentedMesh(package!.PackageName),Vega.CAD.StepProjectionKind.Top,60);
            Vega.StencilUI.Controls.SodPackageDrawing.Paint(DrawingCanvas,top,p=>new Point(centerX-(p.Y-top.Height/2),centerY+p.X-top.Width/2));
            return;
        }
        if(Vega.StencilUI.Controls.MelfPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.MelfPackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60,true);
            return;
        }
        if(Vega.StencilUI.Controls.SotPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.SotPackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60,false);
            return;
        }
        if(Vega.StencilUI.Controls.Sot23MultiLeadPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.Sot23MultiLeadPackageDrawing.PaintTop(DrawingCanvas,package!.PackageName,centerX,centerY,60);
            return;
        }
        if(Vega.StencilUI.Controls.SotMicroPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.SotMicroPackageDrawing.PaintTop(DrawingCanvas,package!.PackageName,centerX,centerY,60,true);
            return;
        }
        if(Vega.StencilUI.Controls.Sot523PackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.Sot523PackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60);
            return;
        }
        if(Vega.StencilUI.Controls.Sot563PackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.Sot563PackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60);
            return;
        }
        if(Vega.StencilUI.Controls.Sot723PackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.Sot723PackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60);
            return;
        }
        if(Vega.StencilUI.Controls.SotPowerPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.SotPowerPackageDrawing.PaintTop(DrawingCanvas,package!.PackageName,centerX,centerY,60,true);
            return;
        }
        if(Vega.StencilUI.Controls.DpakPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.DpakPackageDrawing.PaintTop(DrawingCanvas,package!.PackageName,centerX,centerY,60);
            return;
        }
        if(Vega.StencilUI.Controls.SoicPackageDrawing.Supports(package?.PackageName))
        {
            Vega.StencilUI.Controls.SoicPackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60);
            return;
        }
        if(Vega.StencilUI.Controls.Soic16PackageDrawing.Supports(package?.PackageName))
        {
            // TI DCT is packed across the feed direction: the 4.25 mm lead span
            // follows B0=4.40 mm and the 3.00 mm body length follows A0=3.45 mm.
            // With sprocket holes above the pockets and feed to the right, TI's
            // documented Q3 is the lower-left pocket quadrant. The -90 degree
            // turn places the package marker there.
            if(package!.PackageName.Equals("SSOP08P065W43",StringComparison.OrdinalIgnoreCase))
            {
                var rotatedLayer=new Canvas{Width=1,Height=1,IsHitTestVisible=false,ClipToBounds=false,
                    RenderTransform=new RotateTransform(-90,centerX,centerY)};
                DrawingCanvas.Children.Add(rotatedLayer);
                Vega.StencilUI.Controls.Soic16PackageDrawing.PaintTop(rotatedLayer,centerX,centerY,60,false,package.PackageName);
            }
            else
                Vega.StencilUI.Controls.Soic16PackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60,false,package.PackageName);
            return;
        }
        if(Vega.StencilUI.Controls.Qfp32PackageDrawing.Supports(package?.PackageName))
        {
            // ST TN1206 specifies Pin 1 in the upper-right corner with
            // sprocket holes above and feed to the right (Q2).  The native
            // top view already has that orientation, so it must not rotate.
            Vega.StencilUI.Controls.Qfp32PackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60,false);
            return;
        }
        if(Vega.StencilUI.Controls.Qfn32PackageDrawing.Supports(package?.PackageName))
        {
            // DS14186 fixes Pin 1 on the sprocket-hole side.  Keep the native
            // top view unrotated so its marker stays on that side of the tape.
            Vega.StencilUI.Controls.Qfn32PackageDrawing.PaintTop(DrawingCanvas,centerX,centerY,60,false);
            return;
        }
        var length = package?.BodyLength > 0 ? package.BodyLength : package?.Length ?? 0;
        var width = package?.BodyWidth > 0 ? package.BodyWidth : package?.Width ?? 0;
        var bodyWidth = length > 0 && width > 0
            ? Math.Min(pocketWidth * .86, width * 60)
            : pocketWidth * .5 / .65;
        var bodyHeight = length > 0 && width > 0
            ? Math.Min(pocketHeight * .86, length * 60)
            : pocketHeight * 1.0 / 1.15;
        var x = centerX - bodyWidth / 2;
        var y = centerY - bodyHeight / 2;

        // SOD packages use the same neutral appearance as the STEP model:
        // a light grey moulded body, metallic end contacts and a distinct
        // cathode bar.  Do not use the brown CHIP silhouette for diodes.
        if (string.Equals(package?.PackageFamily, "SOD", StringComparison.OrdinalIgnoreCase) ||
            (package?.PackageName?.StartsWith("SOD", StringComparison.OrdinalIgnoreCase) == true))
        {
            var contactHeight = Math.Max(8, bodyHeight * .16);
            var bodyY = y + contactHeight * .55;
            var bodyH = bodyHeight - contactHeight * 1.10;
            var modelBodyWidth = bodyWidth * .76;
            var modelBodyX = centerX - modelBodyWidth / 2;
            var chamfer = Math.Min(modelBodyWidth * .12, bodyH * .08);
            AddRoundedRectangle(modelBodyX, bodyY, modelBodyWidth, bodyH,
                new SolidColorBrush(Color.FromRgb(0x56, 0x65, 0x72)),
                new SolidColorBrush(Color.FromRgb(30, 31, 35)), chamfer);
            var contact = new SolidColorBrush(Color.FromRgb(215, 214, 202));
            var modelContactWidth = bodyWidth * .44;
            var contactX = centerX - modelContactWidth / 2;
            AddRectangle(contactX, y, modelContactWidth, contactHeight * .82, contact, new SolidColorBrush(Color.FromRgb(90, 91, 88)));
            AddRectangle(contactX, y + bodyHeight - contactHeight * .82, modelContactWidth, contactHeight * .82, contact, new SolidColorBrush(Color.FromRgb(90, 91, 88)));
            var cathodeBandHeight = Math.Max(5, bodyH * .105);
            AddRectangle(modelBodyX, bodyY + bodyH * .18, modelBodyWidth, cathodeBandHeight,
                new SolidColorBrush(Color.FromRgb(236, 233, 216)), Brushes.Transparent);
            return;
        }

        AddRectangle(x, y, bodyWidth, bodyHeight, new SolidColorBrush(Color.FromRgb(32, 17, 8)), Brushes.Transparent);
        var endHeight = bodyHeight * .2;
        AddRectangle(x, y, bodyWidth, endHeight, new SolidColorBrush(Color.FromRgb(170, 169, 152)), Brushes.Transparent);
        AddRectangle(x, y + bodyHeight - endHeight, bodyWidth, endHeight, new SolidColorBrush(Color.FromRgb(170, 169, 152)), Brushes.Transparent);
        if (string.Equals(package?.ComponentType, "Diode", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(package?.PackageFamily, "SOD", StringComparison.OrdinalIgnoreCase))
        {
            // SOD/SMD diodes are polarized: the contrasting transverse band is the cathode mark.
            // Keep the cathode mark separate from the two terminal areas so it
            // remains visible at the card's scale and cannot be mistaken for a lead.
            var cathodeBandHeight = Math.Max(5, bodyHeight * .105);
            AddRectangle(x, y + endHeight + bodyHeight * .09, bodyWidth, cathodeBandHeight,
                new SolidColorBrush(Color.FromRgb(235, 232, 216)), Brushes.Transparent);
        }
    }

    private void AddReferenceHorizontalDimension(double from, double to, double sourceY, double dimensionY, string label,double fontSize=48)
    {
        AddLine(from, sourceY, from, dimensionY + 8, ComponentCardPalette.Dimension, 1.2);
        AddLine(to, sourceY, to, dimensionY + 8, ComponentCardPalette.Dimension, 1.2);
        AddLine(from, dimensionY, to, dimensionY, ComponentCardPalette.Dimension, 1.2);
        AddTriangleArrow(from, dimensionY, true, false);
        AddTriangleArrow(to, dimensionY, false, false);
        AddCenteredText(label, (from + to) / 2, dimensionY - fontSize*(fontSize>48?1.05:1.25)-16, ComponentCardPalette.Dimension, fontSize);
    }

    private void AddReferenceVerticalDimension(double x, double from, double to, string label,double fontSize=48)
    {
        AddLine(x, from, x, to, ComponentCardPalette.Dimension, 1.2);
        AddLine(x - 33, from, x + 8, from, ComponentCardPalette.Dimension, 1.2);
        AddLine(x - 33, to, x + 8, to, ComponentCardPalette.Dimension, 1.2);
        AddTriangleArrow(x, from, true, true);
        AddTriangleArrow(x, to, false, true);
        var text = new TextBlock { Text = label, Foreground = ComponentCardPalette.Dimension, FontSize = fontSize, FontFamily = new FontFamily("Arial"), LayoutTransform = new RotateTransform(-90) };
        text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(text, x - fontSize*1.5);
        Canvas.SetTop(text, (from + to) / 2 - text.DesiredSize.Height / 2);
        DrawingCanvas.Children.Add(text);
    }

    private void AddTriangleArrow(double x, double y, bool inward, bool vertical)
    {
        var arrow = new Polygon { Fill = ComponentCardPalette.Dimension };
        arrow.Points = vertical
            ? (inward ? new PointCollection { new(x, y), new(x - 3, y + 9), new(x + 3, y + 9) } : new PointCollection { new(x, y), new(x - 3, y - 9), new(x + 3, y - 9) })
            : (inward ? new PointCollection { new(x, y), new(x + 9, y - 3), new(x + 9, y + 3) } : new PointCollection { new(x, y), new(x - 9, y - 3), new(x - 9, y + 3) });
        DrawingCanvas.Children.Add(arrow);
    }

    private void AddCenteredText(string text, double x, double y, Brush brush, double fontSize)
    {
        var block = new TextBlock { Text = text, Foreground = brush, FontSize = fontSize, FontFamily = new FontFamily("Arial") };
        block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(block, x - block.DesiredSize.Width / 2);
        Canvas.SetTop(block, y);
        DrawingCanvas.Children.Add(block);
    }

    private void AddReferenceTape(double x, double y, double width, double height)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
        context.BeginFigure(new Point(x, y), true, true);
        context.LineTo(new Point(x + width, y), true, false);
        context.BezierTo(new Point(x + width - 20, y + height * .15), new Point(x + width - 30, y + height * .34), new Point(x + width - 30, y + height * .46), true, false);
        context.BezierTo(new Point(x + width - 30, y + height * .60), new Point(x + width, y + height * .83), new Point(x + width, y + height), true, false);
        context.LineTo(new Point(x, y + height), true, false);
        context.BezierTo(new Point(x, y + height * .83), new Point(x + 30, y + height * .60), new Point(x + 30, y + height * .46), true, false);
        context.BezierTo(new Point(x + 30, y + height * .34), new Point(x + 20, y + height * .15), new Point(x, y), true, false);
        }
        geometry.Freeze();
        DrawingCanvas.Children.Add(new Path { Data = geometry, Fill = ComponentCardPalette.Tape, Stroke = ComponentCardPalette.Border, StrokeThickness = 1.2 });
    }

    private void AddPackageComponent(PackageDefinition? package, double centerX, double centerY, double pocketLength, double pocketWidth, double rotation)
    {
        if (package is null) return;

        if (string.Equals(package.PackageName, "C0402", StringComparison.OrdinalIgnoreCase))
        {
            var bodyWidth = Math.Max(8, pocketWidth * .52);
            var bodyHeight = Math.Max(22, pocketLength * .80);
            var x = centerX - bodyWidth / 2;
            var y = centerY - bodyHeight / 2;
            AddRectangle(x, y, bodyWidth, bodyHeight, new SolidColorBrush(Color.FromRgb(56, 38, 31)), Brushes.DimGray);
            var end = Math.Max(4, bodyHeight * .18);
            AddRectangle(x, y, bodyWidth, end, new SolidColorBrush(Color.FromRgb(205, 202, 190)), Brushes.DimGray);
            AddRectangle(x, y + bodyHeight - end, bodyWidth, end, new SolidColorBrush(Color.FromRgb(205, 202, 190)), Brushes.DimGray);
            return;
        }

        var size = Math.Max(20, Math.Min(pocketLength, pocketWidth) * .72);
        var drawing = new PackageDrawingPreview
        {
            Package = package,
            ShowDimensions = false,
            Width = 160,
            Height = 160,
            IsHitTestVisible = false
        };
        var component = new Viewbox
        {
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            Child = drawing,
            IsHitTestVisible = false,
            LayoutTransform = new RotateTransform(NormalizeRotation(rotation))
        };
        Canvas.SetLeft(component, centerX - size / 2);
        Canvas.SetTop(component, centerY - size / 2);
        DrawingCanvas.Children.Add(component);
    }

    private static double NormalizeRotation(double rotation)
    {
        var normalized = rotation % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }
    private static bool HasDrawingGeometry(ComponentTapeReelGeometry? geometry) =>
        geometry is not null &&
        geometry.CarrierTapeWidth > 0 &&
        geometry.PocketPitch > 0 &&
        geometry.PocketLength > 0 &&
        geometry.PocketWidth > 0 &&
        geometry.SprocketHolePitch > 0 &&
        geometry.SprocketHoleDiameter > 0;
    private void AddRectangle(double x, double y, double width, double height, Brush fill, Brush stroke) { var shape = new Rectangle { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = 1 }; Canvas.SetLeft(shape, x); Canvas.SetTop(shape, y); DrawingCanvas.Children.Add(shape); }
    private void AddRoundedRectangle(double x, double y, double width, double height, Brush fill, Brush stroke, double radius)
    {
        var shape = new Rectangle { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = 1, RadiusX = radius, RadiusY = radius };
        Canvas.SetLeft(shape, x); Canvas.SetTop(shape, y); DrawingCanvas.Children.Add(shape);
    }
    private void AddPolygon(IReadOnlyList<Point> points, Brush fill, Brush stroke)
    {
        var shape = new Polygon { Points = new PointCollection(points), Fill = fill, Stroke = stroke, StrokeThickness = 1 };
        DrawingCanvas.Children.Add(shape);
    }
    private void AddEllipse(double x, double y, double width, double height, Brush fill, Brush? stroke) { var shape = new Ellipse { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = stroke is null ? 0 : 1 }; Canvas.SetLeft(shape, x); Canvas.SetTop(shape, y); DrawingCanvas.Children.Add(shape); }
    private void AddLine(double x1, double y1, double x2, double y2, Brush brush, double thickness) { DrawingCanvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness }); }
    private void AddText(string text, double x, double y, Brush brush) => AddText(text, x, y, brush, 11);
    private void AddText(string text, double x, double y, Brush brush, double fontSize) { var block = new TextBlock { Text = text, Foreground = brush, FontSize = fontSize, FontFamily = new FontFamily("Arial") }; Canvas.SetLeft(block, x); Canvas.SetTop(block, y); DrawingCanvas.Children.Add(block); }
    private void AddDimension(double from, double to, double y, string label)
    {
        AddLine(from, y, to, y, ComponentCardPalette.Dimension, 1);
        AddLine(from, y - 4, from, y + 4, ComponentCardPalette.Dimension, 1);
        AddLine(to, y - 4, to, y + 4, ComponentCardPalette.Dimension, 1);
        AddText(label, (from + to) / 2 - 26, y - 17, ComponentCardPalette.Dimension);
    }
    private void AddVerticalDimension(double x, double from, double to, string label)
    {
        AddLine(x, from, x, to, ComponentCardPalette.Dimension, 1);
        AddLine(x - 4, from, x + 4, from, ComponentCardPalette.Dimension, 1);
        AddLine(x - 4, to, x + 4, to, ComponentCardPalette.Dimension, 1);
        AddText(label, x - 62, (from + to) / 2 - 6, ComponentCardPalette.Dimension);
    }
    private void AddOrientationArrow(double x, double y, TapePocketOrientation orientation)
    {
        var length = 14d;
        var direction = orientation switch { TapePocketOrientation.Deg90 => new Vector(0, -1), TapePocketOrientation.Deg180 => new Vector(-1, 0), TapePocketOrientation.Deg270 => new Vector(0, 1), _ => new Vector(1, 0) };
        var end = new Point(x + direction.X * length, y + direction.Y * length);
        AddLine(x, y, end.X, end.Y, ComponentCardPalette.Dimension, 2);
        AddLine(end.X, end.Y, end.X - direction.X * 5 - direction.Y * 3, end.Y - direction.Y * 5 + direction.X * 3, ComponentCardPalette.Dimension, 2);
        AddLine(end.X, end.Y, end.X - direction.X * 5 + direction.Y * 3, end.Y - direction.Y * 5 - direction.X * 3, ComponentCardPalette.Dimension, 2);
    }
}
