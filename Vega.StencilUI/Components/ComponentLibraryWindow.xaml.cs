using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.StencilUI.Components;

public partial class ComponentLibraryWindow : Window
{
    private readonly ComponentLibraryViewModel _viewModel = new();
    public ComponentLibraryWindow() { InitializeComponent(); DataContext = _viewModel; }

    private void ComponentsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FindParent<DataGridRow>(e.OriginalSource as DependencyObject) is null || _viewModel.SelectedComponent is null)
            return;

        var editor = new ComponentEditorWindow(_viewModel.SelectedComponent) { Owner = this };
        if (editor.ShowDialog() == true)
            _viewModel.ReloadComponents(editor.SavedComponentId, editor.SavedPreviewImagePath);
    }

    private static T? FindParent<T>(DependencyObject? element) where T : DependencyObject
    {
        while (element is not null)
        {
            if (element is T parent) return parent;
            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }
    private void CreateProfile_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedComponent is null) return;
        OpenEditor(new ComponentTapeReelGeometry { ComponentDefinitionId = _viewModel.SelectedComponent.Id, IsActive = true }, false);
    }
    private void EditProfile_Click(object sender, RoutedEventArgs e) { if (_viewModel.SelectedProfile is not null) OpenEditor(_viewModel.SelectedProfile, true); }
    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        var profile = _viewModel.SelectedProfile; if (profile is null) return;
        if (MessageBox.Show("Удалить выбранный профиль Tape & Reel?", "Компоненты (MPN)", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try { _viewModel.TapeService.DeleteProfile(profile.Id); _viewModel.RefreshProfiles(); }
        catch (Exception exception) { MessageBox.Show(exception.Message, "Компоненты (MPN)", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private void SetDefault_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedComponent is null || _viewModel.SelectedProfile is null) return;
        try { _viewModel.TapeService.SetDefaultProfile(_viewModel.SelectedComponent.Id, _viewModel.SelectedProfile.Id); _viewModel.RefreshProfiles(); }
        catch (Exception exception) { MessageBox.Show(exception.Message, "Компоненты (MPN)", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private void OpenEditor(ComponentTapeReelGeometry profile, bool isEdit)
    {
        var editor = new ComponentTapeReelEditorWindow(profile, isEdit) { Owner = this };
        if (editor.ShowDialog() == true) _viewModel.RefreshProfiles(editor.SavedProfileId);
    }
}

internal sealed class ComponentLibraryViewModel : INotifyPropertyChanged
{
    private readonly ComponentDefinitionService _componentService = new();
    private readonly StencilTechnologyRuleService _stencilTechnologyRuleService = new();
    private string _searchText = string.Empty;
    private ComponentDefinition? _selectedComponent;
    private ComponentTapeReelGeometry? _selectedProfile;
    private StencilTechnologyRule? _selectedStencilRule;
    public ComponentTapeReelGeometryService TapeService { get; } = new();
    public ObservableCollection<ComponentDefinition> Components { get; } = [];
    public ObservableCollection<ComponentTapeReelGeometry> Profiles { get; } = [];
    public string SearchText { get => _searchText; set { if (Set(ref _searchText, value)) LoadComponents(); } }
    public ComponentDefinition? SelectedComponent { get => _selectedComponent; set { if (!Set(ref _selectedComponent, value)) return; RefreshProfiles(); RefreshStencilRule(); Raise(nameof(HasSelectedComponent)); Raise(nameof(StatusText)); } }
    public ComponentTapeReelGeometry? SelectedProfile { get => _selectedProfile; set { if (Set(ref _selectedProfile, value)) Raise(nameof(HasSelectedProfile)); } }
    public StencilTechnologyRule? SelectedStencilRule { get => _selectedStencilRule; private set => Set(ref _selectedStencilRule, value); }
    public bool HasSelectedComponent => SelectedComponent is not null;
    public bool HasSelectedProfile => SelectedProfile is not null;
    public string StatusText => $"Компонентов: {Components.Count}   Выбран: {SelectedComponent?.ManufacturerPartNumber ?? "—"}";
    public ComponentLibraryViewModel() => LoadComponents();
    public void RefreshProfiles(int? selectedId = null)
    {
        Profiles.Clear();
        if (SelectedComponent is not null) foreach (var profile in TapeService.GetProfiles(SelectedComponent.Id)) Profiles.Add(profile);
        SelectedProfile = selectedId is null ? Profiles.FirstOrDefault() : Profiles.FirstOrDefault(x => x.Id == selectedId) ?? Profiles.FirstOrDefault();
    }
    private void RefreshStencilRule()
    {
        SelectedStencilRule = SelectedComponent?.Package is { } package
            ? _stencilTechnologyRuleService.GetRule(package, ApertureStrategy.StandardPasteRelease)
            : null;
    }
    public void ReloadComponents(int? selectedId = null, string? previewImagePath = null) => LoadComponents(selectedId, previewImagePath);

    private void LoadComponents(int? selectedId = null, string? previewImagePath = null)
    {
        var query = SearchText.Trim(); var all = _componentService.GetAll();
        var filtered = string.IsNullOrWhiteSpace(query) ? all : all.Where(component => component.Manufacturer.Contains(query, StringComparison.OrdinalIgnoreCase) || component.ManufacturerPartNumber.Contains(query, StringComparison.OrdinalIgnoreCase) || (component.Package?.PackageName ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase));
        Components.Clear(); foreach (var component in filtered.OrderBy(x => x.ManufacturerPartNumber)) Components.Add(component);
        SelectedComponent = selectedId is null ? Components.FirstOrDefault() : Components.FirstOrDefault(x => x.Id == selectedId) ?? Components.FirstOrDefault();
        if (SelectedComponent is not null && previewImagePath is not null) SelectedComponent.PreviewImagePath = previewImagePath;
        Raise(nameof(StatusText));
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; Raise(propertyName!); return true; }
    private void Raise(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
