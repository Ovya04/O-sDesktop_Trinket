using System.Windows;
using System.Windows.Media;

namespace Trinket.Models;

/// <summary>
/// Describes one charm's appearance and behavior. Deliberately contains no
/// physics or position data - only what it looks like and how it reacts.
/// </summary>
public class CharmDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CharmShape Shape { get; init; }
    public double Size { get; init; } = 60;
    public Color FillColor { get; init; } = Colors.White;
    public Color StrokeColor { get; init; } = Colors.Gray;
    public CharmReactionType Reaction { get; init; } = CharmReactionType.None;

    /// <summary>Only set for Shape == Custom - full path to the imported image file.</summary>
    public string? ImagePath { get; init; }

       /// <summary>Only used by Shape == Initial. Letter customization UI is not yet built - defaults to "T".</summary>
    public string InitialLetter { get; init; } = "T";
    /// <summary>
    /// Relative (0..1) point within the charm's visual where the cord
    /// attaches, and around which it rotates while swinging. Every built-in
    /// charm defaults to (0.5, 0.5) - the exact center - matching their
    /// original behavior from earlier phases. Custom imported charms set
    /// this from the user's click in AttachmentPointEditorWindow.
    /// </summary>
    public Point AttachmentPoint { get; init; } = new(0.5, 0.5);
}