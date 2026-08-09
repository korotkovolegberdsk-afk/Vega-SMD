using System.Windows;
using System.Windows.Media;
using Vega.Models.MasterLibrary;

namespace Vega.StencilUI.Controls;
public sealed class PackageDrawingPreview : FrameworkElement
{
    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(nameof(Package), typeof(PackageDefinition), typeof(PackageDrawingPreview), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public PackageDefinition? Package { get => (PackageDefinition?)GetValue(PackageProperty); set => SetValue(PackageProperty,value); }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc); var p=Package; if(p is null) return; var w=ActualWidth; var h=ActualHeight; var pen=new Pen(Brushes.SteelBlue,1.5); var bodyW=Math.Max(70,w*.45); var bodyH=Math.Max(50,h*.38); var x=(w-bodyW)/2; var y=(h-bodyH)/2;
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(45,65,80)),pen,new Rect(x,y,bodyW,bodyH),4,4);
        var family=p.PackageFamily.ToUpperInvariant(); var pads=Math.Max(2,p.LeadCount>0?p.LeadCount:p.PadCount);
        if(family is "QFN" or "QFP" or "BGA") for(var i=0;i<pads;i++){ var angle=2*Math.PI*i/pads; var px=x+bodyW/2+Math.Cos(angle)*(bodyW/2+8); var py=y+bodyH/2+Math.Sin(angle)*(bodyH/2+8); dc.DrawEllipse(Brushes.Gold,null,new Point(px,py),3,3); }
        else for(var i=0;i<pads;i++){ var py=y+10+(bodyH-20)*i/Math.Max(1,pads-1); dc.DrawRectangle(Brushes.Gold,null,new Rect(x-8,py-2,6,4)); dc.DrawRectangle(Brushes.Gold,null,new Rect(x+bodyW+2,py-2,6,4)); }
        if(family=="QFN" && p.ThermalPadLength>0) dc.DrawRectangle(Brushes.IndianRed,null,new Rect(x+bodyW*.3,y+bodyH*.3,bodyW*.4,bodyH*.4));
        var text=new FormattedText($"A={p.BodyLength:0.000}  B={p.BodyWidth:0.000}  H={p.Height:0.000}\nP={p.Pitch:0.000}  L={p.LeadLength:0.000}  W={p.LeadWidth:0.000}\nE={p.ThermalPadLength:0.000}×{p.ThermalPadWidth:0.000} mm",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),12,Brushes.White,VisualTreeHelper.GetDpi(this).PixelsPerDip); dc.DrawText(text,new Point(10,10));
    }
}