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





    private string _category = "";

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
        }
    }




    private string _family = "";

    public string Family
    {
        get => _family;
        set
        {
            _family = value;
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




    private string _jedecName = "";

    public string JEDECName
    {
        get => _jedecName;
        set
        {
            _jedecName = value;
            OnPropertyChanged();
        }
    }




    private string _yamahaName = "";

    public string YamahaName
    {
        get => _yamahaName;
        set
        {
            _yamahaName = value;
            OnPropertyChanged();
        }
    }




    private string _mirtecName = "";

    public string MirtecName
    {
        get => _mirtecName;
        set
        {
            _mirtecName = value;
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





    private double _pitch;

    public double Pitch
    {
        get => _pitch;
        set
        {
            _pitch = value;
            OnPropertyChanged();
        }
    }





    private int _leads;

    public int Leads
    {
        get => _leads;
        set
        {
            _leads = value;
            OnPropertyChanged();
        }
    }





    private double _stencilThickness;

    public double StencilThickness
    {
        get => _stencilThickness;
        set
        {
            _stencilThickness = value;
            OnPropertyChanged();
        }
    }





    private double _areaRatio;

    public double AreaRatio
    {
        get => _areaRatio;
        set
        {
            _areaRatio = value;
            OnPropertyChanged();
        }
    }





    private double _aspectRatio;

    public double AspectRatio
    {
        get => _aspectRatio;
        set
        {
            _aspectRatio = value;
            OnPropertyChanged();
        }
    }





    private string _aperture = "";

    public string Aperture
    {
        get => _aperture;
        set
        {
            _aperture = value;
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





    private string _spiRecommendations = "";

    public string SPIRecommendations
    {
        get => _spiRecommendations;
        set
        {
            _spiRecommendations = value;
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