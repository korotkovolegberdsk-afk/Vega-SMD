using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Vega.Models.MasterLibrary;
using Vega.UI.ViewModels;

namespace Vega.UI.Views;

public partial class PackageEditorWindow : Window
{
    private readonly PackageEditorMasterViewModel _viewModel;

    public PackageEditorWindow()
    {
        InitializeComponent();

        _viewModel = new PackageEditorMasterViewModel();
        DataContext = _viewModel;
    }

    public PackageEditorWindow(int packageId)
        : this()
    {
        _viewModel.Load(packageId);
        LoadPackage();
    }

    private void LoadPackage()
    {
        PackageNameTextBox.Text = _viewModel.PackageName;
        DisplayNameTextBox.Text = _viewModel.DisplayName;
        DescriptionTextBox.Text = _viewModel.Description;
        StandardTextBox.Text = _viewModel.IPCName;
        LengthTextBox.Text = FormatNumber(_viewModel.Length);
        WidthTextBox.Text = FormatNumber(_viewModel.Width);
        HeightTextBox.Text = FormatNumber(_viewModel.Height);
        PitchTextBox.Text = FormatNumber(_viewModel.Pitch);
        LeadCountTextBox.Text = _viewModel.LeadCount.ToString(CultureInfo.CurrentCulture);
        PadCountTextBox.Text = _viewModel.PadCount.ToString(CultureInfo.CurrentCulture);
        ThermalPadCountTextBox.Text = _viewModel.ThermalPadCount.ToString(CultureInfo.CurrentCulture);
        IPCNameTextBox.Text = _viewModel.IPCName;
        JEDECNameTextBox.Text = _viewModel.JEDECName;
        LandPatternNameTextBox.Text = _viewModel.LandPatternName;
        StencilThicknessTextBox.Text = FormatNumber(_viewModel.StencilThickness);
        ApertureTypeTextBox.Text = _viewModel.ApertureType;
        ApertureReductionTextBox.Text = _viewModel.ApertureReduction;
        AreaRatioTextBox.Text = FormatNumber(_viewModel.AreaRatio);
        AspectRatioTextBox.Text = FormatNumber(_viewModel.AspectRatio);
        SPIRecommendationsTextBox.Text = _viewModel.SPIRecommendations;
        AOIRecommendationsTextBox.Text = _viewModel.AOIRecommendations;
        TypicalDefectsTextBox.Text = _viewModel.TypicalDefects;
        RecommendedProfileTextBox.Text = _viewModel.RecommendedProfile;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PackageNameTextBox.Text))
        {
            ShowValidationError("Укажите имя корпуса.", PackageNameTextBox);
            return;
        }

        if (!TryReadDouble(LengthTextBox, "длину корпуса", out var length)
            || !TryReadDouble(WidthTextBox, "ширину корпуса", out var width)
            || !TryReadDouble(HeightTextBox, "высоту корпуса", out var height)
            || !TryReadDouble(PitchTextBox, "шаг выводов", out var pitch)
            || !TryReadInt(LeadCountTextBox, "количество выводов", out var leadCount)
            || !TryReadInt(PadCountTextBox, "количество площадок", out var padCount)
            || !TryReadInt(ThermalPadCountTextBox, "количество тепловых площадок", out var thermalPadCount)
            || !TryReadDouble(StencilThicknessTextBox, "толщину трафарета", out var stencilThickness)
            || !TryReadDouble(AreaRatioTextBox, "Area Ratio", out var areaRatio)
            || !TryReadDouble(AspectRatioTextBox, "Aspect Ratio", out var aspectRatio))
        {
            return;
        }

        _viewModel.PackageName = PackageNameTextBox.Text.Trim();
        _viewModel.DisplayName = DisplayNameTextBox.Text.Trim();
        _viewModel.Description = DescriptionTextBox.Text.Trim();
        _viewModel.Length = length;
        _viewModel.Width = width;
        _viewModel.Height = height;
        _viewModel.Pitch = pitch;
        _viewModel.LeadCount = leadCount;
        _viewModel.PadCount = padCount;
        _viewModel.ThermalPadCount = thermalPadCount;
        _viewModel.IPCName = string.IsNullOrWhiteSpace(IPCNameTextBox.Text)
            ? StandardTextBox.Text.Trim()
            : IPCNameTextBox.Text.Trim();
        _viewModel.JEDECName = JEDECNameTextBox.Text.Trim();
        _viewModel.LandPatternName = LandPatternNameTextBox.Text.Trim();
        _viewModel.StencilThickness = stencilThickness;
        _viewModel.ApertureType = ApertureTypeTextBox.Text.Trim();
        _viewModel.ApertureReduction = ApertureReductionTextBox.Text.Trim();
        _viewModel.AreaRatio = areaRatio;
        _viewModel.AspectRatio = aspectRatio;
        _viewModel.SPIRecommendations = SPIRecommendationsTextBox.Text.Trim();
        _viewModel.AOIRecommendations = AOIRecommendationsTextBox.Text.Trim();
        _viewModel.TypicalDefects = TypicalDefectsTextBox.Text.Trim();
        _viewModel.RecommendedProfile = RecommendedProfileTextBox.Text.Trim();

        try
        {
            _viewModel.Save();
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(exception.Message, PackageNameTextBox);
            return;
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                "Vega-SMD",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.CurrentCulture);

    private bool TryReadDouble(TextBox textBox, string fieldName, out double value)
    {
        var text = textBox.Text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            value = 0;
            return true;
        }

        var styles = NumberStyles.Float | NumberStyles.AllowThousands;
        var parsed = double.TryParse(text, styles, CultureInfo.CurrentCulture, out value)
            || double.TryParse(text, styles, CultureInfo.InvariantCulture, out value);

        if (parsed && value >= 0)
        {
            return true;
        }

        ShowValidationError(
            $"Введите корректное неотрицательное значение: {fieldName}.",
            textBox);
        return false;
    }

    private bool TryReadInt(TextBox textBox, string fieldName, out int value)
    {
        var text = textBox.Text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            value = 0;
            return true;
        }

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)
            && value >= 0)
        {
            return true;
        }

        ShowValidationError(
            $"Введите корректное неотрицательное значение: {fieldName}.",
            textBox);
        return false;
    }

    private void ShowValidationError(string message, TextBox textBox)
    {
        MessageBox.Show(
            this,
            message,
            "Vega-SMD",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        textBox.Focus();
        textBox.SelectAll();
    }

    private void AddAlias_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.AddAlias();
        EquipmentAliasesGrid.SelectedIndex = _viewModel.Aliases.Count - 1;
        EquipmentAliasesGrid.ScrollIntoView(EquipmentAliasesGrid.SelectedItem);
    }

    private void DeleteAlias_Click(object sender, RoutedEventArgs e)
    {
        if (EquipmentAliasesGrid.SelectedItem is not EquipmentAlias alias)
        {
            return;
        }

        _viewModel.RemoveAlias(alias);
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
