using System.Windows;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.StencilUI.Components;

public partial class ComponentEditorWindow : Window
{
    private readonly ComponentDefinitionService _componentService = new();
    private readonly PackageDefinitionService _packageService = new();
    private readonly ComponentDefinition _component;

    public int SavedComponentId { get; private set; }
    public string SavedPreviewImagePath { get; private set; } = string.Empty;

    public ComponentEditorWindow(ComponentDefinition component)
    {
        InitializeComponent();
        _component = Clone(component);
        LoadPackages();
        LoadComponent();
    }

    private void LoadPackages()
    {
        PackageComboBox.ItemsSource = _packageService.GetAll().Where(package => package.IsActive).OrderBy(package => package.PackageName).ToList();
    }

    private void LoadComponent()
    {
        ManufacturerTextBox.Text = _component.Manufacturer;
        MpnTextBox.Text = _component.ManufacturerPartNumber;
        DescriptionTextBox.Text = _component.Description;
        PackageComboBox.SelectedValue = _component.PackageId;
        PreviewImagePathTextBox.Text = _component.PreviewImagePath;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (PackageComboBox.SelectedValue is not int packageId)
        {
            MessageBox.Show("Выберите физический корпус (Package).", "Редактор компонента", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _component.Manufacturer = ManufacturerTextBox.Text.Trim();
        _component.ManufacturerPartNumber = MpnTextBox.Text.Trim();
        _component.Description = DescriptionTextBox.Text.Trim();
        _component.PackageId = packageId;
        _component.PreviewImagePath = PreviewImagePathTextBox.Text.Trim();

        try
        {
            _componentService.Update(_component);
            SavedComponentId = _component.Id;
            SavedPreviewImagePath = _component.PreviewImagePath;
            DialogResult = true;
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Редактор компонента", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static ComponentDefinition Clone(ComponentDefinition source) => new()
    {
        Id = source.Id,
        ManufacturerPartNumber = source.ManufacturerPartNumber,
        Manufacturer = source.Manufacturer,
        Description = source.Description,
        ComponentType = source.ComponentType,
        Value = source.Value,
        Tolerance = source.Tolerance,
        VoltageRating = source.VoltageRating,
        PowerRating = source.PowerRating,
        PackageId = source.PackageId,
        PreviewImagePath = source.PreviewImagePath,
        LifecycleStatus = source.LifecycleStatus,
        DatasheetUrl = source.DatasheetUrl,
        InternalPartNumber = source.InternalPartNumber,
        Notes = source.Notes,
        IsActive = source.IsActive,
        CreatedAt = source.CreatedAt,
        CreatedBy = source.CreatedBy,
        UpdatedAt = source.UpdatedAt,
        UpdatedBy = source.UpdatedBy,
        Version = source.Version,
        ChangeComment = source.ChangeComment
    };
}