using System;
using System.Collections.Generic;
using System.Windows;

namespace Trinket.Physics;

public class RopeSimulation
{
    private readonly PhysicsSettings _settings;
    private readonly List<RopeNode> _nodes;
    private double _segmentLength;

    private bool _isDragging;
    private Point _dragTarget;
    private Vector _dragVelocity;

    public Point Anchor { get; set; }
    public IReadOnlyList<RopeNode> Nodes => _nodes;
    public RopeNode CharmNode => _nodes[_nodes.Count - 1];
    public bool IsDragging => _isDragging;

    public RopeSimulation(Point anchor, double ropeLength, PhysicsSettings settings)
    {
        _settings = settings;
        Anchor = anchor;

        int nodeCount = Math.Max(2, settings.RopeNodeCount);
        _segmentLength = ropeLength / (nodeCount - 1);

        _nodes = new List<RopeNode>(nodeCount);

        for (int i = 0; i < nodeCount; i++)
        {
            var startPosition = new Point(anchor.X, anchor.Y + _segmentLength * i);

            bool isAnchorNode = i == 0;
            bool isCharmNode = i == nodeCount - 1;

            double inverseMass = isAnchorNode ? 0.0
                : isCharmNode ? settings.CharmInverseMassFactor
                : 1.0;

            _nodes.Add(new RopeNode(startPosition, inverseMass, isAnchorNode));
        }
    }

    public void BeginDrag()
    {
        _isDragging = true;
        _dragTarget = CharmNode.CurrentPosition;
        _dragVelocity = new Vector(0, 0);
        CharmNode.OldPosition = CharmNode.CurrentPosition;
    }

    public void SetDragTarget(Point target, Vector velocity)
    {
        double speed = velocity.Length;
        if (speed > _settings.MaxNodeSpeed)
        {
            velocity *= _settings.MaxNodeSpeed / speed;
        }

        _dragTarget = target;
        _dragVelocity = velocity;
    }

    public void EndDrag()
    {
        _isDragging = false;
    }

    public void Reset()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            var position = new Point(Anchor.X, Anchor.Y + _segmentLength * i);
            _nodes[i].CurrentPosition = position;
            _nodes[i].OldPosition = position;
        }

        _isDragging = false;
    }

        /// <summary>
    /// Applies a one-off velocity nudge directly to the charm node, without
    /// affecting its OldPosition the way a drag does - this reads as a
    /// gentle push rather than teleporting the node. Has no effect while the
    /// user is actively dragging (their input always takes priority).
    /// </summary>
    public void ApplyImpulseToCharm(Vector impulse)
    {
        if (_isDragging)
        {
            return;
        }

        CharmNode.OldPosition = new Point(
            CharmNode.OldPosition.X - impulse.X,
            CharmNode.OldPosition.Y - impulse.Y);
    }

    /// <summary>
    /// Changes the target rope length live. Node positions are left where
    /// they are - the constraint solver naturally pulls them toward the new
    /// segment length over the next few frames, producing a smooth, organic
    /// resize rather than an instant teleport.
    /// </summary>
    public void SetLength(double ropeLength)
    {
        _segmentLength = ropeLength / (_nodes.Count - 1);
    }

    public void Step(double dt)
    {
        _nodes[0].CurrentPosition = Anchor;
        _nodes[0].OldPosition = Anchor;

        ApplyVerletIntegration(dt);
        SolveConstraints();
    }

    public double GetMaxNodeSpeed(double dt)
    {
        double maxSpeed = 0.0;

        foreach (RopeNode node in _nodes)
        {
            if (node.IsPinned)
            {
                continue;
            }

            Point velocity = Subtract(node.CurrentPosition, node.OldPosition);
            double speed = Length(velocity) / dt;

            if (speed > maxSpeed)
            {
                maxSpeed = speed;
            }
        }

        return maxSpeed;
    }

    private void ApplyVerletIntegration(double dt)
    {
        double gravityDisplacement = _settings.Gravity * dt * dt;

        for (int i = 1; i < _nodes.Count; i++)
        {
            RopeNode node = _nodes[i];

            if (node.IsPinned)
            {
                continue;
            }

            bool isCharmBeingDragged = _isDragging && node == CharmNode;

            if (isCharmBeingDragged)
            {
                node.CurrentPosition = _dragTarget;
                node.OldPosition = new Point(
                    _dragTarget.X - _dragVelocity.X * dt,
                    _dragTarget.Y - _dragVelocity.Y * dt);
                continue;
            }

            Point velocity = Subtract(node.CurrentPosition, node.OldPosition);
            velocity = Scale(velocity, _settings.VelocityDamping);

            double speed = Length(velocity);
            if (speed > _settings.MaxNodeSpeed)
            {
                velocity = Scale(velocity, _settings.MaxNodeSpeed / speed);
            }

            var newPosition = new Point(
                node.CurrentPosition.X + velocity.X,
                node.CurrentPosition.Y + velocity.Y + gravityDisplacement);

            node.OldPosition = node.CurrentPosition;
            node.CurrentPosition = newPosition;

            if (double.IsNaN(node.CurrentPosition.X) || double.IsNaN(node.CurrentPosition.Y) ||
                double.IsInfinity(node.CurrentPosition.X) || double.IsInfinity(node.CurrentPosition.Y))
            {
                node.CurrentPosition = node.OldPosition;
            }
        }
    }

    private void SolveConstraints()
    {
        for (int iteration = 0; iteration < _settings.ConstraintIterations; iteration++)
        {
            for (int i = 0; i < _nodes.Count - 1; i++)
            {
                SatisfyDistanceConstraint(_nodes[i], _nodes[i + 1]);
            }

            _nodes[0].CurrentPosition = Anchor;
        }
    }

    private void SatisfyDistanceConstraint(RopeNode a, RopeNode b)
    {
        Point delta = Subtract(b.CurrentPosition, a.CurrentPosition);
        double distance = Length(delta);

        if (distance < 0.0001)
        {
            return;
        }

        double difference = (distance - _segmentLength) / distance;
        double totalInverseMass = a.InverseMass + b.InverseMass;

        if (totalInverseMass <= 0.0)
        {
            return;
        }

        double aRatio = a.InverseMass / totalInverseMass;
        double bRatio = b.InverseMass / totalInverseMass;

        Point correction = Scale(delta, difference);

        if (a.InverseMass > 0.0)
        {
            a.CurrentPosition = Add(a.CurrentPosition, Scale(correction, aRatio));
        }

        if (b.InverseMass > 0.0)
        {
            b.CurrentPosition = Subtract(b.CurrentPosition, Scale(correction, bRatio));
        }
    }

    private static Point Add(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    private static Point Subtract(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    private static Point Scale(Point p, double s) => new(p.X * s, p.Y * s);
    private static double Length(Point p) => Math.Sqrt(p.X * p.X + p.Y * p.Y);
}