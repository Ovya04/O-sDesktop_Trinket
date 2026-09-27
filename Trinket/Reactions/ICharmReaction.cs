using System.Windows;
using System.Windows.Controls;

namespace Trinket.Reactions;

/// <summary>
/// A small, lightweight, purely decorative animation a charm plays when
/// clicked. Reactions never touch physics or persistent state - they're
/// pure visual flourish, expected to finish and clean up after themselves
/// within roughly half a second to a second.
/// </summary>
public interface ICharmReaction
{
    /// <summary>
    /// charmVisual: the charm's own visual (its reaction transform can be
    /// animated via CharmRenderer.GetReactionTransform). overlayCanvas: the
    /// parent canvas, for reactions that need to add their own temporary
    /// child elements near the charm. charmCenter: the charm's current
    /// on-screen center, in overlayCanvas coordinates.
    /// </summary>
    void Play(FrameworkElement charmVisual, Canvas overlayCanvas, Point charmCenter);
}