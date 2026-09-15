using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// SO geometry from manufacturer package drawings; gull-wing construction is shared
// with the approved SOIC/SOT drawing rule.
internal static class Soic16PackageDrawing
{
    private readonly record struct Geometry(double BodyLength,double BodyWidth,double OverallWidth,double Height,double Pitch,double LeadWidth,double LeadLength,double LeadThickness,int PinsPerSide);
    private static readonly Geometry So14Narrow=new(8.65,3.90,6.00,1.75,1.27,.41,.72,.18,7);
    private static readonly Geometry So18Wide=new(11.55,7.50,10.30,2.65,1.27,.41,.84,.27,9);
    private static readonly Geometry So20Wide=new(12.80,7.50,10.30,2.65,1.27,.41,.84,.22,10);
    private static readonly Geometry So24Wide=new(15.40,7.50,10.30,2.65,1.27,.43,.65,.28,12);
    private static readonly Geometry Narrow=new(10.00,3.90,6.00,1.26,1.27,.41,.84,.18,8);
    private static readonly Geometry Medium=new(10.20,5.30,7.60,2.00,1.27,.43,.75,.20,8);
    private static readonly Geometry Wide=new(10.30,7.50,10.30,2.50,1.27,.41,.58,.27,8);
    private static readonly Geometry Ssop8Dct=new(3.00,3.00,4.25,1.30,.65,.23,.40,.15,4);
    private static readonly Geometry Ssop16Db=new(6.20,5.30,7.80,2.00,.65,.30,.75,.20,8);
    private static readonly Geometry Ssop28Db=new(10.20,5.30,7.80,2.00,.65,.30,.75,.20,14);
    private static readonly Geometry Tssop8Pw=new(3.00,4.40,6.40,1.20,.65,.25,.60,.10,4);
    private static readonly Geometry Tssop14Pw=new(5.00,4.40,6.40,1.20,.65,.25,.60,.10,7);
    private static readonly Geometry Tssop16Pw=new(5.00,4.40,6.40,1.20,.65,.25,.60,.10,8);
    private static readonly Geometry Tssop20Pw=new(6.50,4.40,6.40,1.20,.65,.25,.60,.10,10);
    private static readonly Geometry Tssop24Pw=new(7.80,4.40,6.40,1.20,.65,.25,.60,.10,12);
    private static readonly Geometry Tssop28Pw=new(9.70,4.40,6.40,1.20,.65,.25,.60,.10,14);
    private static readonly Brush Body=new SolidColorBrush(Color.FromRgb(74,91,108));
    private static readonly Brush Outline=new SolidColorBrush(Color.FromRgb(52,67,82));
    private static readonly Brush Metal=new SolidColorBrush(Color.FromRgb(211,214,211));
    public static bool Supports(string? name)=>name is not null &&
        (name.Equals("SO14",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SOIC14",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO14P127W60",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO18P127W103",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO20P127W103",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO24P127W103",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SOIC16",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO16P127W60",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO16P127W76",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO16P127W103",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SSOP08P065W43",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SSOP16P065W78",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SSOP28P065W78",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("TSSOP08P065W64",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("TSSOP14P065W64",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("TSSOP16P065W64",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("TSSOP20P065W64",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("TSSOP24P065W64",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("TSSOP28P065W64",StringComparison.OrdinalIgnoreCase));
    private static Geometry Get(string? name)=>name?.ToUpperInvariant() switch
    {
        "SO14" or "SOIC14" or "SO14P127W60"=>So14Narrow,
        "SO18P127W103"=>So18Wide,
        "SO20P127W103"=>So20Wide,
        "SO24P127W103"=>So24Wide,
        "SO16P127W76"=>Medium,
        "SO16P127W103"=>Wide,
        "SSOP08P065W43"=>Ssop8Dct,
        "SSOP16P065W78"=>Ssop16Db,
        "SSOP28P065W78"=>Ssop28Db,
        "TSSOP08P065W64"=>Tssop8Pw,
        "TSSOP14P065W64"=>Tssop14Pw,
        "TSSOP16P065W64"=>Tssop16Pw,
        "TSSOP20P065W64"=>Tssop20Pw,
        "TSSOP24P065W64"=>Tssop24Pw,
        "TSSOP28P065W64"=>Tssop28Pw,
        _=>Narrow
    };
    private static IEnumerable<double> PinYs(Geometry geometry)
    {
        var center=(geometry.PinsPerSide-1)/2d;
        for(var i=0;i<geometry.PinsPerSide;i++)yield return (i-center)*geometry.Pitch;
    }

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh(string? packageName=null)
    {
        var geometry=Get(packageName);
        var mesh=new List<GlbTriangle>();var body=new Vector4(.29f,.36f,.43f,1);var metal=new Vector4(.83f,.84f,.82f,1);var mark=new Vector4(.98f,.98f,.96f,1);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector4 color){mesh.Add(new(a*.001f,b*.001f,c*.001f,color));mesh.Add(new(a*.001f,c*.001f,d*.001f,color));}
        static Vector4 Shade(Vector4 c,float f)=>new(c.X*f,c.Y*f,c.Z*f,c.W);
        void Box(float x0,float x1,float y0,float y1,float z0,float z1,Vector4 color,bool faceted=false){var top=faceted?Shade(color,1.22f):color;var side=faceted?Shade(color,.68f):color;Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),side);Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),top);Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),color);Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),side);Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),side);Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),color);}
        void Segment(float x0,float z0,float x1,float z1,float y){var half=(float)geometry.LeadWidth/2;var t=(float)geometry.LeadThickness;var a=new Vector3(x0,y-half,z0);var b=new Vector3(x0,y+half,z0);var c=new Vector3(x1,y+half,z1);var d=new Vector3(x1,y-half,z1);var dz=new Vector3(0,0,t);Face(a,b,c,d,metal);Face(a+dz,d+dz,c+dz,b+dz,metal);Face(a,a+dz,b+dz,b,metal);Face(d,c,c+dz,d+dz,metal);Face(a,d,d+dz,a+dz,metal);Face(b,b+dz,c+dz,c,metal);}
        Box(-(float)geometry.BodyWidth/2,(float)geometry.BodyWidth/2,-(float)geometry.BodyLength/2,(float)geometry.BodyLength/2,.10f,(float)geometry.Height,body,true);
        foreach(var y in PinYs(geometry))foreach(var side in new[]{-1f,1f}){var edge=side*(float)geometry.BodyWidth/2;var shoulder=edge+side*.23f;var foot=side*((float)geometry.OverallWidth/2-(float)geometry.LeadLength);var tip=side*(float)geometry.OverallWidth/2;Segment(edge,(float)geometry.Height*.5f,shoulder,(float)geometry.Height*.5f,(float)y);Segment(shoulder,(float)geometry.Height*.5f,foot,0,(float)y);Segment(foot,0,tip,0,(float)y);}
        Box(-(float)geometry.BodyWidth*.38f,-(float)geometry.BodyWidth*.30f,-(float)geometry.BodyLength*.46f,-(float)geometry.BodyLength*.43f,(float)geometry.Height-.004f,(float)geometry.Height+.004f,mark);return mesh;
    }

    public static void PaintTop(Canvas c,double cx,double cy,double s,bool bottom=false,string? packageName=null)
    {
        var geometry=Get(packageName);
        var body=new Rectangle{Width=geometry.BodyWidth*s,Height=geometry.BodyLength*s,Fill=Body,Stroke=Outline,StrokeThickness=.8};Canvas.SetLeft(body,cx-geometry.BodyWidth*s/2);Canvas.SetTop(body,cy-geometry.BodyLength*s/2);c.Children.Add(body);
        foreach(var y in PinYs(geometry))foreach(var side in new[]{-1d,1d})PaintLead(c,cx,cy+y*s,s,side,geometry);
        if(!bottom){var dot=new Ellipse{Width=.28*s,Height=.28*s,Fill=Brushes.White};Canvas.SetLeft(dot,cx-geometry.BodyWidth*.39*s);Canvas.SetTop(dot,cy-geometry.BodyLength*.465*s);c.Children.Add(dot);}
    }
    private static void PaintLead(Canvas c,double cx,double y,double s,double side,Geometry geometry)
    {
        var edge=side*geometry.BodyWidth/2;var tip=side*geometry.OverallWidth/2;var shoulder=edge+side*.24;var half=geometry.LeadWidth/2;var neck=half*.55;Point P(double x,double yy)=>new(cx+x*s,yy);var g=new StreamGeometry();using(var x=g.Open()){x.BeginFigure(P(edge,y-neck*s),true,true);x.LineTo(P(shoulder,y-neck*s),true,false);x.BezierTo(P(shoulder+side*.10,y-neck*s),P(shoulder+side*.13,y-half*s),P(shoulder+side*.24,y-half*s),true,false);x.LineTo(P(tip,y-half*s),true,false);x.LineTo(P(tip,y+half*s),true,false);x.LineTo(P(shoulder+side*.24,y+half*s),true,false);x.BezierTo(P(shoulder+side*.13,y+half*s),P(shoulder+side*.10,y+neck*s),P(shoulder,y+neck*s),true,false);x.LineTo(P(edge,y+neck*s),true,false);}c.Children.Add(new Path{Data=g,Fill=Metal,Stroke=ComponentCardPalette.Border,StrokeThickness=.8});
    }

    public static UIElement Create(StepProjectionKind kind,string? packageName=null)
    {
        var geometry=Get(packageName);
        var culture=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
        string F(double value)=>value.ToString("0.00",culture);
        const double cx=256,cy=178;
        var maximumScale=packageName?.Equals("SSOP08P065W43",StringComparison.OrdinalIgnoreCase)==true?43d:27d;
        var s=Math.Min(maximumScale,245/geometry.BodyLength);var c=new Canvas{Width=512,Height=370,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,cx,cy,s,kind==StepProjectionKind.Bottom,packageName);
            if(kind==StepProjectionKind.Top)
            {
                var outer=(geometry.PinsPerSide-1)/2d;
                var left=cx-geometry.OverallWidth*s/2;
                var right=cx+geometry.OverallWidth*s/2;
                var top=cy-geometry.BodyLength*s/2;
                var bottom=cy+geometry.BodyLength*s/2;
                H(c,left,right,Math.Max(40,top-24),top,F(geometry.OverallWidth));
                HBelow(c,cx-geometry.BodyWidth*s/2,cx+geometry.BodyWidth*s/2,bottom+27,bottom,F(geometry.BodyWidth));
                V(c,left-32,top,bottom,left,F(geometry.BodyLength));
                var finePitch=geometry.Pitch<1;
                V(c,right+(finePitch?22:30),cy-outer*geometry.Pitch*s,cy-(outer-1)*geometry.Pitch*s,right,F(geometry.Pitch),true);
                var pinY=cy+outer*geometry.Pitch*s;
                var leadDimensionOffset=finePitch&&geometry.PinsPerSide<=4?88:30;
                SmallV(c,right+leadDimensionOffset,pinY-geometry.LeadWidth*s/2,pinY+geometry.LeadWidth*s/2,right,pinY-31,F(geometry.LeadWidth));
            }
        }
        else
        {
            const double floor=220;
            var bw=geometry.BodyWidth*s;
            var h=geometry.Height*s;
            var left=cx-geometry.OverallWidth*s/2;
            var right=cx+geometry.OverallWidth*s/2;
            var body=new Rectangle{Width=bw,Height=h,Fill=Body,Stroke=Outline,StrokeThickness=.8};
            Canvas.SetLeft(body,cx-bw/2);
            Canvas.SetTop(body,floor-h);
            c.Children.Add(body);
            GullWingLeadDrawing.PaintHorizontalSide(c,cx/s-geometry.BodyWidth/2,cx/s+geometry.BodyWidth/2,floor,cx/s-geometry.OverallWidth/2,cx/s+geometry.OverallWidth/2,geometry.Height,geometry.LeadThickness,s,Metal);
            V(c,left-32,floor-h,floor,left,F(geometry.Height));
            SideH(c,right-geometry.LeadLength*s,right,floor+46,floor,F(geometry.LeadLength));
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }
    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void HBelow(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,(a+b-t.DesiredSize.Width)/2);Canvas.SetTop(t,y+5);c.Children.Add(t);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+48:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x,shelfY,x+76,shelfY);Label(c,value,x+38,shelfY);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,a-70,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,a-34,y);}
}
