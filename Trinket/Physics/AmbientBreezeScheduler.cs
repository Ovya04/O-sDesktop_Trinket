using System;
using System.Windows;
using Trinket.Models;

namespace Trinket.Physics;

/// <summary>
/// Decides WHEN a random ambient gust should occur and how strong it is.
/// Pure scheduling/math logic with no WPF window dependency, so it can be
/// unit tested independently (Phase 23) - given a sequence of "now" times,
/// it should produce gusts only within the expected interval range.
/// </summary>
public class AmbientBreezeScheduler
{
    private readonly Random _random = new();
    private readonly PhysicsSettings _settings;

    private double _nextTriggerAtSeconds;
    private bool _hasScheduled;

    public AmbientBreezeScheduler(PhysicsSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Call periodically with the current time. Returns true (with an
    /// impulse vector) at most once per scheduled interval - most calls
    /// return false, which is the whole point: long calm stretches between
    /// rare, tiny movements.
    /// </summary>
    public bool TryGetGust(double nowSeconds, AmbientBreezeLevel level, out Vector impulse)
    {
        impulse = default;

        if (level == AmbientBreezeLevel.Off)
        {
            _hasScheduled = false;
            return false;
        }

        if (!_hasScheduled)
        {
            ScheduleNext(nowSeconds, level);
            return false;
        }

        if (nowSeconds < _nextTriggerAtSeconds)
        {
            return false;
        }

        impulse = GenerateGustImpulse(level);
        ScheduleNext(nowSeconds, level);
        return true;
    }

    private void ScheduleNext(double nowSeconds, AmbientBreezeLevel level)
    {
        (double minInterval, double maxInterval) = level == AmbientBreezeLevel.Breezy
            ? (_settings.AmbientBreezyMinIntervalSeconds, _settings.AmbientBreezyMaxIntervalSeconds)
            : (_settings.AmbientGentleMinIntervalSeconds, _settings.AmbientGentleMaxIntervalSeconds);

        double interval = minInterval + _random.NextDouble() * (maxInterval - minInterval);
        _nextTriggerAtSeconds = nowSeconds + interval;
        _hasScheduled = true;
    }

    private Vector GenerateGustImpulse(AmbientBreezeLevel level)
    {
        double strength = level == AmbientBreezeLevel.Breezy
            ? _settings.AmbientBreezyStrength
            : _settings.AmbientGentleStrength;

        // Mostly-horizontal random direction, like a real light breeze -
        // full random angle but with the vertical component dampened, so
        // gusts read as "sideways air movement" rather than a random flick
        // in any direction including straight down/up.
        double angle = _random.NextDouble() * Math.PI * 2;
        double x = Math.Cos(angle) * strength;
        double y = Math.Sin(angle) * strength * 0.4;

        return new Vector(x, y);
    }
}