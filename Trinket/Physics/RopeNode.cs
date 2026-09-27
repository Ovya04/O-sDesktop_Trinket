using System.Windows;

namespace Trinket.Physics;

/// <summary>
/// A single point mass in the rope chain. Deliberately a plain mutable class
/// (not a struct/record) since Verlet integration repeatedly mutates these
/// fields many times per frame across constraint iterations - a struct would
/// cause confusing copy-vs-reference bugs in a List&lt;RopeNode&gt;.
/// </summary>
public class RopeNode
{
    /// <summary>Where the node is right now.</summary>
    public Point CurrentPosition;

    /// <summary>
    /// Where the node was one fixed timestep ago. Verlet integration never
    /// stores velocity directly - it's implicitly (CurrentPosition - OldPosition).
    /// </summary>
    public Point OldPosition;

    /// <summary>
    /// True for the anchor node only. A pinned node ignores gravity/integration
    /// entirely and is manually snapped to the anchor's position every step.
    /// </summary>
    public bool IsPinned;

    /// <summary>
    /// Used by the distance-constraint solver to decide how much this node
    /// moves relative to the node it's connected to. 0 = infinite mass, never
    /// moves during constraint correction (used for the pinned anchor). 1.0 =
    /// a normal rope node. A value below 1.0 makes a node behave "heavier"
    /// (moves less during correction) - this is how the charm gets extra
    /// effective mass without a full force/mass physics model.
    /// </summary>
    public double InverseMass;

    public RopeNode(Point position, double inverseMass, bool isPinned)
    {
        CurrentPosition = position;
        OldPosition = position;
        InverseMass = inverseMass;
        IsPinned = isPinned;
    }
}