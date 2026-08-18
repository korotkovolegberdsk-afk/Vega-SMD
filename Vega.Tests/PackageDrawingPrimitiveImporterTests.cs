using Vega.Data.MasterLibrary.Import;
using Xunit;

namespace Vega.Tests;

public sealed class PackageDrawingPrimitiveImporterTests
{
    [Fact]
    public void ImportInfineonSOT23TopSvg()
    {
        var file = Path.Combine(Path.GetTempPath(), $"sot23-{Guid.NewGuid():N}.svg");
        File.WriteAllText(file, "<svg xmlns='http://www.w3.org/2000/svg' width='2.9mm' height='1.3mm' viewBox='0 0 2.9 1.3'><path data-layer='Body' d='M 0 0 L 2.9 0 L 2.9 1.3 L 0 1.3 Z'/><path data-layer='Lead' d='M 0.1 0.2 L 0.1 0.4'/><path data-layer='Lead' d='M 0.1 0.6 L 0.1 0.8'/><path data-layer='Lead' d='M 2.8 0.45 L 2.8 0.85'/><circle data-layer='Pin1' cx='0.2' cy='0.2' r='0.05'/></svg>");
        try
        {
            var p = new PackageDrawingPrimitiveImporter().ImportSvg(42, file);
            Assert.Equal(5, p.Count); Assert.All(p, x => Assert.Equal("mm", x.CoordinateSystem)); Assert.Equal(42, p[0].ProjectionGeometryId); Assert.Equal("Body", p[0].Layer); Assert.Equal("Lead", p[1].Layer); Assert.Equal("Pin1", p[^1].Layer); Assert.Equal(0.2, p[^1].CenterX); Assert.Equal(0.05, p[^1].Radius);
        }
        finally { File.Delete(file); }
    }
}
