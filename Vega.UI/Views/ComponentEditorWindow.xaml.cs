using System.Windows;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.UI.Views;

public partial class ComponentEditorWindow : Window
{
    private readonly ComponentDefinitionRepository _repository;
    private readonly PackageDefinitionService _packageService;

    private readonly ComponentDefinition? _editingComponent;



    public ComponentEditorWindow()
    {
        InitializeComponent();

        _repository =
            new ComponentDefinitionRepository();
        _packageService = new PackageDefinitionService();

        LoadPackages();
    }



    public ComponentEditorWindow(
        ComponentDefinition component)
        : this()
    {
        _editingComponent = component;

        LoadComponent(component);
    }



    private void LoadPackages()
    {
        var packages = _packageService
            .GetAll()
            .Where(x => x.IsActive)
            .ToList();

        PackageComboBox.ItemsSource = packages;

        if (packages.Count > 0)
        {
            PackageComboBox.SelectedIndex = 0;
        }
    }

    private void LoadComponent(
        ComponentDefinition component)
    {
        PartNumberTextBox.Text =
            component.ManufacturerPartNumber;

        ManufacturerTextBox.Text =
            component.Manufacturer;

        TypeTextBox.Text =
            component.ComponentType;

        DescriptionTextBox.Text =
            component.Description;


        PackageComboBox.SelectedValue =
            component.PackageId;
    }



    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (PackageComboBox.SelectedValue is not int packageId)
        {
            MessageBox.Show(
                "Select a package.",
                "Vega-SMD",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }



        if (_editingComponent == null)
        {
            var component =
                new ComponentDefinition
                {
                    ManufacturerPartNumber =
                        PartNumberTextBox.Text.Trim(),

                    Manufacturer =
                        ManufacturerTextBox.Text.Trim(),

                    ComponentType =
                        TypeTextBox.Text.Trim(),

                    Description =
                        DescriptionTextBox.Text.Trim(),

                    PackageId =
                        packageId,

                    Version = 1
                };


            _repository.Add(component);
        }
        else
        {
            _editingComponent.ManufacturerPartNumber =
                PartNumberTextBox.Text.Trim();

            _editingComponent.Manufacturer =
                ManufacturerTextBox.Text.Trim();

            _editingComponent.ComponentType =
                TypeTextBox.Text.Trim();

            _editingComponent.Description =
                DescriptionTextBox.Text.Trim();

            _editingComponent.PackageId =
                packageId;


            _repository.Update(
                _editingComponent);
        }



        DialogResult = true;

        Close();
    }



    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;

        Close();
    }
}