using System.Text.Json;
using Vega.Services.MasterLibrary.Import;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: Vega.CadDownloader <manifest.json> <destination-folder>");
    Console.Error.WriteLine("Manifest: [{\"manufacturer\":\"...\",\"manufacturerPartNumber\":\"...\",\"sourceUrl\":\"https://...step\"}]");
    return 2;
}

var manifestPath = Path.GetFullPath(args[0]);
var destination = Path.GetFullPath(args[1]);
if (!File.Exists(manifestPath))
{
    Console.Error.WriteLine($"Manifest not found: {manifestPath}");
    return 2;
}

try
{
    var requests = OfficialCadDownloadService.LoadManifest(manifestPath);
    var duplicates = requests
        .GroupBy(x => $"{x.Manufacturer}\u001f{x.ManufacturerPartNumber}\u001f{x.SourceUrl}", StringComparer.OrdinalIgnoreCase)
        .Where(x => x.Count() > 1)
        .Select(x => x.Key.Replace('\u001f', ' '))
        .ToArray();
    if (duplicates.Length > 0)
        throw new InvalidDataException("The manifest repeats source files: " + string.Join(", ", duplicates));

    var results = await new OfficialCadDownloadService().DownloadManyAsync(requests, destination);
    var report = results.Select(x => new
    {
        filePath = x.FilePath,
        sha256 = x.Sha256,
        byteLength = x.Length,
        sourceUrl = x.SourceUrl.AbsoluteUri
    });
    Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
