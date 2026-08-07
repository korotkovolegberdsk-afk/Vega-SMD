using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Vega.Models.Packages;
using Vega.Services.Library;

namespace Vega.UI.Views;

public partial class PackageEditorWindow : Window
{
    private readonly PackageService _service;
    private readonly PackageSearchResult? _editingPackage;

    public PackageEditorWindow()
    {
        InitializeComponent();
        _service = new PackageService();
    }

    public PackageEditorWindow(PackageSearchResult package)
        : this()
    {
        _editingPackage = package;
        LoadPackage(package);
    }

    private void LoadPackage(PackageSearchResult package)
    {
        Title = $"Редактор корпуса — {package.PackageName}";
        PackageNameTextBox.Text = package.PackageName;
        DisplayNameTextBox.Text = package.DisplayName;
        DescriptionTextBox.Text = package.Notes;
        StandardTextBox.Text = package.IPCName;
        LengthTextBox.Text = FormatNumber(package.Length);
        WidthTextBox.Text = FormatNumber(package.Width);
        HeightTextBox.Text = FormatNumber(package.Height);
        PitchTextBox.Text = FormatNumber(package.Pitch);
        LeadCountTextBox.Text = package.LeadCount.ToString(CultureInfo.CurrentCulture);
        IPCNameTextBox.Text = package.IPCName;
        JEDECNameTextBox.Text = package.JEDECName;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var packageName = PackageNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(packageName))
        {
            ShowValidationError("Укажите имя корпуса.", PackageNameTextBox);
            return;
        }

        if (!TryReadDouble(LengthTextBox, "длину корпуса", out var length) ||
            !TryReadDouble(WidthTextBox, "ширину корпуса", out var width) ||
            !TryReadDouble(HeightTextBox, "высоту корпуса", out var height) ||
            !TryReadDouble(PitchTextBox, "шаг выводов", out var pitch) ||
            !TryReadInt(LeadCountTextBox, "количество выводов", out var leadCount))
        {
            return;
        }

        var package = _editingPackage ?? new PackageSearchResult();
        package.PackageName = packageName;
        package.DisplayName = DisplayNameTextBox.Text.Trim();
        package.Notes = DescriptionTextBox.Text.Trim();
        package.Length = length;
        package.Width = width;
        package.Height = height;
        package.Pitch = pitch;
        package.LeadCount = leadCount;
        package.IPCName = string.IsNullOrWhiteSpace(IPCNameTextBox.Text)
            ? StandardTextBox.Text.Trim()
            : IPCNameTextBox.Text.Trim();
        package.JEDECName = JEDECNameTextBox.Text.Trim();

        if (_editingPackage is null)
            _service.AddPackage(package);
        else
            _service.UpdatePackage(package);

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
        var parsed = double.TryParse(text, styles, CultureInfo.CurrentCulture, out value) ||
                     double.TryParse(text, styles, CultureInfo.InvariantCulture, out value);

        if (parsed && value >= 0)
            return true;

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

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value) &&
            value >= 0)
            return true;

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

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}