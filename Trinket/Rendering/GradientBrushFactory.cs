using System;
using System.Windows;
using System.Windows.Media;

namespace Trinket.Rendering;

/// <summary>
/// Turns a flat base color into a subtle glossy-looking radial gradient
/// (off-center highlight, true color, slightly darker edge). Used
/// throughout CharmRenderer in place of plain SolidColorBrush fills to add
/// depth without needing per-charm hand-tuning.
/// </summary>
public static class GradientBrushFactory
{
    public static Brush CreateSheenBrush(Color baseColor)
    {
        Color lighter = Lighten(baseColor, 0.35);
        Color darker = Darken(baseColor, 0.14);

        var brush = new RadialGradientBrush();
        brush.GradientOrigin = new Point(0.32, 0.28);
        brush.Center = new Point(0.5, 0.5);
        brush.RadiusX = 0.75;
        brush.RadiusY = 0.75;
        brush.GradientStops.Add(new GradientStop(lighter, 0.0));
        brush.GradientStops.Add(new GradientStop(baseColor, 0.55));
        brush.GradientStops.Add(new GradientStop(darker, 1.0));
        return brush;
    }

    private static Color Lighten(Color c, double amount)
    {
        byte r = (byte)Math.Min(255, c.R + (255 - c.R) * amount);
        byte g = (byte)Math.Min(255, c.G + (255 - c.G) * amount);
        byte b = (byte)Math.Min(255, c.B + (255 - c.B) * amount);
        return Color.FromArgb(c.A, r, g, b);
    }

    private static Color Darken(Color c, double amount)
    {
        byte r = (byte)(c.R * (1 - amount));
        byte g = (byte)(c.G * (1 - amount));
        byte b = (byte)(c.B * (1 - amount));
        return Color.FromArgb(c.A, r, g, b);
    }
}