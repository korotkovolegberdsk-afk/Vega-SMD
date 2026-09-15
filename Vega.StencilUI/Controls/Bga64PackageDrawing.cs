using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
namespace Vega.StencilUI.Controls;

// ST TFBGA64, 5 x 5 mm, 0.50 mm ball pitch; STM32F103x8 datasheet.
// Ball 1 is upper-left in the native top view. TN1204 fixes the same
// orientation for tape with sprocket holes above and feed to the right.
internal static class Bga64PackageDrawing
{
    private const double Body=5.00, Height=1.20, Pitch=.50, Ball=.30; private const int BallsPerSide=8;
    private static readonly Brush BodyBrush=new SolidColorBrush(Color.FromRgb(76,101,124)),SideBrush=new SolidColorBrush(Color.FromRgb(55,75,94)),Metal=new SolidColorBrush(Color.FromRgb(205,211,215)),Outline=new SolidColorBrush(Color.FromRgb(52,67,82));
    public static bool Supports(string? name)=>name?.Equals("TFBGA064P050W500",System.StringComparison.OrdinalIgnoreCase)==true;
    private static IEnumerable<double> Balls(){var c=(BallsPerSide-1)/2d;for(var i=0;i<BallsPerSide;i++)yield return(i-c)*Pitch;}

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh()
    {
        var mesh=new List<GlbTriangle>();var body=new Vector4(.30f,.40f,.49f,1);var metal=new Vector4(.80f,.83f,.84f,1);var mark=new Vector4(.98f,.98f,.96f,1);
        static Vector4 Shade(Vector4 c,float f)=>new(c.X*f,c.Y*f,c.Z*f,c.W);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector4 color){mesh.Add(new(a*.001f,b*.001f,c*.001f,color));mesh.Add(new(a*.001f,c*.001f,d*.001f,color));}
        void Box(float x0,float x1,float y0,float y1,float z0,float z1,Vector4 color,bool facet=false)
        {var top=facet?Shade(color,1.18f):color;var side=facet?Shade(color,.66f):color;Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),side);Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),top);Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),color);Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),side);Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),side);Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),color);}
        Box(-(float)Body/2,(float)Body/2,-(float)Body/2,(float)Body/2,(float)Ball,(float)Height,body,true);
        foreach(var x in Balls())foreach(var y in Balls()){var r=(float)Ball/2;Box((float)x-r,(float)x+r,(float)y-r,(float)y+r,0,(float)Ball,metal,true);}
        Box(-2.25f,-1.95f,-2.25f,-1.95f,(float)Height,(float)Height+.015f,mark);return mesh;
    }

    public static void PaintTop(Canvas canvas,double cx,double cy,double scale,bool bottom=false)
    {
        var body=new Rectangle{Width=Body*scale,Height=Body*scale,Fill=bottom?SideBrush:BodyBrush,Stroke=Outline,StrokeThickness=.9};
        Canvas.SetLeft(body,cx-Body*scale/2);Canvas.SetTop(body,cy-Body*scale/2);canvas.Children.Add(body);
        if(bottom)foreach(var x in Balls())foreach(var y in Balls())AddBall(canvas,cx+x*scale,cy+y*scale,scale);
        else {var dot=new Ellipse{Width=.28*scale,Height=.28*scale,Fill=Brushes.White};Canvas.SetLeft(dot,cx-Body*.44*scale);Canvas.SetTop(dot,cy-Body*.44*scale);canvas.Children.Add(dot);}
    }
    private static void AddBall(Canvas c,double x,double y,double s){var e=new Ellipse{Width=Ball*s,Height=Ball*s,Fill=Metal,Stroke=Outline,StrokeThickness=.45};Canvas.SetLeft(e,x-Ball*s/2);Canvas.SetTop(e,y-Ball*s/2);c.Children.Add(e);}
    public static UIElement Create(StepProjectionKind kind)
    {
        var c=new Canvas{Width=512,Height=370,Background=Brushes.Transparent};const double cx=256,cy=175,s=42,floor=230;
        string F(double v)=>v.ToString("0.00",System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        { PaintTop(c,cx,cy,s,kind==StepProjectionKind.Bottom); if(kind==StepProjectionKind.Top)
          {var l=cx-Body*s/2,r=cx+Body*s/2,t=cy-Body*s/2,b=cy+Body*s/2;H(c,l,r,t-30,t,F(Body));V(c,l-36,t,b,l,F(Body));H(c,cx-3.5*Pitch*s,cx-2.5*Pitch*s,b+37,b,F(Pitch));var ballX=cx+3.5*Pitch*s;SmallV(c,ballX+42,cy-3.5*Pitch*s-Ball*s/2,cy-3.5*Pitch*s+Ball*s/2,ballX,F(Ball));}}
        else
        {var body=new Rectangle{Width=Body*s,Height=(Height-Ball)*s,Fill=BodyBrush,Stroke=Outline,StrokeThickness=.8};Canvas.SetLeft(body,cx-Body*s/2);Canvas.SetTop(body,floor-Height*s);c.Children.Add(body);var ball=new Ellipse{Width=Ball*s,Height=Ball*s,Fill=Metal,Stroke=Outline,StrokeThickness=.7};Canvas.SetLeft(ball,cx-Ball*s/2);Canvas.SetTop(ball,floor-Ball*s);c.Children.Add(ball);V(c,cx-Body*s/2-36,floor-Height*s,floor,cx-Body*s/2,F(Height));H(c,cx-Ball*s/2,cx+Ball*s/2,floor+42,floor,F(Ball));}
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }
    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,System.Windows.Vector d){d.Normalize();var n=new System.Windows.Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double o,string v){var d=Math.Sign(y-o);Line(c,a,o,a,y+d*7);Line(c,b,o,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,v,(a+b)/2,y);}private static void V(Canvas c,double x,double a,double b,double o,string v){var d=Math.Sign(x-o);Line(c,o,a,x+d*7,a);Line(c,o,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,v,x,(a+b)/2,true);}private static void SmallV(Canvas c,double x,double a,double b,double o,string v){Line(c,o,a,x-6,a);Line(c,o,b,x-6,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,v,x+42,(a+b)/2,true);}
}
