using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Trinket.Rendering;

namespace Trinket.Reactions;

/// <summary>
/// A handful of small star shapes that briefly pop up near the charm,
/// growing and fading out, then remove themselves from the canvas.
/// </summary>
public class SparkleReaction : ICharmReaction
{
    private static readonly Random Random = new();

    public void Play(FrameworkElement charmVisual, Canvas overlayCanvas, Point charmCenter)
    {
        const int sparkleCount = 5;

        for (int i = 0; i < sparkleCount; i++)
        {
            double angle = Random.NextDouble() * Math.PI * 2;
            double distance = 10 + Random.NextDouble() * 18;
            double offsetX = Math.Cos(angle) * distance;
            double offsetY = Math.Sin(angle) * distance;
            double size = 8 + Random.NextDouble() * 6;

                        var sparkle = new Path
            {
                Data = ShapeGeometryFactory.CreateStarGeometry(size),
                Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xD9, 0x66)),
                Stroke = new SolidColorBrush(Color.FromRgb(0xE8, 0xA8, 0x2A)),
                StrokeThickness = 0.75,
                Width = size,
                Height = size,
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0.3, 0.3)
            };

            Canvas.SetLeft(sparkle, charmCenter.X + offsetX - size / 2);
            Canvas.SetTop(sparkle, charmCenter.Y + offsetY - size / 2);
            overlayCanvas.Children.Add(sparkle);

            var delay = TimeSpan.FromMilliseconds(i * 40);

            var opacityAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(120),
                AutoReverse = true,
                BeginTime = delay
            };
            opacityAnim.Completed += (_, _) => overlayCanvas.Children.Remove(sparkle);

            var scaleAnim = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(350),
                BeginTime = delay
            };

            var scaleTransform = (ScaleTransform)sparkle.RenderTransform;
            sparkle.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }
    }
}