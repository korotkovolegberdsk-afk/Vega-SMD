using Vega.CAD.Models;

namespace Vega.StencilInput.Models;

public class StencilInputValidationResult
{
    public bool HasPasteLayer { get; init; }
    public bool HasBoardBounds { get; init; }
    public bool HasSelectedSide { get; init; }
    public bool HasFrame { get; init; }
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

public class StencilInputReport
{
    public string ProjectName { get; init; } = "";
    public string Source { get; init; } = "";
    public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
    public string BoardSize { get; init; } = "";
    public string Paste { get; init; } = "";
    public string ComponentData { get; init; } = "";
    public string Status { get; init; } = "";
    public string Summary { get; init; } = "";
}
