using Vega.CAD.Models;

namespace Vega.CAD;

public interface IPcbImporter
{
    bool CanImport(string fileName);
    PcbProject Import(string fileName);
}
