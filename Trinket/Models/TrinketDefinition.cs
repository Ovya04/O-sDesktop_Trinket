using System.Collections.Generic;
namespace Trinket.Models;

/// <summary>
/// A saved combination of charm + cord + beads + anchor - a named "look"
/// the user can save and reapply later. Distinct from AppSettings, which
/// tracks only the current live configuration.
/// </summary>
public class TrinketDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string CharmId { get; set; } = "moon";
    public double CharmSize { get; set; } = 60.0;

    public CordStyle CordStyle { get; set; } = CordStyle.Thread;
    public string CordColorHex { get; set; } = "#FFAAAAAA";
    public double RopeLength { get; set; } = 220.0;

    public BeadShape BeadShape { get; set; } = BeadShape.Pearl;

    public AnchorPreset AnchorPreset { get; set; } = AnchorPreset.TopRight;
}