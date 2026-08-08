using Vega.Gerber.Models;
using Vega.StencilCAM.Models;
using Vega.StencilViewer.Models;

namespace Vega.StencilViewer;

public class StencilOverlayService
{
    private readonly List<StencilOverlayLayer> _layers = [];
    private StencilViewDocument? _document;

    public IReadOnlyList<StencilOverlayLayer> LoadProject(StencilViewDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _document = document;
        _layers.Clear();
        if (document.OriginalPasteLayer is not null) AddLayer(CreatePasteLayer("Original Paste", StencilOverlayLayerType.OriginalPaste, document.OriginalPasteLayer.Primitives));
        if (document.CorrectedPasteLayer is not null) AddLayer(CreatePasteLayer("Corrected Paste", StencilOverlayLayerType.CorrectedPaste, document.CorrectedPasteLayer.CorrectedPrimitives));
        if (document.Frame is not null) AddLayer(CreateFrameLayer(document.Frame));
        if (document.Fiducials.Count > 0) AddLayer(CreateFiducialLayer(document.Fiducials));
        if (document.MarkingLayer.Count > 0) AddLayer(CreateMarkingLayer(document.MarkingLayer));
        return _layers;
    }

    public void AddLayer(StencilOverlayLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        _layers.RemoveAll(current => current.Name.Equals(layer.Name, StringComparison.OrdinalIgnoreCase));
        _layers.Add(layer);
    }

