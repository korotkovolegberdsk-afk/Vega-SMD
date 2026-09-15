using System.Security.Cryptography;
using System.IO.Compression;
using System.Text.Json;

namespace Vega.Services.MasterLibrary.Import;

/// <summary>Downloads CAD only from an explicitly supplied public manufacturer URL.</summary>
public sealed class OfficialCadDownloadService
{
    private readonly HttpClient _http;
    public OfficialCadDownloadService(HttpClient? http = null) => _http = http ?? new HttpClient();

    public async Task<OfficialCadDownloadResult> DownloadAsync(Uri source, string manufacturer, string mpn, string destinationRoot, CancellationToken cancellationToken = default)
    {
        if (source.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Only HTTPS manufacturer URLs are accepted.", nameof(source));
        if (!OfficialCadSourcePolicy.IsOfficialManufacturerHost(manufacturer, source))
            throw new ArgumentException("The source host is not registered for this manufacturer.", nameof(source));
        var extension = Path.GetExtension(source.AbsolutePath).ToLowerInvariant();
        if (extension is not ".step" and not ".stp" and not ".zip" and not ".kicad_mod") throw new ArgumentException("Only STEP/STP/ZIP/KiCad footprint files are accepted.", nameof(source));
        var safeManufacturer = string.Concat(manufacturer.Where(char.IsLetterOrDigit));
        var safeMpn = string.Concat(mpn.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
        if (safeManufacturer.Length == 0 || safeMpn.Length == 0) throw new ArgumentException("Manufacturer and MPN are required.");
        var folder = Path.Combine(destinationRoot, safeManufacturer, safeMpn);
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, safeMpn + extension);
        var existing = Directory.EnumerateFiles(folder, safeMpn + extension, SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (existing is not null)
        {
            var existingBytes = await File.ReadAllBytesAsync(existing, cancellationToken);
            return new OfficialCadDownloadResult(existing, Convert.ToHexString(SHA256.HashData(existingBytes)), existingBytes.Length, source);
        }
        using var response = await _http.GetAsync(source, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using (var input = await response.Content.ReadAsStreamAsync(cancellationToken))
        await using (var output = File.Create(file)) await input.CopyToAsync(output, cancellationToken);
        if (extension == ".zip")
        {
            string extracted;
            using (var archive = ZipFile.OpenRead(file))
            {
                var model = archive.Entries.FirstOrDefault(e => Path.GetExtension(e.Name).Equals(".step", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(e.Name).Equals(".stp", StringComparison.OrdinalIgnoreCase));
                if (model is null) throw new InvalidDataException("CAD archive contains no STEP/STP model.");
                extracted = Path.Combine(folder, safeMpn + Path.GetExtension(model.Name).ToLowerInvariant());
                await using var input = model.Open();
                await using var output = File.Create(extracted);
                await input.CopyToAsync(output, cancellationToken);
            }
            file = extracted;
            File.Delete(Path.Combine(folder, safeMpn + ".zip"));
        }
        var bytes = await File.ReadAllBytesAsync(file, cancellationToken);
        if (bytes.Length == 0) { File.Delete(file); throw new InvalidDataException("Manufacturer returned an empty CAD file."); }
        return new OfficialCadDownloadResult(file, Convert.ToHexString(SHA256.HashData(bytes)), bytes.Length, source);
    }

    public async Task<IReadOnlyList<OfficialCadDownloadResult>> DownloadManyAsync(IEnumerable<OfficialCadDownloadRequest> requests, string destinationRoot, CancellationToken cancellationToken = default)
    {
        var results = new List<OfficialCadDownloadResult>();
        foreach (var request in requests)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await DownloadAsync(request.SourceUrl, request.Manufacturer, request.ManufacturerPartNumber, destinationRoot, cancellationToken));
        }
        return results;
    }

    public static IReadOnlyList<OfficialCadDownloadRequest> LoadManifest(string filePath)
    {
        var requests = JsonSerializer.Deserialize<List<OfficialCadDownloadManifestItem>>(File.ReadAllText(filePath), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
        return requests.Select(x => new OfficialCadDownloadRequest(x.Manufacturer, x.ManufacturerPartNumber, new Uri(x.SourceUrl, UriKind.Absolute))).ToList();
    }
}

/// <summary>Allow-list of public manufacturer domains. Add a manufacturer here before its CAD is queued.</summary>
public static class OfficialCadSourcePolicy
{
    static readonly IReadOnlyDictionary<string, string[]> Domains = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Analog Devices"] = ["analog.com"],
        ["Bourns"] = ["bourns.com"],
        ["Espressif"] = ["espressif.com", "github.com", "raw.githubusercontent.com"],
        ["Infineon"] = ["infineon.com"],
        ["KEMET"] = ["kemet.com", "yageo.com"],
        ["Littelfuse"] = ["littelfuse.com"],
        ["Molex"] = ["molex.com"],
        ["Murata"] = ["murata.com"],
        ["onsemi"] = ["onsemi.com"],
        ["Panasonic"] = ["panasonic.com"],
        ["ROHM"] = ["rohm.com"],
        ["Semtech"] = ["semtech.com"],
        ["STMicroelectronics"] = ["st.com"],
        ["TDK"] = ["tdk.com"],
        ["Texas Instruments"] = ["ti.com"],
        ["Würth Elektronik"] = ["we-online.com"],
        ["Wurth Elektronik"] = ["we-online.com"],
        ["Yageo"] = ["yageo.com"]
    };

    public static bool IsOfficialManufacturerHost(string manufacturer, Uri source)
    {
        if (!Domains.TryGetValue(manufacturer.Trim(), out var domains)) return false;
        if (manufacturer.Equals("Espressif", StringComparison.OrdinalIgnoreCase) &&
            (source.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) || source.Host.Equals("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase)))
            return source.AbsolutePath.StartsWith("/espressif/", StringComparison.OrdinalIgnoreCase);
        return domains.Any(domain => source.Host.Equals(domain, StringComparison.OrdinalIgnoreCase) || source.Host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record OfficialCadDownloadResult(string FilePath, string Sha256, long Length, Uri SourceUrl);
public sealed record OfficialCadDownloadRequest(string Manufacturer, string ManufacturerPartNumber, Uri SourceUrl);
public sealed record OfficialCadDownloadManifestItem(string Manufacturer, string ManufacturerPartNumber, string SourceUrl);
