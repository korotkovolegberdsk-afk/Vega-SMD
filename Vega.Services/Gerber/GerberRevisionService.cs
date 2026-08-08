using Vega.Gerber.Models;

namespace Vega.Services.Gerber;

public class GerberRevisionService
{
    public GerberRevisionInfo Create(
        string projectName,
        PasteLayer layer,
        int revision,
        int changesCount,
        string softwareVersion = "Vega-SMD")
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ArgumentException("Укажите имя проекта.", nameof(projectName));
        }

        if (revision <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(revision));
        }

        var side = layer.Side.ToUpperInvariant();
        var extension = Path.GetExtension(layer.FileName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = side == "BOTTOM" ? ".GBS" : ".GTP";
        }

        var revisionText = $"V{revision:000}";

        return new GerberRevisionInfo
        {
            ProjectName = projectName,
            OriginalFile = $"{projectName}_{side}_PASTE_ORIGINAL{extension.ToUpperInvariant()}",
            GeneratedFile = $"{projectName}_{side}_PASTE_CORRECTED_{revisionText}{extension.ToUpperInvariant()}",
            Revision = revisionText,
            CreatedDate = DateTime.UtcNow,
            SoftwareVersion = softwareVersion,
            ChangesCount = changesCount
        };
    }
}
