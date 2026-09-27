using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Trinket.Models;

namespace Trinket.Rendering;

public static class CharmRenderer
{
    public static FrameworkElement CreateCharmVisual(CharmDefinition definition)
    {
        double size = definition.Size;

        FrameworkElement visual = definition.Shape switch
        {
            CharmShape.Moon => CreateSimplePath(ShapeGeometryFactory.CreateMoonGeometry(size), definition),
            CharmShape.Star => CreateSimplePath(ShapeGeometryFactory.CreateStarGeometry(size), definition),
            CharmShape.Heart => CreateSimplePath(ShapeGeometryFactory.CreateHeartGeometry(size), definition),
            CharmShape.Cloud => CreateSimplePath(ShapeGeometryFactory.CreateCloudGeometry(size), definition),
            CharmShape.Saturn => CreateSimplePath(ShapeGeometryFactory.CreateSaturnGeometry(size), definition),
            CharmShape.Paw => CreateSimplePath(ShapeGeometryFactory.CreatePawGeometry(size), definition),
            CharmShape.Initial => CreateSimplePath(ShapeGeometryFactory.CreateInitialGeometry(size, definition.InitialLetter), definition),

            CharmShape.Sunflower => CreateSunflowerVisual(size),
            CharmShape.EvilEye => CreateEvilEyeVisual(size),
            CharmShape.Cherries => CreateCherriesVisual(size),
            CharmShape.Mushroom => CreateMushroomVisual(size),
            CharmShape.Rainbow => CreateRainbowVisual(size),
            CharmShape.DiscoBall => CreateDiscoBallVisual(size),
            CharmShape.Cat => CreateCatVisual(size),
            CharmShape.Strawberry => CreateStrawberryVisual(size),
            CharmShape.Bow => CreateBowVisual(size),
            CharmShape.TeddyBear => CreateTeddyBearVisual(size),
            CharmShape.Daisy => CreateDaisyVisual(size),
            CharmShape.CoffeeCup => CreateCoffeeCupVisual(size),
            CharmShape.GraduationCap => CreateGraduationCapVisual(size),

            CharmShape.Custom => CreateCustomImageVisual(definition),

            _ => CreateSimplePath(ShapeGeometryFactory.CreateCircleGeometry(size), definition)
        };

        visual.RenderTransformOrigin = definition.AttachmentPoint;

        visual.Effect = new DropShadowEffect
        {
            Color = Colors.Black,
            Opacity = 0.22,
            BlurRadius = 6,
            ShadowDepth = 2,
            Direction = 270
        };

        return visual;
    }

    public static void UpdateRotation(FrameworkElement charmVisual, Point fromNode, Point toNode)
    {
        double dx = toNode.X - fromNode.X;
        double dy = toNode.Y - fromNode.Y;
        double angleRadians = Math.Atan2(dx, dy);
        double angleDegrees = angleRadians * (180.0 / Math.PI);

        if (charmVisual.RenderTransform is TransformGroup group && group.Children.Count > 0 &&
            group.Children[0] is RotateTransform physicsRotation)
        {
            physicsRotation.Angle = angleDegrees;
        }
    }

    public static RotateTransform? GetReactionTransform(FrameworkElement charmVisual)
    {
        if (charmVisual.RenderTransform is TransformGroup group && group.Children.Count > 1)
        {
            return group.Children[1] as RotateTransform;
        }
        return null;
    }
    public static ScaleTransform? GetReactionScaleTransform(FrameworkElement charmVisual)
    {
        if (charmVisual.RenderTransform is TransformGroup group && group.Children.Count > 2)
        {
            return group.Children[2] as ScaleTransform;
        }
        return null;
    }

     private static TransformGroup CreatePhysicsAndReactionTransform()
    {
        var group = new TransformGroup();
        group.Children.Add(new RotateTransform(0));  // index 0: physics-driven swing
        group.Children.Add(new RotateTransform(0));  // index 1: reaction rotation (Wiggle/Spin)
        group.Children.Add(new ScaleTransform(1, 1)); // index 2: reaction scale (Pulse/Bounce)
        return group;
    }

    private static Path CreateSimplePath(Geometry geometry, CharmDefinition definition)
    {
        return new Path
        {
            Data = geometry,
            Fill = GradientBrushFactory.CreateSheenBrush(definition.FillColor),
            Stroke = new SolidColorBrush(definition.StrokeColor),
            StrokeThickness = 2,
            Width = definition.Size,
            Height = definition.Size,
            Stretch = Stretch.Uniform,
            RenderTransform = CreatePhysicsAndReactionTransform()
        };
    }

