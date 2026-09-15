using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;
using Vector = System.Windows.Vector;

namespace Vega.StencilUI.Controls;

// Limit dimensions from Diodes Incorporated SOD123, revision 2017-03-16.
// A documented parametric model is used because the generic KiCad STEP has
// a 0.20 mm terminal and does not satisfy the 0.10–0.15 mm source limits.
internal static class SodPackageDrawing
{
    private sealed record PackageSpec(
        float BodyLength, float BodyWidth, float OverallLength, float OverallHeight,
        float BodyBottom, float LeadWidth, float LeadThickness, float FootLength,
        string OverallLengthText, string BodyLengthText, string BodyWidthText,
        string LeadWidthText, string HeightText, string LeadThicknessText, string FootLengthText);

    private static readonly IReadOnlyDictionary<string, PackageSpec> Specs =
        new Dictionary<string, PackageSpec>(StringComparer.OrdinalIgnoreCase)
        {
            ["SOD123"] = new(2.65f, 1.55f, 3.65f, 1.05f, .05f, .57f, .11f, .30f,
                "3,65", "2,65", "1,55", "0,57", "1,05", "0,11", "0,30"),
            ["SOD323"] = new(1.70f, 1.30f, 2.50f, 1.10f, .05f, .30f, .11f, .30f,
                "2,50", "1,70", "1,30", "0,30", "1,05", "0,11", "0,30"),
            ["SOD523"] = new(1.20f, .80f, 1.60f, .60f, 0, .30f, .14f, .20f,
                "1,60", "1,20", "0,80", "0,30", "0,60", "0,14", "0,20"),
            ["SOD723"] = new(1.00f, .60f, 1.40f, .52f, 0, .285f, .115f, .20f,
                "1,40", "1,00", "0,60", "0,29", "0,52", "0,12", "0,20"),
            ["SOD923"] = new(.80f, .60f, 1.00f, .37f, 0, .20f, .12f, .10f,
                "1,00", "0,80", "0,60", "0,20", "0,37", "0,12", "0,10"),
            ["SMA"] = new(4.30f, 2.605f, 5.195f, 2.18f, .125f, 1.45f, .23f, 1.14f,
                "5,20", "4,30", "2,61", "1,45", "2,18", "0,23", "1,14"),
            ["SMB"] = new(4.315f, 3.62f, 5.295f, 2.25f, .125f, 2.085f, .23f, 1.14f,
                "5,30", "4,32", "3,62", "2,09", "2,25", "0,23", "1,14"),
            ["SMC"] = new(6.855f, 5.905f, 7.94f, 2.25f, .15f, 2.965f, .23f, 1.14f,
                "7,94", "6,86", "5,91", "2,97", "2,25", "0,23", "1,14")
        };

    public static bool Supports(string? name) => name is not null && Specs.ContainsKey(name);

