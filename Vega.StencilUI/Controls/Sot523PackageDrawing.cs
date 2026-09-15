using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Nominal geometry and tape orientation from Diodes Incorporated SOT523
// package information, revision 2018-04-09.
internal static class Sot523PackageDrawing
{
    private static readonly Brush Body = new SolidColorBrush(Color.FromRgb(76, 80, 89));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(218, 218, 211));
    private static readonly Brush Mark = Brushes.White;
    private static readonly System.Globalization.CultureInfo Ru = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

    public static bool Supports(string? name) =>
        name?.Equals("SOT523", StringComparison.OrdinalIgnoreCase) == true;

    private static IEnumerable<(double X, double Side)> Pins()
    {
        yield return (-.50, -1);
        yield return (.50, -1);
        yield return (0, 1);
    }

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh()
    {
        var mesh = new List<GlbTriangle>();
        var body = new Vector4(.30f, .32f, .35f, 1);
        var metal = new Vector4(.85f, .85f, .82f, 1);
        var mark = new Vector4(.96f, .96f, .93f, 1);
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 color)
        {
            mesh.Add(new(a * .001f, b * .001f, c * .001f, color));
            mesh.Add(new(a * .001f, c * .001f, d * .001f, color));
        }
        void Box(float x0, float x1, float y0, float y1, float z0, float z1, Vector4 color)
        {
            Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),color);
            Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),color);
            Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),color);
            Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),color);
            Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),color);
            Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),color);
        }
        Box(-.80f,.80f,-.40f,.40f,.05f,.75f,body);
        foreach (var pin in Pins())
            GullWingLeadDrawing.AddVerticalMesh(mesh,(float)pin.X,pin.Side<0?-.40f:.40f,pin.Side<0?-.80f:.80f,.22f,.12f,.75f,metal);
        Box(-.66f,-.52f,-.30f,-.16f,.748f,.752f,mark);
        return mesh;
    }

    public static void PaintTop(Canvas canvas, double cx, double cy, double pxPerMm)
    {
        void Box(double x,double y,double w,double h,Brush fill)
        {
            var r=new Rectangle{Width=w,Height=h,Fill=fill};
            Canvas.SetLeft(r,x); Canvas.SetTop(r,y); canvas.Children.Add(r);
        }
        Box(cx-.80*pxPerMm,cy-.40*pxPerMm,1.60*pxPerMm,.80*pxPerMm,Body);
        foreach(var pin in Pins())
            GullWingLeadDrawing.PaintVerticalTop(canvas,cx+pin.X*pxPerMm,cy+(pin.Side<0?-.40:.40)*pxPerMm,
                cy+(pin.Side<0?-.80:.80)*pxPerMm,.22*pxPerMm,1,Metal,ComponentCardPalette.Border);
        var dot=new Ellipse{Width=.14*pxPerMm,Height=.14*pxPerMm,Fill=Mark};
        Canvas.SetLeft(dot,cx-.66*pxPerMm); Canvas.SetTop(dot,cy+.16*pxPerMm); canvas.Children.Add(dot);
    }

    public static UIElement Create(StepProjectionKind kind)
    {
        const double cx=256,cy=176,s=105;
        var c=new Canvas{Width=512,Height=350,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,cx,cy,s);
            if(kind==StepProjectionKind.Top)
            {
                H(c,cx-.80*s,cx+.80*s,42,cy-.40*s,"1,60");
                V(c,52,cy-.80*s,cy+.80*s,cx-.80*s,"1,60");
                V(c,458,cy-.40*s,cy+.40*s,cx+.80*s,"0,80",true);
                H(c,cx-.50*s,cx+.50*s,330,cy+.80*s,"1,00");
                SmallH(c,cx+.50*s-.11*s,cx+.50*s+.11*s,286,cy+.80*s,"0,22");
            }
        }
        else
        {
            const double floor=218; const double h=.75;
            var body=new Rectangle{Width=.80*s,Height=h*s,Fill=Body};
            Canvas.SetLeft(body,cx-.40*s); Canvas.SetTop(body,floor-h*s); c.Children.Add(body);
            GullWingLeadDrawing.PaintHorizontalSide(c,cx/s-.40,cx/s+.40,floor,cx/s-.80,cx/s+.80,h,.12,s,Metal);
            V(c,82,floor-h*s,floor+.03*s,cx-.40*s,"0,75");
            SideH(c,cx+.40*s,cx+.80*s,306,floor+.12*s,"0,33");
            SmallV(c,442,floor,floor+.12*s,cx+.80*s,68,"0,12");
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void SmallH(Canvas c,double a,double b,double y,double origin,string value){var shelf=y-15;Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,a-12,y,b+15,y);Line(c,b+15,y,b+15,shelf);Line(c,b+15,shelf,430,shelf);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,385,shelf);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,190,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,245,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+46:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-145,shelfY,x,shelfY);Label(c,value,x-72,shelfY);}
}