    public bool RemoveLayer(string name) => _layers.RemoveAll(layer => layer.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0;

    public void SetVisibility(string name, bool visible)
    {
        var layer = _layers.SingleOrDefault(layer => layer.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (layer is null) throw new ArgumentException("Overlay layer was not found.", nameof(name));
        layer.Visible = visible;
    }

    public IReadOnlyList<StencilOverlayLayer> CreateOverlay(StencilViewMode mode) => mode switch
    {
        StencilViewMode.Original => Visible(StencilOverlayLayerType.OriginalPaste),
        StencilViewMode.Corrected => Visible(StencilOverlayLayerType.CorrectedPaste),
        StencilViewMode.Overlay => Visible(StencilOverlayLayerType.OriginalPaste, StencilOverlayLayerType.CorrectedPaste),
        StencilViewMode.Production => Visible(StencilOverlayLayerType.Frame, StencilOverlayLayerType.CorrectedPaste, StencilOverlayLayerType.OriginalPaste, StencilOverlayLayerType.Fiducial, StencilOverlayLayerType.Marking),
        _ => []
    };

    public StencilValidationOverlay Validate()
    {
        if (_document?.Frame is null) return new StencilValidationOverlay { Errors = ["Stencil frame is required."] };
        var frame = _document.Frame;
        var frameBounds = new Bounds(frame.OriginX, frame.OriginY, frame.OriginX + ActiveWidth(frame), frame.OriginY + ActiveHeight(frame));
        IEnumerable<PastePrimitive> paste = (IEnumerable<PastePrimitive>?)_document.CorrectedPasteLayer?.CorrectedPrimitives ?? (IEnumerable<PastePrimitive>?)_document.OriginalPasteLayer?.Primitives ?? Array.Empty<PastePrimitive>();
        var pasteInside = paste.All(primitive => Contains(frameBounds, primitive.X, primitive.Y, primitive.Width, primitive.Height));
        var fiducialsInside = _document.Fiducials.All(fiducial => Contains(frameBounds, fiducial.X, fiducial.Y, fiducial.Diameter, fiducial.Diameter));
        var markingOutside = _document.MarkingLayer.All(marking => !Contains(frameBounds, marking.PositionX, marking.PositionY, 0, 0));
        var side = _document.OriginalPasteLayer?.Side ?? _document.CorrectedPasteLayer?.Side ?? string.Empty;
        var isBottom = side.Equals("Bottom", StringComparison.OrdinalIgnoreCase);
        var bottomTransformed = !isBottom || _document.Transformations is { MirrorX: true } or { MirrorY: true };
        var errors = new List<string>();
        if (!pasteInside) errors.Add("Paste geometry is outside the frame working area.");
        if (!fiducialsInside) errors.Add("Fiducials are outside the frame working area.");
        if (!markingOutside) errors.Add("Marking intersects the frame working area.");
        if (!bottomTransformed) errors.Add("Bottom paste transformation is not applied.");
        return new StencilValidationOverlay
        {
            PasteInsideFrame = pasteInside, FiducialsInsideFrame = fiducialsInside, MarkingOutsideWorkingArea = markingOutside,
            BottomTransformationApplied = bottomTransformed, IsValid = errors.Count == 0, Errors = errors
        };
    }

    public GerberCompareViewModel CreateCompareView()
    {
        var corrected = _document?.CorrectedPasteLayer;
        if (corrected is null) return new GerberCompareViewModel();
        var changes = corrected.Changes.Select(change => $"{change.RefDes}: {change.OriginalGeometry} -> {change.NewGeometry}").ToList();
        return new GerberCompareViewModel
        {
            AddedApertures = Math.Max(0, corrected.CorrectedPrimitiveCount - corrected.OriginalPrimitiveCount),
            RemovedApertures = Math.Max(0, corrected.OriginalPrimitiveCount - corrected.CorrectedPrimitiveCount),
            ModifiedApertures = corrected.Changes.Count,
            ChangedShape = corrected.Changes.Any(change => change.ChangeType.Contains("Segmentation", StringComparison.OrdinalIgnoreCase) || !change.OriginalGeometry.Equals(change.NewGeometry, StringComparison.OrdinalIgnoreCase)),
            Changes = changes
        };
    }

    private IReadOnlyList<StencilOverlayLayer> Visible(params StencilOverlayLayerType[] types) => _layers.Where(layer => layer.Visible && types.Contains(layer.LayerType)).ToList();
    private static StencilOverlayLayer CreatePasteLayer(string name, StencilOverlayLayerType type, IEnumerable<PastePrimitive> primitives) => new()
    {
        Name = name, LayerType = type, Geometry = primitives.Select(primitive => new OverlayGeometry { X = primitive.X, Y = primitive.Y, Width = primitive.Width, Height = primitive.Height, Rotation = primitive.Rotation, Shape = primitive.ShapeType?.ToString() ?? "Rectangle" }).ToList()
    };
    private static StencilOverlayLayer CreateFrameLayer(StencilFrame frame) => new()
    {
        Name = "Frame", LayerType = StencilOverlayLayerType.Frame, Geometry = [new OverlayGeometry { X = frame.OriginX, Y = frame.OriginY, Width = ActiveWidth(frame), Height = ActiveHeight(frame), Shape = "Frame" }]
    };
    private static StencilOverlayLayer CreateFiducialLayer(IEnumerable<StencilFiducial> fiducials) => new()
    {
        Name = "Fiducials", LayerType = StencilOverlayLayerType.Fiducial, Geometry = fiducials.Select(fiducial => new OverlayGeometry { X = fiducial.X, Y = fiducial.Y, Width = fiducial.Diameter, Height = fiducial.Diameter, Rotation = fiducial.Rotation, Shape = fiducial.Shape }).ToList()
    };
    private static StencilOverlayLayer CreateMarkingLayer(IEnumerable<StencilMarking> markings) => new()
    {
        Name = "Marking", LayerType = StencilOverlayLayerType.Marking, Geometry = markings.Select(marking => new OverlayGeometry { X = marking.PositionX, Y = marking.PositionY, Rotation = marking.Rotation, Shape = marking.Mirror ? "MirroredText" : "Text", Text = marking.Text }).ToList()
    };
    private static bool Contains(Bounds frame, double x, double y, double width, double height) => x - width / 2 >= frame.MinX && y - height / 2 >= frame.MinY && x + width / 2 <= frame.MaxX && y + height / 2 <= frame.MaxY;
    private static double ActiveWidth(StencilFrame frame) => frame.StencilWidth > 0 ? frame.StencilWidth : frame.FrameWidth;
    private static double ActiveHeight(StencilFrame frame) => frame.StencilHeight > 0 ? frame.StencilHeight : frame.FrameHeight;
    private readonly record struct Bounds(double MinX, double MinY, double MaxX, double MaxY);
}
