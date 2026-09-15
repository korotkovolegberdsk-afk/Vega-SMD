using Vega.CAD;
using Xunit;

namespace Vega.Tests;

public sealed class ModelPathResolverTests
{
    [Fact]
    public void UnknownPackageDoesNotUseAnArbitraryModel()
    {
        Assert.Null(KiCadModelRenderService.ResolveModelPath("UNKNOWN_PACKAGE", null));
    }

    [Fact]
    public void ExistingConfiguredModelIsPreferred()
    {
        var path = Path.GetTempFileName();
        try
        {
            Assert.Equal(path, KiCadModelRenderService.ResolveModelPath("UNKNOWN_PACKAGE", path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
