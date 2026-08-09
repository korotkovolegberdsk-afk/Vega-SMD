using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Vega.StencilUI.ViewModels;

namespace Vega.StencilUI.Views;

public partial class EngineeringWorkspaceView : UserControl
{
    private EngineeringWorkspaceViewModel? _workspace;

    public EngineeringWorkspaceView()
    {
        Debug.WriteLine("[INFO] EngineeringWorkspaceView created");
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_workspace is not null) _workspace.ActionRequested -= OnActionRequested;
        _workspace = e.NewValue as EngineeringWorkspaceViewModel;
        if (_workspace is not null) _workspace.ActionRequested += OnActionRequested;
    }

    private void OnActionRequested(string action)
    {
        if (!string.Equals(action, "OpenPreview", StringComparison.Ordinal) || _workspace?.PreviewDocument is null) return;
        Dispatcher.BeginInvoke(() =>
        {
            var viewer = new StencilViewerWindow(_workspace.PreviewDocument) { Owner = Window.GetWindow(this) };
            viewer.Show();
        });
    }
}