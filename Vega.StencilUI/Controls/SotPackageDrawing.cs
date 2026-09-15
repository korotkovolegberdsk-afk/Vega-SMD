using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Nominal SOT23 geometry from Nexperia SOT23 package information (2022-10-12).
internal static class SotPackageDrawing
{
    public static bool Supports(string? name) => name?.Equals("SOT23",StringComparison.OrdinalIgnoreCase)==true;
    private static readonly Brush Body=new SolidColorBrush(Color.FromRgb(103,107,116));
    private static readonly Brush Metal=new SolidColorBrush(Color.FromRgb(210,210,202));
    private static readonly Brush Mark=new SolidColorBrush(Color.FromRgb(225,225,218));

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh()
    {
        var mesh=new List<GlbTriangle>();
        var body=new Vector4(.40f,.42f,.46f,1);var metal=new Vector4(.82f,.82f,.79f,1);var mark=new Vector4(.90f,.90f,.86f,1);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector4 color){mesh.Add(new(a*.001f,b*.001f,c*.001f,color));mesh.Add(new(a*.001f,c*.001f,d*.001f,color));}
        void Box(float x0,float x1,float y0,float y1,float z0,float z1,Vector4 color)
        {
            Face(new(x0,y0,z0),new(x1,y0,z0),new(x1,y1,z0),new(x0,y1,z0),color);Face(new(x0,y0,z1),new(x0,y1,z1),new(x1,y1,z1),new(x1,y0,z1),color);
            Face(new(x0,y0,z0),new(x0,y0,z1),new(x1,y0,z1),new(x1,y0,z0),color);Face(new(x0,y1,z0),new(x1,y1,z0),new(x1,y1,z1),new(x0,y1,z1),color);
            Face(new(x0,y0,z0),new(x0,y1,z0),new(x0,y1,z1),new(x0,y0,z1),color);Face(new(x1,y0,z0),new(x1,y0,z1),new(x1,y1,z1),new(x1,y1,z0),color);
        }
        Box(-1.45f,1.45f,-.65f,.65f,.05f,1.03f,body);
        foreach(var pin in new[]{(-.95f,1f),(.95f,1f),(0f,-1f)})
            GullWingLeadDrawing.AddVerticalMesh(mesh,pin.Item1,pin.Item2<0?-.65f:.65f,pin.Item2<0?-1.20f:1.20f,.40f,.12f,1.03f,metal);
        Box(-1.18f,-.98f,-.53f,-.33f,1.001f,1.006f,mark);
        return mesh;
    }

    public static double CameraFieldMetres => 5.6/1000;

    public static void PaintTop(Canvas canvas,double cx,double cy,double pxPerMm,bool vertical)
    {
        void Box(double x,double y,double w,double h,Brush fill){var r=new Rectangle{Width=w,Height=h,Fill=fill,StrokeThickness=0};Canvas.SetLeft(r,x);Canvas.SetTop(r,y);canvas.Children.Add(r);}
        if(!vertical)
        {
            Box(cx-1.45*pxPerMm,cy-.65*pxPerMm,2.90*pxPerMm,1.30*pxPerMm,Body);
            foreach(var pin in new[]{(-.95,1d),(.95,1d),(0d,-1d)})
                GullWingLeadDrawing.PaintVerticalTop(canvas,cx+pin.Item1*pxPerMm,cy+(pin.Item2<0?-.65:.65)*pxPerMm,
                    cy+(pin.Item2<0?-1.20:1.20)*pxPerMm,.40*pxPerMm,1,Metal,ComponentCardPalette.Border);
            var dot=new Ellipse{Width=.18*pxPerMm,Height=.18*pxPerMm,Fill=Mark};Canvas.SetLeft(dot,cx-1.18*pxPerMm);Canvas.SetTop(dot,cy+.35*pxPerMm);canvas.Children.Add(dot);
        }
        else
        {
            Box(cx-.65*pxPerMm,cy-1.45*pxPerMm,1.30*pxPerMm,2.90*pxPerMm,Body);
            foreach(var pin in new[]{(-.95,1d),(.95,1d),(0d,-1d)})
                GullWingLeadDrawing.PaintVerticalTop(canvas,cy-pin.Item1*pxPerMm,cx+(pin.Item2<0?-.65:.65)*pxPerMm,
                    cx+(pin.Item2<0?-1.20:1.20)*pxPerMm,.40*pxPerMm,1,Metal,ComponentCardPalette.Border);
            var dot=new Ellipse{Width=.18*pxPerMm,Height=.18*pxPerMm,Fill=Mark};Canvas.SetLeft(dot,cx-.53*pxPerMm);Canvas.SetTop(dot,cy+1.00*pxPerMm);canvas.Children.Add(dot);
        }
    }

    public static UIElement Create(StepProjectionKind kind)
    {
        const double cx=256,cy=165,s=80;
        var c=new Canvas{Width=512,Height=320,Background=Brushes.Transparent};
        if(kind is StepProjectionKind.Top or StepProjectionKind.Bottom)
        {
            PaintTop(c,cx,cy,s,false);
            if(kind==StepProjectionKind.Top)
            {
                H(c,cx-1.45*s,cx+1.45*s,50,cy-.65*s,"2,90");
                // E1 (overall lead span) is on the left and E (body width)
                // is on the right in the manufacturer package drawing.
                V(c,72,cy-1.20*s,cy+1.20*s,cx-1.45*s,"2,40");
                V(c,438,cy-.65*s,cy+.65*s,cx+1.45*s,"1,30",true);
                H(c,cx-.95*s,cx+.95*s,305,cy+1.20*s,"1,90");
                // Lead width b belongs to the top projection. Attach the
                // extension lines directly to the upper lead, as in the PDF.
                LeadWidthH(c,cx-.20*s,cx+.20*s,105,cy-1.20*s,"0,40");
            }
        }
        else
        {
            const double floor=205;
            var body=new Rectangle{Width=1.3*s,Height=.98*s,Fill=Body};Canvas.SetLeft(body,cx-.65*s);Canvas.SetTop(body,floor-.98*s);c.Children.Add(body);
            GullWingLeadDrawing.PaintHorizontalSide(c,cx/s-.65,cx/s+.65,floor,cx/s-1.20,cx/s+1.20,.98,.12,s,Metal);
            V(c,92,floor-.98*s,floor+.05*s,cx-.65*s,"1,03");
            SmallH(c,cx+.65*s,cx+1.20*s,282,floor+.12*s,"0,55");
            SmallV(c,430,floor,floor+.12*s,cx+1.20*s,58,"0,12");
        }
        return new Viewbox{Stretch=Stretch.Uniform,Child=c,Margin=new Thickness(8)};
    }

    private static void Line(Canvas c,double x,double y,double xx,double yy)=>c.Children.Add(new Line{X1=x,Y1=y,X2=xx,Y2=yy,Stroke=ComponentCardPalette.Dimension,StrokeThickness=1.1});
    private static void Arrow(Canvas c,Point tip,Vector d){d.Normalize();var n=new Vector(-d.Y,d.X);c.Children.Add(new Polygon{Fill=ComponentCardPalette.Dimension,Points=new PointCollection{tip,tip+d*8+n*2.5,tip+d*8-n*2.5}});}
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false){var t=new TextBlock{Text=value,FontFamily=new FontFamily("Arial"),FontSize=27,Foreground=ComponentCardPalette.Dimension,Background=ComponentCardPalette.Projection,Padding=new Thickness(3,0,3,0)};if(vertical)t.LayoutTransform=new RotateTransform(-90);t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-12);c.Children.Add(t);}
    private static void H(Canvas c,double a,double b,double y,double origin,string value,double? shelf=null){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);if(shelf is double sh){Line(c,a-12,y,sh+62,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,sh,y-12);}else{Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}}
    private static void SmallH(Canvas c,double a,double b,double y,double origin,string value,bool shelfLeft=true){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);if(shelfLeft){Line(c,a-110,y,b+15,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,a-62,y);}else{Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}}
    private static void LeadWidthH(Canvas c,double a,double b,double y,double origin,string value){var d=Math.Sign(y-origin);Line(c,a,origin,a,y+d*7);Line(c,b,origin,b,y+d*7);Line(c,a-12,y,b+112,y);Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,b+67,y);}
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false){var d=Math.Sign(x-origin);Line(c,origin,a,x+d*7,a);Line(c,origin,b,x+d*7,b);Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));Label(c,value,right?x+48:x,(a+b)/2,true);}
    private static void SmallV(Canvas c,double x,double a,double b,double origin,double shelfY,string value){Line(c,origin,a,x+7,a);Line(c,origin,b,x+7,b);Line(c,x,shelfY,x,b+16);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));Line(c,x-160,shelfY,x,shelfY);Label(c,value,x-80,shelfY);}
}
