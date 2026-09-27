using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Trinket.Rendering;

namespace Trinket.Reactions;

/// <summary>A quick squash-and-stretch bounce, using the dedicated reaction scale slot.</summary>
public class BounceReaction : ICharmReaction
{
    public void Play(FrameworkElement charmVisual, Canvas overlayCanvas, Point charmCenter)
    {
        ScaleTransform? scaleTransform = CharmRenderer.GetReactionScaleTransform(charmVisual);
        if (scaleTransform == null) return;

        var scaleY = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(400) };
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0)));
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(0.75, KeyTime.FromPercent(0.25)));
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1.15, KeyTime.FromPercent(0.6)));
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0)));

        var scaleX = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(400) };
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0)));
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame(1.2, KeyTime.FromPercent(0.25)));
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame(0.9, KeyTime.FromPercent(0.6)));
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0)));

        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
    }
}