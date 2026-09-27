namespace Trinket.Physics;

/// <summary>
/// Tunable constants for the rope simulation. Centralized here (rather than
/// scattered magic numbers inside RopeSimulation) so behavior can be tuned,
/// saved, or exposed in the Studio later without touching the simulation
/// math itself.
/// </summary>
public class PhysicsSettings
{
    /// <summary>
    /// Downward acceleration, in pixels/sec^2. Not real-world gravity - this
    /// is an arbitrary constant tuned purely for how the rope should feel.
    /// </summary>
    public double Gravity { get; set; } = 1800.0;

    /// <summary>
    /// The simulation always advances in fixed-size time slices, regardless
    /// of the actual (variable) frame rate. Keeps behavior consistent across
    /// different machines/refresh rates.
    /// </summary>
    public double FixedTimeStep { get; set; } = 1.0 / 120.0;

    /// <summary>
    /// Upper bound on how much real time a single rendered frame may feed
    /// into the physics accumulator, preventing a "spiral of death" catch-up
    /// after a lag spike.
    /// </summary>
    public double MaxFrameDelta { get; set; } = 0.25;

    /// <summary>
    /// Multiplier applied to each node's implicit Verlet velocity every step
    /// (velocity = CurrentPosition - OldPosition). Slightly below 1.0 so the
    /// rope gradually loses energy and settles, rather than swinging forever.
    /// </summary>
    public double VelocityDamping { get; set; } = 0.98;

    /// <summary>
    /// Number of nodes in the rope chain, including the pinned anchor node
    /// (index 0) and the charm node (the last index). Segment count is
    /// NodeCount - 1. More nodes = smoother-looking curve, more constraint
    /// solving cost per step.
    /// </summary>
    public int RopeNodeCount { get; set; } = 6;

    /// <summary>
    /// How many times per step we re-run the full pass of distance
    /// constraints across every segment. A single pass isn't enough for a
    /// multi-segment chain - fixing one segment's length can stretch its
    /// neighbor, so we need to repeat until the whole rope settles into a
    /// consistent shape. Higher = stiffer, more accurate rope, more CPU cost.
    /// </summary>
    public int ConstraintIterations { get; set; } = 8;

    /// <summary>
    /// Inverse mass assigned to the charm (last) node, relative to a normal
    /// rope node's inverse mass of 1.0. Below 1.0 makes the charm move less
    /// during constraint correction relative to its neighboring rope node -
    /// i.e. it behaves like it has more mass than a plain segment of cord.
    /// </summary>
    public double CharmInverseMassFactor { get; set; } = 0.5;

    /// <summary>
    /// Hard clamp on a node's per-step implicit speed (pixels/sec), applied
    /// as a safety net against instability from extreme drags or edge cases -
    /// part of the NaN/instability protection required by the project spec.
    /// </summary>
    public double MaxNodeSpeed { get; set; } = 4000.0;
    
        /// <summary>
    /// How close (pixels) the cursor must get to the charm's center before a
    /// breeze impulse can apply at all. Deliberately fairly tight - this is
    /// meant to feel like the cursor grazing the charm, not a wide field.
    /// </summary>
    public double MouseBreezeRadius { get; set; } = 90.0;

    /// <summary>
    /// Minimum real cursor speed (pixels/sec) required near the charm before
    /// any impulse is applied - a slow, deliberate approach shouldn't nudge
    /// it; only a fast pass-by should.
    /// </summary>
    public double MouseBreezeMinCursorSpeed { get; set; } = 800.0;

        public double AmbientGentleMinIntervalSeconds { get; set; } = 25.0;
    public double AmbientGentleMaxIntervalSeconds { get; set; } = 70.0;
    public double AmbientBreezyMinIntervalSeconds { get; set; } = 10.0;
    public double AmbientBreezyMaxIntervalSeconds { get; set; } = 30.0;

    /// <summary>
    /// Impulse strength (same units as MouseBreeze's applied impulse - a
    /// small positional nudge, not a raw velocity). Deliberately modest:
    /// ambient breeze should be a barely-there flicker, not a visible shove.
    /// </summary>
    public double AmbientGentleStrength { get; set; } = 1.0;
    public double AmbientBreezyStrength { get; set; } = 2.5;
    /// <summary>
    /// Impulse strength multipliers for each breeze setting, applied to the
    /// charm node's velocity as a fraction of the cursor's own speed.
    /// </summary>
    public double MouseBreezeGentleStrength { get; set; } = 0.03;
    public double MouseBreezeStrongStrength { get; set; } = 0.08;

    /// <summary>
    /// Minimum real time (seconds) between two applied breeze impulses,
    /// regardless of how often mouse-move events fire - prevents a gust from
    /// being recalculated (and re-applied) hundreds of times per second.
    /// </summary>
    public double MouseBreezeCooldownSeconds { get; set; } = 0.08;

    /// <summary>
    /// Below this speed (pixels/sec), a node is considered "essentially
    /// still" for sleep purposes. Sleep triggers only once EVERY non-pinned
    /// node in the rope stays below this for SleepDelaySeconds continuously.
    /// </summary>
    public double SleepVelocityThreshold { get; set; } = 2.0;

    /// <summary>
    /// How long (seconds) node speed must remain continuously below
    /// SleepVelocityThreshold before we actually stop the render/physics
    /// loop. Without this delay, a swinging pendulum would trigger sleep
    /// repeatedly and incorrectly at every zero-velocity crossing point of
    /// its oscillation, not just when it has genuinely come to rest.
    /// </summary>
    public double SleepDelaySeconds { get; set; } = 0.5;
}