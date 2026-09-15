using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Nominal SOT23-5L and SOT23-6L geometry from AOSMD package drawings
// PO-00112 and PO-00050. Tolerances intentionally are not shown on the card.
internal static class Sot23MultiLeadPackageDrawing
{
    private static readonly Brush Body = new SolidColorBrush(Color.FromRgb(70, 73, 81));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(210, 210, 202));
    private static readonly Brush Mark = new SolidColorBrush(Color.FromRgb(225, 225, 218));

    public static bool Supports(string? name) =>
        name?.Equals("SOT25", StringComparison.OrdinalIgnoreCase) == true ||
        name?.Equals("SOT26", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsFive(string name) => name.Equals("SOT25", StringComparison.OrdinalIgnoreCase);
    private static double Height(string name) => IsFive(name) ? .95 : 1.10;

    private static IEnumerable<(double X, double Side)> Pins(string name)
    {
        yield return (-.95, -1); yield return (0, -1); yield return (.95, -1);
        yield return (-.95, 1);
        if (!IsFive(name)) yield return (0, 1);
        yield return (.95, 1);
    }

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh(string name)
    {
        var mesh = new List<GlbTriangle>();
        var body = new Vector4(.27f, .29f, .32f, 1); var metal = new Vector4(.82f, .82f, .79f, 1); var mark = new Vector4(.90f, .90f, .86f, 1);
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 color) { mesh.Add(new(a * .001f, b * .001f, c * .001f, color)); mesh.Add(new(a * .001f, c * .001f, d * .001f, color)); }
        void Box(float x0, float x1, float y0, float y1, float z0, float z1, Vector4 color)
        {
            Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),color); Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),color);
            Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),color); Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),color);
            Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),color); Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),color);
        }
        var h = (float)Height(name);
        Box(-1.45f, 1.45f, -.80f, .80f, .05f, h, body);
        foreach (var pin in Pins(name))
            GullWingLeadDrawing.AddVerticalMesh(mesh,(float)pin.X,pin.Side<0?-.80f:.80f,pin.Side<0?-1.40f:1.40f,.40f,.13f,h,metal);
        Box(-1.18f,-.98f,-.66f,-.46f,h-.004f,h+.001f,mark);
        return mesh;
    }

    public static void PaintTop(Canvas canvas, string name, double cx, double cy, double pxPerMm)
    {
        void Box(double x,double y,double w,double h,Brush fill) { var r=new Rectangle{Width=w,Height=h,Fill=fill}; Canvas.SetLeft(r,x); Canvas.SetTop(r,y); canvas.Children.Add(r); }
        Box(cx-1.45*pxPerMm,cy-.80*pxPerMm,2.90*pxPerMm,1.60*pxPerMm,Body);
        foreach(var pin in Pins(name))
            GullWingLeadDrawing.PaintVerticalTop(canvas,cx+pin.X*pxPerMm,cy+(pin.Side<0?-.80:.80)*pxPerMm,
                cy+(pin.Side<0?-1.40:1.40)*pxPerMm,.40*pxPerMm,1,Metal,ComponentCardPalette.Border);
        var dot=new Ellipse{Width=.18*pxPerMm,Height=.18*pxPerMm,Fill=Mark}; Canvas.SetLeft(dot,cx-1.18*pxPerMm); Canvas.SetTop(dot,cy+.48*pxPerMm); canvas.Children.Add(dot);
    }

    public static UIElement Create(string name, StepProjectionKind kind)
    {
        const double cx=256,cy=165,s=70;
        var c=new Canvas{Width=512,Height=320,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,name,cx,cy,s);
            if(kind==StepProjectionKind.Top)
            {
                H(c,cx-1.45*s,cx+1.45*s,58,cy-.80*s,"2,90");
                V(c,52,cy-1.40*s,cy+1.40*s,cx-1.45*s,"2,80");
                V(c,454,cy-.80*s,cy+.80*s,cx+1.45*s,"1,60",true);
                H(c,cx-.95*s,cx+.95*s,306,cy+1.40*s,"1,90");
                // Width b is attached to the real centre lead.  It is placed
                // below the package so its label cannot cross the E dimension.
                SmallH(c,cx-1.15*s,cx-.75*s,280,cy+1.40*s,"0,40");
            }
        }
        else
        {
            const double floor=205; var h=Height(name);
            var body=new Rectangle{Width=1.60*s,Height=h*s,Fill=Body}; Canvas.SetLeft(body,cx-.80*s); Canvas.SetTop(body,floor-h*s); c.Children.Add(body);
            GullWingLeadDrawing.PaintHorizontalSide(c,cx/s-.80,cx/s+.80,floor,cx/s-1.40,cx/s+1.40,h,.13,s,Metal);
            V(c,84,floor-h*s,floor+.04*s,cx-.80*s,h.ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("ru-RU")));
            SideH(c,cx+.80*s,cx+1.40*s,282,floor+.13*s,"0,60");
            SmallV(c,438,floor,floor+.13*s,cx+1.40*s,54,"0,13");
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=27,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-12);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void SmallH(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);var shelf=y-50;Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a-12,y,b+15,y);Line(c,b+15,y,b+15,shelf);Line(c,b+15,shelf,430,shelf);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,370,shelf);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,200,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,250,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+48:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-160,shelfY,x,shelfY);Label(c,value,x-80,shelfY);}
}
