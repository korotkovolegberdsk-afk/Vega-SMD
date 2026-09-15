using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Nominal geometry from Nexperia SOT323, SOT353 and SOT363 package outlines.
// Tape orientation is taken from the matching Nexperia packing information.
internal static class SotMicroPackageDrawing
{
    private static readonly Brush Body = new SolidColorBrush(Color.FromRgb(76, 80, 89));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(218, 218, 211));
    private static readonly Brush Mark = Brushes.White;

    public static bool Supports(string? name) => name is not null &&
        (name.Equals("SOT323", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SOT353", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SOT363", StringComparison.OrdinalIgnoreCase));

    private static bool Is323(string name) => name.Equals("SOT323", StringComparison.OrdinalIgnoreCase);
    private static bool Is353(string name) => name.Equals("SOT353", StringComparison.OrdinalIgnoreCase);
    private static double LeadWidth(string name) => Is323(name) ? .35 : .25;
    private static double LeadThickness(string name) => .18;

    private static IEnumerable<(double X, double Side)> Pins(string name)
    {
        if (Is323(name))
        {
            yield return (-.65, -1); yield return (.65, -1); yield return (0, 1);
            yield break;
        }
        yield return (-.65, -1); yield return (0, -1); yield return (.65, -1);
        yield return (-.65, 1);
        if (!Is353(name)) yield return (0, 1);
        yield return (.65, 1);
    }

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh(string name)
    {
        var mesh = new List<GlbTriangle>();
        var body = new Vector4(.30f, .32f, .35f, 1); var metal = new Vector4(.85f, .85f, .82f, 1); var mark = new Vector4(.96f, .96f, .93f, 1);
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 color) { mesh.Add(new(a*.001f,b*.001f,c*.001f,color)); mesh.Add(new(a*.001f,c*.001f,d*.001f,color)); }
        void Box(float x0,float x1,float y0,float y1,float z0,float z1,Vector4 color)
        {
            Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),color); Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),color);
            Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),color); Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),color);
            Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),color); Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),color);
        }
        Box(-1f,1f,-.625f,.625f,.05f,.95f,body);
        var thickness=(float)LeadThickness(name);
        foreach(var pin in Pins(name))
            GullWingLeadDrawing.AddVerticalMesh(mesh,(float)pin.X,pin.Side<0?-.625f:.625f,pin.Side<0?-1.05f:1.05f,
                (float)LeadWidth(name),thickness,.95f,metal);
        Box(-.80f,-.62f,-.48f,-.30f,.948f,.952f,mark);
        return mesh;
    }

    public static void PaintTop(Canvas canvas,string name,double cx,double cy,double pxPerMm,bool tapeMode=false)
    {
        void Box(double x,double y,double w,double h,Brush fill){var r=new Rectangle{Width=w,Height=h,Fill=fill};Canvas.SetLeft(r,x);Canvas.SetTop(r,y);canvas.Children.Add(r);}
        Box(cx-1*pxPerMm,cy-.625*pxPerMm,2*pxPerMm,1.25*pxPerMm,Body);
        var leadWidth=LeadWidth(name);
        foreach(var source in Pins(name))
        {
            var pin=tapeMode && !Is323(name)?(-source.X,-source.Side):source;
            GullWingLeadDrawing.PaintVerticalTop(canvas,cx+pin.Item1*pxPerMm,cy+(pin.Item2<0?-.625:.625)*pxPerMm,
                cy+(pin.Item2<0?-1.05:1.05)*pxPerMm,leadWidth*pxPerMm,1,Metal,ComponentCardPalette.Border);
        }
        var dotX=tapeMode&&!Is323(name)?.62:-.80; var dotY=tapeMode&&!Is323(name)?-.48:.30;
        var dot=new Ellipse{Width=.18*pxPerMm,Height=.18*pxPerMm,Fill=Mark};Canvas.SetLeft(dot,cx+dotX*pxPerMm);Canvas.SetTop(dot,cy+dotY*pxPerMm);canvas.Children.Add(dot);
    }

    public static UIElement Create(string name,StepProjectionKind kind)
    {
        const double cx=256,cy=176,s=86;
        var c=new Canvas{Width=512,Height=350,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,name,cx,cy,s);
            if(kind==StepProjectionKind.Top)
            {
                H(c,cx-s,cx+s,42,cy-.625*s,"2,00");
                V(c,54,cy-1.05*s,cy+1.05*s,cx-s,"2,10");
                V(c,458,cy-.625*s,cy+.625*s,cx+s,"1,25",true);
                H(c,cx-.65*s,cx+.65*s,329,cy+1.05*s,"1,30");
                SmallH(c,cx+.65*s-LeadWidth(name)*s/2,cx+.65*s+LeadWidth(name)*s/2,291,cy+1.05*s,LeadWidth(name).ToString("0.00",Ru));
            }
        }
        else
        {
            const double floor=220; const double h=.95;
            var body=new Rectangle{Width=1.25*s,Height=h*s,Fill=Body};Canvas.SetLeft(body,cx-.625*s);Canvas.SetTop(body,floor-h*s);c.Children.Add(body);
            GullWingLeadDrawing.PaintHorizontalSide(c,cx/s-.625,cx/s+.625,floor,cx/s-1.05,cx/s+1.05,h,LeadThickness(name),s,Metal);
            V(c,82,floor-h*s,floor+.04*s,cx-.625*s,"0,95");
            SideH(c,cx+.625*s,cx+1.05*s,306,floor+LeadThickness(name)*s,"0,30");
            SmallV(c,442,floor,floor+LeadThickness(name)*s,cx+1.05*s,70,LeadThickness(name).ToString("0.00",Ru));
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static readonly System.Globalization.CultureInfo Ru=System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void SmallH(Canvas c,double a,double b,double y,double origin,string value){var shelf=y-14;Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,a-12,y,b+15,y);Line(c,b+15,y,b+15,shelf);Line(c,b+15,shelf,430,shelf);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,385,shelf);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,190,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,245,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+46:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-145,shelfY,x,shelfY);Label(c,value,x-72,shelfY);}
}
