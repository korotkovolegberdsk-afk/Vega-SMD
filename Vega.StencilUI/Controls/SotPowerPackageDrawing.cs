using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Diodes Incorporated package drawings: SOT89 Rev. 2026-01-16 and
// SOT223 Rev. 2017-03-06. The same geometry is reused in the tape view.
internal static class SotPowerPackageDrawing
{
    private static readonly Brush Body = new SolidColorBrush(Color.FromRgb(57, 60, 67));
    private static readonly Brush Metal = new SolidColorBrush(Color.FromRgb(218, 218, 211));
    private static readonly Brush Mark = Brushes.White;

    public static bool Supports(string? name) => name is not null &&
        (name.Equals("SOT89", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SOT223", StringComparison.OrdinalIgnoreCase));

    private static bool Is223(string name) => name.Equals("SOT223", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh(string name)
    {
        var mesh = new List<GlbTriangle>();
        var body = new Vector4(.22f, .24f, .27f, 1);
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

        if (Is223(name))
        {
            Box(-3.25f,3.25f,-1.75f,1.75f,.25f,1.60f,body);
            Box(-1.50f,1.50f,1.75f,3.50f,0,.25f,metal);
            foreach (var x in new[] {-2.30f,0f,2.30f}) Box(x-.35f,x+.35f,-3.50f,-1.75f,0,.25f,metal);
            Box(-2.85f,-2.55f,-1.47f,-1.17f,1.598f,1.602f,mark);
        }
        else
        {
            Box(-2.25f,2.25f,-1.25f,1.25f,.30f,1.50f,body);
            Box(-.867f,.867f,1.25f,2.05f,0,.38f,metal);
            foreach (var x in new[] {-1.50f,0f,1.50f})
            {
                var half = x == 0 ? .28f : .24f;
                Box(x-half,x+half,-2.05f,-1.25f,0,.38f,metal);
            }
            Box(-1.93f,-1.63f,-.98f,-.68f,1.498f,1.502f,mark);
        }
        return mesh;
    }

    public static void PaintTop(Canvas canvas, string name, double cx, double cy, double pxPerMm, bool tapeOrientation = false, bool bottom = false)
    {
        var is223 = Is223(name);
        Point P(double x, double y)
        {
            if (tapeOrientation && !is223) { x = -x; y = -y; }
            if (bottom) x = -x;
            return new Point(cx + x * pxPerMm, cy + y * pxPerMm);
        }
        void Box(double x, double y, double w, double h, Brush fill)
        {
            var a=P(x,y); var b=P(x+w,y+h);
            var r=new Rectangle{Width=Math.Abs(b.X-a.X),Height=Math.Abs(b.Y-a.Y),Fill=fill};
            Canvas.SetLeft(r,Math.Min(a.X,b.X)); Canvas.SetTop(r,Math.Min(a.Y,b.Y)); canvas.Children.Add(r);
        }

        if (is223)
        {
            Box(-3.25,-1.75,6.50,3.50,Body);
            Box(-1.50,-3.50,3.00,1.75,Metal);
            foreach(var x in new[]{-2.30,0d,2.30}) Box(x-.35,1.75,.70,1.75,Metal);
            if(!bottom)
            {
                var p=P(-2.80,1.15); var dot=new Ellipse{Width=.30*pxPerMm,Height=.30*pxPerMm,Fill=Mark};
                Canvas.SetLeft(dot,p.X); Canvas.SetTop(dot,p.Y); canvas.Children.Add(dot);
            }
        }
        else
        {
            Box(-2.25,-1.25,4.50,2.50,Body);
            Box(-.867,-2.05,1.734,.80,Metal);
            foreach(var x in new[]{-1.50,0d,1.50}) Box(x-(x==0?.28:.24),1.25,x==0?.56:.48,.80,Metal);
            if(!bottom)
            {
                var p=P(-1.90,.65); var dot=new Ellipse{Width=.30*pxPerMm,Height=.30*pxPerMm,Fill=Mark};
                Canvas.SetLeft(dot,p.X); Canvas.SetTop(dot,p.Y); canvas.Children.Add(dot);
            }
        }
    }

    public static UIElement Create(string name, StepProjectionKind kind)
    {
        var is223=Is223(name); var s=is223?34d:48d; const double cx=256,cy=176;
        var c=new Canvas{Width=512,Height=350,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,name,cx,cy,s,false,kind==StepProjectionKind.Bottom);
            if(kind==StepProjectionKind.Top)
            {
                var halfLength=(is223?6.50:4.50)*s/2;
                var bodyHalf=(is223?3.50:2.50)*s/2;
                var overallHalf=(is223?7.00:4.10)*s/2;
                H(c,cx-halfLength,cx+halfLength,42,cy-bodyHalf,is223?"6,50":"4,50");
                V(c,44,cy-overallHalf,cy+overallHalf,cx-halfLength,is223?"7,00":"4,10");
                V(c,420,cy-bodyHalf,cy+bodyHalf,cx+halfLength,is223?"3,50":"2,50");
                var pitch=(is223?2.30:1.50)*s;
                H(c,cx-pitch,cx,326,cy+overallHalf,is223?"2,30":"1,50");
                SmallH(c,cx+pitch-(is223?.35:.24)*s,cx+pitch+(is223?.35:.24)*s,292,cy+overallHalf,is223?"0,70":"0,48");
            }
        }
        else
        {
            const double floor=224; var height=(is223?1.60:1.50)*s;
            var bodyWidth=(is223?3.50:2.50)*s;
            var totalWidth=(is223?7.00:4.10)*s;
            var leadThickness=(is223?.25:.38)*s;
            var body=new Rectangle{Width=bodyWidth,Height=height-leadThickness,Fill=Body};
            Canvas.SetLeft(body,cx-bodyWidth/2); Canvas.SetTop(body,floor-height); c.Children.Add(body);
            var lead=new Rectangle{Width=totalWidth,Height=leadThickness,Fill=Metal};
            Canvas.SetLeft(lead,cx-totalWidth/2); Canvas.SetTop(lead,floor-leadThickness); c.Children.Add(lead);
            V(c,72,floor-height,floor,cx-bodyWidth/2,is223?"1,60":"1,50");
            SideH(c,cx+bodyWidth/2,cx+totalWidth/2,310,floor,is223?"0,95":"1,05");
            SmallV(c,444,floor-leadThickness,floor,cx+totalWidth/2,76,is223?"0,25":"0,38");
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void SmallH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,a-12,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Line(c,b+15,y,b+15,y-18);Line(c,b+15,y-18,350,y-18);Label(c,value,405,288);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,180,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,238,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+46:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-145,shelfY,x,shelfY);Label(c,value,x-72,shelfY);}
}
