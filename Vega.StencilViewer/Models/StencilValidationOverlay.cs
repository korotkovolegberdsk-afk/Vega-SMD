namespace Vega.StencilViewer.Models;

public class StencilValidationOverlay
{
    public bool PasteInsideFrame { get; init; }
    public bool FiducialsInsideFrame { get; init; }
    public bool MarkingOutsideWorkingArea { get; init; }
    public bool BottomTransformationApplied { get; init; }
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
