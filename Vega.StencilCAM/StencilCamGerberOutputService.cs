using Vega.Gerber;
using Vega.Gerber.Models;
using Vega.StencilCAM.Models;

namespace Vega.StencilCAM;

public class StencilCamGerberOutputService
{
    private readonly GerberPasteWriterService _pasteWriter = new();

    public (string PasteFile, string MarkingFile) Write(
        string projectName,
        StencilPlacementResult placement,
        StencilMarking marking,
        string outputDirectory,
        int revision = 1)
    {
        if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is required.", nameof(projectName));
        if (revision <= 0) throw new ArgumentOutOfRangeException(nameof(revision));
        Directory.CreateDirectory(outputDirectory);
        var revisionText = $"V{revision:000}";
        var side = placement.PlacedLayer.Side.Equals("Bottom", StringComparison.OrdinalIgnoreCase) ? "BOTTOM" : "TOP";
        var pasteFile = Path.Combine(outputDirectory, $"{projectName}_PASTE_{side}_{revisionText}.GTP");
        var markingFile = Path.Combine(outputDirectory, $"{projectName}_MARKING_{revisionText}.GBR");
        var corrected = new CorrectedPasteLayer
        {
            OriginalFileName = placement.PlacedLayer.FileName, Side = placement.PlacedLayer.Side,
            OriginalPrimitiveCount = placement.PlacedLayer.Primitives.Count,
            CorrectedPrimitiveCount = placement.PlacedLayer.Primitives.Count,
            OriginalLayer = placement.PlacedLayer, CorrectedPrimitives = placement.PlacedLayer.Primitives
        };
        _pasteWriter.Write(corrected, pasteFile);
        File.WriteAllLines(markingFile,
        [
            "G04 Vega-SMD stencil marking layer*", "%FSLAX24Y24*%", "%MOMM*%",
            $"G04 TEXT={marking.Text}; X={marking.PositionX:0.####}; Y={marking.PositionY:0.####}; MIRROR=TRUE; ROTATION=0*", "M02*"
        ]);
        return (pasteFile, markingFile);
    }
}
