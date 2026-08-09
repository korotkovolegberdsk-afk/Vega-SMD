using System.Collections.ObjectModel;
using Vega.StencilCAM.Models;
using Vega.StencilProjects;
using Vega.StencilProjects.Models;

namespace Vega.StencilUI.ViewModels;

public class NewStencilProjectViewModel
{
    private readonly StencilProjectService _service = new();
    public string ProjectName { get; set; } = "Stencil Project";
    public string Customer { get; set; } = "";
    public bool IsPasteGerber { get; set; } = true;
    public bool IsAltiumPcbDoc { get; set; }
    public bool IsPanelGerber { get; set; }
    public bool IsTop { get; set; } = true;
    public bool IsBottom { get; set; }
    public string TopPasteFile { get; set; } = "";
    public string BottomPasteFile { get; set; } = "";
    public string AssemblyDrawingFile { get; set; } = "";
    public ObservableCollection<StencilFrame> Frames { get; } = new();
    public StencilFrame? SelectedFrame { get; set; }
    public StencilProjectSession? Session { get; private set; }

    public NewStencilProjectViewModel()
    {
        foreach (var frame in _service.GetFrames()) Frames.Add(frame);
        SelectedFrame = Frames.FirstOrDefault(frame => frame.IsDefault) ?? Frames.FirstOrDefault();
    }

    public void Create()
    {
        var source = IsAltiumPcbDoc ? StencilProjectInputSource.AltiumPcbDoc : IsPanelGerber ? StencilProjectInputSource.PanelGerber : StencilProjectInputSource.PasteGerber;
        var side = IsBottom ? StencilProjectPasteSide.Bottom : StencilProjectPasteSide.Top;
        Session = _service.Create(ProjectName, Customer, source, side, SelectedFrame);
        if (source != StencilProjectInputSource.PasteGerber) throw new NotSupportedException("The first production workflow supports Paste Gerber input.");
        _service.LoadPasteGerber(Session, TopPasteFile, BottomPasteFile, AssemblyDrawingFile);
    }
}