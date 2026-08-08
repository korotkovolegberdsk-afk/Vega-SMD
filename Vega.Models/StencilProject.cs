using Vega.PnP.Models;

namespace Vega.Models;

public class StencilProject
{
    public string ProjectName { get; init; } = "";
    public string TopPasteFile { get; init; } = "";
    public string BottomPasteFile { get; init; } = "";
    public string PnpFile { get; init; } = "";
    public IReadOnlyList<PnpComponent> Components { get; init; } = Array.Empty<PnpComponent>();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}
