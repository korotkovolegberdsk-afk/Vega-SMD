using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vega.StencilUI.Models;

public sealed class EditableAperture : INotifyPropertyChanged
{
    private string _shape = "Rectangle";
    private double _width;
    private double _height;
    private double _radius;
    public int ApertureId { get; init; }
    public string Shape { get => _shape; set => SetField(ref _shape, value); }
    public double Width { get => _width; set => SetField(ref _width, value); }
    public double Height { get => _height; set => SetField(ref _height, value); }
    public double Radius { get => _radius; set => SetField(ref _radius, value); }
    public double X { get; init; }
    public double Y { get; init; }
    public double Rotation { get; init; }
    public int UsageCount { get; init; }
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; PropertyChanged?.Invoke(this, new(name)); return true; }
}

public sealed class ModifiedPasteLayer
{
    public ObservableCollection<EditableAperture> Apertures { get; } = [];
}