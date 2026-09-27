using System.Collections.Generic;
namespace Trinket.Models;


/// <summary>
/// Everything about the app's configuration that should survive a restart.
/// A plain POCO, serialized directly to JSON.
/// </summary>
public class AppSettings
{
    public bool IsPaused { get; set; } = false;

    public double RopeLength { get; set; } = 220.0;

    public AnchorPreset AnchorPreset { get; set; } = AnchorPreset.TopRight;

        public string InitialLetter { get; set; } = "T";
    public double AnchorTopMargin { get; set; } = 0.0;

        /// <summary>Ids of the 2nd and 3rd charms in the cluster, if any. Empty = single charm (the default, unchanged behavior).</summary>
    
    public string CharmId { get; set; } = "moon";

    public double CharmSize { get; set; } = 60.0;
    public BreezeLevel MouseBreeze { get; set; } = BreezeLevel.Off;
        public AmbientBreezeLevel AmbientBreeze { get; set; } = AmbientBreezeLevel.Off;
    public CordStyle CordStyle { get; set; } = CordStyle.Thread;
        /// <summary>Null means "use the primary monitor" - the safe default and fallback.</summary>
    public string? MonitorDeviceName { get; set; } = null;
    /// <summary>
    /// Stored as a hex string (e.g. "#FFAAAAAA") rather than a WPF Color
    /// directly, since Color isn't natively JSON-friendly. Converted to/from
    /// an actual Color only where it's used for rendering.
    /// </summary>
    public string CordColorHex { get; set; } = "#FFAAAAAA";

    public BeadShape BeadShape { get; set; } = BeadShape.Pearl;
}