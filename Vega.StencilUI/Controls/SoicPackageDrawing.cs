using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Diodes Incorporated SO-8 nominal package geometry. Tolerances are retained
// in the source migration but intentionally omitted from the card.
internal static class SoicPackageDrawing
{
    private const double BodyLength=4.90, BodyWidth=3.90, OverallWidth=6.00;
    private const double Height=1.45, Pitch=1.27, LeadWidth=.38, LeadLength=.67, LeadThickness=.175;
    private static readonly Brush Body=new SolidColorBrush(Color.FromRgb(74,91,108));
    private static readonly Brush BodyOutline=new SolidColorBrush(Color.FromRgb(52,67,82));
    private static readonly Brush Metal=new SolidColorBrush(Color.FromRgb(211,214,211));
    private static readonly Brush Mark=Brushes.White;

    public static bool Supports(string? name)=>name is not null &&
        (name.Equals("SOIC8",StringComparison.OrdinalIgnoreCase) ||
         name.Equals("SO08P127W60",StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<double> PinYs()
    {
        yield return -1.5*Pitch; yield return -.5*Pitch; yield return .5*Pitch; yield return 1.5*Pitch;
    }

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh()
    {
        var mesh=new List<GlbTriangle>(); var body=new Vector4(.29f,.36f,.43f,1); var metal=new Vector4(.83f,.84f,.82f,1); var mark=new Vector4(.98f,.98f,.96f,1);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector4 color){mesh.Add(new(a*.001f,b*.001f,c*.001f,color));mesh.Add(new(a*.001f,c*.001f,d*.001f,color));}
        static Vector4 Shade(Vector4 c,float f)=>new(c.X*f,c.Y*f,c.Z*f,c.W);
        void Box(float x0,float x1,float y0,float y1,float z0,float z1,Vector4 color,bool faceted=false)
        {
            var top=faceted?Shade(color,1.22f):color;var front=faceted?Shade(color,.94f):color;var back=faceted?Shade(color,.76f):color;var side=faceted?Shade(color,.62f):color;
            Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),side);Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),top);
            Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),front);Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),back);
            Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),side);Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),front);
        }
        void LeadSegment(float x0,float z0,float x1,float z1,float centerY)
        {
            var half=(float)LeadWidth/2;var t=(float)LeadThickness;
            var a=new Vector3(x0,centerY-half,z0);var b=new Vector3(x0,centerY+half,z0);
            var c=new Vector3(x1,centerY+half,z1);var d=new Vector3(x1,centerY-half,z1);
            var aa=a+new Vector3(0,0,t);var bb=b+new Vector3(0,0,t);var cc=c+new Vector3(0,0,t);var dd=d+new Vector3(0,0,t);
            Face(a,b,c,d,metal);Face(aa,dd,cc,bb,metal);Face(a,aa,bb,b,metal);Face(d,c,cc,dd,metal);Face(a,d,dd,aa,metal);Face(b,bb,cc,c,metal);
        }
        Box(-(float)BodyWidth/2,(float)BodyWidth/2,-(float)BodyLength/2,(float)BodyLength/2,.10f,(float)Height,body,true);
        foreach(var y in PinYs()) foreach(var side in new[]{-1f,1f})
        {
            var edge=side*(float)BodyWidth/2;var shoulder=edge+side*.23f;var footStart=side*((float)OverallWidth/2-(float)LeadLength);var tip=side*(float)OverallWidth/2;
            LeadSegment(edge,.72f,shoulder,.72f,(float)y);
            LeadSegment(shoulder,.72f,footStart,0, (float)y);
            LeadSegment(footStart,0,tip,0,(float)y);
        }
        Box(-1.48f,-1.18f,-2.05f,-1.75f,(float)Height-.004f,(float)Height+.004f,mark);
        return mesh;
    }

    public static void PaintTop(Canvas c,double cx,double cy,double scale,bool bottom=false)
    {
        void Box(double x,double y,double w,double h,Brush fill,Brush stroke){var r=new Rectangle{Width=w*scale,Height=h*scale,Fill=fill,Stroke=stroke,StrokeThickness=.8};Canvas.SetLeft(r,cx+x*scale);Canvas.SetTop(r,cy+y*scale);c.Children.Add(r);}
        void Lead(double centerY,double side)
        {
            var edge=side*BodyWidth/2;var tip=side*OverallWidth/2;var shoulder=edge+side*.24;var half=LeadWidth/2;var neck=half*.55;
            Point P(double x,double y)=>new(cx+x*scale,cy+y*scale);
            var g=new StreamGeometry();
            using(var x=g.Open())
            {
                x.BeginFigure(P(edge,centerY-neck),true,true);
                x.LineTo(P(shoulder,centerY-neck),true,false);
                x.BezierTo(P(shoulder+side*.10,centerY-neck),P(shoulder+side*.13,centerY-half),P(shoulder+side*.24,centerY-half),true,false);
                x.LineTo(P(tip,centerY-half),true,false);
                x.LineTo(P(tip,centerY+half),true,false);
                x.LineTo(P(shoulder+side*.24,centerY+half),true,false);
                x.BezierTo(P(shoulder+side*.13,centerY+half),P(shoulder+side*.10,centerY+neck),P(shoulder,centerY+neck),true,false);
                x.LineTo(P(edge,centerY+neck),true,false);
            }
            c.Children.Add(new Path{Data=g,Fill=Metal,Stroke=ComponentCardPalette.Border,StrokeThickness=.8});
        }
        Box(-BodyWidth/2,-BodyLength/2,BodyWidth,BodyLength,Body,BodyOutline);
        foreach(var y in PinYs()) foreach(var side in new[]{-1d,1d}) Lead(y,side);
        if(!bottom){var dot=new Ellipse{Width=.28*scale,Height=.28*scale,Fill=Mark};Canvas.SetLeft(dot,cx-1.52*scale);Canvas.SetTop(dot,cy-2.08*scale);c.Children.Add(dot);}
    }

    public static UIElement Create(StepProjectionKind kind)
    {
        const double cx=256,cy=195,s=40;var c=new Canvas{Width=512,Height=350,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,cx,cy,s,kind==StepProjectionKind.Bottom);
            if(kind==StepProjectionKind.Top)
            {
                H(c,cx-OverallWidth*s/2,cx+OverallWidth*s/2,45,cy-BodyLength*s/2,"6,00");
                H(c,cx-BodyWidth*s/2,cx+BodyWidth*s/2,90,cy-BodyLength*s/2,"3,90");
                V(c,58,cy-BodyLength*s/2,cy+BodyLength*s/2,cx-OverallWidth*s/2,"4,90");
                V(c,438,cy-1.5*Pitch*s,cy-.5*Pitch*s,cx+OverallWidth*s/2,"1,27",true);
                SmallV(c,474,cy+1.5*Pitch*s-LeadWidth*s/2,cy+1.5*Pitch*s+LeadWidth*s/2,cx+OverallWidth*s/2,320,"0,38");
            }
        }
        else
        {
            const double floor=230;var bodyW=BodyWidth*s;var totalW=OverallWidth*s;var h=Height*s;var t=LeadThickness*s;
            var body=new Rectangle{Width=bodyW,Height=h,Fill=Body,Stroke=BodyOutline,StrokeThickness=.8};Canvas.SetLeft(body,cx-bodyW/2);Canvas.SetTop(body,floor-h);c.Children.Add(body);
            foreach(var side in new[]{-1d,1d})
            {
                var edge=cx+side*bodyW/2;var shoulder=edge+side*.23*s;var footStart=cx+side*(OverallWidth/2-LeadLength)*s;var tip=cx+side*OverallWidth*s/2;
                var g=new StreamGeometry();
                using(var x=g.Open())
                {
                    x.BeginFigure(new Point(edge,floor-h*.48),false,false);
                    x.LineTo(new Point(shoulder,floor-h*.48),true,false);
                    x.BezierTo(new Point(shoulder+side*.12*s,floor-h*.46),new Point(footStart-side*.10*s,floor-t),new Point(footStart,floor-t/2),true,false);
                    x.LineTo(new Point(tip,floor-t/2),true,false);
                }
                c.Children.Add(new Path{Data=g,Stroke=Metal,StrokeThickness=t,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,StrokeLineJoin=PenLineJoin.Round});
            }
            V(c,82,floor-h,floor,cx-bodyW/2,"1,45");
            SideH(c,cx+totalW/2-LeadLength*s,cx+totalW/2,304,floor,"0,67");
            SmallV(c,444,floor-t,floor,cx+totalW/2,72,"0,175");
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=25,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-10);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+48:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-145,shelfY,x,shelfY);Label(c,value,x-72,shelfY);}
    private static void SideH(Canvas c,double a,double b,double y,double origin,string value){Line(c,a,origin,a,y+7);Line(c,b,origin,b,y+7);Line(c,185,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,230,y);}
}
