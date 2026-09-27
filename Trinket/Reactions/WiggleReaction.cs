using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Trinket.Rendering;

namespace Trinket.Reactions;

/// <summary>
/// A quick back-and-forth rotational shimmy, layered on top of (never
/// replacing) the charm's physics-driven rotation, via the charm's
/// dedicated reaction RotateTransform.
/// </summary>
public class WiggleReaction : ICharmReaction
{
    public void Play(FrameworkElement charmVisual, Canvas overlayCanvas, Point charmCenter)
    {
        RotateTransform? reactionTransform = CharmRenderer.GetReactionTransform(charmVisual);
        if (reactionTransform == null)
        {
            return;
        }

        var animation = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(500)
        };

        animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(-12, KeyTime.FromPercent(0.2)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(10, KeyTime.FromPercent(0.45)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(-6, KeyTime.FromPercent(0.7)));
        animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1.0)));

        // No cleanup needed: the keyframes return to 0, matching the
        // transform's permanent baseline, so it just holds at 0 afterward.
        reactionTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
    }
}