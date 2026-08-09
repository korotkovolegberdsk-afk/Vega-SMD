using System.IO;
using System.Text.Json;
using Vega.Localization.Models;

namespace Vega.Localization;

public class ApplicationSettingsService
{
    private readonly string _settingsPath;
    public ApplicationSettingsService(string? settingsPath = null) => _settingsPath = settingsPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vega-SMD", "settings.json");
    public ApplicationSettings Load()
    {
        if (!File.Exists(_settingsPath)) return new ApplicationSettings();
        try { return JsonSerializer.Deserialize<ApplicationSettings>(File.ReadAllText(_settingsPath)) ?? new ApplicationSettings(); }
        catch (JsonException) { return new ApplicationSettings(); }
    }
    public void Save(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings); var directory = Path.GetDirectoryName(Path.GetFullPath(_settingsPath)); Directory.CreateDirectory(directory!);
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }
}