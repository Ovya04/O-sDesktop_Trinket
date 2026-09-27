using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Trinket.Models;
using Trinket.Physics;
using System.Windows.Controls;

namespace Trinket.Rendering;

public class CordVisualHandle
{
    public CordStyle Style;
    public List<Ellipse> Links { get; } = new();
    public List<Polygon> RibbonSegments { get; } = new();
    public Polyline? BraidLineA;
    public Polyline? BraidLineB;
}

public static class RopeRenderer
{
    public static void ApplyBackboneStyle(Polyline backbone, CordStyle style, Color color)
    {
        switch (style)
        {
            case CordStyle.Thread:
            case CordStyle.PearlString:
                // Pearl String keeps a thin visible thread beneath the
                // pearls - without it, only the beads render, leaving gaps
                // at the anchor and charm ends where no bead sits.
                backbone.Stroke = new SolidColorBrush(color);
                backbone.StrokeThickness = 2;
                backbone.StrokeDashArray = null;
                break;

            case CordStyle.Vine:
                backbone.Stroke = new SolidColorBrush(color);
                backbone.StrokeThickness = 3;
                backbone.StrokeDashArray = new DoubleCollection { 2, 0.6 };
                break;

            default:
                // Chain / Braided / Ribbon draw their own shapes in the
                // decoration canvas - hide the plain backbone line.
                backbone.Stroke = Brushes.Transparent;
                backbone.StrokeThickness = 0;
                backbone.StrokeDashArray = null;
                break;
        }
    }

    /// <summary>
    /// segmentLength is used to size Chain links proportionally to the
    /// actual gap between rope nodes, so links nearly touch instead of
    /// leaving visible gaps regardless of the chosen rope length.
    /// </summary>
    public static CordVisualHandle BuildCordVisual(Canvas decorationCanvas, CordStyle style, Color color, int nodeCount, double segmentLength)
    {
        decorationCanvas.Children.Clear();
        var handle = new CordVisualHandle { Style = style };

        switch (style)
        {
            case CordStyle.Chain:
                double linkWidth = segmentLength * 0.92;
                double linkHeight = Math.Max(4, segmentLength * 0.4);
                for (int i = 0; i < nodeCount - 1; i++)
                {
                    var link = new Ellipse
                    {
                        Width = linkWidth, Height = linkHeight,
                        Fill = GradientBrushFactory.CreateSheenBrush(color),
                        Stroke = new SolidColorBrush(Darken(color)),
                        StrokeThickness = 0.75,
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new RotateTransform(0)
                    };
                    decorationCanvas.Children.Add(link);
                    handle.Links.Add(link);
                }
                break;

            case CordStyle.PearlString:
                for (int i = 1; i < nodeCount - 1; i++)
                {
                    var pearl = new Ellipse
                    {
                        Width = 9, Height = 9,
                        Fill = GradientBrushFactory.CreateSheenBrush(Lighten(color)),
                        Stroke = new SolidColorBrush(Darken(color)),
                        StrokeThickness = 0.5
                    };
                    decorationCanvas.Children.Add(pearl);
                    handle.Links.Add(pearl);
                }
                break;

            case CordStyle.Braided:
                handle.BraidLineA = new Polyline { Stroke = new SolidColorBrush(Lighten(color)), StrokeThickness = 2.5 };
                handle.BraidLineB = new Polyline { Stroke = new SolidColorBrush(Darken(color)), StrokeThickness = 2.5 };
                decorationCanvas.Children.Add(handle.BraidLineA);
                decorationCanvas.Children.Add(handle.BraidLineB);
                break;

            case CordStyle.Ribbon:
                for (int i = 0; i < nodeCount - 1; i++)
                {
                    var quad = new Polygon
                    {
                        Fill = GradientBrushFactory.CreateSheenBrush(color),
                        Stroke = new SolidColorBrush(Darken(color)),
                        StrokeThickness = 0.5,
                        Points = new PointCollection { new Point(), new Point(), new Point(), new Point() }
                    };
                    decorationCanvas.Children.Add(quad);
                    handle.RibbonSegments.Add(quad);
                }
                break;
        }

        return handle;
    }

