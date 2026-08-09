using System.Windows;
using System.Windows.Input;
using Vega.StencilUI.ViewModels;
using Vega.StencilViewer.Models;

namespace Vega.StencilUI.Views;

public partial class StencilViewerWindow : Window
{
    private Point? _lastPanPoint;
    private Point? _selectionStart;
    public StencilViewerWindow(StencilViewDocument document) { InitializeComponent(); DataContext = new StencilViewerWindowViewModel(document); }
    private StencilViewerWindowViewModel? ViewModel => DataContext as StencilViewerWindowViewModel;
    private void PreviewSurface_MouseWheel(object sender, MouseWheelEventArgs e) { if (Keyboard.Modifiers != ModifierKeys.Control) return; ViewModel?.ZoomByMouseWheel(e.Delta); e.Handled = true; }
    private void PreviewSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        PreviewSurface.Focus();
        if (ViewModel?.EditMode == true) { _selectionStart = e.GetPosition(PreviewCanvas); PreviewCanvas.SelectionRectangle = Rect.Empty; PreviewSurface.CaptureMouse(); PreviewSurface.Cursor = Cursors.Cross; e.Handled = true; return; }
        _lastPanPoint = e.GetPosition(PreviewSurface); PreviewSurface.CaptureMouse(); PreviewSurface.Cursor = Cursors.SizeAll; e.Handled = true;
    }
    private void PreviewSurface_MouseMove(object sender, MouseEventArgs e)
    {
        if (_selectionStart is Point start && e.LeftButton == MouseButtonState.Pressed) { PreviewCanvas.SelectionRectangle = new Rect(start, e.GetPosition(PreviewCanvas)); return; }
        if (_lastPanPoint is not Point previous || e.LeftButton != MouseButtonState.Pressed) return;
        var current = e.GetPosition(PreviewSurface); ViewModel?.Pan(current.X - previous.X, current.Y - previous.Y); _lastPanPoint = current;
    }
    private void PreviewSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectionStart is Point start)
        {
            var end = e.GetPosition(PreviewCanvas); var rectangle = new Rect(start, end);
            if (ViewModel is not null)
            {
                if (rectangle.Width < 4 && rectangle.Height < 4) { var primitive = PreviewCanvas.HitTestPrimitive(end); if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) ViewModel.TogglePrimitive(primitive); else ViewModel.SelectSingle(primitive); }
                else ViewModel.SelectRange(PreviewCanvas.GetPrimitivesInRectangle(rectangle, end.X >= start.X));
            }
            _selectionStart = null; PreviewCanvas.SelectionRectangle = Rect.Empty;
        }
        _lastPanPoint = null; PreviewSurface.ReleaseMouseCapture(); PreviewSurface.Cursor = Cursors.Arrow; e.Handled = true;
    }
    private void PreviewSurface_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Q || ViewModel is null) return;
        ViewModel.SelectApertureFromViewer();
        if (ViewModel.SelectedAperture is not null) { ApertureTable.ScrollIntoView(ViewModel.SelectedAperture); ApertureTable.Focus(); }
        e.Handled = true;
    }
}