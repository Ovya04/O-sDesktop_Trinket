using System;
using System.Windows;
    using System.Globalization; // add to the top of the fileusing System.Globalization; // add to the top of the file
using System.Windows.Media;

namespace Trinket.Rendering;

/// <summary>
/// Shared, reusable geometry construction for simple original vector shapes.
/// Each method builds one SINGLE-COLOR shape as a Geometry, meant for a
/// single Path. Multi-color charms (evil eye, cherries, sunflower, mushroom,
/// rainbow, disco ball) are composed from several of these plus WPF Shape
/// elements directly in CharmRenderer's composite builders instead.
/// </summary>
public static class ShapeGeometryFactory
{
    public static Geometry CreateCircleGeometry(double size)
    {
        double radius = size / 2;
        return new EllipseGeometry(new Point(radius, radius), radius, radius);
    }
        /// <summary>Converts a single letter into a fillable/strokeable outline, centered in a size x size box.</summary>
    public static Geometry CreateInitialGeometry(double size, string letter)
    {
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        var formattedText = new FormattedText(
            letter,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            size * 0.75,
            Brushes.Black,
            1.0);

        Geometry glyphGeometry = formattedText.BuildGeometry(new Point(0, 0));
        Rect bounds = glyphGeometry.Bounds;

        double offsetX = size / 2 - (bounds.X + bounds.Width / 2);
        double offsetY = size / 2 - (bounds.Y + bounds.Height / 2);

        var group = new GeometryGroup();
        group.Children.Add(glyphGeometry);
        group.Transform = new TranslateTransform(offsetX, offsetY);
        return group;
    }

    public static Geometry CreateMoonGeometry(double size)
    {
        double radius = size / 2;
        var outer = new EllipseGeometry(new Point(radius, radius), radius, radius);
        var inner = new EllipseGeometry(new Point(radius * 1.35, radius * 0.85), radius * 0.85, radius * 0.85);
        return new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);
    }


    public static Geometry CreateStarGeometry(double size)
    {
        double outerRadius = size / 2;
        double innerRadius = outerRadius * 0.45;
        var center = new Point(outerRadius, outerRadius);

        var points = new PointCollection();
        for (int i = 0; i < 10; i++)
        {
            double radius = (i % 2 == 0) ? outerRadius : innerRadius;
            double angle = Math.PI / 5 * i - Math.PI / 2;
            points.Add(new Point(
                center.X + radius * Math.Cos(angle),
                center.Y + radius * Math.Sin(angle)));
        }

        var figure = new PathFigure { StartPoint = points[0], IsClosed = true };
        for (int i = 1; i < points.Count; i++)
        {
            figure.Segments.Add(new LineSegment(points[i], true));
        }

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    public static Geometry CreateHeartGeometry(double size)
    {
        double s = size;
        var start = new Point(s * 0.5, s * 0.3);

        var figure = new PathFigure { StartPoint = start, IsClosed = true };

        figure.Segments.Add(new BezierSegment(
            new Point(s * 0.0, s * 0.0),
            new Point(s * -0.05, s * 0.55),
            new Point(s * 0.5, s * 0.8),
            true));

        figure.Segments.Add(new BezierSegment(
            new Point(s * 1.05, s * 0.55),
            new Point(s * 1.0, s * 0.0),
            new Point(s * 0.5, s * 0.3),
            true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>Several overlapping circles unioned together into one puffy outline.</summary>
    public static Geometry CreateCloudGeometry(double size)
    {
        var group = new GeometryGroup { FillRule = FillRule.Nonzero };
        group.Children.Add(new EllipseGeometry(new Point(size * 0.35, size * 0.55), size * 0.25, size * 0.22));
        group.Children.Add(new EllipseGeometry(new Point(size * 0.6, size * 0.45), size * 0.3, size * 0.28));
        group.Children.Add(new EllipseGeometry(new Point(size * 0.8, size * 0.58), size * 0.2, size * 0.18));
        group.Children.Add(new EllipseGeometry(new Point(size * 0.5, size * 0.62), size * 0.35, size * 0.16));
        return group;
    }

    /// <summary>A planet circle plus a tilted thin ring, unioned together.</summary>
    public static Geometry CreateSaturnGeometry(double size)
    {
        double planetRadius = size * 0.3;
        var planet = new EllipseGeometry(new Point(size / 2, size / 2), planetRadius, planetRadius);

        var ring = new EllipseGeometry(new Point(size / 2, size / 2), size * 0.5, size * 0.12);
        ring.Transform = new RotateTransform(-20, size / 2, size / 2);

        var group = new GeometryGroup { FillRule = FillRule.Nonzero };
        group.Children.Add(ring);
        group.Children.Add(planet);
        return group;
    }

    /// <summary>A round head with two triangular ears, unioned into one silhouette.</summary>
    public static Geometry CreateCatGeometry(double size)
    {
        double headRadius = size * 0.4;
        var head = new EllipseGeometry(new Point(size / 2, size * 0.58), headRadius, headRadius * 0.9);

        Geometry leftEar = CreateTriangle(
            new Point(size * 0.22, size * 0.32),
            new Point(size * 0.34, size * 0.05),
            new Point(size * 0.46, size * 0.3));

        Geometry rightEar = CreateTriangle(
            new Point(size * 0.54, size * 0.3),
            new Point(size * 0.66, size * 0.05),
            new Point(size * 0.78, size * 0.32));

        var group = new GeometryGroup { FillRule = FillRule.Nonzero };
        group.Children.Add(head);
        group.Children.Add(leftEar);
        group.Children.Add(rightEar);
        return group;
    }

    /// <summary>A rounded teardrop shape, tapering to a point at the top.</summary>
    public static Geometry CreateStrawberryGeometry(double size)
    {
        double s = size;
        var figure = new PathFigure { StartPoint = new Point(s * 0.5, s * 0.05), IsClosed = true };

        figure.Segments.Add(new BezierSegment(
            new Point(s * 0.95, s * 0.35),
            new Point(s * 0.85, s * 0.95),
            new Point(s * 0.5, s * 0.95),
            true));

        figure.Segments.Add(new BezierSegment(
            new Point(s * 0.15, s * 0.95),
            new Point(s * 0.05, s * 0.35),
            new Point(s * 0.5, s * 0.05),
            true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>One large central pad plus four small toe ovals, unioned together.</summary>
    public static Geometry CreatePawGeometry(double size)
    {
        var group = new GeometryGroup { FillRule = FillRule.Nonzero };
        group.Children.Add(new EllipseGeometry(new Point(size * 0.5, size * 0.62), size * 0.28, size * 0.24));
        group.Children.Add(new EllipseGeometry(new Point(size * 0.28, size * 0.32), size * 0.13, size * 0.16));
        group.Children.Add(new EllipseGeometry(new Point(size * 0.5, size * 0.2), size * 0.13, size * 0.16));
        group.Children.Add(new EllipseGeometry(new Point(size * 0.72, size * 0.32), size * 0.13, size * 0.16));
        return group;
    }

    public static Geometry CreateTriangle(Point a, Point b, Point c)
    {
        var figure = new PathFigure { StartPoint = a, IsClosed = true };
        figure.Segments.Add(new LineSegment(b, true));
        figure.Segments.Add(new LineSegment(c, true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }
}