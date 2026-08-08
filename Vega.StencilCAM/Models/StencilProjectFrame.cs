namespace Vega.StencilCAM.Models;

public class StencilProjectFrame
{
    public int ProjectId { get; init; }
    public int FrameId { get; init; }
    public string FrameName { get; init; } = "";
    public DateTime AssignedDate { get; init; }
}
