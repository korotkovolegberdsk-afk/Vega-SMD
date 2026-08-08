using Vega.Altium.Models;
using Vega.CAD;
using Vega.CAD.Models;

namespace Vega.Altium;

public class AltiumPcbProjectAdapter
{
    private readonly PcbProjectService _projectService = new();

    public PcbProject Adapt(AltiumImportResult source, string sourceFile)
    {
        ArgumentNullException.ThrowIfNull(source);
        var project = _projectService.CreateProject(source.ProjectName, PcbSourceType.AltiumPcbDoc, sourceFile);
        foreach (var sourceComponent in source.Components)
        {
            var side = ToSide(sourceComponent.Layer);
            _projectService.AddComponent(project, new Component
            {
                RefDes = sourceComponent.RefDes, Value = sourceComponent.Value, Description = sourceComponent.Description,
                Footprint = sourceComponent.Footprint, PackageName = sourceComponent.Footprint,
                Manufacturer = sourceComponent.Manufacturer, ManufacturerPartNumber = sourceComponent.ManufacturerPartNumber,
                X = sourceComponent.X, Y = sourceComponent.Y, Rotation = sourceComponent.Rotation, Side = side
            });
            _projectService.AddPlacement(project, new Placement
            {
                RefDes = sourceComponent.RefDes, X = sourceComponent.X, Y = sourceComponent.Y,
                Rotation = sourceComponent.Rotation, Side = side, Comment = sourceComponent.Comment
            });
        }

        foreach (var sourceBom in source.Bom)
        {
            project.BomItems.Add(new BomItem
            {
                PartNumber = sourceBom.PartNumber, Description = sourceBom.Description,
                Footprint = sourceBom.Package, Quantity = sourceBom.Quantity, Manufacturer = sourceBom.Manufacturer
            });
        }

        foreach (var layer in source.Components.Select(component => component.Layer).Where(layer => !string.IsNullOrWhiteSpace(layer)).Distinct(StringComparer.OrdinalIgnoreCase))
            project.Board.Layers.Add(layer);
        SetBoardSize(project);
        return project;
    }

    private static BoardSide ToSide(string layer) => layer.Contains("bottom", StringComparison.OrdinalIgnoreCase)
        ? BoardSide.Bottom : BoardSide.Top;

    private static void SetBoardSize(PcbProject project)
    {
        if (project.Components.Count == 0) return;
        project.Board.Width = project.Components.Max(component => component.X) - project.Components.Min(component => component.X);
        project.Board.Height = project.Components.Max(component => component.Y) - project.Components.Min(component => component.Y);
    }
}
