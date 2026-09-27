using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Trinket.Rendering;

namespace Trinket.Reactions;

/// <summary>A gentle grow-then-settle, using the dedicated reaction scale slot.</summary>
public class PulseReaction : ICharmReaction
{
    public void Play(FrameworkElement charmVisual, Canvas overlayCanvas, Point charmCenter)
    {
        ScaleTransform? scaleTransform = CharmRenderer.GetReactionScaleTransform(charmVisual);
        if (scaleTransform == null) return;

        var animation = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(450) };
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(1.18, KeyTime.FromPercent(0.4)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0)));

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }
}