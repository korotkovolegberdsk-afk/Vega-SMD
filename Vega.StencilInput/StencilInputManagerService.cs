using Vega.Altium;
using Vega.CAD.Models;
using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.StencilCAM;
using Vega.StencilCAM.Models;
using Vega.StencilInput.Models;

namespace Vega.StencilInput;

public class StencilInputManagerService
{
    private readonly StencilFrameService _frameService = new();

    public StencilInputSourceType DetectInputType(IEnumerable<string> fileNames)
    {
        var files = fileNames.Where(file => !string.IsNullOrWhiteSpace(file)).ToList();
        if (files.Any(file => Path.GetExtension(file).Equals(".PcbDoc", StringComparison.OrdinalIgnoreCase)))
            return StencilInputSourceType.AltiumProject;
        if (files.Any(file => Path.GetFileName(file).Contains("panel", StringComparison.OrdinalIgnoreCase)))
            return StencilInputSourceType.PanelGerber;
        return files.Any(IsPasteFile) ? StencilInputSourceType.PasteOnly : StencilInputSourceType.Manual;
    }

    public StencilInputProject LoadPasteOnlyProject(string? topPasteFile, string? bottomPasteFile, string? assemblyDrawing = null)
    {
        var project = CreateProject(StencilInputSourceType.PasteOnly, topPasteFile ?? bottomPasteFile);
        AddPasteFile(project, topPasteFile);
        AddPasteFile(project, bottomPasteFile);
        project.AssemblyDrawing = assemblyDrawing ?? "";
        if (!string.IsNullOrWhiteSpace(assemblyDrawing)) project.SourceFiles.Add(assemblyDrawing);
        SetOutlineFromPaste(project);
        return project;
    }

    public StencilInputProject LoadAltiumProject(string pcbDocFile, IEnumerable<string>? pasteFiles = null, string? assemblyDrawing = null)
    {
        var pcbProject = new AltiumPcbImporter().Import(pcbDocFile);
        var project = CreateProject(StencilInputSourceType.AltiumProject, pcbDocFile);
        project.PcbProject = pcbProject;
        project.Components.AddRange(pcbProject.Components);
        project.BomItems.AddRange(pcbProject.BomItems);
        project.Placements.AddRange(pcbProject.Placements);
        project.Fiducials.AddRange(pcbProject.Fiducials);
        foreach (var pasteFile in pasteFiles ?? Array.Empty<string>()) AddPasteFile(project, pasteFile);
        project.AssemblyDrawing = assemblyDrawing ?? "";
        if (!string.IsNullOrWhiteSpace(assemblyDrawing)) project.SourceFiles.Add(assemblyDrawing);
        SetOutlineFromPaste(project);
        if (project.BoardOutline is null && (pcbProject.Board.Width > 0 || pcbProject.Board.Height > 0))
            project.BoardOutline = new StencilBounds(0, 0, pcbProject.Board.Width, pcbProject.Board.Height);
        return project;
    }

    public StencilInputProject LoadPanelProject(IEnumerable<string> panelGerberFiles)
    {
        var files = panelGerberFiles.ToList();
        var project = CreateProject(StencilInputSourceType.PanelGerber, files.FirstOrDefault());
        foreach (var file in files) AddPasteFile(project, file);
        SetOutlineFromPaste(project);
        return project;
    }

    public StencilInputValidationResult Validate(StencilInputProject project, BoardSide? selectedSide, StencilFrame? frame)
    {
        ArgumentNullException.ThrowIfNull(project);
        var errors = new List<string>();
        var hasPaste = project.PasteLayers.Count > 0;
        var hasBounds = project.BoardOutline is { Width: > 0, Height: > 0 };
        var hasSide = selectedSide.HasValue && project.PasteLayers.Any(layer => ToBoardSide(layer.Side) == selectedSide.Value);
        var hasFrame = frame is { IsActive: true };
        if (!hasPaste) errors.Add("Paste layer is required.");
        if (!hasBounds) errors.Add("Board bounds are required.");
        if (!hasSide) errors.Add("A paste side must be selected.");
        if (!hasFrame) errors.Add("An active stencil frame must be selected.");
        return new StencilInputValidationResult
        {
            HasPasteLayer = hasPaste, HasBoardBounds = hasBounds, HasSelectedSide = hasSide, HasFrame = hasFrame,
            IsValid = errors.Count == 0, Errors = errors
        };
    }

    public StencilInputReport CreateReport(StencilInputProject project, StencilInputValidationResult validation, BoardSide? side = null)
    {
        var size = project.BoardOutline is null ? "Not available" : $"{project.BoardOutline.Width:0.##}x{project.BoardOutline.Height:0.##} mm";
        var componentData = project.Components.Count == 0 ? "Not available" : $"{project.Components.Count} components";
        var status = validation.IsValid ? "Ready for stencil generation" : string.Join(" ", validation.Errors);
        return new StencilInputReport
        {
            ProjectName = project.ProjectName, Source = project.SourceType.ToString(), Files = project.SourceFiles,
            BoardSize = size, Paste = side?.ToString() ?? "Not selected", ComponentData = componentData,
            Status = status, Summary = $"PROJECT: {project.ProjectName}; SOURCE: {project.SourceType}; STATUS: {status}"
        };
    }

    private StencilInputProject CreateProject(StencilInputSourceType sourceType, string? sourceFile)
    {
        var project = new StencilInputProject
        {
            ProjectName = string.IsNullOrWhiteSpace(sourceFile) ? "Stencil Project" : Path.GetFileNameWithoutExtension(sourceFile),
            SourceType = sourceType
        };
        if (!string.IsNullOrWhiteSpace(sourceFile)) project.SourceFiles.Add(sourceFile);
        return project;
    }

    private void AddPasteFile(StencilInputProject project, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;
        var parser = new GerberPasteParserService();
        parser.Load(fileName);
        var layer = parser.Parse();
        project.PasteLayers.Add(layer);
        if (!project.SourceFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase)) project.SourceFiles.Add(fileName);
    }

    private void SetOutlineFromPaste(StencilInputProject project)
    {
        var bounds = project.PasteLayers.Select(_frameService.CalculateBounds).Where(bounds => bounds.Width > 0 || bounds.Height > 0).ToList();
        if (bounds.Count == 0) return;
        project.BoardOutline = new StencilBounds(bounds.Min(item => item.MinX), bounds.Min(item => item.MinY), bounds.Max(item => item.MaxX), bounds.Max(item => item.MaxY));
    }

    private static bool IsPasteFile(string fileName) => Path.GetExtension(fileName).Equals(".gtp", StringComparison.OrdinalIgnoreCase)
        || Path.GetExtension(fileName).Equals(".gbp", StringComparison.OrdinalIgnoreCase)
        || Path.GetExtension(fileName).Equals(".gbs", StringComparison.OrdinalIgnoreCase);

    private static BoardSide ToBoardSide(string side) => side.Equals("Bottom", StringComparison.OrdinalIgnoreCase) ? BoardSide.Bottom : BoardSide.Top;
}
