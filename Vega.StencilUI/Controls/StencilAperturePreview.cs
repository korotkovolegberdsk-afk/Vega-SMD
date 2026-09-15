using System.Windows;
using System.Windows.Media;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Vega.StencilUI.PackageDrawing;

namespace Vega.StencilUI.Controls;

/// <summary>Read-only aperture preview derived from package contacts and the selected stencil rule.</summary>
public sealed class StencilAperturePreview : FrameworkElement
{
    public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(
        nameof(Package), typeof(PackageDefinition), typeof(StencilAperturePreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RuleProperty = DependencyProperty.Register(
        nameof(Rule), typeof(StencilTechnologyRule), typeof(StencilAperturePreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty FootprintProperty = DependencyProperty.Register(
        nameof(Footprint), typeof(ComponentFootprint), typeof(StencilAperturePreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public PackageDefinition? Package
    {
        get => (PackageDefinition?)GetValue(PackageProperty);
        set => SetValue(PackageProperty, value);
    }

    public StencilTechnologyRule? Rule
    {
        get => (StencilTechnologyRule?)GetValue(RuleProperty);
        set => SetValue(RuleProperty, value);
    }
    public ComponentFootprint? Footprint { get => (ComponentFootprint?)GetValue(FootprintProperty); set => SetValue(FootprintProperty, value); }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        // The preview occupies the fixed 205-unit illustration row.  During a
        // layout pass WPF can momentarily retain a taller drawing context; do
        // not let that stale background overflow into the text row below.
        dc.DrawRectangle(ComponentCardPalette.Panel, null, new Rect(0, 0, RenderSize.Width, Math.Min(RenderSize.Height, 205)));
        if (Package is null || Rule is null || ActualWidth < 40 || ActualHeight < 40) return;
        if (Sot723PackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 3 }) { DrawSot723Footprint(dc); return; }
        if (Sot563PackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 6 }) { DrawSot563Footprint(dc,Footprint); return; }
        if (SotPowerPackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 3 or 4 }) { DrawSotPowerFootprint(dc,Package.PackageName); return; }
        if (DpakPackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 3 or 4 }) { DrawDpakFootprint(dc,Package.PackageName); return; }
        if (SoicPackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 8 }) { DrawSoic8Footprint(dc,Footprint); return; }
        if (Soic16PackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 8 or 14 or 16 or 18 or 20 or 24 or 28 }) { DrawSoic16Footprint(dc,Footprint,Package.PackageName); return; }
        if (Qfp32PackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 32 }) { DrawQfp32Footprint(dc,Footprint); return; }
        if (Qfn32PackageDrawing.Supports(Package.PackageName) && Footprint is { PadCount: 33 }) { DrawQfn32Footprint(dc,Footprint); return; }
        if (Footprint is { PadCount: 2, PadLength: > 0, PadWidth: > 0 }) { DrawTwoPadFootprint(dc, Footprint); return; }
        if (Footprint is { PadCount: 3, PadLength: > 0, PadWidth: > 0 } && string.Equals(Package.PackageName,"SOT23",StringComparison.OrdinalIgnoreCase)) { DrawSot23Footprint(dc,Footprint); return; }
        if (Footprint is { PadCount: 5 or 6, PadLength: > 0, PadWidth: > 0 } && Sot23MultiLeadPackageDrawing.Supports(Package.PackageName)) { DrawSot23MultiLeadFootprint(dc,Footprint); return; }
        if (Footprint is { PadCount: 3 or 5 or 6, PadLength: > 0, PadWidth: > 0 } && SotMicroPackageDrawing.Supports(Package.PackageName)) { DrawSotMicroFootprint(dc,Footprint); return; }
        if (Footprint is { PadCount: 3, PadLength: > 0, PadWidth: > 0 } && Sot523PackageDrawing.Supports(Package.PackageName)) { DrawSot523Footprint(dc,Footprint); return; }
        if (string.Equals(Package.PackageName, "C0402", StringComparison.OrdinalIgnoreCase))
        {
            DrawC0402Reference(dc);
            return;
        }

        // A package outline is not an MPN footprint.  Do not invent aperture geometry
        // from a parametric body when the selected part has no verified source data.
        // The card presents that state as text below this area instead.
        return;
    }

    private void DrawTwoPadFootprint(DrawingContext dc, ComponentFootprint footprint)
    {
        var fill = ComponentCardPalette.Aperture; var outline = new Pen(Brushes.Black, .8);
        var scale = ScaleForPad(footprint.PadLength, footprint.PadWidth);
        var width = footprint.PadLength * scale; var height = footprint.PadWidth * scale; var y = CenteredPadTop(height);
        var left = ActualWidth * .27 - width / 2; var right = ActualWidth * .73 - width / 2;
        var culture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        DrawC0402Pad(dc, new Rect(left, y, width, height), fill, outline, footprint.PadLength.ToString("0.00", culture), footprint.PadWidth.ToString("0.00", culture));
        DrawC0402Pad(dc, new Rect(right, y, width, height), fill, outline, footprint.PadLength.ToString("0.00", culture), footprint.PadWidth.ToString("0.00", culture), true);
    }

    private void DrawSot23Footprint(DrawingContext dc,ComponentFootprint footprint)
    {
        var fill=ComponentCardPalette.Aperture;var outline=new Pen(Brushes.Black,.8);const double scale=59;
        // Keep the upper dimension label fully below the 64 px window title,
        // while retaining all three pads inside the 205 px drawing area.
        var w=footprint.PadLength*scale;var h=footprint.PadWidth*scale;var cx=ActualWidth/2;var cy=Math.Max(140,ActualHeight*.58);
        var pads=new[]{new Rect(cx-.95*scale-w/2,cy+h*.35,w,h),new Rect(cx+.95*scale-w/2,cy+h*.35,w,h),new Rect(cx-w/2,cy-h*1.35,w,h)};
        foreach(var pad in pads.Take(2))dc.DrawRoundedRectangle(fill,outline,pad,7,7);
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        DrawC0402Pad(dc,pads[2],fill,outline,footprint.PadLength.ToString("0.00",culture),footprint.PadWidth.ToString("0.00",culture),true);
    }

    private void DrawSot23MultiLeadFootprint(DrawingContext dc, ComponentFootprint footprint)
    {
        var fill=ComponentCardPalette.Aperture; var outline=new Pen(Brushes.Black,.8); const double scale=47;
        var w=footprint.PadLength*scale; var h=footprint.PadWidth*scale; var cx=ActualWidth/2; var cy=Math.Max(142,ActualHeight*.59);
        var xs=new[]{-.95,0d,.95};
        foreach(var x in xs)
        {
            dc.DrawRoundedRectangle(fill,outline,new Rect(cx+x*scale-w/2,cy+scale*.72-h/2,w,h),6,6);
            if(footprint.PadCount==6 || x!=0) dc.DrawRoundedRectangle(fill,outline,new Rect(cx+x*scale-w/2,cy-scale*.72-h/2,w,h),6,6);
        }
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        var referencePad=new Rect(cx-.95*scale-w/2,cy-scale*.72-h/2,w,h);
        DrawC0402Pad(dc,referencePad,Brushes.Transparent,new Pen(Brushes.Transparent,0),footprint.PadLength.ToString("0.00",culture),footprint.PadWidth.ToString("0.00",culture));
    }

    private void DrawSotMicroFootprint(DrawingContext dc, ComponentFootprint footprint)
    {
        var fill=ComponentCardPalette.Aperture; var outline=new Pen(Brushes.Black,.8); const double scale=63;
        var packageName=Package!.PackageName; var is323=packageName.Equals("SOT323",StringComparison.OrdinalIgnoreCase);
        var is353=packageName.Equals("SOT353",StringComparison.OrdinalIgnoreCase); var cx=ActualWidth/2; var cy=Math.Max(143,ActualHeight*.59);
        var xs=new[]{-.65,0d,.65}; var rowY=.66*scale;
        void Pad(double x,double y,double widthMm)
        {
            var w=.60*scale; var h=widthMm*scale;
            dc.DrawRoundedRectangle(fill,outline,new Rect(cx+x*scale-w/2,cy+y-h/2,w,h),5,5);
        }
        if(is323)
        {
            Pad(-.65,rowY,.60); Pad(.65,rowY,.60); Pad(0,-rowY,.60);
        }
        else
        {
            Pad(-.65,rowY,.60); Pad(0,rowY,.40); Pad(.65,rowY,.60);
            Pad(-.65,-rowY,.60); if(!is353) Pad(0,-rowY,.40); Pad(.65,-rowY,.60);
        }
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        var reference=new Rect(cx-.65*scale-.30*scale,cy-rowY-.30*scale,.60*scale,.60*scale);
        DrawC0402Pad(dc,reference,Brushes.Transparent,new Pen(Brushes.Transparent,0),.60.ToString("0.00",culture),.60.ToString("0.00",culture));
    }

    private void DrawSot723Footprint(DrawingContext dc)
    {
        const double s=90, cy=138; var cx=ActualWidth/2;
        var fill=ComponentCardPalette.Aperture; var pen=new Pen(Brushes.Black,.8);
        var top=new Rect(cx-.21*s,cy-.5*s-.15*s,.42*s,.30*s);
        var left=new Rect(cx-.4*s-.16*s,cy+.5*s-.15*s,.32*s,.30*s);
        var right=new Rect(cx+.4*s-.16*s,cy+.5*s-.15*s,.32*s,.30*s);
        foreach(var p in new[]{top,left,right}) dc.DrawRoundedRectangle(fill,pen,p,3,3);
        DrawC0402Pad(dc,top,Brushes.Transparent,new Pen(Brushes.Transparent,0),"0,42","0,30",true);
        var tf=new Typeface("Arial");
        var text=new FormattedText("0,32",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,24,ComponentCardPalette.Dimension,1);
        var y=left.Top-10;
        dc.DrawLine(new Pen(ComponentCardPalette.Dimension,1),new Point(left.Left,y),new Point(left.Right,y));
        dc.DrawLine(new Pen(ComponentCardPalette.Dimension,1),new Point(left.Left,y-5),new Point(left.Left,left.Top));
        dc.DrawLine(new Pen(ComponentCardPalette.Dimension,1),new Point(left.Right,y-5),new Point(left.Right,left.Top));
        DrawArrow(dc,new Point(left.Left,y),true); DrawArrow(dc,new Point(left.Right,y),false);
        dc.DrawText(text,new Point(left.Left+(left.Width-text.Width)/2,y-text.Height-6));
    }

    private void DrawSot563Footprint(DrawingContext dc, ComponentFootprint footprint)
    {
        const double scale=60, cy=140;
        var cx=ActualWidth/2; var w=footprint.PadLength*scale; var h=footprint.PadWidth*scale;
        var fill=ComponentCardPalette.Aperture; var pen=new Pen(Brushes.Black,.8);
        foreach(var x in new[]{-.5,0d,.5}) foreach(var y in new[]{-.635,.635})
            dc.DrawRoundedRectangle(fill,pen,new Rect(cx+x*scale-w/2,cy+y*scale-h/2,w,h),3,3);
        var reference=new Rect(cx-.5*scale-w/2,cy-.635*scale-h/2,w,h);
        DrawC0402Pad(dc,reference,Brushes.Transparent,new Pen(Brushes.Transparent,0),"0,30","0,67");
    }

    private void DrawSotPowerFootprint(DrawingContext dc, string packageName)
    {
        var fill=ComponentCardPalette.Aperture; var outline=new Pen(Brushes.Black,.8);
        var cx=ActualWidth/2;
        if(packageName.Equals("SOT223",StringComparison.OrdinalIgnoreCase))
        {
            const double scale=14, cy=128;
            var upper=new Rect(cx-1.65*scale,cy-4.0*scale,3.30*scale,1.60*scale);
            dc.DrawRoundedRectangle(fill,outline,upper,3,3);
            foreach(var x in new[]{-2.30,0d,2.30})
                dc.DrawRoundedRectangle(fill,outline,new Rect(cx+(x-.60)*scale,cy+2.40*scale,1.20*scale,1.60*scale),3,3);
            DrawC0402Pad(dc,upper,Brushes.Transparent,new Pen(Brushes.Transparent,0),"3,30","1,60",true);
            var sample=new Rect(cx-2.90*scale,cy+2.40*scale,1.20*scale,1.60*scale);
            DrawWidthAbove(dc,sample,"1,20",151);
            return;
        }

        const double s=24, centerY=126;
        var left=new Rect(cx-1.79*s,centerY+.635*s,.58*s,1.63*s);
        var right=new Rect(cx+1.21*s,centerY+.635*s,.58*s,1.63*s);
        dc.DrawRoundedRectangle(fill,outline,left,3,3);
        dc.DrawRoundedRectangle(fill,outline,right,3,3);
        var tShape=new StreamGeometry();
        using(var g=tShape.Open())
        {
            g.BeginFigure(new Point(cx-.9665*s,centerY-2.265*s),true,true);
            g.LineTo(new Point(cx+.9665*s,centerY-2.265*s),true,false);
            g.LineTo(new Point(cx+.9665*s,centerY+.765*s),true,false);
            g.LineTo(new Point(cx+.38*s,centerY+.765*s),true,false);
            g.LineTo(new Point(cx+.38*s,centerY+2.265*s),true,false);
            g.LineTo(new Point(cx-.38*s,centerY+2.265*s),true,false);
            g.LineTo(new Point(cx-.38*s,centerY+.765*s),true,false);
            g.LineTo(new Point(cx-.9665*s,centerY+.765*s),true,false);
        }
        dc.DrawGeometry(fill,outline,tShape);
        var topReference=new Rect(cx-.9665*s,centerY-2.265*s,1.933*s,3.03*s);
        DrawC0402Pad(dc,topReference,Brushes.Transparent,new Pen(Brushes.Transparent,0),"1,93","3,03",true);
        DrawWidthAbove(dc,left,"0,58",134);
    }

    private void DrawDpakFootprint(DrawingContext dc, string packageName)
    {
        var isD2=packageName.Equals("D2PAK",StringComparison.OrdinalIgnoreCase);
        var fill=ComponentCardPalette.Aperture; var outline=new Pen(Brushes.Black,.8);
        var thermalMm=isD2 ? new Size(9.75,12.20) : new Size(6.70,6.70);
        var leadMm=isD2 ? new Size(1.60,3.50) : new Size(1.60,3.00);
        var rows=isD2?3:2; var columns=isD2?3:2; var scale=isD2?6.5:10.5;
        var cx=ActualWidth*.46;
        var thermal=new Rect(cx-thermalMm.Width*scale/2,64,thermalMm.Width*scale,thermalMm.Height*scale);
        var group=new GeometryGroup(); var gap=4d;
        var cellWidth=(thermal.Width-gap*(columns-1))/columns; var cellHeight=(thermal.Height-gap*(rows-1))/rows;
        for(var row=0;row<rows;row++) for(var column=0;column<columns;column++)
            group.Children.Add(new RectangleGeometry(new Rect(thermal.Left+column*(cellWidth+gap),thermal.Top+row*(cellHeight+gap),cellWidth,cellHeight),3,3));
        dc.DrawGeometry(fill,outline,group);

        var leadCount=isD2?3:2; var leadWidth=leadMm.Width*scale; var leadHeight=leadMm.Height*scale;
        var pitch=(isD2?5.45:2.28)*scale; var leadTop=thermal.Bottom+(isD2?18:16);
        var sample=Rect.Empty;
        for(var i=0;i<leadCount;i++)
        {
            var index=isD2?i-1:i*2-1;
            var pad=new Rect(cx+index*pitch-leadWidth/2,leadTop,leadWidth,leadHeight);
            dc.DrawRoundedRectangle(fill,outline,pad,4,4); if(i==leadCount-1)sample=pad;
        }
        DrawCompactDimensions(dc,thermal,isD2?"9,75":"6,70",isD2?"12,20":"6,70",false);
        DrawLeadDimensions(dc,sample,"1,60",isD2?"3,50":"3,00");
    }

    private void DrawSoic8Footprint(DrawingContext dc,ComponentFootprint footprint)
    {
        const double scale=25;var cx=ActualWidth/2;const double cy=131;
        var fill=ComponentCardPalette.Aperture;var outline=new Pen(Brushes.Black,.8);
        var padW=footprint.PadLength*scale;var padH=footprint.PadWidth*scale;var rowX=4.612*scale/2;
        var sample=Rect.Empty;
        for(var row=0;row<4;row++)
        {
            var y=cy+(row-1.5)*footprint.PadPitch*scale-padH/2;
            foreach(var side in new[]{-1d,1d})
            {
                var pad=new Rect(cx+side*rowX-padW/2,y,padW,padH);
                dc.DrawRoundedRectangle(fill,outline,pad,4,4);
                if(row==0&&side<0)sample=pad;
            }
        }
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        DrawC0402Pad(dc,sample,Brushes.Transparent,new Pen(Brushes.Transparent,0),
            footprint.PadLength.ToString("0.000",culture),footprint.PadWidth.ToString("0.000",culture));
    }

    private void DrawSoic16Footprint(DrawingContext dc,ComponentFootprint footprint,string packageName)
    {
        var padsPerSide=footprint.PadCount/2;
        var maximumScale=padsPerSide<=4?25d:13d;
        var scale=Math.Min(maximumScale,118/((padsPerSide-1)*footprint.PadPitch+footprint.PadWidth));
        var cx=ActualWidth/2;const double cy=135;
        var fill=ComponentCardPalette.Aperture;var outline=new Pen(Brushes.Black,.8);
        var rowCenter=packageName.ToUpperInvariant() switch
        {
            "SO14P127W60"=>5.90,
            "SO18P127W103"=>9.30,
            "SO20P127W103"=>9.50,
            "SO24P127W103"=>8.80,
            "SO16P127W76"=>6.50,
            "SO16P127W103"=>9.30,
            "SSOP08P065W43"=>3.80,
            "SSOP16P065W78"=>7.00,
            "SSOP28P065W78"=>7.00,
            "TSSOP08P065W64" or "TSSOP14P065W64" or "TSSOP16P065W64" or "TSSOP20P065W64" or "TSSOP24P065W64" or "TSSOP28P065W64"=>5.80,
            _=>4.95
        };
        var padW=footprint.PadLength*scale;var padH=footprint.PadWidth*scale;var rowX=rowCenter*scale/2;
        var sample=Rect.Empty;
        var center=(padsPerSide-1)/2d;
        for(var row=0;row<padsPerSide;row++)
        {
            var y=cy+(row-center)*footprint.PadPitch*scale-padH/2;
            foreach(var side in new[]{-1d,1d})
            {
                var pad=new Rect(cx+side*rowX-padW/2,y,padW,padH);
                dc.DrawRoundedRectangle(fill,outline,pad,3,3);
                if(row==0&&side<0)sample=pad;
            }
        }
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        DrawC0402Pad(dc,sample,Brushes.Transparent,new Pen(Brushes.Transparent,0),footprint.PadLength.ToString("0.00",culture),footprint.PadWidth.ToString("0.00",culture));
    }

    private void DrawQfp32Footprint(DrawingContext dc, ComponentFootprint footprint)
    {
        const int pinsPerSide=8; var scale=Math.Min(17d,96/((pinsPerSide-1)*footprint.PadPitch+footprint.PadWidth));
        // Four perimeter rows need a higher centre than a two-sided package so
        // the bottom row remains inside the 205 px aperture drawing area.
        var cx=ActualWidth/2;const double cy=112;var fill=ComponentCardPalette.Aperture;var outline=new Pen(Brushes.Black,.8);
        var halfSpan=4.35*scale;var longSide=footprint.PadLength*scale;var shortSide=footprint.PadWidth*scale;var center=(pinsPerSide-1)/2d;var sample=Rect.Empty;
        for(var i=0;i<pinsPerSide;i++)
        {
            var offset=(i-center)*footprint.PadPitch*scale;
            var left=new Rect(cx-halfSpan-longSide/2,cy+offset-shortSide/2,longSide,shortSide);
            var right=new Rect(cx+halfSpan-longSide/2,cy+offset-shortSide/2,longSide,shortSide);
            var top=new Rect(cx+offset-shortSide/2,cy-halfSpan-longSide/2,shortSide,longSide);
            var bottom=new Rect(cx+offset-shortSide/2,cy+halfSpan-longSide/2,shortSide,longSide);
            foreach(var pad in new[]{left,right,top,bottom})dc.DrawRoundedRectangle(fill,outline,pad,3,3);
            // The outer lower pad leaves room for both labels away from the
            // aperture array, without crossing another row of pads.
            if(i==pinsPerSide-1)sample=bottom;
        }
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        DrawLeadDimensions(dc,sample,footprint.PadWidth.ToString("0.00",culture),footprint.PadLength.ToString("0.00",culture));
    }

    private void DrawQfn32Footprint(DrawingContext dc, ComponentFootprint footprint)
    {
        const int pinsPerSide=8; const double scale=23, thermal=3.45, window=.89;
        var fill=ComponentCardPalette.Aperture; var outline=new Pen(Brushes.Black,.8); var cx=ActualWidth/2; const double cy=109;
        var outer=5.30*scale; var center=(pinsPerSide-1)/2d;
        Rect Pad(double x,double y,double width,double height)=>new(x-width/2,y-height/2,width,height);
        var radial=footprint.PadLength*scale; var tangential=footprint.PadWidth*scale;
        Rect sample=default;
        for(var i=0;i<pinsPerSide;i++)
        {
            var p=(i-center)*footprint.PadPitch*scale;
            var left=Pad(cx-outer/2+radial/2,cy+p,radial,tangential); var right=Pad(cx+outer/2-radial/2,cy+p,radial,tangential);
            var top=Pad(cx+p,cy-outer/2+radial/2,tangential,radial); var bottom=Pad(cx+p,cy+outer/2-radial/2,tangential,radial);
            dc.DrawRoundedRectangle(fill,outline,left,2,2);dc.DrawRoundedRectangle(fill,outline,right,2,2);dc.DrawRoundedRectangle(fill,outline,top,2,2);dc.DrawRoundedRectangle(fill,outline,bottom,2,2);
            // Measure a terminal in the centre of the right side: the labels
            // stay in the open margin instead of crossing the WindowPane grid.
            if(i==pinsPerSide/2) sample=right;
        }
        var startX=cx-thermal*scale/2+window*scale/2;
        var startY=cy-thermal*scale/2+window*scale/2;
        var windowStep=(thermal*scale-window*scale)/2;
        for(var row=0;row<3;row++) for(var col=0;col<3;col++)
            dc.DrawRoundedRectangle(fill,outline,Pad(startX+col*windowStep,startY+row*windowStep,window*scale,window*scale),2,2);
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        DrawQfnLeadDimensions(dc,sample,footprint.PadLength.ToString("0.00",culture),footprint.PadWidth.ToString("0.00",culture));
    }

    private static void DrawQfnLeadDimensions(DrawingContext dc, Rect pad, string lengthText, string widthText)
    {
        var pen=new Pen(ComponentCardPalette.Dimension,1); var tf=new Typeface("Arial");
        var length=new FormattedText(lengthText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,22,ComponentCardPalette.Dimension,1);
        var width=new FormattedText(widthText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,22,ComponentCardPalette.Dimension,1);
        // Keep the two annotations in deliberately open areas: above the right
        // terminal and to the right of the full aperture field.
        const double y=70;
        dc.DrawLine(pen,new Point(pad.Left,pad.Top),new Point(pad.Left,y+4)); dc.DrawLine(pen,new Point(pad.Right,pad.Top),new Point(pad.Right,y+4));
        dc.DrawLine(pen,new Point(pad.Left,y),new Point(pad.Right,y)); DrawArrow(dc,new Point(pad.Left,y),true); DrawArrow(dc,new Point(pad.Right,y),false);
        dc.DrawText(length,new Point(pad.Left+(pad.Width-length.Width)/2,y-length.Height-3));
        var x=pad.Right+72;
        dc.DrawLine(pen,new Point(pad.Right,pad.Top),new Point(x-6,pad.Top)); dc.DrawLine(pen,new Point(pad.Right,pad.Bottom),new Point(x-6,pad.Bottom));
        dc.DrawLine(pen,new Point(x,pad.Top),new Point(x,pad.Bottom)); DrawArrow(dc,new Point(x,pad.Top),true,true); DrawArrow(dc,new Point(x,pad.Bottom),false,true);
        var labelX=x+7+width.Height/2; var labelY=pad.Top+pad.Height/2;
        dc.PushTransform(new RotateTransform(-90,labelX,labelY)); dc.DrawText(width,new Point(labelX-width.Width/2,labelY-width.Height/2)); dc.Pop();
    }

    private static void DrawLeadDimensions(DrawingContext dc,Rect pad,string widthText,string heightText)
    {
        var pen=new Pen(ComponentCardPalette.Dimension,1); var tf=new Typeface("Arial");
        var width=new FormattedText(widthText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,22,ComponentCardPalette.Dimension,1);
        var height=new FormattedText(heightText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,22,ComponentCardPalette.Dimension,1);
        var y=pad.Top-10;
        dc.DrawLine(pen,new Point(pad.Left,pad.Top),new Point(pad.Left,y-4)); dc.DrawLine(pen,new Point(pad.Right,pad.Top),new Point(pad.Right,y-4));
        dc.DrawLine(pen,new Point(pad.Left,y),new Point(pad.Right,y)); DrawArrow(dc,new Point(pad.Left,y),true); DrawArrow(dc,new Point(pad.Right,y),false);
        dc.DrawLine(pen,new Point(pad.Right+5,y),new Point(pad.Right+105,y));
        dc.DrawText(width,new Point(pad.Right+38,y-width.Height-5));
        var x=pad.Right+20;
        dc.DrawLine(pen,new Point(pad.Right,pad.Top),new Point(x+5,pad.Top)); dc.DrawLine(pen,new Point(pad.Right,pad.Bottom),new Point(x+5,pad.Bottom));
        dc.DrawLine(pen,new Point(x,pad.Top),new Point(x,pad.Bottom)); DrawArrow(dc,new Point(x,pad.Top),true,true); DrawArrow(dc,new Point(x,pad.Bottom),false,true);
        var labelX=x+7+height.Height/2; var labelY=pad.Top+pad.Height/2;
        dc.PushTransform(new RotateTransform(-90,labelX,labelY)); dc.DrawText(height,new Point(labelX-height.Width/2,labelY-height.Height/2)); dc.Pop();
    }

    private static void DrawCompactDimensions(DrawingContext dc,Rect pad,string widthText,string heightText,bool rightSide)
    {
        var pen=new Pen(ComponentCardPalette.Dimension,1); var tf=new Typeface("Arial");
        var width=new FormattedText(widthText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,22,ComponentCardPalette.Dimension,1);
        var height=new FormattedText(heightText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,22,ComponentCardPalette.Dimension,1);
        var y=Math.Max(40,pad.Top-13);
        dc.DrawLine(pen,new Point(pad.Left,pad.Top),new Point(pad.Left,y-4)); dc.DrawLine(pen,new Point(pad.Right,pad.Top),new Point(pad.Right,y-4));
        dc.DrawLine(pen,new Point(pad.Left,y),new Point(pad.Right,y)); DrawArrow(dc,new Point(pad.Left,y),true); DrawArrow(dc,new Point(pad.Right,y),false);
        dc.DrawText(width,new Point(pad.Left+(pad.Width-width.Width)/2,y-width.Height-3));
        var x=rightSide?pad.Right+28:pad.Left-28; var edge=rightSide?pad.Right:pad.Left; var ext=rightSide?x+5:x-5;
        dc.DrawLine(pen,new Point(edge,pad.Top),new Point(ext,pad.Top)); dc.DrawLine(pen,new Point(edge,pad.Bottom),new Point(ext,pad.Bottom));
        dc.DrawLine(pen,new Point(x,pad.Top),new Point(x,pad.Bottom)); DrawArrow(dc,new Point(x,pad.Top),true,true); DrawArrow(dc,new Point(x,pad.Bottom),false,true);
        var labelX=x+(rightSide?1:-1)*(7+height.Height/2); var labelY=pad.Top+pad.Height/2;
        dc.PushTransform(new RotateTransform(-90,labelX,labelY)); dc.DrawText(height,new Point(labelX-height.Width/2,labelY-height.Height/2)); dc.Pop();
    }

    private static void DrawWidthBelow(DrawingContext dc, Rect pad, string value, double y)
    {
        var pen=new Pen(ComponentCardPalette.Dimension,1); var tf=new Typeface("Arial");
        var text=new FormattedText(value,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,24,ComponentCardPalette.Dimension,1);
        dc.DrawLine(pen,new Point(pad.Left,y),new Point(pad.Right,y));
        dc.DrawLine(pen,new Point(pad.Left,pad.Bottom),new Point(pad.Left,y+6));
        dc.DrawLine(pen,new Point(pad.Right,pad.Bottom),new Point(pad.Right,y+6));
        DrawArrow(dc,new Point(pad.Left,y),true); DrawArrow(dc,new Point(pad.Right,y),false);
        dc.DrawText(text,new Point(pad.Left+(pad.Width-text.Width)/2,y+5));
    }

    private static void DrawWidthAbove(DrawingContext dc, Rect pad, string value, double y)
    {
        var pen=new Pen(ComponentCardPalette.Dimension,1); var tf=new Typeface("Arial");
        var text=new FormattedText(value,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,tf,24,ComponentCardPalette.Dimension,1);
        dc.DrawLine(pen,new Point(pad.Left,y),new Point(pad.Right,y));
        dc.DrawLine(pen,new Point(pad.Left,y-5),new Point(pad.Left,pad.Top));
        dc.DrawLine(pen,new Point(pad.Right,y-5),new Point(pad.Right,pad.Top));
        DrawArrow(dc,new Point(pad.Left,y),true); DrawArrow(dc,new Point(pad.Right,y),false);
        dc.DrawText(text,new Point(pad.Left+(pad.Width-text.Width)/2,y-text.Height-6));
    }

    private void DrawSot523Footprint(DrawingContext dc, ComponentFootprint footprint)
    {
        var fill=ComponentCardPalette.Aperture; var outline=new Pen(Brushes.Black,.8); const double scale=75;
        // Keep the complete two-row land pattern inside the 205 px drawing row.
        // Overflow from this element would otherwise cover the text row below.
        var w=footprint.PadLength*scale; var h=footprint.PadWidth*scale; var cx=ActualWidth/2; const double cy=132;
        var pads=new[]
        {
            new Rect(cx-.50*scale-w/2,cy+.645*scale-h/2,w,h),
            new Rect(cx+.50*scale-w/2,cy+.645*scale-h/2,w,h),
            new Rect(cx-w/2,cy-.645*scale-h/2,w,h)
        };
        foreach(var pad in pads) dc.DrawRoundedRectangle(fill,outline,pad,5,5);
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        // The upper land is close to the window title. Put its horizontal
        // dimension below the land, in the open space between the two rows.
        DrawSot523Dimensions(dc,pads[2],footprint.PadLength.ToString("0.00",culture),footprint.PadWidth.ToString("0.00",culture));
    }

    private static void DrawSot523Dimensions(DrawingContext dc, Rect pad, string widthText, string heightText)
    {
        var pen=new Pen(ComponentCardPalette.Dimension,1); var typeface=new Typeface("Arial");
        var width=new FormattedText(widthText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,typeface,24,ComponentCardPalette.Dimension,1);
        var height=new FormattedText(heightText,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,typeface,24,ComponentCardPalette.Dimension,1);
        var y=pad.Bottom+32;
        dc.DrawLine(pen,new Point(pad.Left,pad.Bottom),new Point(pad.Left,y+6));
        dc.DrawLine(pen,new Point(pad.Right,pad.Bottom),new Point(pad.Right,y+6));
        dc.DrawLine(pen,new Point(pad.Left,y),new Point(pad.Right,y));
        DrawArrow(dc,new Point(pad.Left,y),true);
        DrawArrow(dc,new Point(pad.Right,y),false);
        dc.DrawText(width,new Point(pad.Left+(pad.Width-width.Width)/2,y-width.Height-7));

        var x=pad.Right+40;
        dc.DrawLine(pen,new Point(pad.Right,pad.Top),new Point(x+6,pad.Top));
        dc.DrawLine(pen,new Point(pad.Right,pad.Bottom),new Point(x+6,pad.Bottom));
        dc.DrawLine(pen,new Point(x,pad.Top),new Point(x,pad.Bottom));
        DrawArrow(dc,new Point(x,pad.Top),true,true);
        DrawArrow(dc,new Point(x,pad.Bottom),false,true);
        var labelX=x+8+height.Height/2;
        dc.PushTransform(new RotateTransform(-90,labelX,pad.Top+pad.Height/2));
        dc.DrawText(height,new Point(labelX-height.Width/2,pad.Top+pad.Height/2-height.Height/2));
        dc.Pop();
    }

    private void DrawC0402Reference(DrawingContext dc)
    {
        var fill = ComponentCardPalette.Aperture;
        var outline = new Pen(Brushes.Black, .8);
        // Show the documented nominal outline. Reduction is a textual recommendation,
        // not a transformation of the reference drawing.
        const double widthMm = .60, heightMm = .40;
        var scale = ScaleForPad(widthMm, heightMm);
        var width = widthMm * scale;
        var height = heightMm * scale;
        var y = CenteredPadTop(height);
        var left = ActualWidth * .27 - width / 2;
        var right = ActualWidth * .73 - width / 2;
        var culture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        var widthText = widthMm.ToString("0.00", culture);
        var heightText = heightMm.ToString("0.00", culture);
        DrawC0402Pad(dc, new Rect(left, y, width, height), fill, outline, widthText, heightText);
        DrawC0402Pad(dc, new Rect(right, y, width, height), fill, outline, widthText, heightText, true);
    }

    // The nominal C0402 preview still uses 200 px/mm.  Larger exact-MPN pads
    // are additionally constrained by their vertical dimensions and labels so
    // that neither the title strip nor the explanatory text below is covered.
    private double ScaleForPad(double padLength, double padWidth) =>
        Math.Min(200, Math.Min(ActualWidth * .18 / padLength, Math.Max(1, ActualHeight - 92) / padWidth));

    private double CenteredPadTop(double padHeight)
    {
        const double topClearance = 72;
        const double bottomClearance = 20;
        var preferred = ActualHeight * .58 - padHeight / 2;
        return Math.Clamp(preferred, topClearance, Math.Max(topClearance, ActualHeight - bottomClearance - padHeight));
    }

    private static void DrawC0402Pad(DrawingContext dc, Rect pad, Brush fill, Pen outline, string widthText, string heightText, bool rightSide = false)
    {
        dc.DrawRoundedRectangle(fill, outline, pad, 10, 10);
        var pen = new Pen(ComponentCardPalette.Dimension, 1);
        var typeface = new Typeface("Arial");
        var width = new FormattedText(widthText, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 24, ComponentCardPalette.Dimension, 1);
        var height = new FormattedText(heightText, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 24, ComponentCardPalette.Dimension, 1);
        var y = pad.Top - 40;
        dc.DrawLine(pen, new Point(pad.Left, pad.Top), new Point(pad.Left, y - 6));
        dc.DrawLine(pen, new Point(pad.Right, pad.Top), new Point(pad.Right, y - 6));
        dc.DrawLine(pen, new Point(pad.Left, y), new Point(pad.Right, y));
        DrawArrow(dc, new Point(pad.Left, y), true);
        DrawArrow(dc, new Point(pad.Right, y), false);
        dc.DrawText(width, new Point(pad.Left + (pad.Width - width.Width) / 2, y - width.Height - 7));
        var x = rightSide ? pad.Right + 40 : pad.Left - 40;
        var edge = rightSide ? pad.Right : pad.Left;
        var extension = rightSide ? x + 6 : x - 6;
        dc.DrawLine(pen, new Point(edge, pad.Top), new Point(extension, pad.Top));
        dc.DrawLine(pen, new Point(edge, pad.Bottom), new Point(extension, pad.Bottom));
        dc.DrawLine(pen, new Point(x, pad.Top), new Point(x, pad.Bottom));
        DrawArrow(dc, new Point(x, pad.Top), true, true);
        DrawArrow(dc, new Point(x, pad.Bottom), false, true);
        var labelX = x + (rightSide ? 1 : -1) * (8 + height.Height / 2);
        dc.PushTransform(new RotateTransform(-90, labelX, pad.Top + pad.Height / 2));
        dc.DrawText(height, new Point(labelX - height.Width / 2, pad.Top + pad.Height / 2 - height.Height / 2));
        dc.Pop();
    }

    private static void DrawDimensions(DrawingContext dc, Rect aperture, double widthMm, double heightMm)
    {
        var pen = new Pen(ComponentCardPalette.Dimension, 1);
        var typeface = new Typeface("Arial");
        var widthText = new FormattedText(
            FormatMm(widthMm), System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 13, ComponentCardPalette.Dimension, 1.0);
        var heightText = new FormattedText(
            FormatMm(heightMm), System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 13, ComponentCardPalette.Dimension, 1.0);
        var y = aperture.Top - 18;
        dc.DrawLine(pen, new Point(aperture.Left, aperture.Top), new Point(aperture.Left, y + 3));
        dc.DrawLine(pen, new Point(aperture.Right, aperture.Top), new Point(aperture.Right, y + 3));
        dc.DrawLine(pen, new Point(aperture.Left, y), new Point(aperture.Right, y));
        DrawArrow(dc, new Point(aperture.Left, y), true);
        DrawArrow(dc, new Point(aperture.Right, y), false);
        dc.DrawText(widthText, new Point(aperture.Left + (aperture.Width - widthText.Width) / 2, y - widthText.Height - 2));

        var x = aperture.Left - 18;
        dc.DrawLine(pen, new Point(aperture.Left, aperture.Top), new Point(x + 3, aperture.Top));
        dc.DrawLine(pen, new Point(aperture.Left, aperture.Bottom), new Point(x + 3, aperture.Bottom));
        dc.DrawLine(pen, new Point(x, aperture.Top), new Point(x, aperture.Bottom));
        DrawArrow(dc, new Point(x, aperture.Top), true, vertical: true);
        DrawArrow(dc, new Point(x, aperture.Bottom), false, vertical: true);
        dc.PushTransform(new RotateTransform(-90, x - 4, aperture.Top + aperture.Height / 2));
        dc.DrawText(heightText, new Point(x - 4 - heightText.Width / 2, aperture.Top + aperture.Height / 2 - heightText.Height / 2));
        dc.Pop();
    }

    private static string FormatMm(double value) => value.ToString("0.###", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));

    private static void DrawArrow(DrawingContext dc, Point point, bool inward, bool vertical = false)
    {
        const double size = 8;
        var direction = inward ? 1 : -1;
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
        if (!vertical)
        {
            context.BeginFigure(point, true, true);
            context.LineTo(new Point(point.X + direction * size, point.Y - size / 2), true, false);
            context.LineTo(new Point(point.X + direction * size, point.Y + size / 2), true, false);
        }
        else
        {
            context.BeginFigure(point, true, true);
            context.LineTo(new Point(point.X - size / 2, point.Y + direction * size), true, false);
            context.LineTo(new Point(point.X + size / 2, point.Y + direction * size), true, false);
        }
        }
        geometry.Freeze();
        dc.DrawGeometry(ComponentCardPalette.Dimension, null, geometry);
    }

    private static Rect ToScreen(Rect source, double scale, Vector offset) =>
        new(source.X * scale + offset.X, source.Y * scale + offset.Y, source.Width * scale, source.Height * scale);

    private static Rect InsetByReduction(Rect source, double reductionX, double reductionY)
    {
        var width = Math.Max(2, source.Width * (1 - reductionX));
        var height = Math.Max(2, source.Height * (1 - reductionY));
        return new Rect(source.Left + (source.Width - width) / 2, source.Top + (source.Height - height) / 2, width, height);
    }

    private static void DrawAperture(DrawingContext dc, Rect aperture, ApertureShapeType shape)
    {
        var fill = ComponentCardPalette.Aperture;
        var outline = new Pen(Brushes.Black, .8);
        switch (shape)
        {
            case ApertureShapeType.Round:
            case ApertureShapeType.Ellipse:
                dc.DrawEllipse(fill, outline, new Point(aperture.Left + aperture.Width / 2, aperture.Top + aperture.Height / 2), aperture.Width / 2, aperture.Height / 2);
                break;
            case ApertureShapeType.Square:
                var side = Math.Min(aperture.Width, aperture.Height);
                dc.DrawRectangle(fill, outline, new Rect(aperture.Left + (aperture.Width - side) / 2, aperture.Top + (aperture.Height - side) / 2, side, side));
                break;
            case ApertureShapeType.Oblong:
                dc.DrawRoundedRectangle(fill, outline, aperture, Math.Min(aperture.Width, aperture.Height) / 2, Math.Min(aperture.Width, aperture.Height) / 2);
                break;
            case ApertureShapeType.Array:
                var gap = Math.Max(1.5, Math.Min(aperture.Width, aperture.Height) * .08);
                var cellWidth = Math.Max(1, (aperture.Width - gap) / 2);
                var cellHeight = Math.Max(1, (aperture.Height - gap) / 2);
                for (var row = 0; row < 2; row++)
                for (var column = 0; column < 2; column++)
                    dc.DrawRectangle(fill, outline, new Rect(aperture.Left + column * (cellWidth + gap), aperture.Top + row * (cellHeight + gap), cellWidth, cellHeight));
                break;
            case ApertureShapeType.HomePlate:
            case ApertureShapeType.InvertedHomePlate:
                var inset = aperture.Width * .18;
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    context.BeginFigure(new Point(aperture.Left + inset, aperture.Top), true, true);
                    context.LineTo(new Point(aperture.Right, aperture.Top), true, false);
                    context.LineTo(new Point(aperture.Right, aperture.Bottom), true, false);
                    context.LineTo(new Point(aperture.Left + inset, aperture.Bottom), true, false);
                    context.LineTo(new Point(aperture.Left, aperture.Top + aperture.Height / 2), true, false);
                }
                dc.DrawGeometry(fill, outline, geometry);
                break;
            default:
                var radius = Math.Min(aperture.Width, aperture.Height) * 0.15;
                dc.DrawRoundedRectangle(fill, outline, aperture, radius, radius);
                break;
        }
    }
}
