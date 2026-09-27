using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Trinket.Rendering;

namespace Trinket.Reactions;

/// <summary>A single full rotation, layered on the reaction rotation slot - never touches the physics-driven swing rotation.</summary>
public class SpinReaction : ICharmReaction
{
    public void Play(FrameworkElement charmVisual, Canvas overlayCanvas, Point charmCenter)
    {
        RotateTransform? reactionTransform = CharmRenderer.GetReactionTransform(charmVisual);
        if (reactionTransform == null) return;

        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        reactionTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
    }
}