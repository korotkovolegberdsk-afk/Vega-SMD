using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vega.CAD;

namespace Vega.StencilUI.Controls;

/// <summary>Shared true gull-wing lead geometry for SOIC and SOT package cards.</summary>
internal static class GullWingLeadDrawing
{
    public static void PaintVerticalTop(Canvas canvas, double centerX, double bodyEdgeY, double tipY,
        double leadWidth, double pxPerMm, Brush fill, Brush? stroke = null)
    {
        var direction = Math.Sign(tipY - bodyEdgeY);
        var reach = Math.Abs(tipY - bodyEdgeY);
        var shoulderY = bodyEdgeY + direction * Math.Min(reach * .38, .24);
        var half = leadWidth / 2;
        // The stamped lead is visibly narrower at the mould body and widens
        // into the outer soldering foot; keep that change readable at card scale.
        var neck = half * .55;
        Point P(double x, double y) => new(x * pxPerMm, y * pxPerMm);

        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            g.BeginFigure(P(centerX - neck, bodyEdgeY), true, true);
            g.LineTo(P(centerX - neck, shoulderY), true, false);
            g.BezierTo(P(centerX - neck, shoulderY + direction * reach * .10),
                P(centerX - half, shoulderY + direction * reach * .15),
                P(centerX - half, shoulderY + direction * reach * .25), true, false);
            g.LineTo(P(centerX - half, tipY), true, false);
            g.LineTo(P(centerX + half, tipY), true, false);
            g.LineTo(P(centerX + half, shoulderY + direction * reach * .25), true, false);
            g.BezierTo(P(centerX + half, shoulderY + direction * reach * .15),
                P(centerX + neck, shoulderY + direction * reach * .10),
                P(centerX + neck, shoulderY), true, false);
            g.LineTo(P(centerX + neck, bodyEdgeY), true, false);
        }
        var path = new Path { Data = geometry, Fill = fill, Stroke = stroke, StrokeThickness = stroke is null ? 0 : .8 };
        Canvas.SetLeft(path, 0); Canvas.SetTop(path, 0); canvas.Children.Add(path);
    }

    public static void PaintHorizontalSide(Canvas canvas, double bodyLeft, double bodyRight, double floor,
        double totalLeft, double totalRight, double bodyHeight, double leadThickness, double pxPerMm, Brush metal)
    {
        foreach (var side in new[] { -1d, 1d })
        {
            var edge = side < 0 ? bodyLeft : bodyRight;
            var tip = side < 0 ? totalLeft : totalRight;
            var shoulder = edge + side * Math.Abs(tip - edge) * .25;
            var footStart = tip - side * Math.Abs(tip - edge) * .40;
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(new Point(edge * pxPerMm, floor - bodyHeight * pxPerMm * .46), false, false);
                g.LineTo(new Point(shoulder * pxPerMm, floor - bodyHeight * pxPerMm * .46), true, false);
                g.BezierTo(new Point((shoulder + side * .10 * Math.Abs(tip-edge)) * pxPerMm, floor - bodyHeight * pxPerMm * .42),
                    new Point((footStart - side * .08 * Math.Abs(tip-edge)) * pxPerMm, floor - leadThickness * pxPerMm),
                    new Point(footStart * pxPerMm, floor - leadThickness * pxPerMm / 2), true, false);
                g.LineTo(new Point(tip * pxPerMm, floor - leadThickness * pxPerMm / 2), true, false);
            }
            canvas.Children.Add(new Path
            {
                Data = geometry, Stroke = metal, StrokeThickness = Math.Max(2, leadThickness * pxPerMm),
                StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            });
        }
    }

    public static void AddVerticalMesh(List<GlbTriangle> mesh, float centerX, float bodyEdgeY, float tipY,
        float leadWidth, float leadThickness, float bodyHeight, Vector4 color)
    {
        void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            mesh.Add(new(a * .001f, b * .001f, c * .001f, color));
            mesh.Add(new(a * .001f, c * .001f, d * .001f, color));
        }
        void Segment(float y0, float z0, float y1, float z1)
        {
            var half = leadWidth / 2;
            var a = new Vector3(centerX-half,y0,z0); var b = new Vector3(centerX+half,y0,z0);
            var c = new Vector3(centerX+half,y1,z1); var d = new Vector3(centerX-half,y1,z1);
            var dz = new Vector3(0,0,leadThickness);
            Face(a,b,c,d); Face(a+dz,d+dz,c+dz,b+dz); Face(a,a+dz,b+dz,b);
            Face(d,c,c+dz,d+dz); Face(a,d,d+dz,a+dz); Face(b,b+dz,c+dz,c);
        }
        var direction = Math.Sign(tipY-bodyEdgeY);
        var reach = Math.Abs(tipY-bodyEdgeY);
        var shoulder = bodyEdgeY + direction * reach * .28f;
        var footStart = tipY - direction * reach * .38f;
        var upper = bodyHeight * .50f;
        Segment(bodyEdgeY,upper,shoulder,upper);
        Segment(shoulder,upper,footStart,0);
        Segment(footStart,0,tipY,0);
    }
}