    public static IReadOnlyList<GlbTriangle> BuildDocumentedMesh(string packageName)
    {
        if (!Supports(packageName)) throw new ArgumentException("Unsupported documented SOD package.", nameof(packageName));
        var spec=Specs[packageName];
        var bodyHalfLength=spec.BodyLength/2;
        var bodyHalfWidth=spec.BodyWidth/2;
        var envelopeHalfLength=spec.OverallLength/2;
        var height=spec.OverallHeight;
        var middle=(spec.BodyBottom+height)/2;
        var terminalHalfWidth=spec.LeadWidth/2;
        var mesh = new List<GlbTriangle>();
        var bodyColor = new Vector4(.46f,.47f,.50f,1);
        var metalColor = new Vector4(.82f,.82f,.78f,1);
        void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector4 color)
        {
            mesh.Add(new(a*.001f,b*.001f,c*.001f,color));
            mesh.Add(new(a*.001f,c*.001f,d*.001f,color));
        }
        // Rounded, 7-degree drafted mould: maximum body is 2.65 x 1.55,
        // total height 1.05, standoff 0.05. Centre seam is the widest section.
        var rings = new List<Vector3[]>();
        foreach(var z in new[]{spec.BodyBottom,middle,height})
        {
            var inset=MathF.Abs(z-middle)*MathF.Tan(7*MathF.PI/180);
            var hx=bodyHalfLength-inset;var hy=bodyHalfWidth-inset;
            var points=new List<Vector3>();
            for(var corner=0;corner<4;corner++)
            {
                var sx=corner is 0 or 3?1:-1;var sy=corner<2?1:-1;
                for(var i=0;i<=8;i++)
                {
                    var angle=(corner*90+i*90f/8)*MathF.PI/180;
                    points.Add(new(sx*(hx-.1f)+.1f*MathF.Cos(angle),sy*(hy-.1f)+.1f*MathF.Sin(angle),z));
                }
            }
            rings.Add(points.ToArray());
        }
        for(var r=0;r<2;r++)for(var i=0;i<rings[r].Length;i++)
            Face(rings[r][i],rings[r][(i+1)%rings[r].Length],rings[r+1][(i+1)%rings[r].Length],rings[r+1][i],bodyColor);
        foreach(var ring in new[]{rings[0],rings[2]})
            for(var i=0;i<ring.Length;i++)
                mesh.Add(new(new Vector3(0,0,ring[0].Z)*.001f,ring[i]*.001f,ring[(i+1)%ring.Length]*.001f,bodyColor));
        // Cathode band is a surface marking on the left end of the top face.
        var bandWidth=MathF.Min(.18f,spec.BodyLength*.12f);
        var bandInset=MathF.Min(.185f,spec.BodyLength*.14f);
        var markLeft=-bodyHalfLength+bandInset;var markRight=markLeft+bandWidth;var markHalfWidth=MathF.Max(.08f,bodyHalfWidth-MathF.Min(.125f,spec.BodyWidth*.16f));
        Face(new(markLeft,-markHalfWidth,height+.0001f),new(markRight,-markHalfWidth,height+.0001f),new(markRight,markHalfWidth,height+.0001f),new(markLeft,markHalfWidth,height+.0001f),new(.70f,.69f,.63f,1));
        foreach(var sign in new[]{-1f,1f})
        {
            var rise=MathF.Max(0,middle-spec.LeadThickness);
            var bendStart=bodyHalfLength-MathF.Min(.075f,spec.BodyLength*.06f);
            var bendEnd=envelopeHalfLength-spec.FootLength;
            var profile=new List<Vector2>{new(bodyHalfLength-MathF.Min(.175f,spec.BodyLength*.12f),rise)};
            for(var i=0;i<=16;i++)
            {
                var t=i/16f;var easing=t*t*(3-2*t);
                profile.Add(new(bendStart+(bendEnd-bendStart)*t,rise*(1-easing)));
            }
            profile.Add(new(envelopeHalfLength,0));
            for(var i=0;i<profile.Count-1;i++)
            {
                var p=profile[i];var q=profile[i+1];
                Vector3 P(Vector2 v,float y,float dz)=>new(sign*v.X,y,v.Y+dz);
                Face(P(p,-terminalHalfWidth,0),P(q,-terminalHalfWidth,0),P(q,terminalHalfWidth,0),P(p,terminalHalfWidth,0),metalColor);
                Face(P(p,-terminalHalfWidth,spec.LeadThickness),P(p,terminalHalfWidth,spec.LeadThickness),P(q,terminalHalfWidth,spec.LeadThickness),P(q,-terminalHalfWidth,spec.LeadThickness),metalColor);
                foreach(var y in new[]{-terminalHalfWidth,terminalHalfWidth})Face(P(p,y,0),P(p,y,spec.LeadThickness),P(q,y,spec.LeadThickness),P(q,y,0),metalColor);
            }
            foreach(var p in new[]{profile[0],profile[^1]})Face(new(sign*p.X,-terminalHalfWidth,p.Y),new(sign*p.X,terminalHalfWidth,p.Y),new(sign*p.X,terminalHalfWidth,p.Y+spec.LeadThickness),new(sign*p.X,-terminalHalfWidth,p.Y+spec.LeadThickness),metalColor);
        }
        return mesh;
    }
    private const double Left = 125;
    private const double Top = 150;
    private static readonly Brush Dimension = ComponentCardPalette.Dimension;

    // Projection coordinates arrive at 250 units/mm.  Fit every package to
    // the same useful drawing width, while preserving the approved SOD123
    // scale exactly.  The resulting factor is shared by all 2D views and 3D.
    private static double DrawingScale(string packageName)
    {
        if (packageName.Equals("SOD123", StringComparison.OrdinalIgnoreCase)) return .25;
        return Math.Clamp(.88 / Specs[packageName].OverallLength, .10, .90);
    }

    public static double CameraFieldMetres(string packageName)
    {
        if (packageName.Equals("SOD123", StringComparison.OrdinalIgnoreCase)) return 512.0 / 62.5 / 1000;
        var visibleMillimetres = 512.0 / (250 * DrawingScale(packageName));
        return visibleMillimetres / 1000;
    }

    public static void Paint(Canvas canvas, StepProjection projection, Func<Vector2,Point> map)
    {
        GeometryGroup? batch = null;
        Vector4? previous = null;
        foreach(var t in projection.Triangles)
        {
            if(previous != t.Color)
            {
                batch = new GeometryGroup { FillRule = FillRule.Nonzero };
                var col=Color.FromRgb((byte)(255*t.Color.X),(byte)(255*t.Color.Y),(byte)(255*t.Color.Z));
                canvas.Children.Add(new Path { Data=batch,Fill=new SolidColorBrush(col),StrokeThickness=0 });
                previous=t.Color;
            }
            var geometry=new StreamGeometry();
            var a=map(t.A);var b=map(t.B);var d=map(t.C);
            if(Vector.CrossProduct(b-a,d-a)<0)(b,d)=(d,b);
            using(var ctx=geometry.Open()){ctx.BeginFigure(a,true,true);ctx.LineTo(b,true,false);ctx.LineTo(d,true,false);}
            batch!.Children.Add(geometry);
        }
    }

    public static UIElement Create(StepProjection projection, StepProjectionKind kind, string packageName)
    {
        var spec=Specs[packageName];
        var canvas = new Canvas { Width = 512, Height = 320, Background = Brushes.Transparent };
        var scale=DrawingScale(packageName);
        Point Map(Vector2 p) => new(Left + p.X * scale, Top + p.Y * scale);
        Paint(canvas,projection,Map);
        var groups = projection.Triangles.GroupBy(t => t.Color).Select(g => new
        {
            Color = g.Key,
            Points = g.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray()
        }).Select(g => new
        {
            g.Color, g.Points,
            Bounds = Bounds(g.Points.Select(Map))
        }).ToArray();
        var all = Bounds(projection.Triangles.SelectMany(t => new[] { t.A, t.B, t.C }).Select(Map));
        // The moulded body occupies the largest projected area. Terminals can
        // span the envelope but have much less actual triangle area.
        var bodyColor = projection.Triangles.GroupBy(t => t.Color)
            .OrderByDescending(g => g.Sum(t => Math.Abs((t.B.X-t.A.X)*(t.C.Y-t.A.Y)-(t.B.Y-t.A.Y)*(t.C.X-t.A.X))))
            .First().Key;
        var body = groups.Single(g => g.Color == bodyColor).Bounds;
        var terminalPoints = groups.Where(g => g.Color != bodyColor)
            .SelectMany(g => g.Points).Select(Map).Where(p => p.X < body.Left - 0.5).ToArray();
        if (kind == StepProjectionKind.Top)
        {
            H(canvas, all.Left, all.Right, 53, all.Top + all.Height / 2, spec.OverallLengthText);
            H(canvas, body.Left, body.Right, 108, body.Top, spec.BodyLengthText);
            V(canvas, 82, body.Top, body.Bottom, body.Left, spec.BodyWidthText);
            var terminal = Bounds(terminalPoints);
            V(canvas, 429, terminal.Top, terminal.Bottom, all.Right, spec.LeadWidthText, true);
        }
        else if (kind == StepProjectionKind.Longitudinal)
        {
            var bodyHeightOnly=packageName.Equals("SOD323",StringComparison.OrdinalIgnoreCase);
            V(canvas, 82, bodyHeightOnly?body.Top:all.Top, bodyHeightOnly?body.Bottom:all.Bottom, body.Left, spec.HeightText);
            var end = terminalPoints.Where(p => Math.Abs(p.X-all.Left) < .1).ToArray();
            var endBounds = Bounds(end);
            // Thickness is vertical on the flat soldering foot, never horizontal.
            SmallVertical(canvas, 400, endBounds.Top, endBounds.Bottom, all.Right, 53, spec.LeadThicknessText);
            var footEnd = terminalPoints.Where(p => Math.Abs(p.Y-all.Bottom) < .0001).Max(p => p.X);
            H(canvas, all.Left, footEnd, 286, all.Bottom, spec.FootLengthText, 264);
        }
        return new Viewbox { Stretch = Stretch.Uniform, Child = canvas, Margin = new Thickness(8) };
    }

    private static Rect Bounds(IEnumerable<Point> values)
    {
        var p = values.ToArray();
        return new Rect(new Point(p.Min(v=>v.X),p.Min(v=>v.Y)),new Point(p.Max(v=>v.X),p.Max(v=>v.Y)));
    }
    private static void Line(Canvas c,double x,double y,double xx,double yy) => c.Children.Add(new Line { X1=x,Y1=y,X2=xx,Y2=yy,Stroke=Dimension,StrokeThickness=1.1 });
    private static void Arrow(Canvas c,Point tip,Vector direction)
    {
        direction.Normalize(); var normal = new Vector(-direction.Y,direction.X);
        c.Children.Add(new Polygon { Fill=Dimension,Points=new PointCollection { tip,tip+direction*8+normal*2.5,tip+direction*8-normal*2.5 } });
    }
    private static void Label(Canvas c,string value,double x,double y,bool vertical=false)
    {
        var t=new TextBlock { FontFamily=new FontFamily("Arial"),FontSize=29,Foreground=Dimension };
        t.Inlines.Add(new Run(value));
        if(vertical)t.LayoutTransform=new RotateTransform(-90);
        t.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
        Canvas.SetLeft(t,vertical?x-t.DesiredSize.Width-8:x-t.DesiredSize.Width/2);
        Canvas.SetTop(t,vertical?y-t.DesiredSize.Height/2:y-t.DesiredSize.Height-8);
        c.Children.Add(t);
    }
    private static void H(Canvas c,double a,double b,double y,double origin,string value,double? shelf=null)
    {
        var direction=Math.Sign(y-origin);
        Line(c,a,origin,a,y+direction*7);Line(c,b,origin,b,y+direction*7);
        if(shelf is double s)
        {
            Line(c,a-15,y,s+90,y);
            Arrow(c,new(a,y),new(-1,0));Arrow(c,new(b,y),new(1,0));Label(c,value,s,y);
        }
        else {Line(c,a,y,b,y);Arrow(c,new(a,y),new(1,0));Arrow(c,new(b,y),new(-1,0));Label(c,value,(a+b)/2,y);}
    }
    private static void V(Canvas c,double x,double a,double b,double origin,string value,bool right=false)
    {
        var direction=Math.Sign(x-origin);
        Line(c,origin,a,x+direction*7,a);Line(c,origin,b,x+direction*7,b);
        Line(c,x,a,x,b);Arrow(c,new(x,a),new(0,1));Arrow(c,new(x,b),new(0,-1));
        Label(c,value,right?x+53:x,(a+b)/2,true);
    }
    private static void SmallVertical(Canvas c,double x,double a,double b,double origin,double shelfY,string value)
    {
        Line(c,origin,a,x+(x<origin?-7:7),a);Line(c,origin,b,x+(x<origin?-7:7),b);
        Line(c,x,shelfY,x,b+18);Arrow(c,new(x,a),new(0,-1));Arrow(c,new(x,b),new(0,1));
        var right=x<256;
        var length = shelfY>80?290:184;
        Line(c,x,shelfY,x+(right?length:-length),shelfY);Label(c,value,x+(right?length/2:-length/2),shelfY);
    }
}