    private static Image CreateCustomImageVisual(CharmDefinition definition)
    {
        var image = new Image
        {
            Width = definition.Size,
            Height = definition.Size,
            Stretch = Stretch.Uniform,
            RenderTransform = CreatePhysicsAndReactionTransform()
        };

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(definition.ImagePath!, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            image.Source = bitmap;
        }
        catch (Exception)
        {
            image.Source = null;
        }

        return image;
    }

    private static Canvas CreateCompositeRoot(double size)
    {
        return new Canvas
        {
            Width = size,
            Height = size,
            RenderTransform = CreatePhysicsAndReactionTransform()
        };
    }

    private static Ellipse CreateCircle(double diameter, Color fill, Color? stroke = null)
    {
        return new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Fill = GradientBrushFactory.CreateSheenBrush(fill),
            Stroke = stroke.HasValue ? new SolidColorBrush(stroke.Value) : null,
            StrokeThickness = stroke.HasValue ? 1 : 0
        };
    }

    private static Canvas CreateSunflowerVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        double center = size / 2;
        var petalColor = Color.FromRgb(0xF4, 0xC5, 0x4D);
        for (int i = 0; i < 8; i++)
        {
            var petal = new Ellipse
            {
                Width = size * 0.22, Height = size * 0.5,
                Fill = GradientBrushFactory.CreateSheenBrush(petalColor),
                RenderTransformOrigin = new Point(0.5, 1.0)
            };
            petal.RenderTransform = new RotateTransform(i * 45);
            Canvas.SetLeft(petal, center - petal.Width / 2);
            Canvas.SetTop(petal, 0);
            root.Children.Add(petal);
        }
        var centerCircle = CreateCircle(size * 0.32, Color.FromRgb(0x8A, 0x5A, 0x2A));
        Canvas.SetLeft(centerCircle, center - centerCircle.Width / 2);
        Canvas.SetTop(centerCircle, center - centerCircle.Height / 2);
        root.Children.Add(centerCircle);
        return root;
    }

    private static Canvas CreateEvilEyeVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        double center = size / 2;
        void AddRing(double diameter, Color color)
        {
            Ellipse ring = CreateCircle(diameter, color);
            Canvas.SetLeft(ring, center - diameter / 2);
            Canvas.SetTop(ring, center - diameter / 2);
            root.Children.Add(ring);
        }
        AddRing(size * 0.95, Color.FromRgb(0x2E, 0x5E, 0x9A));
        AddRing(size * 0.7, Color.FromRgb(0xFF, 0xFF, 0xFF));
        AddRing(size * 0.5, Color.FromRgb(0x4A, 0x9A, 0xD4));
        AddRing(size * 0.22, Color.FromRgb(0x15, 0x1F, 0x2E));

        var highlight = new Ellipse
        {
            Width = size * 0.14, Height = size * 0.09,
            Fill = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF))
        };
        Canvas.SetLeft(highlight, size * 0.27);
        Canvas.SetTop(highlight, size * 0.21);
        root.Children.Add(highlight);

        return root;
    }

    private static Canvas CreateCherriesVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var stemBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x8A, 0x4A));
        var cherryColor = Color.FromRgb(0xC5, 0x2B, 0x3A);
        var leftStem = new Path { Stroke = stemBrush, StrokeThickness = size * 0.03, Data = new PathGeometry(new[] { new PathFigure(new Point(size * 0.5, size * 0.05), new PathSegment[] { new BezierSegment(new Point(size * 0.4, size * 0.2), new Point(size * 0.3, size * 0.35), new Point(size * 0.28, size * 0.45), true) }, false) }) };
        var rightStem = new Path { Stroke = stemBrush, StrokeThickness = size * 0.03, Data = new PathGeometry(new[] { new PathFigure(new Point(size * 0.5, size * 0.05), new PathSegment[] { new BezierSegment(new Point(size * 0.6, size * 0.2), new Point(size * 0.68, size * 0.35), new Point(size * 0.7, size * 0.45), true) }, false) }) };
        root.Children.Add(leftStem);
        root.Children.Add(rightStem);
        Ellipse leftCherry = CreateCircle(size * 0.34, cherryColor);
        Canvas.SetLeft(leftCherry, size * 0.28 - leftCherry.Width / 2);
        Canvas.SetTop(leftCherry, size * 0.45);
        root.Children.Add(leftCherry);
        Ellipse rightCherry = CreateCircle(size * 0.34, cherryColor);
        Canvas.SetLeft(rightCherry, size * 0.7 - rightCherry.Width / 2);
        Canvas.SetTop(rightCherry, size * 0.5);
        root.Children.Add(rightCherry);
        return root;
    }

    private static Canvas CreateMushroomVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var capGeometry = new CombinedGeometry(GeometryCombineMode.Intersect, new EllipseGeometry(new Point(size * 0.5, size * 0.4), size * 0.45, size * 0.4), new RectangleGeometry(new Rect(0, 0, size, size * 0.42)));
        var cap = new Path { Data = capGeometry, Fill = GradientBrushFactory.CreateSheenBrush(Color.FromRgb(0xE0, 0x4F, 0x4F)), Stroke = new SolidColorBrush(Color.FromRgb(0xA8, 0x2E, 0x2E)), StrokeThickness = 1.5 };
        root.Children.Add(cap);
        var stem = new Rectangle { Width = size * 0.3, Height = size * 0.45, RadiusX = size * 0.1, RadiusY = size * 0.1, Fill = new SolidColorBrush(Color.FromRgb(0xF3, 0xE8, 0xD8)), Stroke = new SolidColorBrush(Color.FromRgb(0xC9, 0xB8, 0x9A)), StrokeThickness = 1 };
        Canvas.SetLeft(stem, size * 0.35);
        Canvas.SetTop(stem, size * 0.4);
        root.Children.Add(stem);
        var spotBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0xFF, 0xFF, 0xFF));
        (double x, double y, double s)[] spots = { (size * 0.28, size * 0.18, size * 0.08), (size * 0.5, size * 0.1, size * 0.07), (size * 0.68, size * 0.22, size * 0.06) };
        foreach (var (x, y, s) in spots)
        {
            var spot = new Ellipse { Width = s, Height = s, Fill = spotBrush };
            Canvas.SetLeft(spot, x);
            Canvas.SetTop(spot, y);
            root.Children.Add(spot);
        }
        return root;
    }

    private static Canvas CreateRainbowVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        Color[] colors = { Color.FromRgb(0xE0, 0x4F, 0x4F), Color.FromRgb(0xF0, 0xA0, 0x40), Color.FromRgb(0x6A, 0xB0, 0x5A), Color.FromRgb(0x4A, 0x8A, 0xD4) };
        double bandThickness = size * 0.1;
        double baseRadius = size * 0.48;
        for (int i = 0; i < colors.Length; i++)
        {
            double radius = baseRadius - i * (bandThickness * 0.9);
            var arc = new Path { Stroke = new SolidColorBrush(colors[i]), StrokeThickness = bandThickness, Data = new PathGeometry(new[] { new PathFigure(new Point(size / 2 - radius, size), new PathSegment[] { new ArcSegment(new Point(size / 2 + radius, size), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true) }, false) }) };
            root.Children.Add(arc);
        }
        return root;
    }

    private static Canvas CreateDiscoBallVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        double center = size / 2;
        Ellipse ball = CreateCircle(size * 0.9, Color.FromRgb(0xC9, 0xCE, 0xD6), Color.FromRgb(0x8A, 0x90, 0x99));
        Canvas.SetLeft(ball, center - ball.Width / 2);
        Canvas.SetTop(ball, center - ball.Height / 2);
        root.Children.Add(ball);
        var gridBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x50, 0x50, 0x50));
        for (int i = 1; i < 4; i++)
        {
            double y = size * 0.1 + i * size * 0.2;
            root.Children.Add(new Line { X1 = size * 0.08, Y1 = y, X2 = size * 0.92, Y2 = y, Stroke = gridBrush, StrokeThickness = 1 });
        }
        for (int i = 1; i < 4; i++)
        {
            double x = size * 0.1 + i * size * 0.2;
            root.Children.Add(new Line { X1 = x, Y1 = size * 0.08, X2 = x, Y2 = size * 0.92, Stroke = gridBrush, StrokeThickness = 1 });
        }
        var sparkleBrush = new SolidColorBrush(Color.FromArgb(0xDD, 0xFF, 0xFF, 0xFF));
        (double x, double y, double s)[] sparkles = { (size * 0.22, size * 0.28, size * 0.1), (size * 0.66, size * 0.6, size * 0.08) };
        foreach (var (x, y, s) in sparkles)
        {
            var sparkle = new Path { Data = ShapeGeometryFactory.CreateStarGeometry(s), Fill = sparkleBrush, Width = s, Height = s };
            Canvas.SetLeft(sparkle, x);
            Canvas.SetTop(sparkle, y);
            root.Children.Add(sparkle);
        }
        return root;
    }

    private static Canvas CreateCatVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var furColor = Color.FromRgb(0x4A, 0x4A, 0x4A);

        var basePath = new Path
        {
            Data = ShapeGeometryFactory.CreateCatGeometry(size),
            Fill = GradientBrushFactory.CreateSheenBrush(furColor),
            Stroke = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A)),
            StrokeThickness = 1.5
        };
        root.Children.Add(basePath);

        var innerEarColor = new SolidColorBrush(Color.FromRgb(0xE8, 0x9A, 0xB4));
        var innerEarL = new Path { Fill = innerEarColor, Data = ShapeGeometryFactory.CreateTriangle(new Point(size * 0.27, size * 0.28), new Point(size * 0.34, size * 0.11), new Point(size * 0.41, size * 0.27)) };
        var innerEarR = new Path { Fill = innerEarColor, Data = ShapeGeometryFactory.CreateTriangle(new Point(size * 0.59, size * 0.27), new Point(size * 0.66, size * 0.11), new Point(size * 0.73, size * 0.28)) };
        root.Children.Add(innerEarL);
        root.Children.Add(innerEarR);

        var eyeBrush = new SolidColorBrush(Colors.Black);
        var eyeL = new Ellipse { Width = size * 0.06, Height = size * 0.08, Fill = eyeBrush };
        Canvas.SetLeft(eyeL, size * 0.36); Canvas.SetTop(eyeL, size * 0.54);
        var eyeR = new Ellipse { Width = size * 0.06, Height = size * 0.08, Fill = eyeBrush };
        Canvas.SetLeft(eyeR, size * 0.58); Canvas.SetTop(eyeR, size * 0.54);
        root.Children.Add(eyeL);
        root.Children.Add(eyeR);

        var nose = new Path
        {
            Fill = new SolidColorBrush(Color.FromRgb(0xE8, 0x9A, 0xB4)),
            Data = ShapeGeometryFactory.CreateTriangle(new Point(size * 0.47, size * 0.62), new Point(size * 0.53, size * 0.62), new Point(size * 0.5, size * 0.66))
        };
        root.Children.Add(nose);

        var whiskerBrush = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
        double[] whiskerYs = { size * 0.6, size * 0.64, size * 0.68 };
        foreach (double y in whiskerYs)
        {
            root.Children.Add(new Line { X1 = size * 0.1, Y1 = y, X2 = size * 0.34, Y2 = y - size * 0.01, Stroke = whiskerBrush, StrokeThickness = 1 });
            root.Children.Add(new Line { X1 = size * 0.66, Y1 = y - size * 0.01, X2 = size * 0.9, Y2 = y, Stroke = whiskerBrush, StrokeThickness = 1 });
        }

        return root;
    }

    private static Canvas CreateStrawberryVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var berryColor = Color.FromRgb(0xE0, 0x4F, 0x5F);

        var body = new Path
        {
            Data = ShapeGeometryFactory.CreateStrawberryGeometry(size),
            Fill = GradientBrushFactory.CreateSheenBrush(berryColor),
            Stroke = new SolidColorBrush(Color.FromRgb(0xB0, 0x30, 0x40)),
            StrokeThickness = 1.5
        };
        root.Children.Add(body);

        var leafBrush = new SolidColorBrush(Color.FromRgb(0x5A, 0x9A, 0x4A));
        for (int i = 0; i < 5; i++)
        {
            double baseX = size * (0.28 + i * 0.11);
            var leaf = new Path
            {
                Fill = leafBrush,
                Data = ShapeGeometryFactory.CreateTriangle(
                    new Point(baseX, size * 0.09),
                    new Point(baseX + size * 0.045, size * 0.0),
                    new Point(baseX + size * 0.09, size * 0.09))
            };
            root.Children.Add(leaf);
        }

        var seedBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0xE0, 0x8A));
        var random = new Random(unchecked((int)size * 7919));
        for (int i = 0; i < 10; i++)
        {
            double sx = size * (0.28 + random.NextDouble() * 0.44);
            double sy = size * (0.32 + random.NextDouble() * 0.52);
            var seed = new Ellipse { Width = size * 0.03, Height = size * 0.05, Fill = seedBrush };
            Canvas.SetLeft(seed, sx);
            Canvas.SetTop(seed, sy);
            root.Children.Add(seed);
        }

        return root;
    }

    private static Canvas CreateBowVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var bowColor = Color.FromRgb(0xE8, 0x9A, 0xB4);
        var outline = new SolidColorBrush(Color.FromRgb(0xB8, 0x5F, 0x80));

        Geometry LoopGeometry(bool flip)
        {
            double s = size;
            double dir = flip ? -1 : 1;
            var figure = new PathFigure { StartPoint = new Point(s * 0.5, s * 0.5), IsClosed = true };
            figure.Segments.Add(new BezierSegment(
                new Point(s * 0.5 + dir * s * 0.1, s * 0.25),
                new Point(s * 0.5 + dir * s * 0.42, s * 0.18),
                new Point(s * 0.5 + dir * s * 0.42, s * 0.5), true));
            figure.Segments.Add(new BezierSegment(
                new Point(s * 0.5 + dir * s * 0.42, s * 0.82),
                new Point(s * 0.5 + dir * s * 0.1, s * 0.75),
                new Point(s * 0.5, s * 0.5), true));
            var geom = new PathGeometry();
            geom.Figures.Add(figure);
            return geom;
        }

        var leftLoop = new Path { Data = LoopGeometry(true), Fill = GradientBrushFactory.CreateSheenBrush(bowColor), Stroke = outline, StrokeThickness = 1.5 };
        var rightLoop = new Path { Data = LoopGeometry(false), Fill = GradientBrushFactory.CreateSheenBrush(bowColor), Stroke = outline, StrokeThickness = 1.5 };
        var knot = CreateCircle(size * 0.16, Color.FromRgb(0xD8, 0x7A, 0x9C));
        Canvas.SetLeft(knot, size * 0.5 - knot.Width / 2);
        Canvas.SetTop(knot, size * 0.5 - knot.Height / 2);

        root.Children.Add(leftLoop);
        root.Children.Add(rightLoop);
        root.Children.Add(knot);
        return root;
    }

    private static Canvas CreateTeddyBearVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var furColor = Color.FromRgb(0xC9, 0x9A, 0x6A);
        var darkColor = Color.FromRgb(0x5A, 0x3E, 0x24);

        Ellipse ear1 = CreateCircle(size * 0.22, furColor, Color.FromRgb(0x8F, 0x68, 0x44));
        Canvas.SetLeft(ear1, size * 0.06); Canvas.SetTop(ear1, size * 0.02);
        Ellipse ear2 = CreateCircle(size * 0.22, furColor, Color.FromRgb(0x8F, 0x68, 0x44));
        Canvas.SetLeft(ear2, size * 0.72); Canvas.SetTop(ear2, size * 0.02);
        Ellipse head = CreateCircle(size * 0.78, furColor, Color.FromRgb(0x8F, 0x68, 0x44));
        Canvas.SetLeft(head, size * 0.11); Canvas.SetTop(head, size * 0.14);
        Ellipse snout = CreateCircle(size * 0.32, Color.FromRgb(0xE8, 0xD3, 0xB0));
        Canvas.SetLeft(snout, size * 0.34); Canvas.SetTop(snout, size * 0.48);
        var nose = new Ellipse { Width = size * 0.1, Height = size * 0.08, Fill = new SolidColorBrush(darkColor) };
        Canvas.SetLeft(nose, size * 0.45); Canvas.SetTop(nose, size * 0.52);
        var eyeL = new Ellipse { Width = size * 0.06, Height = size * 0.06, Fill = new SolidColorBrush(darkColor) };
        Canvas.SetLeft(eyeL, size * 0.3); Canvas.SetTop(eyeL, size * 0.36);
        var eyeR = new Ellipse { Width = size * 0.06, Height = size * 0.06, Fill = new SolidColorBrush(darkColor) };
        Canvas.SetLeft(eyeR, size * 0.62); Canvas.SetTop(eyeR, size * 0.36);

        root.Children.Add(ear1);
        root.Children.Add(ear2);
        root.Children.Add(head);
        root.Children.Add(snout);
        root.Children.Add(eyeL);
        root.Children.Add(eyeR);
        root.Children.Add(nose);
        return root;
    }

    private static Canvas CreateDaisyVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        double center = size / 2;
        var petalColor = Color.FromRgb(0xFF, 0xFF, 0xF7);

        for (int i = 0; i < 10; i++)
        {
            var petal = new Ellipse
            {
                Width = size * 0.14, Height = size * 0.42,
                Fill = GradientBrushFactory.CreateSheenBrush(petalColor),
                Stroke = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xD0)),
                StrokeThickness = 0.5,
                RenderTransformOrigin = new Point(0.5, 1.0)
            };
            petal.RenderTransform = new RotateTransform(i * 36);
            Canvas.SetLeft(petal, center - petal.Width / 2);
            Canvas.SetTop(petal, size * 0.08);
            root.Children.Add(petal);
        }

        var centerCircle = CreateCircle(size * 0.26, Color.FromRgb(0xF4, 0xC5, 0x4D));
        Canvas.SetLeft(centerCircle, center - centerCircle.Width / 2);
        Canvas.SetTop(centerCircle, center - centerCircle.Height / 2);
        root.Children.Add(centerCircle);
        return root;
    }

    private static Canvas CreateCoffeeCupVisual(double size)
    {
        var root = CreateCompositeRoot(size);
        var cupColor = Color.FromRgb(0xE8, 0xDC, 0xC8);
        var outline = new SolidColorBrush(Color.FromRgb(0xA0, 0x7A, 0x50));

        var handleOuter = new EllipseGeometry(new Point(size * 0.75, size * 0.6), size * 0.14, size * 0.14);
        var handleInner = new EllipseGeometry(new Point(size * 0.75, size * 0.6), size * 0.07, size * 0.07);
        var handleGeom = new CombinedGeometry(GeometryCombineMode.Exclude, handleOuter, handleInner);
        var handle = new Path { Data = handleGeom, Fill = GradientBrushFactory.CreateSheenBrush(cupColor), Stroke = outline, StrokeThickness = 1.5 };
        root.Children.Add(handle);

        var cupBody = new Rectangle
        {
            Width = size * 0.5, Height = size * 0.42,
            RadiusX = size * 0.06, RadiusY = size * 0.06,
            Fill = GradientBrushFactory.CreateSheenBrush(cupColor), Stroke = outline, StrokeThickness = 1.5
        };
        Canvas.SetLeft(cupBody, size * 0.18);
        Canvas.SetTop(cupBody, size * 0.4);
        root.Children.Add(cupBody);

        var steam = new Path
        {
            Stroke = new SolidColorBrush(Color.FromArgb(0x99, 0xB0, 0xB0, 0xB0)),
            StrokeThickness = 1.5,
            Data = new PathGeometry(new[]
            {
                new PathFigure(new Point(size * 0.32, size * 0.36), new PathSegment[]
                {
                    new BezierSegment(new Point(size * 0.24, size * 0.26), new Point(size * 0.4, size * 0.2), new Point(size * 0.32, size * 0.1), false)
                }, false)
            })
        };
        root.Children.Add(steam);

        return root;
    }

    private static Canvas CreateGraduationCapVisual(double size)
    {
        var root = CreateCompositeRoot(size);

        var board = new Rectangle
        {
            Width = size * 0.62, Height = size * 0.62,
            Fill = GradientBrushFactory.CreateSheenBrush(Color.FromRgb(0x2A, 0x2A, 0x3A)),
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform(45)
        };
        Canvas.SetLeft(board, size * 0.19);
        Canvas.SetTop(board, size * 0.06);
        root.Children.Add(board);

        var band = new Rectangle
        {
            Width = size * 0.3, Height = size * 0.22,
            RadiusX = size * 0.03, RadiusY = size * 0.03,
            Fill = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x26))
        };
        Canvas.SetLeft(band, size * 0.35);
        Canvas.SetTop(band, size * 0.5);
        root.Children.Add(band);

        var tassel = new Line
        {
            X1 = size * 0.5, Y1 = size * 0.4, X2 = size * 0.5, Y2 = size * 0.78,
            Stroke = new SolidColorBrush(Color.FromRgb(0xD4, 0xAF, 0x37)), StrokeThickness = 1.5
        };
        root.Children.Add(tassel);

        Ellipse tasselEnd = CreateCircle(size * 0.08, Color.FromRgb(0xD4, 0xAF, 0x37));
        Canvas.SetLeft(tasselEnd, size * 0.46); Canvas.SetTop(tasselEnd, size * 0.76);
        root.Children.Add(tasselEnd);

        return root;
    }
}