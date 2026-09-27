using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Trinket.Models;

namespace Trinket.Rendering;

/// <summary>
/// Turns a BeadDefinition into an actual WPF visual. Beads have no physics
/// of their own - they're drawn wherever an existing rope node already is,
/// so this class is pure rendering, same as CharmRenderer.
/// </summary>
public static class BeadRenderer
{
    public static Path CreateBeadVisual(BeadDefinition definition)
    {
        Geometry geometry = definition.Shape switch
        {
            BeadShape.Pearl => ShapeGeometryFactory.CreateCircleGeometry(definition.Size),
            BeadShape.Round => ShapeGeometryFactory.CreateCircleGeometry(definition.Size),
            BeadShape.Star => ShapeGeometryFactory.CreateStarGeometry(definition.Size),
            BeadShape.Heart => ShapeGeometryFactory.CreateHeartGeometry(definition.Size),
            _ => ShapeGeometryFactory.CreateCircleGeometry(definition.Size)
        };

        Brush fill = definition.Shape == BeadShape.Pearl
            ? CreatePearlBrush(definition.FillColor)
            : new SolidColorBrush(definition.FillColor);

        return new Path
        {
            Data = geometry,
            Fill = fill,
            Stroke = new SolidColorBrush(definition.StrokeColor),
            StrokeThickness = 1,
            Width = definition.Size,
            Height = definition.Size,
            Stretch = Stretch.Uniform
        };
    }

    /// <summary>
    /// A radial gradient from white toward the base color, offset slightly
    /// off-center, giving a simple glossy "pearl sheen" effect without any
    /// image assets.
    /// </summary>
    private static Brush CreatePearlBrush(Color baseColor)
    {
        var brush = new RadialGradientBrush();
        brush.GradientOrigin = new Point(0.35, 0.3);
        brush.Center = new Point(0.5, 0.5);
        brush.RadiusX = 0.6;
        brush.RadiusY = 0.6;
        brush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
        brush.GradientStops.Add(new GradientStop(baseColor, 1.0));
        return brush;
    }
}