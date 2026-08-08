using Vega.CAD;
using Vega.CAD.Models;

namespace Vega.Altium;

public class AltiumPcbImporter : IPcbImporter
{
    public bool CanImport(string fileName) =>
        Path.GetExtension(fileName).Equals(".PcbDoc", StringComparison.OrdinalIgnoreCase);

    public PcbProject Import(string fileName)
    {
        if (!CanImport(fileName)) throw new ArgumentException("Only Altium .PcbDoc files are supported.", nameof(fileName));
        var parser = new AltiumPcbDocParserService();
        parser.Load(fileName);
        return parser.ImportPcbProject();
    }
}
