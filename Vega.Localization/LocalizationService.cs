using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using Vega.Localization.Models;

namespace Vega.Localization;

public class LocalizationService : INotifyPropertyChanged
{
    private const string LanguageDictionaryMarker = "Vega.Localization.LanguageDictionary";
    private const string ThemeDictionaryMarker = "Vega.Localization.ThemeDictionary";
    private readonly ApplicationSettingsService _settingsService;
    private ApplicationSettings _settings;
    private ResourceDictionary _dictionary = new();
    private ApplicationLanguage _currentLanguage;

    public static LocalizationService Default { get; } = new();

    public IReadOnlyList<ApplicationLanguage> Languages { get; } =
    [
        new() { Id = 1, Code = "ru-RU", Name = "Russian", NativeName = "Русский" },
        new() { Id = 2, Code = "en-US", Name = "English", NativeName = "English" },
        new() { Id = 3, Code = "zh-CN", Name = "Chinese", NativeName = "中文" }
    ];

    public LocalizationService(ApplicationSettingsService? settingsService = null)
    {
        _settingsService = settingsService ?? new ApplicationSettingsService();
        _settings = _settingsService.Load();
        _currentLanguage = ResolveLanguage(_settings.Language);
        _dictionary = LoadLanguageDictionary(_currentLanguage.Code);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;
    public event EventHandler? ThemeChanged;

    public ApplicationLanguage CurrentLanguage => _currentLanguage;
    public string CurrentTheme => NormalizeTheme(_settings.Theme);
    public string this[string key] => GetString(key);
    public ApplicationSettings Settings => _settings;

    public void Initialize()
    {
        SetLanguage(string.IsNullOrWhiteSpace(_settings.Language) ? CultureInfo.CurrentUICulture.Name : _settings.Language, save: false);
        SetTheme(_settings.Theme, save: false);
    }

    public void SetLanguage(string languageCode) => SetLanguage(languageCode, save: true);
    public void SetTheme(string theme) => SetTheme(theme, save: true);
    public string GetCurrentLanguage() => _currentLanguage.Code;
    public string GetString(string key) => _dictionary.Contains(key) ? _dictionary[key]?.ToString() ?? key : key;

    public void UpdateSettings(string theme, string lastProjectPath)
    {
        SetTheme(theme, save: false);
        _settings.LastProjectPath = lastProjectPath;
        _settings.Language = _currentLanguage.Code;
        _settingsService.Save(_settings);
        OnPropertyChanged(nameof(Settings));
    }

    private void SetLanguage(string languageCode, bool save)
    {
        _currentLanguage = ResolveLanguage(languageCode);
        _dictionary = LoadLanguageDictionary(_currentLanguage.Code);
        ReplaceApplicationDictionary(_dictionary, LanguageDictionaryMarker);
        _settings.Language = _currentLanguage.Code;
        if (save) _settingsService.Save(_settings);

        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(Settings));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetTheme(string theme, bool save)
    {
        var normalizedTheme = NormalizeTheme(theme);
        var dictionary = LoadThemeDictionary(normalizedTheme);
        ReplaceApplicationDictionary(dictionary, ThemeDictionaryMarker);
        _settings.Theme = normalizedTheme;
        if (save) _settingsService.Save(_settings);

        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(Settings));
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private ApplicationLanguage ResolveLanguage(string? code) =>
        Languages.FirstOrDefault(item => item.Code.Equals(code, StringComparison.OrdinalIgnoreCase)) ??
        Languages.First(item => item.Code == "ru-RU");

    private static string NormalizeTheme(string? theme) =>
        string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";

    private static ResourceDictionary LoadLanguageDictionary(string code)
    {
        var dictionaryCode = code is "ru-RU" or "en-US" ? code : "en-US";
        var dictionary = (ResourceDictionary)Application.LoadComponent(new Uri($"/Vega.Localization;component/Resources/Languages/Language.{dictionaryCode}.xaml", UriKind.Relative));
        dictionary[LanguageDictionaryMarker] = true;
        return dictionary;
    }

    private static ResourceDictionary LoadThemeDictionary(string theme) =>
        CreateMarkedDictionary($"/Vega.StencilUI;component/Themes/Theme.{theme}.xaml", ThemeDictionaryMarker);

    private static ResourceDictionary CreateMarkedDictionary(string uri, string marker)
    {
        var dictionary = (ResourceDictionary)Application.LoadComponent(new Uri(uri, UriKind.Relative));
        dictionary[marker] = true;
        return dictionary;
    }

    private static void ReplaceApplicationDictionary(ResourceDictionary dictionary, string marker)
    {
        var application = Application.Current;
        if (application is null) return;

        var dictionaries = application.Resources.MergedDictionaries;
        for (var index = dictionaries.Count - 1; index >= 0; index--)
        {
            if (dictionaries[index].Contains(marker)) dictionaries.RemoveAt(index);
        }

        dictionaries.Add(dictionary);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}