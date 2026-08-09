using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Localization;
using Vega.Localization.Models;

namespace Vega.StencilUI.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly LocalizationService _localization;
    private ApplicationLanguage? _selectedLanguage;
    private string _selectedTheme;

    public SettingsViewModel()
        : this(LocalizationService.Default)
    {
    }

    public SettingsViewModel(LocalizationService? localization = null)
    {
        _localization = localization ?? LocalizationService.Default;
        Languages = _localization.Languages;
        SelectedLanguage = Languages.FirstOrDefault(item => item.Code == _localization.GetCurrentLanguage());
        _selectedTheme = _localization.Settings.Theme;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyList<ApplicationLanguage> Languages { get; }
    public IReadOnlyList<string> Themes { get; } = ["Light", "Dark"];
    public ApplicationLanguage? SelectedLanguage { get => _selectedLanguage; set => SetField(ref _selectedLanguage, value); }
    public string SelectedTheme { get => _selectedTheme; set => SetField(ref _selectedTheme, value); }
    public void Save() { if (SelectedLanguage is not null) _localization.SetLanguage(SelectedLanguage.Code); _localization.UpdateSettings(SelectedTheme, _localization.Settings.LastProjectPath); }
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); return true; }
}