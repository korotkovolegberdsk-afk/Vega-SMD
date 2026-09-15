using System.IO.Compression;
using System.Net;
using System.Net.Http;
using Vega.Services.MasterLibrary.Import;

namespace Vega.Tests.MasterLibrary;

public sealed class OfficialCadDownloadServiceTests
{
    [Fact]
    public async Task DownloadsStepAndReusesTheVerifiedLocalCopy()
    {
        var requests = 0;
        using var http = new HttpClient(new StubHandler(_ =>
        {
            requests++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent("ISO-10303-21;"u8.ToArray())
            };
        }));
        var root = Path.Combine(Path.GetTempPath(), "Vega-Cad-" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new OfficialCadDownloadService(http);
            var source = new Uri("https://www.st.com/model.step");
            var first = await service.DownloadAsync(source, "STMicroelectronics", "STM32G431CBT6", root);
            var second = await service.DownloadAsync(source, "STMicroelectronics", "STM32G431CBT6", root);

            Assert.True(File.Exists(first.FilePath));
            Assert.Equal(first.Sha256, second.Sha256);
            Assert.Equal(1, requests);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task ExtractsStepFromOfficialZip()
    {
        var zip = CreateZip("cad/model.stp", "ISO-10303-21;");
        using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(zip)
        }));
        var root = Path.Combine(Path.GetTempPath(), "Vega-Cad-" + Guid.NewGuid().ToString("N"));
        try
        {
            var result = await new OfficialCadDownloadService(http).DownloadAsync(
                new Uri("https://www.ti.com/model.zip"), "Texas Instruments", "BQ25601RTWR", root);

            Assert.EndsWith(".stp", result.FilePath, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("ISO-10303-21;", await File.ReadAllTextAsync(result.FilePath));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task RejectsThirdPartyCadHost()
    {
        using var http = new HttpClient(new StubHandler(_ => throw new InvalidOperationException("Must not request an untrusted host.")));
        var root = Path.Combine(Path.GetTempPath(), "Vega-Cad-" + Guid.NewGuid().ToString("N"));
        try
        {
            await Assert.ThrowsAsync<ArgumentException>(() => new OfficialCadDownloadService(http).DownloadAsync(
                new Uri("https://third-party.example/model.step"), "Texas Instruments", "BQ25601RTWR", root));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    static byte[] CreateZip(string entryName, string contents)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
        {
            using var writer = new StreamWriter(archive.CreateEntry(entryName).Open());
            writer.Write(contents);
        }
        return memory.ToArray();
    }

    sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(response(request));
    }
}
