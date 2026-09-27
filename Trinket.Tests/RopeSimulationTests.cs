using System.Windows;
using Trinket.Physics;
using Xunit;

namespace Trinket.Tests;

public class RopeSimulationTests
{
    private static PhysicsSettings CreateDefaultSettings() => new();

    private static RopeSimulation CreateRope(Point anchor, double length = 200.0, PhysicsSettings? settings = null)
    {
        return new RopeSimulation(anchor, length, settings ?? CreateDefaultSettings());
    }

    // --- Segment distance correctness ---

    [Fact]
    public void AfterSettling_AllSegmentsMatchExpectedLength()
    {
        var rope = CreateRope(new Point(0, 0), length: 200.0);
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 500; i++)
        {
            rope.Step(settings.FixedTimeStep);
        }

        double expectedSegmentLength = 200.0 / (settings.RopeNodeCount - 1);

        for (int i = 0; i < rope.Nodes.Count - 1; i++)
        {
            double actualDistance = Distance(rope.Nodes[i].CurrentPosition, rope.Nodes[i + 1].CurrentPosition);

            Assert.True(
                Math.Abs(actualDistance - expectedSegmentLength) < 1.0,
                $"Segment {i}: expected ~{expectedSegmentLength:F2}, got {actualDistance:F2}");
        }
    }

    [Fact]
    public void AnchorNode_AlwaysStaysExactlyAtAnchorPoint()
    {
        var anchor = new Point(150, 0);
        var rope = CreateRope(anchor);
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 300; i++)
        {
            rope.Step(settings.FixedTimeStep);
        }

        Assert.Equal(anchor.X, rope.Nodes[0].CurrentPosition.X, precision: 6);
        Assert.Equal(anchor.Y, rope.Nodes[0].CurrentPosition.Y, precision: 6);
    }

    // --- Extreme dragging ---

    [Fact]
    public void ExtremeDrag_StaysBoundedAndRecoversAfterRelease()
    {
        var rope = CreateRope(new Point(0, 0), length: 200.0);
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 100; i++) rope.Step(settings.FixedTimeStep);

        rope.BeginDrag();

        var random = new Random(42);
        for (int i = 0; i < 200; i++)
        {
            var wildTarget = new Point(random.Next(-3000, 3000), random.Next(-3000, 3000));
            var wildVelocity = new Vector(random.Next(-15000, 15000), random.Next(-15000, 15000));
            rope.SetDragTarget(wildTarget, wildVelocity);
            rope.Step(settings.FixedTimeStep);

            foreach (var node in rope.Nodes)
            {
                Assert.True(Math.Abs(node.CurrentPosition.X) < 20000 && Math.Abs(node.CurrentPosition.Y) < 20000,
                    $"Step {i}: node position diverged unexpectedly: {node.CurrentPosition}");
            }
        }

        rope.EndDrag();

        for (int i = 0; i < 300; i++)
        {
            rope.Step(settings.FixedTimeStep);
        }

        double expectedSegmentLength = 200.0 / (settings.RopeNodeCount - 1);
        for (int i = 0; i < rope.Nodes.Count - 1; i++)
        {
            double actualDistance = Distance(rope.Nodes[i].CurrentPosition, rope.Nodes[i + 1].CurrentPosition);
            Assert.True(Math.Abs(actualDistance - expectedSegmentLength) < 2.0,
                $"Segment {i} failed to recover after drag ended: {actualDistance:F2} vs expected {expectedSegmentLength:F2}");
        }
    }

    [Fact]
    public void ExtremeDrag_NeverProducesNaNOrInfinity()
    {
        var rope = CreateRope(new Point(0, 0));
        var settings = CreateDefaultSettings();

        rope.BeginDrag();

        var random = new Random(7);
        for (int i = 0; i < 500; i++)
        {
            var wildTarget = new Point(random.Next(-100000, 100000), random.Next(-100000, 100000));
            var wildVelocity = new Vector(random.Next(-1000000, 1000000), random.Next(-1000000, 1000000));
            rope.SetDragTarget(wildTarget, wildVelocity);
            rope.Step(settings.FixedTimeStep);

            foreach (var node in rope.Nodes)
            {
                Assert.False(double.IsNaN(node.CurrentPosition.X) || double.IsNaN(node.CurrentPosition.Y),
                    $"Step {i}: NaN position detected");
                Assert.False(double.IsInfinity(node.CurrentPosition.X) || double.IsInfinity(node.CurrentPosition.Y),
                    $"Step {i}: Infinite position detected");
            }
        }
    }

    // --- NaN protection at the integration level directly ---

    [Fact]
    public void Step_RecoversGracefullyEvenAfterManyRapidSteps()
    {
        var rope = CreateRope(new Point(0, 0));
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 5000; i++)
        {
            rope.Step(settings.FixedTimeStep);
        }

        Point charmPosition = rope.CharmNode.CurrentPosition;
        Assert.False(double.IsNaN(charmPosition.X) || double.IsNaN(charmPosition.Y));
        Assert.False(double.IsInfinity(charmPosition.X) || double.IsInfinity(charmPosition.Y));

        Assert.True(Math.Abs(charmPosition.X) < 1.0, $"Expected near X=0, got {charmPosition.X}");
    }

    // --- Different frame rates / timesteps ---

    [Theory]
    [InlineData(1.0 / 30.0)]
    [InlineData(1.0 / 60.0)]
    [InlineData(1.0 / 120.0)]
    [InlineData(1.0 / 240.0)]
    public void DifferentTimeSteps_AllProduceStableSettling(double dt)
    {
        var rope = CreateRope(new Point(0, 0), length: 200.0);

        int stepsNeededForFiveSeconds = (int)(5.0 / dt);

        for (int i = 0; i < stepsNeededForFiveSeconds; i++)
        {
            rope.Step(dt);
        }

        Point charmPosition = rope.CharmNode.CurrentPosition;

        Assert.False(double.IsNaN(charmPosition.X) || double.IsNaN(charmPosition.Y));
        Assert.True(Math.Abs(charmPosition.X) < 2.0,
            $"At dt={dt:F5}, expected settled charm near X=0, got {charmPosition.X:F2}");
    }

    // --- Sleep/wake-relevant behavior: velocity detection ---

    [Fact]
    public void GetMaxNodeSpeed_IsZeroWhenFullySettled()
    {
        var rope = CreateRope(new Point(0, 0));
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 1000; i++)
        {
            rope.Step(settings.FixedTimeStep);
        }

        double maxSpeed = rope.GetMaxNodeSpeed(settings.FixedTimeStep);

        Assert.True(maxSpeed < settings.SleepVelocityThreshold,
            $"Expected settled speed below sleep threshold ({settings.SleepVelocityThreshold}), got {maxSpeed:F2}");
    }

    [Fact]
    public void GetMaxNodeSpeed_IsSignificantImmediatelyAfterAThrow()
    {
        var rope = CreateRope(new Point(0, 0));
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 200; i++) rope.Step(settings.FixedTimeStep);

        rope.BeginDrag();
        rope.SetDragTarget(new Point(300, 200), new Vector(2000, 0));
        rope.Step(settings.FixedTimeStep);
        rope.EndDrag();

        double maxSpeed = rope.GetMaxNodeSpeed(settings.FixedTimeStep);

        Assert.True(maxSpeed > settings.SleepVelocityThreshold,
            "Expected a fresh throw to register well above the sleep threshold");
    }

    // --- Reset() correctness ---

    [Fact]
    public void Reset_ReturnsRopeToStraightLineBelowAnchor()
    {
        var anchor = new Point(100, 0);
        var rope = CreateRope(anchor, length: 200.0);
        var settings = CreateDefaultSettings();

        for (int i = 0; i < 100; i++) rope.Step(settings.FixedTimeStep);
        rope.BeginDrag();
        rope.SetDragTarget(new Point(500, 400), new Vector(3000, 3000));
        rope.Step(settings.FixedTimeStep);
        rope.EndDrag();

        rope.Reset();

        double expectedSegmentLength = 200.0 / (settings.RopeNodeCount - 1);
        for (int i = 0; i < rope.Nodes.Count; i++)
        {
            Assert.Equal(anchor.X, rope.Nodes[i].CurrentPosition.X, precision: 6);
            Assert.Equal(anchor.Y + expectedSegmentLength * i, rope.Nodes[i].CurrentPosition.Y, precision: 6);
        }
    }

    private static double Distance(Point a, Point b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}