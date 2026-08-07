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
        BodyLengthTextBox.Text = FormatNumber(_viewModel.Geometry.BodyLength);
        BodyWidthTextBox.Text = FormatNumber(_viewModel.Geometry.BodyWidth);
        BodyHeightTextBox.Text = FormatNumber(_viewModel.Geometry.BodyHeight);
        CenterXTextBox.Text = FormatNumber(_viewModel.Geometry.CenterX);
        CenterYTextBox.Text = FormatNumber(_viewModel.Geometry.CenterY);
        LeadLengthTextBox.Text = FormatNumber(_viewModel.Geometry.LeadLength);
        LeadWidthTextBox.Text = FormatNumber(_viewModel.Geometry.LeadWidth);
        LeadPitchTextBox.Text = FormatNumber(_viewModel.Geometry.LeadPitch);
        GeometryLeadCountTextBox.Text = _viewModel.Geometry.LeadCount.ToString(CultureInfo.CurrentCulture);
        PadLengthTextBox.Text = FormatNumber(_viewModel.Geometry.PadLength);
        PadWidthTextBox.Text = FormatNumber(_viewModel.Geometry.PadWidth);
        PadPitchTextBox.Text = FormatNumber(_viewModel.Geometry.PadPitch);
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
        PatternNameTextBox.Text = _viewModel.Footprint.PatternName;
        FootprintStandardNameTextBox.Text = _viewModel.Footprint.StandardName;
        FootprintDescriptionTextBox.Text = _viewModel.Footprint.Description;
        FootprintPadCountTextBox.Text = _viewModel.Footprint.PadCount.ToString(CultureInfo.CurrentCulture);
        FootprintPadLengthTextBox.Text = FormatNumber(_viewModel.Footprint.PadLength);
        FootprintPadWidthTextBox.Text = FormatNumber(_viewModel.Footprint.PadWidth);
        FootprintPadPitchTextBox.Text = FormatNumber(_viewModel.Footprint.PadPitch);
        Pin1OffsetTextBox.Text = FormatNumber(_viewModel.Footprint.Pin1Offset);
        RowCountTextBox.Text = _viewModel.Footprint.RowCount.ToString(CultureInfo.CurrentCulture);
        ColumnCountTextBox.Text = _viewModel.Footprint.ColumnCount.ToString(CultureInfo.CurrentCulture);
        PasteReductionTextBox.Text = FormatNumber(_viewModel.Footprint.PasteReduction);
        FootprintApertureTypeTextBox.Text = _viewModel.Footprint.ApertureType;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PackageNameTextBox.Text))
        {
            ShowValidationError("Укажите имя корпуса.", PackageNameTextBox);
            return;
        }

        if (!TryReadDouble(BodyLengthTextBox, "длину корпуса", out var bodyLength)
            || !TryReadDouble(BodyWidthTextBox, "ширину корпуса", out var bodyWidth)
            || !TryReadDouble(BodyHeightTextBox, "высоту корпуса", out var bodyHeight)
            || !TryReadDouble(CenterXTextBox, "координату центра X", out var centerX)
            || !TryReadDouble(CenterYTextBox, "координату центра Y", out var centerY)
            || !TryReadDouble(LeadLengthTextBox, "длину вывода", out var leadLength)
            || !TryReadDouble(LeadWidthTextBox, "ширину вывода", out var leadWidth)
            || !TryReadDouble(LeadPitchTextBox, "шаг выводов", out var leadPitch)
            || !TryReadInt(GeometryLeadCountTextBox, "количество выводов", out var geometryLeadCount)
            || !TryReadDouble(PadLengthTextBox, "длину площадки", out var padLength)
            || !TryReadDouble(PadWidthTextBox, "ширину площадки", out var padWidth)
            || !TryReadDouble(PadPitchTextBox, "шаг площадок", out var padPitch)            || !TryReadDouble(StencilThicknessTextBox, "толщину трафарета", out var stencilThickness)
            || !TryReadDouble(AreaRatioTextBox, "Area Ratio", out var areaRatio)
            || !TryReadDouble(AspectRatioTextBox, "Aspect Ratio", out var aspectRatio)            || !TryReadInt(FootprintPadCountTextBox, "количество площадок", out var footprintPadCount)
            || !TryReadDouble(FootprintPadLengthTextBox, "длину площадки", out var footprintPadLength)
            || !TryReadDouble(FootprintPadWidthTextBox, "ширину площадки", out var footprintPadWidth)
            || !TryReadDouble(FootprintPadPitchTextBox, "шаг площадок", out var footprintPadPitch)
            || !TryReadDouble(Pin1OffsetTextBox, "смещение вывода 1", out var pin1Offset)
            || !TryReadInt(RowCountTextBox, "количество рядов", out var rowCount)
            || !TryReadInt(ColumnCountTextBox, "количество колонок", out var columnCount)
            || !TryReadDouble(PasteReductionTextBox, "уменьшение пасты", out var pasteReduction))
        {
            return;
        }

        _viewModel.PackageName = PackageNameTextBox.Text.Trim();
        _viewModel.DisplayName = DisplayNameTextBox.Text.Trim();
        _viewModel.Description = DescriptionTextBox.Text.Trim();
        _viewModel.Geometry.BodyLength = bodyLength;
        _viewModel.Geometry.BodyWidth = bodyWidth;
        _viewModel.Geometry.BodyHeight = bodyHeight;
        _viewModel.Geometry.CenterX = centerX;
        _viewModel.Geometry.CenterY = centerY;
        _viewModel.Geometry.LeadLength = leadLength;
        _viewModel.Geometry.LeadWidth = leadWidth;
        _viewModel.Geometry.LeadPitch = leadPitch;
        _viewModel.Geometry.LeadCount = geometryLeadCount;
        _viewModel.Geometry.PadLength = padLength;
        _viewModel.Geometry.PadWidth = padWidth;
        _viewModel.Geometry.PadPitch = padPitch;
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
        _viewModel.Footprint.PatternName = PatternNameTextBox.Text.Trim();
        _viewModel.Footprint.StandardName = FootprintStandardNameTextBox.Text.Trim();
        _viewModel.Footprint.Description = FootprintDescriptionTextBox.Text.Trim();
        _viewModel.Footprint.PadCount = footprintPadCount;
        _viewModel.Footprint.PadLength = footprintPadLength;
        _viewModel.Footprint.PadWidth = footprintPadWidth;
        _viewModel.Footprint.PadPitch = footprintPadPitch;
        _viewModel.Footprint.Pin1Offset = pin1Offset;
        _viewModel.Footprint.RowCount = rowCount;
        _viewModel.Footprint.ColumnCount = columnCount;
        _viewModel.Footprint.PasteReduction = pasteReduction;
        _viewModel.Footprint.ApertureType = FootprintApertureTypeTextBox.Text.Trim();

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
