using System.Windows.Input;

namespace Vega.StencilUI.ViewModels;

public sealed class WorkspaceCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public WorkspaceCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}