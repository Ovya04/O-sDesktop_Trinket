using Trinket.Models;

namespace Trinket.Reactions;

public static class CharmReactionFactory
{
    private static readonly SparkleReaction Sparkle = new();
    private static readonly WiggleReaction Wiggle = new();
    private static readonly SpinReaction Spin = new();
    private static readonly PulseReaction Pulse = new();
    private static readonly BounceReaction Bounce = new();

    public static ICharmReaction? GetReaction(CharmReactionType type) => type switch
    {
        CharmReactionType.Sparkle => Sparkle,
        CharmReactionType.Wiggle => Wiggle,
        CharmReactionType.Spin => Spin,
        CharmReactionType.Pulse => Pulse,
        CharmReactionType.Bounce => Bounce,
        _ => null
    };
}