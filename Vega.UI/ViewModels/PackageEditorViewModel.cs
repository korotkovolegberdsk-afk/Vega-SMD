using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vega.UI.ViewModels;

public class PackageEditorViewModel : INotifyPropertyChanged
{

    private string _packageName = "";

    public string PackageName
    {
        get => _packageName;

        set
        {
            _packageName = value;
            OnPropertyChanged();
        }
    }



    private string _displayName = "";

    public string DisplayName
    {
        get => _displayName;

        set
        {
            _displayName = value;
            OnPropertyChanged();
        }
    }



    private double _length;

    public double Length
    {
        get => _length;

        set
        {
            _length = value;
            OnPropertyChanged();
        }
    }



    private double _width;

    public double Width
    {
        get => _width;

        set
        {
            _width = value;
            OnPropertyChanged();
        }
    }



    private double _height;

    public double Height
    {
        get => _height;

        set
        {
            _height = value;
            OnPropertyChanged();
        }
    }



    private string _ipcName = "";

    public string IPCName
    {
        get => _ipcName;

        set
        {
            _ipcName = value;
            OnPropertyChanged();
        }
    }



    private string _aoiRecommendations = "";

    public string AOIRecommendations
    {
        get => _aoiRecommendations;

        set
        {
            _aoiRecommendations = value;
            OnPropertyChanged();
        }
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