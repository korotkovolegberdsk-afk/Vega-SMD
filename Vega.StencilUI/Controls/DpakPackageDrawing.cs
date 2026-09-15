using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// STMicroelectronics DPAK (TO-252) and D2PAK (TO-263) nominal package data.
internal static class DpakPackageDrawing
{
    private static readonly Brush Body = new SolidColorBrush(Color.FromRgb(92, 119, 143));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(218, 218, 211));
    private static readonly Brush BodyOutline = new SolidColorBrush(Color.FromRgb(70, 93, 114));
    private static readonly Brush Mark = Brushes.White;

    public static bool Supports(string? name) => name is not null &&
        (name.Equals("DPAK",StringComparison.OrdinalIgnoreCase) || name.Equals("D2PAK",StringComparison.OrdinalIgnoreCase));
    private static bool IsD2(string name) => name.Equals("D2PAK",StringComparison.OrdinalIgnoreCase);

    private readonly record struct Spec(double BodyX,double BodyY,double OverallY,double Height,double Pitch,double LeadWidth,double LeadLength,double LeadThickness,double TabWidth);
    private static Spec Get(string name) => IsD2(name)
        ? new(10.20,9.15,15.43,4.50,2.54,.82,2.54,.53,8.50)
        : new(6.50,6.10,9.73,2.30,2.28,.77,1.25,.53,5.30);

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh(string name)
    {
        var d=Get(name); var mesh=new List<GlbTriangle>();
        var body=new Vector4(.21f,.23f,.26f,1); var metal=new Vector4(.85f,.85f,.82f,1); var mark=new Vector4(.96f,.96f,.93f,1);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 e,Vector4 color){mesh.Add(new(a*.001f,b*.001f,c*.001f,color));mesh.Add(new(a*.001f,c*.001f,e*.001f,color));}
        static Vector4 Shade(Vector4 color,float factor)=>new(color.X*factor,color.Y*factor,color.Z*factor,color.W);
        void Box(float x0,float x1,float y0,float y1,float z0,float z1,Vector4 color,bool faceted=false)
        {
            var bottom=faceted?Shade(color,.55f):color;var top=faceted?Shade(color,1.30f):color;
            var front=faceted?Shade(color,1.00f):color;var back=faceted?Shade(color,.74f):color;
            var left=faceted?Shade(color,.52f):color;var right=faceted?Shade(color,.62f):color;
            Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),bottom); Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),top);
            Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),front); Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),back);
            Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),left); Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),right);
        }
        float hx=(float)d.BodyX/2,hy=(float)d.BodyY/2,oy=(float)d.OverallY/2,hh=(float)d.Height,lt=(float)d.LeadThickness;
        Box(-hx,hx,-hy,hy,lt,hh,new Vector4(.34f,.43f,.52f,1),true);
        Box(-(float)d.TabWidth/2,(float)d.TabWidth/2,hy,oy,0,lt,metal);
        foreach(var x in new[]{-(float)d.Pitch,0f,(float)d.Pitch}) Box(x-(float)d.LeadWidth/2,x+(float)d.LeadWidth/2,-oy,-hy,0,lt,metal);
        Box(-hx+.35f,-hx+.75f,hy-.75f,hy-.35f,hh-.004f,hh+.004f,mark);
        return mesh;
    }

    public static void PaintTop(Canvas c,string name,double cx,double cy,double scale,bool bottom=false)
    {
        var d=Get(name);
        Point P(double x,double y){if(bottom)x=-x;return new(cx+x*scale,cy+y*scale);}
        void Box(double x,double y,double w,double h,Brush fill){var a=P(x,y);var b=P(x+w,y+h);var r=new Rectangle{Width=Math.Abs(b.X-a.X),Height=Math.Abs(b.Y-a.Y),Fill=fill,Stroke=fill==Body?BodyOutline:ComponentCardPalette.Border,StrokeThickness=.8};Canvas.SetLeft(r,Math.Min(a.X,b.X));Canvas.SetTop(r,Math.Min(a.Y,b.Y));c.Children.Add(r);}
        Box(-d.BodyX/2,-d.BodyY/2,d.BodyX,d.BodyY,Body);
        Box(-d.TabWidth/2,-d.OverallY/2,d.TabWidth,(d.OverallY-d.BodyY)/2,Metal);
        foreach(var x in new[]{-d.Pitch,0d,d.Pitch}) Box(x-d.LeadWidth/2,d.BodyY/2,d.LeadWidth,(d.OverallY-d.BodyY)/2,Metal);
        if(!bottom){var p=P(-d.BodyX/2+.35,d.BodyY/2-.75);var dot=new Ellipse{Width=.40*scale,Height=.40*scale,Fill=Mark};Canvas.SetLeft(dot,p.X);Canvas.SetTop(dot,p.Y);c.Children.Add(dot);}
    }

    public static UIElement Create(string name,StepProjectionKind kind)
    {
        var d=Get(name); var scale=IsD2(name)?17d:26d; const double cx=256,cy=175;
        var c=new Canvas{Width=512,Height=350,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,name,cx,cy,scale,kind==StepProjectionKind.Bottom);
            if(kind==StepProjectionKind.Top)
            {
                H(c,cx-d.BodyX*scale/2,cx+d.BodyX*scale/2,40,cy-d.OverallY*scale/2,F(d.BodyX));
                V(c,42,cy-d.OverallY*scale/2,cy+d.OverallY*scale/2,cx-d.BodyX*scale/2,F(d.OverallY));
                V(c,420,cy-d.BodyY*scale/2,cy+d.BodyY*scale/2,cx+d.BodyX*scale/2,F(d.BodyY));
                SmallPitchH(c,cx-d.Pitch*scale,cx,343,cy+d.OverallY*scale/2,F(d.Pitch));
                SmallH(c,cx+d.Pitch*scale-d.LeadWidth*scale/2,cx+d.Pitch*scale+d.LeadWidth*scale/2,343,cy+d.OverallY*scale/2,F(d.LeadWidth));
            }
        }
        else
        {
            const double floor=224; var height=d.Height*scale; var bodyWidth=d.BodyY*scale; var totalWidth=d.OverallY*scale; var t=d.LeadThickness*scale;
            var body=new Rectangle{Width=bodyWidth,Height=height-t,Fill=Body};Canvas.SetLeft(body,cx-bodyWidth/2);Canvas.SetTop(body,floor-height);c.Children.Add(body);
            var lead=new Rectangle{Width=totalWidth,Height=t,Fill=Metal};Canvas.SetLeft(lead,cx-totalWidth/2);Canvas.SetTop(lead,floor-t);c.Children.Add(lead);
            V(c,70,floor-height,floor,cx-bodyWidth/2,F(d.Height));
            SideH(c,cx+bodyWidth/2,cx+totalWidth/2,310,floor,F(d.LeadLength));
            SmallV(c,444,floor-t,floor,cx+totalWidth/2,76,F(d.LeadThickness));
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static string F(double v)=>v.ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var s=Math.Sign(y-origin);Line(c,a,origin,a,y+s*7);Line(c,b,origin,b,y+s*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void SmallPitchH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,185,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,120,y-45);}
    private static void SmallH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,a-12,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Line(c,b+15,y,b+15,y-18);Line(c,b+15,y-18,350,y-18);Label(c,value,405,y);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,175,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,235,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value){var s=Math.Sign(x-origin);Line(c,origin,a,x+s*7,a);Line(c,origin,b,x+s*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-145,shelfY,x,shelfY);Label(c,value,x-72,shelfY);}
}
