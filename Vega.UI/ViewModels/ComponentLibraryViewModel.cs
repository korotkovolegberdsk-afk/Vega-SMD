using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.UI.ViewModels;

public class ComponentLibraryViewModel : INotifyPropertyChanged
{
    private readonly ComponentDefinitionRepository _repository;


    private List<ComponentDefinitionView> _allComponents = new();


    public ObservableCollection<ComponentDefinitionView> Components { get; }
        = new();


    private ComponentDefinitionView? _selectedComponent;

    public ComponentDefinitionView? SelectedComponent
    {
        get => _selectedComponent;

        set
        {
            _selectedComponent = value;

            OnPropertyChanged();
        }
    }


    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;

        set
        {
            _searchText = value;

            OnPropertyChanged();

            FilterComponents();
        }
    }


    public ComponentLibraryViewModel()
    {
        _repository =
            new ComponentDefinitionRepository();

        Reload();
    }


    public void Reload()
    {
        _allComponents =
            _repository.GetComponentViews();

        FilterComponents();
    }


    private void FilterComponents()
    {
        Components.Clear();

        var text =
            SearchText.Trim();

        foreach (var component in _allComponents)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                bool match =
                    Contains(
                        component.ManufacturerPartNumber,
                        text)
                    ||
                    Contains(
                        component.Manufacturer,
                        text)
                    ||
                    Contains(
                        component.PackageName,
                        text)
                    ||
                    Contains(
                        component.PackageCategory,
                        text)
                    ||
                    Contains(
                        component.PackageFamily,
                        text);

                if (!match)
                {
                    continue;
                }
            }

            Components.Add(component);
        }

        if (SelectedComponent != null
            && !Components.Contains(SelectedComponent))
        {
            SelectedComponent =
                Components.FirstOrDefault();
        }
    }


    private static bool Contains(
        string? value,
        string text)
    {
        return !string.IsNullOrEmpty(value)
               && value.Contains(
                   text,
                   StringComparison.OrdinalIgnoreCase);
    }


    public event PropertyChangedEventHandler? PropertyChanged;


    private void OnPropertyChanged(
        [CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(name));
    }
}