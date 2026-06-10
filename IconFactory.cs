using AppKit;
using CoreGraphics;

namespace Pomu;

static class IconFactory
{
    const float IconSize = 18f;
    const int LeafCount = 5;

    public static NSImage MenuBarIcon()
    {
        var image = NSImage.ImageWithSize(new CGSize(IconSize, IconSize), false, _ =>
        {
            DrawTomato();
            return true;
        });
        image.Template = true;
        return image;
    }

    static void DrawTomato()
    {
        NSColor.Black.SetFill();

        var body = NSBezierPath.FromOvalInRect(new CGRect(2f, 1.5f, 14f, 12.5f));
        body.Fill();

        DrawCalyx(new CGPoint(9f, 13.5f));
    }

    static void DrawCalyx(CGPoint center)
    {
        const float innerRadius = 1.6f;
        const float outerRadius = 4.2f;
        const float startAngle = 90f;

        var path = new NSBezierPath();
        for (int i = 0; i < LeafCount; i++)
        {
            float angle = startAngle + i * (360f / LeafCount);
            var tip = PointOnCircle(center, outerRadius, angle);
            var left = PointOnCircle(center, innerRadius, angle - 36f);
            var right = PointOnCircle(center, innerRadius, angle + 36f);

            path.MoveTo(center);
            path.LineTo(left);
            path.LineTo(tip);
            path.LineTo(right);
            path.ClosePath();
        }
        path.Fill();
    }

    static CGPoint PointOnCircle(CGPoint center, float radius, float degrees)
    {
        double radians = degrees * Math.PI / 180.0;
        return new CGPoint(
            center.X + radius * (float)Math.Cos(radians),
            center.Y + radius * (float)Math.Sin(radians));
    }
}