    public static void UpdateCordVisual(CordVisualHandle handle, IReadOnlyList<RopeNode> nodes)
    {
        switch (handle.Style)
        {
            case CordStyle.Chain:
                for (int i = 0; i < handle.Links.Count; i++)
                {
                    Point a = nodes[i].CurrentPosition;
                    Point b = nodes[i + 1].CurrentPosition;
                    Point mid = new((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                    double angle = Math.Atan2(b.Y - a.Y, b.X - a.X) * (180.0 / Math.PI);

                    Ellipse link = handle.Links[i];
                    Canvas.SetLeft(link, mid.X - link.Width / 2);
                    Canvas.SetTop(link, mid.Y - link.Height / 2);
                    ((RotateTransform)link.RenderTransform).Angle = angle;
                }
                break;

            case CordStyle.PearlString:
                for (int i = 0; i < handle.Links.Count; i++)
                {
                    Point p = nodes[i + 1].CurrentPosition;
                    Ellipse pearl = handle.Links[i];
                    Canvas.SetLeft(pearl, p.X - pearl.Width / 2);
                    Canvas.SetTop(pearl, p.Y - pearl.Height / 2);
                }
                break;

            case CordStyle.Braided:
                if (handle.BraidLineA == null || handle.BraidLineB == null) break;
                handle.BraidLineA.Points.Clear();
                handle.BraidLineB.Points.Clear();
                for (int i = 0; i < nodes.Count; i++)
                {
                    Point pos = nodes[i].CurrentPosition;
                    Vector perp = GetPerpendicular(nodes, i);
                    double sign = (i % 2 == 0) ? 1 : -1;
                    handle.BraidLineA.Points.Add(pos + perp * 2.2 * sign);
                    handle.BraidLineB.Points.Add(pos - perp * 2.2 * sign);
                }
                break;

            case CordStyle.Ribbon:
                for (int i = 0; i < handle.RibbonSegments.Count; i++)
                {
                    Point a = nodes[i].CurrentPosition;
                    Point b = nodes[i + 1].CurrentPosition;
                    Vector dir = b - a;
                    double length = dir.Length;
                    if (length < 0.0001) continue;
                    Vector perp = new(-dir.Y / length, dir.X / length);
                    const double halfWidth = 3.5;

                    Polygon quad = handle.RibbonSegments[i];
                    quad.Points[0] = a + perp * halfWidth;
                    quad.Points[1] = b + perp * halfWidth;
                    quad.Points[2] = b - perp * halfWidth;
                    quad.Points[3] = a - perp * halfWidth;
                }
                break;
        }
    }

    private static Vector GetPerpendicular(IReadOnlyList<RopeNode> nodes, int index)
    {
        Point a = nodes[Math.Max(0, index - 1)].CurrentPosition;
        Point b = nodes[Math.Min(nodes.Count - 1, index + 1)].CurrentPosition;
        Vector dir = b - a;
        double length = dir.Length;
        if (length < 0.0001) return new Vector(1, 0);
        return new Vector(-dir.Y / length, dir.X / length);
    }

    private static Color Lighten(Color c)
    {
        byte r = (byte)Math.Min(255, c.R + (255 - c.R) * 0.4);
        byte g = (byte)Math.Min(255, c.G + (255 - c.G) * 0.4);
        byte b = (byte)Math.Min(255, c.B + (255 - c.B) * 0.4);
        return Color.FromArgb(c.A, r, g, b);
    }

    private static Color Darken(Color c) =>
        Color.FromArgb(c.A, (byte)(c.R * 0.7), (byte)(c.G * 0.7), (byte)(c.B * 0.7));
}