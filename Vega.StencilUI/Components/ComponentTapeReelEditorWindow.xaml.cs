using System.ComponentModel;
using System.Reflection;
using System.Windows;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;

namespace Vega.StencilUI.Components;

public partial class ComponentTapeReelEditorWindow : Window
{
    private readonly TapeEditorViewModel _viewModel;
    public int SavedProfileId { get; private set; }

    public ComponentTapeReelEditorWindow(ComponentTapeReelGeometry profile, bool isEdit)
    {
        InitializeComponent();
        _viewModel = new TapeEditorViewModel(profile, isEdit);
        DataContext = _viewModel;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_viewModel.IsEdit) _viewModel.Service.UpdateProfile(_viewModel.WorkingCopy);
            else _viewModel.Service.CreateProfile(_viewModel.WorkingCopy);
            SavedProfileId = _viewModel.WorkingCopy.Id;
            DialogResult = true;
            Close();
        }
        catch (Exception exception)
        {
            _viewModel.Error = exception.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
}

internal sealed class TapeEditorViewModel : INotifyPropertyChanged
{
    public ComponentTapeReelGeometryService Service { get; } = new();
    public ComponentTapeReelGeometry WorkingCopy { get; }
    public bool IsEdit { get; }
    public Array FeedDirections { get; } = Enum.GetValues<TapeFeedDirection>();
    public Array PocketOrientations { get; } = Enum.GetValues<TapePocketOrientation>();
    public Array VerificationStatuses { get; } = Enum.GetValues<TapeVerificationStatus>();
    private string _error = string.Empty;
    public string Error { get => _error; set { _error = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error))); } }

    public TapeEditorViewModel(ComponentTapeReelGeometry profile, bool isEdit)
    {
        IsEdit = isEdit;
        WorkingCopy = Copy(profile);
    }

    private static ComponentTapeReelGeometry Copy(ComponentTapeReelGeometry source)
    {
        var copy = new ComponentTapeReelGeometry();
        foreach (var property in typeof(ComponentTapeReelGeometry).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead && x.CanWrite))
            property.SetValue(copy, property.GetValue(source));
        return copy;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
