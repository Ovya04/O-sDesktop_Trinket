using System.Text.Json;
using Trinket.Models;
using Xunit;

namespace Trinket.Tests;

/// <summary>
/// Tests the serialization shape settings round-trip through, without
/// touching the real %LOCALAPPDATA% file (SettingsService itself is a
/// static class tied to a fixed real path, so these tests exercise the
/// underlying JSON contract directly instead of risking interference with
/// a real user's saved settings file).
/// </summary>
public class SettingsSerializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    [Fact]
    public void AppSettings_RoundTripsThroughJsonWithoutLoss()
    {
        var original = new AppSettings
        {
            IsPaused = true,
            RopeLength = 275.5,
            AnchorPreset = AnchorPreset.TopCenter,
            CharmId = "star",
            CharmSize = 88.0,
            CordStyle = CordStyle.Braided,
            CordColorHex = "#FFAABBCC",
            BeadShape = BeadShape.Heart,
            MouseBreeze = BreezeLevel.Strong,
            AmbientBreeze = AmbientBreezeLevel.Breezy,
            MonitorDeviceName = "\\\\.\\DISPLAY1"
        };

        string json = JsonSerializer.Serialize(original, Options);
        AppSettings? restored = JsonSerializer.Deserialize<AppSettings>(json, Options);

        Assert.NotNull(restored);
        Assert.Equal(original.IsPaused, restored!.IsPaused);
        Assert.Equal(original.RopeLength, restored.RopeLength);
        Assert.Equal(original.AnchorPreset, restored.AnchorPreset);
        Assert.Equal(original.CharmId, restored.CharmId);
        Assert.Equal(original.CharmSize, restored.CharmSize);
        Assert.Equal(original.CordStyle, restored.CordStyle);
        Assert.Equal(original.CordColorHex, restored.CordColorHex);
        Assert.Equal(original.BeadShape, restored.BeadShape);
        Assert.Equal(original.MouseBreeze, restored.MouseBreeze);
        Assert.Equal(original.AmbientBreeze, restored.AmbientBreeze);
        Assert.Equal(original.MonitorDeviceName, restored.MonitorDeviceName);
    }

    [Fact]
    public void AppSettings_DeserializingEmptyJsonObject_FallsBackToDefaults()
    {
        // Simulates a settings file that exists but is missing fields
        // (e.g. from an older app version) - deserialization should fill
        // in defaults rather than throwing.
        AppSettings? restored = JsonSerializer.Deserialize<AppSettings>("{}", Options);

        Assert.NotNull(restored);
        Assert.False(restored!.IsPaused);
        Assert.Equal(220.0, restored.RopeLength);
        Assert.Equal("moon", restored.CharmId);
    }
}