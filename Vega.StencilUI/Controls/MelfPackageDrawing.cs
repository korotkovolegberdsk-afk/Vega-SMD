using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

internal static class MelfPackageDrawing
{
    public static bool Supports(string? name) => name?.Equals("MELF1206", StringComparison.OrdinalIgnoreCase) == true;

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh()
    {
        const float total=3.50f, diameter=1.50f, cap=.30f;
        const int segments=32;
        var mesh=new List<GlbTriangle>();
        var glass=new Vector4(.42f,.22f,.10f,1);
        var metal=new Vector4(.80f,.80f,.76f,1);
        var band=new Vector4(.08f,.08f,.07f,1);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector4 color)
        {
            mesh.Add(new(a*.001f,b*.001f,c*.001f,color));
            mesh.Add(new(a*.001f,c*.001f,d*.001f,color));
        }
        void Tube(float from,float to,Vector4 color,float radius=diameter/2)
        {
            for(var i=0;i<segments;i++)
            {
                var a=2*MathF.PI*i/segments;var b=2*MathF.PI*(i+1)/segments;
                Vector3 P(float x,float angle)=>new(x,radius*MathF.Cos(angle),radius*MathF.Sin(angle));
                Face(P(from,a),P(to,a),P(to,b),P(from,b),color);
            }
        }
        void Disc(float x,Vector4 color,bool reverse)
        {
            for(var i=0;i<segments;i++)
            {
                var a=2*MathF.PI*i/segments;var b=2*MathF.PI*(i+1)/segments;
                var center=new Vector3(x,0,0);var p=new Vector3(x,diameter/2*MathF.Cos(a),diameter/2*MathF.Sin(a));var q=new Vector3(x,diameter/2*MathF.Cos(b),diameter/2*MathF.Sin(b));
                mesh.Add(reverse?new(center*.001f,q*.001f,p*.001f,color):new(center*.001f,p*.001f,q*.001f,color));
            }
        }
        var left=-total/2;var right=total/2;
        Tube(left,left+cap,metal);Tube(left+cap,right-cap,glass);Tube(right-cap,right,metal);
        Tube(left+cap+.16f,left+cap+.34f,band,diameter/2+.01f);
        Disc(left,metal,true);Disc(right,metal,false);
        return mesh;
    }

    public static double CameraFieldMetres => 512.0/(250*.25)/1000;

    public static void PaintTop(Canvas canvas,double centerX,double centerY,double pixelsPerMillimetre,bool vertical)
    {
        const double total=3.50,diameter=1.50,cap=.30,bandStart=.46,bandWidth=.18;
        var metal=new SolidColorBrush(Color.FromRgb(204,204,194));
        var glass=new SolidColorBrush(Color.FromRgb(107,56,26));
        var black=new SolidColorBrush(Color.FromRgb(20,20,18));
        void Box(double x,double y,double width,double height,Brush fill)
        {
            var rectangle=new Rectangle{Width=width,Height=height,Fill=fill,StrokeThickness=0};
            Canvas.SetLeft(rectangle,x);Canvas.SetTop(rectangle,y);canvas.Children.Add(rectangle);
        }
        if(vertical)
        {
            var x=centerX-diameter*pixelsPerMillimetre/2;var y=centerY-total*pixelsPerMillimetre/2;
            Box(x,y,diameter*pixelsPerMillimetre,total*pixelsPerMillimetre,glass);
            Box(x,y,diameter*pixelsPerMillimetre,cap*pixelsPerMillimetre,metal);
            Box(x,y+(total-cap)*pixelsPerMillimetre,diameter*pixelsPerMillimetre,cap*pixelsPerMillimetre,metal);
            Box(x,y+bandStart*pixelsPerMillimetre,diameter*pixelsPerMillimetre,bandWidth*pixelsPerMillimetre,black);
        }
        else
        {
            var x=centerX-total*pixelsPerMillimetre/2;var y=centerY-diameter*pixelsPerMillimetre/2;
            Box(x,y,total*pixelsPerMillimetre,diameter*pixelsPerMillimetre,glass);
            Box(x,y,cap*pixelsPerMillimetre,diameter*pixelsPerMillimetre,metal);
            Box(x+(total-cap)*pixelsPerMillimetre,y,cap*pixelsPerMillimetre,diameter*pixelsPerMillimetre,metal);
            Box(x+bandStart*pixelsPerMillimetre,y,bandWidth*pixelsPerMillimetre,diameter*pixelsPerMillimetre,black);
        }
    }

    public static void Paint(Canvas canvas,StepProjection projection,Func<Vector2,Point> map)
    {
        GeometryGroup? batch=null;Vector4? previous=null;
        foreach(var t in projection.Triangles)
        {
            if(previous!=t.Color)
            {
                batch=new GeometryGroup{FillRule=FillRule.Nonzero};
                var col=Color.FromRgb((byte)(255*t.Color.X),(byte)(255*t.Color.Y),(byte)(255*t.Color.Z));
                canvas.Children.Add(new Path{Data=batch,Fill=new SolidColorBrush(col),StrokeThickness=0});previous=t.Color;
            }
            var g=new StreamGeometry();var a=map(t.A);var b=map(t.B);var d=map(t.C);
            if(Vector.CrossProduct(b-a,d-a)<0)(b,d)=(d,b);
            using(var c=g.Open()){c.BeginFigure(a,true,true);c.LineTo(b,true,false);c.LineTo(d,true,false);}batch!.Children.Add(g);
        }
    }

    public static UIElement Create(StepProjection projection,StepProjectionKind kind)
    {
        const double left=190,top=150;
        var canvas=new Canvas{Width=512,Height=320,Background=Brushes.Transparent};
        Rect all;
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(canvas,left,top,62.5,false);
            all=new Rect(left-109.375,top-46.875,218.75,93.75);
        }
        else
        {
            var circle=new Ellipse{Width=93.75,Height=93.75,Fill=new SolidColorBrush(Color.FromRgb(204,204,194)),StrokeThickness=0};
            Canvas.SetLeft(circle,left-46.875);Canvas.SetTop(circle,top-46.875);canvas.Children.Add(circle);
            all=new Rect(left-46.875,top-46.875,93.75,93.75);
        }
        if(kind==StepProjectionKind.Top)
        {
            H(canvas,all.Left,all.Right,53,all.Top+all.Height/2,"3,50");
            V(canvas,45,all.Top,all.Bottom,all.Left,"1,50");
            H(canvas,all.Left,all.Left+18.75,252,all.Bottom,"0,30",155);
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=canvas,Margin=new Thickness(8)};
    }

    private static Rect Bounds(IEnumerable<Point> values){var p=values.ToArray();return new Rect(new Point(p.Min(v=>v.X),p.Min(v=>v.Y)),new Point(p.Max(v=>v.X),p.Max(v=>v.Y)));}
    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector direction){direction.Normalize();var normal=new Vector(-direction.Y,direction.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+direction*8+normal*2.5,tip+direction*8-normal*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{FontFamily=new FontFamily("Arial"),FontSize=29,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};t.Inlines.Add(new Run(value));if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-8);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value,double? shelf=null){var direction=Math.Sign(y-origin);Line(c,a,origin,a,y+direction*7);Line(c,b,origin,b,y+direction*7);if(shelf is double s){Line(c,a-15,y,s+90,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,s,y);}else{Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}}
    private static void V(Canvas c,double x,double a,double b,double origin,string value){var direction=Math.Sign(x-origin);Line(c,origin,a,x+direction*7,a);Line(c,origin,b,x+direction*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,x,(a+b)/2,true);}
}
