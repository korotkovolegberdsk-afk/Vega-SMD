using Vega.CAD.Models;

namespace Vega.CAD;

public class PcbProjectService
{
    public PcbProject CreateProject(string projectName, PcbSourceType sourceType = PcbSourceType.Manual, string sourceFile = "") => new()
    {
        ProjectName = projectName, SourceType = sourceType, SourceFile = sourceFile
    };

    public void AddComponent(PcbProject project, Component component)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(component);
        project.Components.Add(component);
    }

    public void AddPlacement(PcbProject project, Placement placement)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(placement);
        project.Placements.Add(placement);
    }

    public void AddFiducial(PcbProject project, Fiducial fiducial)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(fiducial);
        project.Fiducials.Add(fiducial);
    }

    public void AddPasteLayer(PcbProject project, PasteLayerInfo pasteLayer)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(pasteLayer);
        project.PasteLayers.Add(pasteLayer);
    }
}
