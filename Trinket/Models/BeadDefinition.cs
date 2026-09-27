using System.Windows.Media;

namespace Trinket.Models;

public enum BeadShape
{
    None,
    Pearl,
    Round,
    Star,
    Heart
}

/// <summary>
/// Describes one bead's appearance. Like CharmDefinition, this only ever
/// answers "what does this bead look like" - its position always comes
/// from whichever rope node it's attached to, not from any state of its own.
/// </summary>
public class BeadDefinition
{
    public string Id { get; init; } = string.Empty;
    public BeadShape Shape { get; init; }
    public double Size { get; init; } = 18;
    public Color FillColor { get; init; } = Colors.White;
    public Color StrokeColor { get; init; } = Colors.Gray;
}